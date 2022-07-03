using System.Collections;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System;

/*
 * Connect via ssh:
 * ssh root@192.168.xx.xx
 * Pwd: root
 * 
 * Stop Default Stream:
 * /opt/StereoPi/stop.sh
 * 
 * Sending stream from Raspberry:
 * 
 * For UDP:
 * raspivid -t 0 -w 1280 -h 720 -fps 30 -3d sbs -cd MJPEG -o - | nc 192.168.1.10 3001 -u
 *
 * For TCP:
 * raspivid -t 0 -w 1280 -h 720 -fps 30 -3d sbs -cd MJPEG -o - | nc 192.168.1.10 3001
 * 
 * where 192.168.1.10 3001 - IP and port
*/

public class FMStereoPi : MonoBehaviour
{
    [HideInInspector] public FMNetworkManager Manager;

    public FMProtocol Protocol = FMProtocol.UDP;

    public void MulticastChecker()
    {
        UdpClient MulticastClient = new UdpClient();
        try
        {
            MulticastClient.Client.SendTimeout = 200;
            MulticastClient.EnableBroadcast = true;

            byte[] _byte = new byte[1];
            MulticastClient.Send(_byte, _byte.Length, new IPEndPoint(IPAddress.Broadcast, ClientListenPort));

            if (MulticastClient != null) MulticastClient.Close();
        }
        catch (Exception e)
        {
            if (MulticastClient != null) MulticastClient.Close();
        }
    }

    public int ClientListenPort = 3000;
    private UdpClient ClientListener;
    private IPEndPoint ServerEp;

    private TcpListener listener;
    private List<TcpClient> clients = new List<TcpClient>();
    private List<NetworkStream> streams = new List<NetworkStream>();
    private bool CreatedServer = false;
    public bool IsConnected = false;


    [HideInInspector] public int CurrentSeenTimeMS;
    [HideInInspector] public int LastReceivedTimeMS;

    private bool stop = false;
    private Queue<byte[]> _appendQueueReceivedBytes = new Queue<byte[]>();
    private object _asyncLockReceived = new object();

    private int ReceivedCount = 0;

    #region TCP
    IEnumerator NetworkServerStartTCP()
    {
        if (!CreatedServer)
        {
            CreatedServer = true;

            // create listener
            listener = new TcpListener(IPAddress.Any, ClientListenPort);
            listener.Start();
            listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // create LOOM thread, only create on first time, otherwise we will crash max thread limit
            // Wait for client to connect in another Thread 
            Loom.RunAsync(() => {
                while (!stop)
                {
                    // Wait for client connection
                    clients.Add(listener.AcceptTcpClient());
                    clients[clients.Count - 1].NoDelay = true;
                    //IsConnected = true;

                    streams.Add(clients[clients.Count - 1].GetStream());
                    streams[streams.Count - 1].WriteTimeout = 500;

                    Loom.QueueOnMainThread(() => {
                        //IsConnected = true;
                        if(clients != null)
                        {
                            if (clients.Count > 0) StartCoroutine(TCPReceiverCOR(clients[clients.Count-1], streams[streams.Count-1]));
                        }
                        
                    });
                    System.Threading.Thread.Sleep(1);
                }
            });

            while (!stop)
            {
                ReceivedCount = _appendQueueReceivedBytes.Count;
                while (_appendQueueReceivedBytes.Count > 0)
                {
                    lock (_asyncLockReceived) Manager.OnReceivedByteDataEvent.Invoke(_appendQueueReceivedBytes.Dequeue());
                }
                yield return null;
            }
        }
        yield break;
    }

    IEnumerator TCPReceiverCOR(TcpClient _client, NetworkStream _stream)
    {
        bool _break = false;
        _stream.ReadTimeout = 1000;

        Loom.RunAsync(() => {
            while (!_client.Connected) System.Threading.Thread.Sleep(1);
            while (!stop && !_break)
            {
                _stream.Flush();
                byte[] bytes = new byte[300000];

                // Loop to receive all the data sent by the client.
                int _length;
                while ((_length = _stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    if (_length > 0)
                    {
                        byte[] _s = new byte[_length];
                        Buffer.BlockCopy(bytes, 0, _s, 0, _length);
                        lock (_asyncLockReceived) _appendQueueReceivedBytes.Enqueue(_s);
                        LastReceivedTimeMS = Environment.TickCount;
                    }
                    System.Threading.Thread.Sleep(1);
                }

                if(_length == 0)
                {
                    if (_stream != null)
                    {
                        try { _stream.Close(); }
                        catch (Exception e) { DebugLog(e.Message); }
                    }

                    if (_client != null)
                    {
                        try { _client.Close(); }
                        catch (Exception e) { DebugLog(e.Message); }
                    }

                    for (int i = 0; i < clients.Count; i++)
                    {
                        if (_client == clients[i])
                        {
                            streams.Remove(streams[i]);
                            clients.Remove(clients[i]);
                        }
                    }
                    _break = true;
                }
            }
            System.Threading.Thread.Sleep(1);
        });

        while (!stop && !_break) yield return null;
        yield break;
    }
    #endregion

    #region UDP
    IEnumerator NetworkClientStartUDP()
    {
        LastReceivedTimeMS = Environment.TickCount;

        stop = false;
        yield return new WaitForSeconds(0.5f);

        MulticastChecker();
        yield return new WaitForSeconds(0.5f);

        //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv Client Receiver vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
        while (Loom.numThreads >= Loom.maxThreads) yield return null;
        Loom.RunAsync(() =>
        {
            while (!stop)
            {
                try
                {
                    if (ClientListener == null)
                    {
                        ClientListener = new UdpClient(ClientListenPort);
                        ClientListener.Client.ReceiveTimeout = 2000;
                        ServerEp = new IPEndPoint(IPAddress.Any, ClientListenPort);
                    }

                    byte[] ReceivedData = ClientListener.Receive(ref ServerEp);
                    LastReceivedTimeMS = Environment.TickCount;

                    //=======================Decode Data=======================
                    lock (_asyncLockReceived) _appendQueueReceivedBytes.Enqueue(ReceivedData);
                }
                catch (SocketException socketException)
                {
                    //DebugLog("Client Receiver Timeout: " + socketException.ToString());
                    if (ClientListener != null) ClientListener.Close(); ClientListener = null;
                }
                //System.Threading.Thread.Sleep(1);
            }
            System.Threading.Thread.Sleep(1);
        });
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Client Receiver ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        while (!stop)
        {
            ReceivedCount = _appendQueueReceivedBytes.Count;
            while (_appendQueueReceivedBytes.Count > 0)
            {
                lock (_asyncLockReceived) Manager.OnReceivedByteDataEvent.Invoke(_appendQueueReceivedBytes.Dequeue());
            }
            yield return null;
        }
        yield break;
    }
    #endregion

    public void Action_StartClient() { StartAll(); }
    public void Action_StopClient() { StopAll(); }

    void StartAll()
    {
        stop = false;
        switch (Protocol)
        {
            case FMProtocol.UDP: StartCoroutine(NetworkClientStartUDP()); break;
            case FMProtocol.TCP: StartCoroutine(NetworkServerStartTCP()); break;
        }
    }

    void StopAll()
    {
        stop = true;
        switch (Protocol)
        {
            case FMProtocol.UDP:
                StopCoroutine(NetworkClientStartUDP());
                break;
            case FMProtocol.TCP:
                foreach (TcpClient client in clients)
                {
                    if (client != null)
                    {
                        try { client.Close(); }
                        catch (Exception e) { DebugLog(e.Message); }
                    }
                    IsConnected = false;
                }
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;
        StartAll();
    }

    private void Update()
    {
        CurrentSeenTimeMS = Environment.TickCount;
        if (CurrentSeenTimeMS < 0 && LastReceivedTimeMS > 0)
        {
            IsConnected = (Mathf.Abs(CurrentSeenTimeMS - int.MinValue) + (int.MaxValue - LastReceivedTimeMS) < 3000) ? true : false;
        }
        else
        {
            IsConnected = ((CurrentSeenTimeMS - LastReceivedTimeMS) < 3000) ? true : false;
        }
    }

    public bool ShowLog = true;
    public void DebugLog(string _value) { if (ShowLog) Debug.Log(_value); }

    private void OnApplicationQuit() { StopAll(); }
    private void OnDisable() { StopAll(); }
    private void OnDestroy() { StopAll(); }
    private void OnEnable()
    {
        if (Time.timeSinceLevelLoad <= 3f) return;
        if (stop) StartAll();
    }
}
