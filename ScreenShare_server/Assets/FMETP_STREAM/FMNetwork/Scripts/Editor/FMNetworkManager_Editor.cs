using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FMNetworkManager))]
[CanEditMultipleObjects]
public class FMNetworkManager_Editor : Editor
{
    private FMNetworkManager FMNetwork;

    private bool ShowServerSettings = false;
    private bool ShowClientSettings = false;
    private bool ShowStereoPiSettings = false;

    SerializedProperty AutoInitProp;
    SerializedProperty NetworkTypeProp;
    SerializedProperty StereoPiProtocolProp;


    SerializedProperty ServerSettings_ServerListenPortProp;
    SerializedProperty ServerSettings_UseAsyncListenerProp;
    SerializedProperty ServerSettings_UseMainThreadSenderProp;
    SerializedProperty ServerSettings_ConnectionCountProp;

    SerializedProperty ClientSettings_ClientListenPortProp;
    SerializedProperty ClientSettings_UseMainThreadSenderProp;
    SerializedProperty ClientSettings_AutoNetworkDiscoveryProp;
    SerializedProperty ClientSettings_ServerIPProp;
    SerializedProperty ClientSettings_ForceBroadcastProp;
    SerializedProperty ClientSettings_IsConnectedProp;

    SerializedProperty StereoPiSettings_StereoPiProtocolProp;
    SerializedProperty StereoPiSettings_ClientListenPortProp;
    SerializedProperty StereoPiSettings_IsConnectedProp;

    SerializedProperty DebugStatusProp;
    SerializedProperty ShowLogProp;
    SerializedProperty UIStatusProp;

    SerializedProperty OnReceivedByteDataEventProp;
    SerializedProperty OnReceivedStringDataEventProp;
    SerializedProperty GetRawReceivedDataProp;

    SerializedProperty NetworkObjectsProp;
    private bool NetworkObjectsFold = false;
    SerializedProperty SyncFPSProp;

    //SerializedProperty labelProp;
    //SerializedProperty dataLengthProp;

    void OnEnable()
    {

        AutoInitProp = serializedObject.FindProperty("AutoInit");
        NetworkTypeProp = serializedObject.FindProperty("NetworkType");
        StereoPiProtocolProp = serializedObject.FindProperty("StereoPiProtocol");

        ServerSettings_ServerListenPortProp = serializedObject.FindProperty("ServerSettings.ServerListenPort");
        ServerSettings_UseAsyncListenerProp = serializedObject.FindProperty("ServerSettings.UseAsyncListener");
        ServerSettings_UseMainThreadSenderProp = serializedObject.FindProperty("ServerSettings.UseMainThreadSender");
        ServerSettings_ConnectionCountProp = serializedObject.FindProperty("ServerSettings.ConnectionCount");


        ClientSettings_ClientListenPortProp = serializedObject.FindProperty("ClientSettings.ClientListenPort");
        ClientSettings_UseMainThreadSenderProp = serializedObject.FindProperty("ClientSettings.UseMainThreadSender");
        ClientSettings_AutoNetworkDiscoveryProp = serializedObject.FindProperty("ClientSettings.AutoNetworkDiscovery");
        ClientSettings_ServerIPProp = serializedObject.FindProperty("ClientSettings.ServerIP");
        ClientSettings_ForceBroadcastProp = serializedObject.FindProperty("ClientSettings.ForceBroadcast");
        ClientSettings_IsConnectedProp = serializedObject.FindProperty("ClientSettings.IsConnected");

        StereoPiSettings_StereoPiProtocolProp = serializedObject.FindProperty("StereoPiSettings.StereoPiProtocol");
        StereoPiSettings_ClientListenPortProp = serializedObject.FindProperty("StereoPiSettings.ClientListenPort");
        StereoPiSettings_IsConnectedProp = serializedObject.FindProperty("StereoPiSettings.IsConnected");

        DebugStatusProp = serializedObject.FindProperty("DebugStatus");
        ShowLogProp = serializedObject.FindProperty("ShowLog");
        UIStatusProp = serializedObject.FindProperty("UIStatus");

        OnReceivedByteDataEventProp = serializedObject.FindProperty("OnReceivedByteDataEvent");
        OnReceivedStringDataEventProp = serializedObject.FindProperty("OnReceivedStringDataEvent");
        GetRawReceivedDataProp = serializedObject.FindProperty("GetRawReceivedData");

        NetworkObjectsProp = serializedObject.FindProperty("NetworkObjects");
        SyncFPSProp = serializedObject.FindProperty("SyncFPS");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        if (FMNetwork == null) FMNetwork = (FMNetworkManager)target;

        serializedObject.Update();


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            {
                //Header
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.white;
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = 15;

                Texture2D backgroundTexture = new Texture2D(1, 1);
                backgroundTexture.SetPixel(0, 0, new Color(0.09019608f, 0.09019608f, 0.2745098f));
                backgroundTexture.Apply();
                style.normal.background = backgroundTexture;

                GUILayout.BeginHorizontal();
                GUILayout.Label("(( FM Network UDP ))", style);
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("- Networking");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(AutoInitProp, new GUIContent("Auto Init"));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(NetworkTypeProp, new GUIContent("NetworkType"));
                GUILayout.EndHorizontal();

                if (FMNetwork.NetworkType == FMNetworkType.Server || FMNetwork.NetworkType == FMNetworkType.Client)
                {
                    GUILayout.BeginVertical();
                    {
                        if (ShowServerSettings)
                        {
                            GUILayout.BeginHorizontal();

                            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                            if (GUILayout.Button("- Server Settings")) ShowServerSettings = false;
                            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                            GUILayout.EndHorizontal();

                            GUILayout.BeginVertical("box");
                            {
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(ServerSettings_ServerListenPortProp, new GUIContent("ServerListenPort"));
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(ServerSettings_UseAsyncListenerProp, new GUIContent("UseAsyncListener"));
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(ServerSettings_UseMainThreadSenderProp, new GUIContent("UseMainThreadSender"));
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(ServerSettings_ConnectionCountProp, new GUIContent("Connection Count"));
                                GUILayout.EndHorizontal();
                            }
                            GUILayout.EndVertical();
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();
                            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                            if (GUILayout.Button("+ Server Settings")) ShowServerSettings = true;
                            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    {
                        if (ShowClientSettings)
                        {
                            GUILayout.BeginHorizontal();
                            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                            if (GUILayout.Button("- Client Settings")) ShowClientSettings = false;
                            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                            GUILayout.EndHorizontal();

                            GUILayout.BeginVertical("box");
                            {
                                GUILayout.BeginVertical("box");
                                {
                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(ClientSettings_ClientListenPortProp, new GUIContent("ClientListenPort"));
                                    GUILayout.EndHorizontal();
                                }
                                GUILayout.EndVertical();

                                GUILayout.BeginVertical("box");
                                {
                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(ClientSettings_UseMainThreadSenderProp, new GUIContent("UseMainThreadSender"));
                                    GUILayout.EndHorizontal();

                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(ClientSettings_ForceBroadcastProp, new GUIContent("ForceBroadcast"));
                                    GUILayout.EndHorizontal();
                                }
                                GUILayout.EndVertical();

                                GUILayout.BeginVertical("box");
                                {
                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(ClientSettings_AutoNetworkDiscoveryProp, new GUIContent("AutoNetworkDiscovery"));
                                    GUILayout.EndHorizontal();

                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(ClientSettings_ServerIPProp, new GUIContent("ServerIP"));
                                    GUILayout.EndHorizontal();

                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(ClientSettings_IsConnectedProp, new GUIContent("IsConnected"));
                                    GUILayout.EndHorizontal();
                                }
                                GUILayout.EndVertical();

                            }
                            GUILayout.EndVertical();
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();
                            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                            if (GUILayout.Button("+ Client Settings")) ShowClientSettings = true;
                            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndVertical();
                }
                else
                {
                    GUILayout.BeginVertical();
                    {
                        if (ShowStereoPiSettings)
                        {
                            GUILayout.BeginHorizontal();

                            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                            if (GUILayout.Button("- StereoPi Settings")) ShowStereoPiSettings = false;
                            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                            GUILayout.EndHorizontal();

                            GUILayout.BeginVertical("box");
                            {
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(StereoPiSettings_StereoPiProtocolProp, new GUIContent("StereoPiProtocol"));
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(StereoPiSettings_ClientListenPortProp, new GUIContent("ClientListenPort"));
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(StereoPiSettings_IsConnectedProp, new GUIContent("IsConnected"));
                                GUILayout.EndHorizontal();
                            }
                            GUILayout.EndVertical();
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();
                            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                            if (GUILayout.Button("+ StereoPi Settings")) ShowStereoPiSettings = true;
                            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        if (FMNetwork.NetworkType == FMNetworkType.Server || FMNetwork.NetworkType == FMNetworkType.Client)
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("box");
            {
                GUILayout.Label("- Sync Transformation from Server");
                GUILayout.BeginVertical("box");
                {
                    int NetworkObjectsNum = NetworkObjectsProp.FindPropertyRelative("Array.size").intValue;

                    if (NetworkObjectsFold)
                    {
                        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                        if (GUILayout.Button("- NetworkObjects: " + NetworkObjectsNum)) NetworkObjectsFold = false;
                        GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                        DrawPropertyArray(NetworkObjectsProp);
                    }
                    else
                    {
                        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                        if (GUILayout.Button("+ NetworkObjects: " + NetworkObjectsNum)) NetworkObjectsFold = true;
                        GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                    }

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(SyncFPSProp, new GUIContent("SyncFPS"));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }


        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Receiver");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(OnReceivedByteDataEventProp, new GUIContent("OnReceivedByteDataEvent"));
                GUILayout.EndHorizontal();

                if (FMNetwork.NetworkType == FMNetworkType.Server || FMNetwork.NetworkType == FMNetworkType.Client)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(OnReceivedStringDataEventProp, new GUIContent("OnReceivedStringDataEvent"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(GetRawReceivedDataProp, new GUIContent("GetRawReceivedData"));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();


        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Debug");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(DebugStatusProp, new GUIContent("Debug Status"));
                GUILayout.EndHorizontal();

                if (FMNetwork.DebugStatus)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Status: ");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(FMNetwork.Status);
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(UIStatusProp, new GUIContent("UIStatus"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ShowLogProp, new GUIContent("ShowLog"));
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();


        serializedObject.ApplyModifiedProperties();
    }

    private void DrawPropertyArray(SerializedProperty property)
    {
        SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");
        EditorGUILayout.PropertyField(arraySizeProp);

        EditorGUI.indentLevel++;

        for (int i = 0; i < arraySizeProp.intValue; i++)
        {
            EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));
        }

        EditorGUI.indentLevel--;
    }
}
