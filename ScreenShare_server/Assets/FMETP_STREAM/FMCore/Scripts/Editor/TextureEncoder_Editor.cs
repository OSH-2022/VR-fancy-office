using System;
using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(TextureEncoder))]
[CanEditMultipleObjects]
public class TextureEncoder_Editor: Editor
{
    private TextureEncoder TEncoder;

    SerializedProperty TextureTypeProp;

    SerializedProperty StreamTextureProp;
    SerializedProperty StreamRenderTextureProp;
    SerializedProperty ResolutionScalarProp;

    SerializedProperty FastModeProp;
    SerializedProperty AsyncModeProp;
    SerializedProperty GZipModeProp;
    SerializedProperty EnableAsyncGPUReadbackProp;

    SerializedProperty QualityProp;
    SerializedProperty ChromaSubsamplingProp;

    SerializedProperty StreamFPSProp;

    SerializedProperty ignoreSimilarTextureProp;
    SerializedProperty similarByteSizeThresholdProp;

    SerializedProperty OnDataByteReadyEventProp;

    SerializedProperty labelProp;
    SerializedProperty dataLengthProp;

    void OnEnable()
    {
        TextureTypeProp = serializedObject.FindProperty("TextureType");

        StreamTextureProp = serializedObject.FindProperty("StreamTexture");
        StreamRenderTextureProp = serializedObject.FindProperty("StreamRenderTexture");

        ResolutionScalarProp = serializedObject.FindProperty("ResolutionScalar");

        FastModeProp = serializedObject.FindProperty("FastMode");
        AsyncModeProp = serializedObject.FindProperty("AsyncMode");
        GZipModeProp = serializedObject.FindProperty("GZipMode");
        EnableAsyncGPUReadbackProp = serializedObject.FindProperty("EnableAsyncGPUReadback");

        QualityProp = serializedObject.FindProperty("Quality");
        ChromaSubsamplingProp = serializedObject.FindProperty("ChromaSubsampling");

        StreamFPSProp = serializedObject.FindProperty("StreamFPS");

        ignoreSimilarTextureProp = serializedObject.FindProperty("ignoreSimilarTexture");
        similarByteSizeThresholdProp = serializedObject.FindProperty("similarByteSizeThreshold");

        OnDataByteReadyEventProp = serializedObject.FindProperty("OnDataByteReadyEvent");

        labelProp = serializedObject.FindProperty("label");
        dataLengthProp = serializedObject.FindProperty("dataLength");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        if (TEncoder == null) TEncoder = (TextureEncoder)target;

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

            GUILayout.Label("- Target Texture");

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(TextureTypeProp, new GUIContent("Texture Type"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            if(TEncoder.TextureType == FMTextureType.Texture2D)
            {
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(StreamTextureProp, new GUIContent("Stream Texture"));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            else if (TEncoder.TextureType == FMTextureType.RenderTexture)
            {
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(StreamRenderTextureProp, new GUIContent("Stream Render Texture"));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            else if (TEncoder.TextureType == FMTextureType.WebcamTexture)
            {
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ResolutionScalarProp, new GUIContent("Resolution Scalar"));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }

        }
        GUILayout.EndVertical();

        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Settings");

            GUILayout.BeginHorizontal();
            GUILayout.Label("(supported format: RGB24, RGBA32)");
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(QualityProp, new GUIContent("Quality"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(StreamFPSProp, new GUIContent("StreamFPS"));
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FastModeProp, new GUIContent("Fast Encode Mode"));
                    GUILayout.EndHorizontal();

                    if (TEncoder.FastMode)
                    {
                        GUILayout.BeginVertical("box");
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(AsyncModeProp, new GUIContent("Async (multi-threading)"));
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical("box");
                        {
                            GUILayout.BeginHorizontal();
                            GUIStyle style = new GUIStyle();
                            style.normal.textColor = TEncoder.SupportsAsyncGPUReadback ? Color.green : Color.gray;
                            GUILayout.Label(" Async GPU Readback (" + (TEncoder.SupportsAsyncGPUReadback ? "Supported" : "Unknown or Not Supported") + ")", style);
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(EnableAsyncGPUReadbackProp, new GUIContent("Enabled When Supported"));
                            GUILayout.EndHorizontal();

                            GUILayout.BeginVertical("box");
                            {
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(ChromaSubsamplingProp, new GUIContent("Chroma Subsampling"));
                                GUILayout.EndHorizontal();
                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.EndVertical();
                    }

                    {
                        GUILayout.BeginHorizontal();
                        GUIStyle style = new GUIStyle();
                        style.normal.textColor = Color.yellow;
                        GUILayout.Label(" Experiment for Mac, Windows, Android (Forced Enabled on iOS)", style);
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(GZipModeProp, new GUIContent("GZip Mode", "Reduce network traffic"));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Label("- Networking");
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ignoreSimilarTextureProp, new GUIContent("ignore Similar Texture"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(similarByteSizeThresholdProp, new GUIContent("similar Byte Size Threshold"));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Encoded");
            if (TEncoder.GetPreviewTexture != null) { GUILayout.Label("Preview " + " ( " + TEncoder.StreamWidth + " x " + TEncoder.StreamHeight + " ) "); }
            else { GUILayout.Label("Preview (Empty)"); }
            GUILayout.BeginVertical("box");
            {
                const float maxLogoWidth = 430.0f;
                EditorGUILayout.Separator();
                float w = EditorGUIUtility.currentViewWidth;
                Rect r = new Rect();
                r.width = Math.Min(w - 40.0f, maxLogoWidth);
                r.height = r.width / 4.886f;
                Rect r2 = GUILayoutUtility.GetRect(r.width, r.height);
                r.x = r2.x;
                r.y = r2.y;

                if (TEncoder.GetPreviewTexture != null) { GUI.DrawTexture(r, TEncoder.GetPreviewTexture, ScaleMode.ScaleToFit); }
                else { GUI.DrawTexture(r, new Texture2D((int)r.width, (int)r.height, TextureFormat.RGB24, false), ScaleMode.ScaleToFit); }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(OnDataByteReadyEventProp, new GUIContent("OnDataByteReadyEvent"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
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

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(dataLengthProp, new GUIContent("Encoded Size(byte)"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
