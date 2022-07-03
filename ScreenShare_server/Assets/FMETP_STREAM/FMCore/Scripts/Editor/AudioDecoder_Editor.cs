using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioDecoder))]
[CanEditMultipleObjects]
public class AudioDecoder_Editor : Editor
{
    private AudioDecoder ADecoder;
    SerializedProperty labelProp;
    SerializedProperty volumeProp;

    SerializedProperty OnPCMFloatReadyEventProp;

    void OnEnable()
    {
        labelProp = serializedObject.FindProperty("label");
        volumeProp = serializedObject.FindProperty("volume");
        OnPCMFloatReadyEventProp = serializedObject.FindProperty("OnPCMFloatReadyEvent");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        if(ADecoder== null) ADecoder = (AudioDecoder)target;

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
                GUILayout.Label("(( FMETP STREAM CORE V2 ))", style);
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("- Playback");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(volumeProp, new GUIContent("Volume"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.Label("- Audio Info");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Source Sample Rate: " + ADecoder.SourceSampleRate);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Source Channels: " + ADecoder.SourceChannels);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Device Sample Rate: " + ADecoder.DeviceSampleRate);
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Audio Data");
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(OnPCMFloatReadyEventProp, new GUIContent("OnPCMFloatReadyEvent"));
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Pair Encoder & Decoder ");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(labelProp, new GUIContent("label"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
