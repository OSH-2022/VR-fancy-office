using System;
using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(GameViewEncoder))]
[CanEditMultipleObjects]
public class GameViewEncoder_Editor : Editor
{
    private GameViewEncoder GVEncoder;

    SerializedProperty CaptureModeProp;

    SerializedProperty ResizeProp;

    SerializedProperty MainCamProp;
    SerializedProperty RenderCamProp;
    SerializedProperty ResolutionProp;
    SerializedProperty MatchScreenAspectProp;

    SerializedProperty FMDesktopFlipXProp;
    SerializedProperty FMDesktopFlipYProp;
    SerializedProperty FMDesktopRangeXProp;
    SerializedProperty FMDesktopRangeYProp;
    SerializedProperty FMDesktopOffsetXProp;
    SerializedProperty FMDesktopOffsetYProp;
    SerializedProperty FMDesktopMonitorIDProp;
    SerializedProperty FMDesktopCorrectRotationProp;


    SerializedProperty FastModeProp;
    SerializedProperty AsyncModeProp;
    SerializedProperty GZipModeProp;
    SerializedProperty EnableAsyncGPUReadbackProp;

    SerializedProperty ColorReductionLevelProp;

    SerializedProperty PanoramaModeProp;
    SerializedProperty CubemapResolutionProp;

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
        CaptureModeProp = serializedObject.FindProperty("CaptureMode");

        ResizeProp = serializedObject.FindProperty("Resize");

        MainCamProp = serializedObject.FindProperty("MainCam");
        RenderCamProp = serializedObject.FindProperty("RenderCam");
        ResolutionProp = serializedObject.FindProperty("Resolution");
        MatchScreenAspectProp = serializedObject.FindProperty("MatchScreenAspect");

        FMDesktopFlipXProp = serializedObject.FindProperty("FMDesktopFlipX");
        FMDesktopFlipYProp = serializedObject.FindProperty("FMDesktopFlipY");
        FMDesktopRangeXProp = serializedObject.FindProperty("FMDesktopRangeX");
        FMDesktopRangeYProp = serializedObject.FindProperty("FMDesktopRangeY");
        FMDesktopOffsetXProp = serializedObject.FindProperty("FMDesktopOffsetX");
        FMDesktopOffsetYProp = serializedObject.FindProperty("FMDesktopOffsetY");
        FMDesktopMonitorIDProp = serializedObject.FindProperty("FMDesktopMonitorID");
        FMDesktopCorrectRotationProp = serializedObject.FindProperty("FMDesktopCorrectRotation");

        FastModeProp = serializedObject.FindProperty("FastMode");
        AsyncModeProp = serializedObject.FindProperty("AsyncMode");
        GZipModeProp = serializedObject.FindProperty("GZipMode");
        EnableAsyncGPUReadbackProp = serializedObject.FindProperty("EnableAsyncGPUReadback");

        ColorReductionLevelProp = serializedObject.FindProperty("ColorReductionLevel");

        PanoramaModeProp = serializedObject.FindProperty("PanoramaMode");
        CubemapResolutionProp = serializedObject.FindProperty("CubemapResolution");

        QualityProp = serializedObject.FindProperty("Quality");
        ChromaSubsamplingProp = serializedObject.FindProperty("ChromaSubsampling");

        StreamFPSProp = serializedObject.FindProperty("StreamFPS");

        ignoreSimilarTextureProp = serializedObject.FindProperty("ignoreSimilarTexture");
        similarByteSizeThresholdProp = serializedObject.FindProperty("similarByteSizeThreshold");

        OnDataByteReadyEventProp = serializedObject.FindProperty("OnDataByteReadyEvent");

        labelProp = serializedObject.FindProperty("label");
        dataLengthProp = serializedObject.FindProperty("dataLength");
    }

    private void Action_SetSymbol()
    {
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        List<string> allDefines = definesString.Split(';').ToList();

        //remove FMCOLOR symbol
        for (int i = 0; i < allDefines.Count; i++)
        {
            if (allDefines[i].Contains("FMETP_URP")) allDefines.RemoveAt(i);
        }

        List<string> newDefines = new List<string>();
        for (int i = 0; i < allDefines.Count; i++)
        {
            for (int j = 0; j < newDefines.Count; j++)
            {
                if (allDefines[i] == newDefines[j]) break;
            }
            newDefines.Add(allDefines[i]);
        }

        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
        {
            //found render pipeline
            newDefines.Add("FMETP_URP");
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", newDefines.ToArray()));
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        if (GVEncoder == null) GVEncoder = (GameViewEncoder)target;

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

            GUILayout.Label("- Mode");

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(CaptureModeProp, new GUIContent("Capture Mode"));
            GUILayout.EndHorizontal();

            if (GVEncoder.CaptureMode == GameViewCaptureMode.MainCam)
            {
                //Add symbol for render pipeline
                Action_SetSymbol();

                if (GVEncoder.MainCam == null)
                {
                    if (GVEncoder.MainCam == null) GVEncoder.MainCam = GVEncoder.gameObject.GetComponent<Camera>();
                    if (GVEncoder.MainCam == null) GVEncoder.MainCam = GVEncoder.gameObject.AddComponent<Camera>();
                }
                else
                {
                    if (GVEncoder.MainCam != GVEncoder.gameObject.GetComponent<Camera>()) GVEncoder.MainCam = null;

                    if (GVEncoder.MainCam == null) GVEncoder.MainCam = GVEncoder.gameObject.GetComponent<Camera>();
                    if (GVEncoder.MainCam == null) GVEncoder.MainCam = GVEncoder.gameObject.AddComponent<Camera>();
                }
                GUILayout.BeginVertical("box");
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.yellow;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("capture camera with screen aspect", style);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            else if (GVEncoder.CaptureMode == GameViewCaptureMode.RenderCam)
            {
                GUILayout.BeginVertical("box");
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.yellow;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("render texture with free aspect", style);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            else if (GVEncoder.CaptureMode == GameViewCaptureMode.FullScreen)
            {
                GUILayout.BeginVertical("box");
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.yellow;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("capture full screen with UI Canvas", style);
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
            GUILayout.BeginVertical("box");
            {
                if (GVEncoder.CaptureMode == GameViewCaptureMode.MainCam)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(MainCamProp, new GUIContent("MainCam"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ResizeProp, new GUIContent("Resize"));
                    GUILayout.EndHorizontal();
                }
                if (GVEncoder.CaptureMode == GameViewCaptureMode.RenderCam)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(RenderCamProp, new GUIContent("RenderCam"));
                    GUILayout.EndHorizontal();

                    if (GVEncoder.RenderCam == null)
                    {
                        //GUILayout.BeginVertical("box");
                        {
                            GUIStyle style = new GUIStyle();
                            style.normal.textColor = Color.red;
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(" Render Camera cannot be null", style);
                            GUILayout.EndHorizontal();

                        }
                        //GUILayout.EndVertical();
                    }

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ResolutionProp, new GUIContent("Resolution"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(MatchScreenAspectProp, new GUIContent("MatchScreenAspect"));
                    GUILayout.EndHorizontal();

                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(PanoramaModeProp, new GUIContent("Panorama Mode", "Render 360 view as Panorama"));
                    GUILayout.EndHorizontal();

                    if (GVEncoder.PanoramaMode)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(CubemapResolutionProp, new GUIContent("Cubemap Sampling"));
                        GUILayout.EndHorizontal();
                    }
                }

                if (GVEncoder.CaptureMode == GameViewCaptureMode.FullScreen)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ResizeProp, new GUIContent("Resize"));
                    GUILayout.EndHorizontal();
                }

                if (GVEncoder.CaptureMode == GameViewCaptureMode.Desktop)
                {
                    GUILayout.BeginHorizontal();
                    GUIStyle style = new GUIStyle();

                    if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.WindowsPlayer)
                    {
                        style.normal.textColor = Color.red;
                        GUILayout.Label("[Experiment] supported Windows 10 only", style);
                    }
                    else
                    {
                        style.normal.textColor = Color.yellow;
                        GUILayout.Label("[Experiment] supported Windows 10 only (Windows 8 may work too)", style);
                    }
                    
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FMDesktopMonitorIDProp, new GUIContent("Monitor ID" + "(Count: " + GVEncoder.FMDesktopMonitorCount + ")"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ResolutionProp, new GUIContent("Resolution"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(MatchScreenAspectProp, new GUIContent("MatchScreenAspect"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FMDesktopFlipXProp, new GUIContent("Flip X"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FMDesktopFlipYProp, new GUIContent("Flip Y"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FMDesktopRangeXProp, new GUIContent("Range X"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FMDesktopRangeYProp, new GUIContent("Range Y"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FMDesktopOffsetXProp, new GUIContent("Offset X"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FMDesktopOffsetYProp, new GUIContent("Offset Y"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    style.normal.textColor = Color.yellow;
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX)
                    GUILayout.Label("[ Monitor Orientation: " + GVEncoder.FMDesktopRotation.ToString() + " ]");
#else
                    GUILayout.Label("[ Monitor Orientation: " + "Unknown" + " ]");
#endif
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FMDesktopCorrectRotationProp, new GUIContent("Correct Rotation"));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

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

                    if (GVEncoder.FastMode)
                    {
                        GUILayout.BeginVertical("box");
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(AsyncModeProp, new GUIContent("Async Encode (multi-threading)"));
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical("box");
                        {
                            GUILayout.BeginHorizontal();
                            //GUILayout.Label("[ Async GPU Readback Support ]");
                            //GUILayout.Label("Async GPU Readback");

                            GUIStyle style = new GUIStyle();
                            style.normal.textColor = GVEncoder.SupportsAsyncGPUReadback ? Color.green : Color.gray;
                            GUILayout.Label(" Async GPU Readback (" + (GVEncoder.SupportsAsyncGPUReadback ? "Supported" : "Unknown or Not Supported") + ")", style);
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(EnableAsyncGPUReadbackProp, new GUIContent("Enabled When Supported"));
                            GUILayout.EndHorizontal();

                        }
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical("box");
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(ChromaSubsamplingProp, new GUIContent("Chroma Subsampling"));
                            GUILayout.EndHorizontal();
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

                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ColorReductionLevelProp, new GUIContent("ColorReductionLevel"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.yellow;
                    GUILayout.Label(" Experiment feature: Reduce network traffic", style);
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
            if (GVEncoder.GetStreamTexture != null)
            {
                GUILayout.Label("Preview " + GVEncoder.GetStreamTexture.GetType().ToString() + " ( " + GVEncoder.GetStreamTexture.width + " x " + GVEncoder.GetStreamTexture.height + " ) ");
            }
            else
            {
                GUILayout.Label("Preview (Empty)");
            }
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
                if (GVEncoder.GetStreamTexture != null)
                {
                    GUI.DrawTexture(r, GVEncoder.GetStreamTexture, ScaleMode.ScaleToFit);
                }
                else
                {
                    GUI.DrawTexture(r, new Texture2D((int)r.width, (int)r.height, TextureFormat.RGB24, false), ScaleMode.ScaleToFit);
                }
            }
            GUILayout.EndVertical();

            //GUILayout.BeginHorizontal();
            //EditorGUILayout.PropertyField(CapturedTextureProp, new GUIContent("Captured Texture"));
            //GUILayout.EndHorizontal();

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


                //GUILayout.BeginHorizontal();
                //GUILayout.Label("Encoded Size(byte): " + GVEncoder.dataLength);
                //GUILayout.EndHorizontal();

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
