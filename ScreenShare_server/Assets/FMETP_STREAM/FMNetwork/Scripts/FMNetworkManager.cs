using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;


using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

public enum FMProtocol { UDP, TCP }
public enum FMNetworkType { Server, Client, StereoPi }
public enum FMSendType { All, Server, Others, TargetIP }
public struct FMPacket
{
    public byte[] SendByte;
    public string SkipIP;
    public FMSendType SendType;
    public string TargetIP;
}
public struct FMNetworkTransform
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localScale;
}

public class FMNetworkManager : MonoBehaviour
{
    public string LocalIPAddress()
    {
        string localIP = "0.0.0.0";
        //ssIPHostEntry host;
        //host = Dns.GetHostEntry(Dns.GetHostName());

        List<string> detectedIPs = new List<string>();
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            //if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            //commented above condition, as it may not work on Android, found issues on Google Pixel Phones, its type returns "0" for unknown reason.
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (ip.IsDnsEligible)
                        {
                            string detectedIP = ip.Address.ToString();
                            if (detectedIP != "127.0.0.1" && detectedIP != "0.0.0.0")
                            {
                                try
                                {
                                    if (ip.AddressValidLifetime / 2 != int.MaxValue)
                                    {
                                        localIP = detectedIP;
                                    }
                                    else
                                    {
                                        //if didn't find any yet, this is the only one
                                        if (localIP == "0.0.0.0") localIP = detectedIP;
                                    }
                                }
                                catch
                                {
                                    localIP = detectedIP;
                                }

                                detectedIPs.Add(localIP);
                            }
                        }
                    }
                }
            }
        }

#if UNITY_EDITOR || UNITY_STANDALONE || WINDOWS_UWP
        if (detectedIPs.Count > 1)
        {
            string endPointIP = "0.0.0.0";
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    endPointIP = endPoint.Address.ToString();
                    if (socket.Connected) socket.Disconnect(true);
                }
            }
            catch { }

            for (int i = 0; i < detectedIPs.Count; i++)
            {
                if (detectedIPs[i] == endPointIP) localIP = detectedIPs[i];
            }
        }
#endif
        return localIP;
    }

    private string _localIP;
    public string ReadLocalIPAddress
    {
        get
        {
            if (_localIP == null) _localIP = LocalIPAddress();
            if (_localIP.Length <= 3) _localIP = LocalIPAddress();
            return _localIP;
        }
    }

    public static FMNetworkManager instance;
    public bool AutoInit = true;

    [HideInInspector]
    public bool Initialised = false;

    [Tooltip("Initialise as Server or Client")]
    public FMNetworkType NetworkType;

    [HideInInspector] public FMServer Server;
    [HideInInspector] public FMClient Client;
    [HideInInspector] public FMStereoPi StereoPi;

    [Serializable]
    public class FMServerSettings
    {
        public int ServerListenPort = 3333;

        [Tooltip("(( on supported devices only ))")]
        public bool UseAsyncListener = false;

        [Tooltip("(( suggested for low-end mobile, but not recommend for streaming large data ))")]
        public bool UseMainThreadSender = true;
        public int ConnectionCount;
    }

    [Serializable]
    public class FMClientSettings
    {
        public int ClientListenPort = 3334;

        [Tooltip("(( suggested for low-end mobile, but not recommend for streaming large data ))")]
        public bool UseMainThreadSender = true;

        [Tooltip("(( Experimental: broadcast data to all devices in local network, and this client will be discovered and registered by multiple servers. However, it's not reliable for important data. ))")]
        public bool ForceBroadcast = false;

        [Tooltip("(( true by default ))")]
        public bool AutoNetworkDiscovery = true;
        [Tooltip("(( only applied when Auto Network Discovery is off ))")]
        public string ServerIP;
        public bool IsConnected;
    }

    [Serializable]
    public class FMStereoPiSettings
    {
        public FMProtocol StereoPiProtocol = FMProtocol.UDP;
        public int ClientListenPort = 3001;
        public bool IsConnected;
    }

    [Tooltip("Network Settings for Server")]
    public FMServerSettings ServerSettings;
    [Tooltip("Network Settings for Client")]
    public FMClientSettings ClientSettings;
    [Tooltip("Network Settings for StereoPi")]
    public FMStereoPiSettings StereoPiSettings;

    public bool DebugStatus = true;
    public bool ShowLog = true;
    [TextArea(1, 10)]
    public string Status;
    public Text UIStatus;

    public UnityEventByteArray OnReceivedByteDataEvent = new UnityEventByteArray();
    public UnityEventString OnReceivedStringDataEvent = new UnityEventString();
    public UnityEventByteArray GetRawReceivedData = new UnityEventByteArray();

    //server events
    public UnityEventString OnClientConnectedEvent = new UnityEventString();
    public UnityEventString OnClientDisconnectedEvent = new UnityEventString();
    public void OnClientConnected(string ClientIP)
    {
        OnClientConnectedEvent.Invoke(ClientIP);
        if (ShowLog) Debug.Log("OnClientConnected: " + ClientIP);
    }
    public void OnClientDisconnected(string ClientIP)
    {
        OnClientDisconnectedEvent.Invoke(ClientIP);
        if (ShowLog) Debug.Log("OnClientDisonnected: " + ClientIP);
    }

    //client events
    public UnityEventString OnFoundServerEvent = new UnityEventString();
    public UnityEventString OnLostServerEvent = new UnityEventString();
    public void OnFoundServer(string ServerIP)
    {
        OnFoundServerEvent.Invoke(ServerIP);
        if (ShowLog) Debug.Log("OnFoundServer: " + ServerIP);
    }
    public void OnLostServer(string ServerIP)
    {
        OnLostServerEvent.Invoke(ServerIP);
        if (ShowLog) Debug.Log("OnLostServer: " + ServerIP);
    }

#region Network Objects Setup
    [Header("[ Sync ] Server => Client")]
    [Tooltip("Sync Transformation of Network Objects. # Both Server and Clients should have same number of NetworkObjects")]
    public GameObject[] NetworkObjects;
    FMNetworkTransform[] NetworkTransform;

    //[Tooltip("Frequency for sync (second)")]
    private float SyncFrequency = 0.05f;
    [Range(1f, 60f)]
    public float SyncFPS = 20f;
    private float SyncTimer = 0f;

    private float LastReceivedTimestamp = 0f;
    private float TargetTimestamp = 0f;
    private float CurrentTimestamp = 0f;

    void Action_SendNetworkObjectTransform()
    {
        if (NetworkType == FMNetworkType.Server)
        {
            byte[] Timestamp = BitConverter.GetBytes(Time.realtimeSinceStartup);

            byte[] Data = new byte[NetworkObjects.Length * 10 * 4];
            byte[] SendByte = new byte[Timestamp.Length + Data.Length];

            int index = 0;
            Buffer.BlockCopy(Timestamp, 0, SendByte, index, Timestamp.Length);
            index += Timestamp.Length;

            foreach (GameObject obj in NetworkObjects)
            {
                byte[] TransformByte = EncodeTransformByte(obj);
                Buffer.BlockCopy(TransformByte, 0, SendByte, index, TransformByte.Length);
                index += TransformByte.Length;
            }
            Server.Action_AddNetworkObjectPacket(SendByte, FMSendType.Others);
        }
    }

    byte[] EncodeTransformByte(GameObject obj)
    {
        byte[] _byte = new byte[40];
        Vector3 _pos = obj.transform.position;
        Quaternion _rot = obj.transform.rotation;
        Vector3 _scale = obj.transform.localScale;

        float[] _float = new float[]
        {
            _pos.x,_pos.y,_pos.z,
            _rot.x,_rot.y,_rot.z,_rot.w,
            _scale.x,_scale.y,_scale.z
        };
        Buffer.BlockCopy(_float, 0, _byte, 0, _byte.Length);
        return _byte;
    }

    float[] DecodeByteToFloatArray(byte[] _data, int _offset)
    {
        float[] _transform = new float[10];
        for (int i = 0; i < _transform.Length; i++)
        {
            _transform[i] = BitConverter.ToSingle(_data, i * 4 + _offset);
        }

        return _transform;
    }

    public void Action_SyncNetworkObjectTransform(byte[] _data)
    {
        float Timestamp = BitConverter.ToSingle(_data, 0);
        int meta_offset = 4;

        if (Timestamp > LastReceivedTimestamp)
        {
            LastReceivedTimestamp = TargetTimestamp;
            TargetTimestamp = Timestamp;
            CurrentTimestamp = LastReceivedTimestamp;

            for (int i = 0; i < NetworkObjects.Length; i++)
            {
                float[] _transform = DecodeByteToFloatArray(_data, meta_offset + i * 40);
                NetworkTransform[i].position = new Vector3(_transform[0], _transform[1], _transform[2]);
                NetworkTransform[i].rotation = new Quaternion(_transform[3], _transform[4], _transform[5], _transform[6]);
                NetworkTransform[i].localScale = new Vector3(_transform[7], _transform[8], _transform[9]);
            }
        }
    }
#endregion

    public void Action_InitAsServer()
    {
        NetworkType = FMNetworkType.Server;
        Init();
    }

    public void Action_InitAsClient()
    {
        NetworkType = FMNetworkType.Client;
        Init();
    }

    public void Action_InitStereoPi()
    {
        NetworkType = FMNetworkType.StereoPi;
        Init();
    }

    /// <summary>
    /// Close connection locally, for either Server or Client
    /// </summary>
    public void Action_Close()
    {
        Initialised = false;
        if (Server != null) Destroy(Server);
        if (Client != null) Destroy(Client);
        if (StereoPi != null) Destroy(StereoPi);

        ServerSettings.ConnectionCount = 0;
        ClientSettings.IsConnected = false;
        StereoPiSettings.IsConnected = false;

        UpdateDebugText();

        GC.Collect();
    }

    /// <summary>
    /// Server Commands only, close client's connection remotely
    /// </summary>
    public void Action_CloseClientConnection(string _clientIP)
    {
        if (NetworkType != FMNetworkType.Server) return;
        if (!Server.IsConnected) return;
        Server.Action_CloseClientConnection(_clientIP);
    }

    /// <summary>
    /// Server Commands only, close all clients' connection remotely
    /// </summary>
    public void Action_CloseAllClientsConnection()
    {
        if (NetworkType != FMNetworkType.Server) return;
        if (!Server.IsConnected) return;

        if (ServerSettings.ConnectionCount > 0)
        {
            for (int i = 0; i < Server.ConnectedIPs.Count; i++)
            {
                Server.Action_CloseClientConnection(Server.ConnectedIPs[i]);
            }
        }
    }

    void Init()
    {
        if (Initialised) Action_Close();

        switch (NetworkType)
        {
            case FMNetworkType.Server:
                Server = this.gameObject.AddComponent<FMServer>();
                Server.hideFlags = HideFlags.HideInInspector;

                Server.Manager = this;

                Server.ServerListenPort = ServerSettings.ServerListenPort;
                Server.ClientListenPort = ClientSettings.ClientListenPort;

                Server.UseAsyncListener = ServerSettings.UseAsyncListener;
                Server.UseMainThreadSender = ServerSettings.UseMainThreadSender;
                break;
            case FMNetworkType.Client:
                Client = this.gameObject.AddComponent<FMClient>();
                Client.hideFlags = HideFlags.HideInInspector;

                Client.Manager = this;

                Client.ServerListenPort = ServerSettings.ServerListenPort;
                Client.ClientListenPort = ClientSettings.ClientListenPort;

                Client.UseMainThreadSender = ClientSettings.UseMainThreadSender;
                Client.AutoNetworkDiscovery = ClientSettings.AutoNetworkDiscovery;
                if (ClientSettings.ServerIP == "") ClientSettings.ServerIP = "127.0.0.1";
                if (!Client.AutoNetworkDiscovery) Client.ServerIP = ClientSettings.ServerIP;

                Client.ForceBroadcast = ClientSettings.ForceBroadcast;

                NetworkTransform = new FMNetworkTransform[NetworkObjects.Length];
                for (int i = 0; i < NetworkTransform.Length; i++)
                {
                    NetworkTransform[i] = new FMNetworkTransform();
                    NetworkTransform[i].position = Vector3.zero;
                    NetworkTransform[i].rotation = Quaternion.identity;
                    NetworkTransform[i].localScale = new Vector3(1f, 1f, 1f);
                }
                break;
            case FMNetworkType.StereoPi:
                StereoPi = this.gameObject.AddComponent<FMStereoPi>();
                StereoPi.hideFlags = HideFlags.HideInInspector;

                StereoPi.Manager = this;

                StereoPi.Protocol = StereoPiSettings.StereoPiProtocol;
                StereoPi.ClientListenPort = StereoPiSettings.ClientListenPort;

                break;
        }

        Initialised = true;
    }

    void Awake()
    {
        Application.runInBackground = true;
        if (instance == null) instance = this;
    }

    //void Awake()
    //{
    //    if (instance == null)
    //    {
    //        instance = this;
    //        this.gameObject.transform.parent = null;
    //        DontDestroyOnLoad(this.gameObject);
    //    }
    //    else
    //    {
    //        Destroy(this.gameObject);
    //    }
    //}

    private void OnEnable()
    {
        if (!Initialised) return;
        switch (NetworkType)
        {
            case FMNetworkType.Server:
                if (Server != null) Server.enabled = true;
                break;
            case FMNetworkType.Client:
                if (Client != null) Client.enabled = true;
                break;
            case FMNetworkType.StereoPi:
                if (StereoPi != null) StereoPi.enabled = true;
                break;
        }

        UpdateDebugText();
    }
    private void OnDisable()
    {
        if (!Initialised) return;
        switch (NetworkType)
        {
            case FMNetworkType.Server:
                if (Server != null) Server.enabled = false;
                break;
            case FMNetworkType.Client:
                if (Client != null) Client.enabled = false;
                break;
            case FMNetworkType.StereoPi:
                if (StereoPi != null) StereoPi.enabled = false;
                break;
        }

        UpdateDebugText(true);
    }

    // Use this for initialization
    void Start() { if (AutoInit) Init(); }

    // Update is called once per frame
    void Update()
    {
        if (Initialised == false) return;
        switch (NetworkType)
        {
            case FMNetworkType.Server:
                //====================Sync Network Object============================
                if (Server.ConnectionCount > 0)
                {
                    if (NetworkObjects.Length > 0)
                    {
                        SyncFrequency = 1f / SyncFPS;
                        SyncTimer += Time.deltaTime;
                        if (SyncTimer > SyncFrequency)
                        {
                            Action_SendNetworkObjectTransform();
                            SyncTimer = SyncTimer % SyncFrequency;
                        }
                    }
                }
                Server.ShowLog = ShowLog;
                //====================Sync Network Object============================
                ServerSettings.ConnectionCount = Server.ConnectionCount;
                break;
            case FMNetworkType.Client:
                //====================Sync Network Object============================
                if (Client.IsConnected)
                {
                    if (NetworkObjects.Length > 0)
                    {
                        for (int i = 0; i < NetworkObjects.Length; i++)
                        {
                            CurrentTimestamp += Time.deltaTime;
                            float step = (CurrentTimestamp - LastReceivedTimestamp) / (TargetTimestamp - LastReceivedTimestamp);
                            step = Mathf.Clamp(step, 0f, 1f);
                            NetworkObjects[i].transform.position = Vector3.Slerp(NetworkObjects[i].transform.position, NetworkTransform[i].position, step);
                            NetworkObjects[i].transform.rotation = Quaternion.Slerp(NetworkObjects[i].transform.rotation, NetworkTransform[i].rotation, step);
                            NetworkObjects[i].transform.localScale = Vector3.Slerp(NetworkObjects[i].transform.localScale, NetworkTransform[i].localScale, step);
                        }
                    }
                }
                Client.ShowLog = ShowLog;
                //====================Sync Network Object============================
                ClientSettings.IsConnected = Client.IsConnected;
                break;
            case FMNetworkType.StereoPi:
                StereoPi.ShowLog = ShowLog;
                StereoPiSettings.IsConnected = StereoPi.IsConnected;
                break;
        }

        UpdateDebugText();
    }

    private void UpdateDebugText(bool onNetworkManagerDisabled = false)
    {
        //====================Update Debug Text============================
        #region Debug Status
        if (DebugStatus)
        {
            string _status = "";
            _status += "Thread: " + Loom.numThreads + " / " + Loom.maxThreads + "\n";
            _status += "Network Type: " + NetworkType.ToString() + "\n";
            _status += "Local IP: " + ReadLocalIPAddress + "\n";

            if (!onNetworkManagerDisabled)
            {
                switch (NetworkType)
                {
                    case FMNetworkType.Server:
                        _status += "Connection Count: " + ServerSettings.ConnectionCount + "\n";
                        _status += "Async Listener: " + ServerSettings.UseAsyncListener + "\n";
                        _status += "Use Main Thread Sender: " + ServerSettings.UseMainThreadSender + "\n";

                        foreach (FMServer.ConnectedClient _cc in Server.ConnectedClients)
                        {
                            if (_cc != null)
                            {
                                _status += "connected ip: " + _cc.IP + "\n";

                                _status += "last seen: " + _cc.LastSeenTimeMS + "\n";
                                _status += "last send: " + _cc.LastSentTimeMS + "\n";
                            }
                            else
                            {
                                _status += "Connected Client: null/unknown issue" + "\n";
                            }
                        }
                        break;
                    case FMNetworkType.Client:
                        _status += "Is Connected: " + ClientSettings.IsConnected + "\n";
                        _status += "Use Main Thread Sender: " + ClientSettings.UseMainThreadSender + "\n";

                        if (ClientSettings.IsConnected)
                        {
                            _status += "last send: " + Client.LastSentTimeMS + "\n";
                            _status += "last received: " + Client.LastReceivedTimeMS + "\n";
                        }
                        break;
                    case FMNetworkType.StereoPi:
                        _status += "Is Connected: " + StereoPi.IsConnected + "\n";
                        _status += "last received: " + StereoPi.LastReceivedTimeMS + "\n";
                        break;
                }
            }
            else
            {
                switch (NetworkType)
                {
                    case FMNetworkType.Server:
                        _status += "Connection Count: " + "0" + "\n";
                        _status += "Async Listener: " + ServerSettings.UseAsyncListener + "\n";
                        _status += "Use Main Thread Sender: " + ServerSettings.UseMainThreadSender + "\n";
                        break;
                    case FMNetworkType.Client:
                        _status += "Is Connected: " + false + "\n";
                        _status += "Use Main Thread Sender: " + ClientSettings.UseMainThreadSender + "\n";
                        break;
                    case FMNetworkType.StereoPi:
                        _status += "Is Connected: " + false + "\n";
                        break;
                }
            }

            Status = _status;
            if (UIStatus != null) UIStatus.text = Status;
        }
        #endregion
        //====================Update Debug Text============================
    }

    #region SENDER MAPPING
    public void Send(byte[] _byteData, FMSendType _type) { Send(_byteData, _type, null); }
    public void Send(string _stringData, FMSendType _type) { Send(_stringData, _type, null); }

    public void SendToAll(byte[] _byteData) { Send(_byteData, FMSendType.All, null); }
    public void SendToServer(byte[] _byteData) { Send(_byteData, FMSendType.Server, null); }
    public void SendToOthers(byte[] _byteData) { Send(_byteData, FMSendType.Others, null); }

    public void SendToAll(string _stringData) { Send(_stringData, FMSendType.All, null); }
    public void SendToServer(string _stringData) { Send(_stringData, FMSendType.Server, null); }
    public void SendToOthers(string _stringData) { Send(_stringData, FMSendType.Others, null); }

    public void SendToTarget(byte[] _byteData, string _targetIP)
    {
        if (NetworkType == FMNetworkType.Server)
        {
            if (Server.ConnectedIPs.Contains(_targetIP))
            {
                Send(_byteData, FMSendType.TargetIP, _targetIP);
            }
            else
            {
                if (_targetIP == ReadLocalIPAddress || _targetIP == "127.0.0.1" || _targetIP == "localhost")
                {
                    OnReceivedByteDataEvent.Invoke(_byteData);
                }
            }
        }
        else
        {
            if (_targetIP == ReadLocalIPAddress || _targetIP == "127.0.0.1" || _targetIP == "localhost")
            {
                OnReceivedByteDataEvent.Invoke(_byteData);
            }
            else
            {
                Send(_byteData, FMSendType.TargetIP, _targetIP);
            }
        }
    }
    public void SendToTarget(string _stringData, string _targetIP)
    {
        if (NetworkType == FMNetworkType.Server)
        {
            if (Server.ConnectedIPs.Contains(_targetIP))
            {
                Send(_stringData, FMSendType.TargetIP, _targetIP);
            }
            else
            {
                if (_targetIP == ReadLocalIPAddress || _targetIP == "127.0.0.1" || _targetIP == "localhost")
                {
                    OnReceivedStringDataEvent.Invoke(_stringData);
                }
            }
        }
        else
        {
            if (_targetIP == ReadLocalIPAddress || _targetIP == "127.0.0.1" || _targetIP == "localhost")
            {
                OnReceivedStringDataEvent.Invoke(_stringData);
            }
            else
            {
                Send(_stringData, FMSendType.TargetIP, _targetIP);
            }
        }
    }

    private void Send(byte[] _byteData, FMSendType _type, string _targetIP)
    {
        if (!Initialised) return;
        if (NetworkType == FMNetworkType.Client && !Client.IsConnected) return;

        if (NetworkType == FMNetworkType.Client)
        {
            if (Client.ForceBroadcast)
            {
                if (_type == FMSendType.All) OnReceivedByteDataEvent.Invoke(_byteData);
                //_type = FMSendType.Server; //when broadcast mode enabled, force the send type to server, then it won't send twice to others
            }
        }

        switch (_type)
        {
            case FMSendType.All:
                if (NetworkType == FMNetworkType.Server)
                {
                    Server.Action_AddPacket(_byteData, _type);
                    OnReceivedByteDataEvent.Invoke(_byteData);
                }
                else
                {
                    Client.Action_AddPacket(_byteData, _type);
                }
                break;
            case FMSendType.Server:
                if (NetworkType == FMNetworkType.Server)
                {
                    OnReceivedByteDataEvent.Invoke(_byteData);
                }
                else
                {
                    Client.Action_AddPacket(_byteData, _type);
                }
                break;
            case FMSendType.Others:
                if (NetworkType == FMNetworkType.Server)
                {
                    Server.Action_AddPacket(_byteData, _type);
                }
                else
                {
                    Client.Action_AddPacket(_byteData, _type);
                }
                break;
            case FMSendType.TargetIP:
                if (NetworkType == FMNetworkType.Server)
                {
                    if (_targetIP.Length > 4) Server.Action_AddPacket(_byteData, _targetIP);
                }
                else
                {
                    if (_targetIP.Length > 4) Client.Action_AddPacket(_byteData, _targetIP);
                }
                break;
        }
    }

    private void Send(string _stringData, FMSendType _type, string _targetIP)
    {
        if (!Initialised) return;
        if (NetworkType == FMNetworkType.Client && !Client.IsConnected) return;

        if (NetworkType == FMNetworkType.Client)
        {
            if (Client.ForceBroadcast)
            {
                if (_type == FMSendType.All) OnReceivedStringDataEvent.Invoke(_stringData);
                //_type = FMSendType.Server; //when broadcast mode enabled, force the send type to server, then it won't send twice to others
            }
        }

        switch (_type)
        {
            case FMSendType.All:
                if (NetworkType == FMNetworkType.Server)
                {
                    Server.Action_AddPacket(_stringData, _type);
                    OnReceivedStringDataEvent.Invoke(_stringData);
                }
                else
                {
                    Client.Action_AddPacket(_stringData, _type);
                }
                break;
            case FMSendType.Server:
                if (NetworkType == FMNetworkType.Server)
                {
                    OnReceivedStringDataEvent.Invoke(_stringData);
                }
                else
                {
                    Client.Action_AddPacket(_stringData, _type);
                }
                break;
            case FMSendType.Others:
                if (NetworkType == FMNetworkType.Server)
                {
                    Server.Action_AddPacket(_stringData, _type);
                }
                else
                {
                    Client.Action_AddPacket(_stringData, _type);
                }
                break;
            case FMSendType.TargetIP:
                if (NetworkType == FMNetworkType.Server)
                {
                    if (_targetIP.Length > 6) Server.Action_AddPacket(_stringData, _targetIP);
                }
                else
                {
                    if (_targetIP.Length > 6) Client.Action_AddPacket(_stringData, _targetIP);
                }
                break;
        }
    }

#endregion

    public void Action_ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

}

