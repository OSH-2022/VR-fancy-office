using System.Collections;
using UnityEngine;
using System;

using UnityEngine.Rendering;

public enum GameViewCaptureMode { RenderCam, MainCam, FullScreen, Desktop }
public enum GameViewResize { Full, Half, Quarter, OneEighth }
public enum GameViewCubemapSample
{
    High = 2048,
    Medium = 1024,
    Low = 512,
    Minimum = 256
}

public class GameViewEncoder : MonoBehaviour
{
    public GameViewCaptureMode CaptureMode = GameViewCaptureMode.RenderCam;
    private GameViewCaptureMode _CaptureMode = GameViewCaptureMode.RenderCam;
    public GameViewResize Resize = GameViewResize.Quarter;

    public Camera MainCam;
    public Camera RenderCam;

    public Vector2 Resolution = new Vector2(512, 512);
    private Vector2 renderResolution = new Vector2(512, 512);
    public bool MatchScreenAspect = true;

    public bool FastMode = false;
    public bool AsyncMode = false;

    public bool GZipMode = false;
    public bool PanoramaMode = false;

    [Range(10, 100)]
    public int Quality = 40;
    public FMChromaSubsamplingOption ChromaSubsampling = FMChromaSubsamplingOption.Subsampling420;

    [Range(0f, 60f)]
    public float StreamFPS = 20f;
    private float interval = 0.05f;

    public bool ignoreSimilarTexture = true;
    private int lastRawDataByte = 0;
    [Tooltip("Compare previous image data size(byte)")]
    public int similarByteSizeThreshold = 8;

    private bool NeedUpdateTexture = false;
    private bool EncodingTexture = false;

    //experimental feature: check if your GPU supports AsyncReadback
    private bool supportsAsyncGPUReadback = false;
    public bool EnableAsyncGPUReadback = true;
    public bool SupportsAsyncGPUReadback { get { return supportsAsyncGPUReadback; } }

    private int streamWidth;
    private int streamHeight;

    public Texture2D CapturedTexture;
    public Texture GetStreamTexture
    {
        get
        {
            if (supportsAsyncGPUReadback && EnableAsyncGPUReadback) return rt;
            return CapturedTexture;
        }
    }
    private RenderTextureDescriptor sourceDescriptor;
    private RenderTexture rt;
    private RenderTexture rt_cube;
    private RenderTexture rt_reserved;
    private bool reservedExistingRenderTexture = false;

    [HideInInspector] public Material MatPano; //has to be public, otherwise the shader will be  missing
    [HideInInspector] public Material MatFMDesktop;

    [HideInInspector] public Material MatColorAdjustment;
    [Range(0, 2)]
    public int ColorReductionLevel = 0;
    private float brightness { get { return 1f / Mathf.Pow(2, ColorReductionLevel); } }

    //for URP only
    [HideInInspector] public Material mat_source;

    public GameViewCubemapSample CubemapResolution = GameViewCubemapSample.Medium;

    private Texture2D Screenshot;
    private ColorSpace ColorSpace;

#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX)
    private Texture2D DesktopTexture;
#endif
    public float FMDesktopRotation = 0f;

    public Vector2 FMDesktopResolution = Vector2.zero;
    public bool FMDesktopFlipX = false;
    public bool FMDesktopFlipY = false;
    [Range(0.00001f, 2f)]
    public float FMDesktopRangeX = 1f;
    [Range(0.00001f, 2f)]
    public float FMDesktopRangeY = 1f;
    [Range(-0.5f, 0.5f)]
    public float FMDesktopOffsetX = 0f;
    [Range(-0.5f, 0.5f)]
    public float FMDesktopOffsetY = 0f;
    [Range(0, 8)]
    public int FMDesktopMonitorID = 0;
    public int FMDesktopMonitorCount = 0;

    public bool FMDesktopCorrectRotation = true;

    public UnityEventByteArray OnDataByteReadyEvent = new UnityEventByteArray();

    //[Header("Pair Encoder & Decoder")]
    public int label = 1001;
    private int dataID = 0;
    private int maxID = 1024;
    private int chunkSize = 8096; //32768
    private float next = 0f;
    private bool stop = false;
    private byte[] dataByte;

    public int dataLength;

    void CaptureModeUpdate()
    {
#if !UNITY_EDITOR_WIN && !UNITY_STANDALONE_WIN
        if (CaptureMode == GameViewCaptureMode.Desktop) CaptureMode = GameViewCaptureMode.FullScreen;
#endif
        if (_CaptureMode != CaptureMode)
        {
            _CaptureMode = CaptureMode;
            if (rt != null) Destroy(rt);
            if (CapturedTexture != null) Destroy(CapturedTexture);
        }
    }

    //init when added component, or reset component
    void Reset()
    {
        MatPano = new Material(Shader.Find("Hidden/FMCubemapToEquirect"));
        MatFMDesktop = new Material(Shader.Find("Hidden/FMDesktopMask"));
        MatColorAdjustment = new Material(Shader.Find("Hidden/FMETPColorAdjustment"));

        //for URP only
        mat_source = new Material(Shader.Find("Unlit/Texture"));
    }

    private void Start()
    {
        Application.runInBackground = true;
        ColorSpace = QualitySettings.activeColorSpace;

#if UNITY_2018_2_OR_NEWER
        try { supportsAsyncGPUReadback = SystemInfo.supportsAsyncGPUReadback; }
        catch { supportsAsyncGPUReadback = false; }
#else
        supportsAsyncGPUReadback = false;
#endif

        sourceDescriptor = (UnityEngine.XR.XRSettings.enabled) ? UnityEngine.XR.XRSettings.eyeTextureDesc : new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.ARGB32);
        sourceDescriptor.depthBufferBits = 16;

#if WINDOWS_UWP
        if (supportsAsyncGPUReadback && EnableAsyncGPUReadback && FastMode) sourceDescriptor.colorFormat = RenderTextureFormat.ARGB32;
#endif

        if (RenderCam != null)
        {
            if (RenderCam.targetTexture != null)
            {
                rt_reserved = RenderCam.targetTexture;
                reservedExistingRenderTexture = true;
            }
        }

        CaptureModeUpdate();
        StartCoroutine(SenderCOR());
    }

    private void Update()
    {
        Resolution.x = Mathf.RoundToInt(Resolution.x);
        Resolution.y = Mathf.RoundToInt(Resolution.y);
        if (Resolution.x <= 1) Resolution.x = 1;
        if (Resolution.y <= 1) Resolution.y = 1;
        renderResolution = Resolution;

        CaptureModeUpdate();

        switch (_CaptureMode)
        {
            case GameViewCaptureMode.MainCam:
                if (MainCam == null) MainCam = this.GetComponent<Camera>();
                renderResolution = new Vector2(Screen.width, Screen.height) / Mathf.Pow(2, (int)Resize);
                if (sourceDescriptor.vrUsage == VRTextureUsage.TwoEyes) renderResolution.x /= 2f;
                break;
            case GameViewCaptureMode.RenderCam:
                if (MatchScreenAspect)
                {
                    if (Screen.width > Screen.height) renderResolution.y = renderResolution.x / (float)(Screen.width) * (float)(Screen.height);
                    if (Screen.width < Screen.height) renderResolution.x = renderResolution.y / (float)(Screen.height) * (float)(Screen.width);
                }
                break;
            case GameViewCaptureMode.FullScreen:
                renderResolution = new Vector2(Screen.width, Screen.height) / Mathf.Pow(2, (int)Resize);
                break;
            case GameViewCaptureMode.Desktop:
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX)
                if (DesktopTexture != null)
                {
                    if (MatchScreenAspect)
                    {
                        if (FMDesktopRangeX == 0) FMDesktopRangeX = 0.00001f;
                        if (FMDesktopRangeY == 0) FMDesktopRangeY = 0.00001f;

                        float TargetWidth = DesktopTexture.width;
                        float TargetHeight = DesktopTexture.height;
                        if (FMDesktopCorrectRotation)
                        {
                            if (FMDesktopRotation == 90f || FMDesktopRotation == 270f)
                            {
                                TargetWidth = DesktopTexture.height;
                                TargetHeight = DesktopTexture.width;
                            }
                        }
                        
                        float TargetRatio = (TargetWidth * FMDesktopRangeX) / (TargetHeight * FMDesktopRangeY);
                        float RenderRatio = renderResolution.x / renderResolution.y;
                        if(TargetRatio > RenderRatio) renderResolution.y = renderResolution.x / TargetRatio;
                        if(TargetRatio < RenderRatio) renderResolution.x = renderResolution.y * TargetRatio;
                    }
                }
#endif
                break;
        }

        if (_CaptureMode != GameViewCaptureMode.RenderCam)
        {
            if (RenderCam != null)
            {
                if (RenderCam.targetTexture != null) RenderCam.targetTexture = null;
            }
        }
    }

    void CheckResolution()
    {
        if (renderResolution.x <= 1) renderResolution.x = 1;
        if (renderResolution.y <= 1) renderResolution.y = 1;

        bool IsLinear = (ColorSpace == ColorSpace.Linear) && (CaptureMode == GameViewCaptureMode.FullScreen);

        sourceDescriptor.width = Mathf.RoundToInt(renderResolution.x);
        sourceDescriptor.height = Mathf.RoundToInt(renderResolution.y);
        sourceDescriptor.sRGB = !IsLinear;

        if (PanoramaMode && CaptureMode == GameViewCaptureMode.RenderCam)
        {
            if (rt_cube == null)
            {
                rt_cube = new RenderTexture((int)CubemapResolution, (int)CubemapResolution, 0, RenderTextureFormat.ARGB32, IsLinear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
                //rt_cube.Create();
            }
            else
            {
                if (rt_cube.width != (int)CubemapResolution || rt_cube.height != (int)CubemapResolution || rt_cube.sRGB != IsLinear)
                {
                    if (MainCam != null) { if (MainCam.targetTexture == rt_cube) MainCam.targetTexture = null; }
                    if (RenderCam != null) { if (RenderCam.targetTexture == rt_cube) RenderCam.targetTexture = null; }
                    Destroy(rt_cube);
                    rt_cube = new RenderTexture((int)CubemapResolution, (int)CubemapResolution, 0, RenderTextureFormat.ARGB32, IsLinear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
                    //rt_cube.Create();
                }
            }

            rt_cube.antiAliasing = 1;
            rt_cube.filterMode = FilterMode.Bilinear;
            rt_cube.anisoLevel = 0;
            rt_cube.dimension = TextureDimension.Cube;
            rt_cube.autoGenerateMips = false;
        }


        if (rt == null)
        {
            //may have unsupport graphic format bug on Unity2019/2018, fallback to not using descriptor
            try { rt = new RenderTexture(sourceDescriptor); }
            catch
            {
                DestroyImmediate(rt);
                rt = new RenderTexture(sourceDescriptor.width, sourceDescriptor.height, 16, RenderTextureFormat.ARGB32);
            }
            rt.Create();
        }
        else
        {
            if (rt.width != sourceDescriptor.width || rt.height != sourceDescriptor.height || rt.sRGB != IsLinear)
            {
                if (MainCam != null) { if (MainCam.targetTexture == rt) MainCam.targetTexture = null; }
                if (RenderCam != null) { if (RenderCam.targetTexture == rt) RenderCam.targetTexture = null; }
                DestroyImmediate(rt);
                //may have unsupport graphic format bug on Unity2019/2018, fallback to not using descriptor
                try { rt = new RenderTexture(sourceDescriptor); }
                catch
                {
                    DestroyImmediate(rt);
                    rt = new RenderTexture(sourceDescriptor.width, sourceDescriptor.height, 16, RenderTextureFormat.ARGB32);
                }
                rt.Create();
            }
        }

        if (CapturedTexture == null) { CapturedTexture = new Texture2D(sourceDescriptor.width, sourceDescriptor.height, TextureFormat.RGB24, false, IsLinear); }
        else
        {
            if (CapturedTexture.width != sourceDescriptor.width || CapturedTexture.height != sourceDescriptor.height)
            {
                DestroyImmediate(CapturedTexture);
                CapturedTexture = new Texture2D(sourceDescriptor.width, sourceDescriptor.height, TextureFormat.RGB24, false, IsLinear);
            }
        }
    }

    void ProcessCapturedTexture()
    {
        streamWidth = rt.width;
        streamHeight = rt.height;

        if (!FastMode) EnableAsyncGPUReadback = false;
        if (supportsAsyncGPUReadback && EnableAsyncGPUReadback) { StartCoroutine(ProcessCapturedTextureGPUReadbackCOR()); }
        else { StartCoroutine(ProcessCapturedTextureCOR()); }
    }

    IEnumerator ProcessCapturedTextureCOR()
    {
        //render texture to texture2d
        RenderTexture.active = rt;
        CapturedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        CapturedTexture.Apply();
        RenderTexture.active = null;

        //encode to byte for streaming
        StartCoroutine(EncodeBytes());
        yield break;
    }


    IEnumerator ProcessCapturedTextureGPUReadbackCOR()
    {
#if UNITY_2018_2_OR_NEWER
        if (rt != null)
        {
            AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(rt, 0, TextureFormat.RGB24);
            while (!request.done) yield return null;
            if (!request.hasError) { StartCoroutine(EncodeBytes(request.GetData<byte>().ToArray())); }
            else { EncodingTexture = false; }
        }
        else { EncodingTexture = false; }
#endif
    }

#if FMETP_URP
    private RenderTexture rt_source;

    IEnumerator DelayAddRenderPipelineListenersCOR(float delaySeconds = 0f)
    {
        yield return new WaitForSeconds(delaySeconds);
        AddRenderPipelineListeners();
    }

    private void AddRenderPipelineListeners()
    {
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
        RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
    }

    private void RemoveRenderPipelineListeners()
    {
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
        RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_beginCameraRendering;
    }

    private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        //OnPreRender();
        if (_CaptureMode != GameViewCaptureMode.MainCam) return;
        if (rt_source == null)
        {
            try { rt_source = new RenderTexture(sourceDescriptor); }
            catch
            {
                DestroyImmediate(rt_source);
                rt_source = new RenderTexture(sourceDescriptor.width, sourceDescriptor.height, 16, RenderTextureFormat.ARGB32);
            }
            rt_source.Create();
        }
        else
        {
            if (rt_source.width != Screen.width || rt_source.height != Screen.height)
            {
                if (MainCam != null) { if (MainCam.targetTexture == rt_source) MainCam.targetTexture = null; }
                DestroyImmediate(rt_source);
                sourceDescriptor.width = Screen.width;
                sourceDescriptor.height = Screen.height;
                renderResolution = new Vector2(Screen.width, Screen.height)/ Mathf.Pow(2, (int)Resize);

                try { rt_source = new RenderTexture(sourceDescriptor); }
                catch
                {
                    DestroyImmediate(rt_source);
                    rt_source = new RenderTexture(sourceDescriptor.width, sourceDescriptor.height, 16, RenderTextureFormat.ARGB32);
                }
                rt_source.Create();
            }
        }

        MainCam.targetTexture = rt_source;
    }

    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        //OnPostRender();
        if (_CaptureMode != GameViewCaptureMode.MainCam) return;
        MainCam.targetTexture = null;
        OnRenderImageURP();
    }

    private void OnRenderImageURP()
    {
        //Graphics.Blit(rt_source, null as RenderTexture);
        if(NeedUpdateTexture && !EncodingTexture)
        {
            NeedUpdateTexture = false;
            CheckResolution();

            if (ColorReductionLevel > 0)
            {
                MatColorAdjustment.SetFloat("_Brightness", brightness);
                Graphics.Blit(rt_source, rt, MatColorAdjustment);
            }
            else { Graphics.Blit(rt_source, rt); }

            //RenderTexture to Texture2D
            ProcessCapturedTexture();
        }

        Graphics.Blit(rt_source, null as RenderTexture, mat_source);
    }
#else
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_CaptureMode == GameViewCaptureMode.MainCam)
        {
            if (NeedUpdateTexture && !EncodingTexture)
            {
                NeedUpdateTexture = false;
                CheckResolution();

                if (ColorReductionLevel > 0)
                {
                    MatColorAdjustment.SetFloat("_Brightness", brightness);
                    Graphics.Blit(source, rt, MatColorAdjustment);
                }
                else { Graphics.Blit(source, rt); }

                //RenderTexture to Texture2D
                ProcessCapturedTexture();
            }
        }

        Graphics.Blit(source, destination);
    }
#endif

    IEnumerator RenderTextureRefresh()
    {
        if (NeedUpdateTexture && !EncodingTexture)
        {
            NeedUpdateTexture = false;
            EncodingTexture = true;

            yield return new WaitForEndOfFrame();
            CheckResolution();

            if (_CaptureMode == GameViewCaptureMode.RenderCam)
            {
                if (RenderCam != null)
                {
                    if (PanoramaMode)
                    {
                        RenderCam.targetTexture = rt_cube;
                        RenderCam.RenderToCubemap(rt_cube);

                        Shader.SetGlobalFloat("FORWARD", RenderCam.transform.eulerAngles.y * 0.01745f);
                        MatPano.SetFloat("_Brightness", brightness);
                        Graphics.Blit(rt_cube, rt, MatPano);
                    }
                    else
                    {
                        if(reservedExistingRenderTexture)
                        {
                            RenderCam.targetTexture = rt_reserved;

                            //apply color adjustment for bandwidth
                            if (ColorReductionLevel > 0)
                            {
                                MatColorAdjustment.SetFloat("_Brightness", brightness);
                                Graphics.Blit(rt_reserved, rt, MatColorAdjustment);
                            }
                            else { Graphics.Blit(rt_reserved, rt); }
                        }
                        else
                        {
                            RenderCam.targetTexture = rt;
                            RenderCam.Render();
                            RenderCam.targetTexture = null;

                            //apply color adjustment for bandwidth
                            if (ColorReductionLevel > 0)
                            {
                                MatColorAdjustment.SetFloat("_Brightness", brightness);
                                Graphics.Blit(rt, rt, MatColorAdjustment);
                            }
                        }
                    }

                    // RenderTexture to Texture2D
                    ProcessCapturedTexture();
                }
                else { EncodingTexture = false; }
            }

            if (_CaptureMode == GameViewCaptureMode.FullScreen)
            {
                if (Resize == GameViewResize.Full)
                {
                    // cleanup
                    if (CapturedTexture != null) Destroy(CapturedTexture);
                    CapturedTexture = ScreenCapture.CaptureScreenshotAsTexture();
                    if (ColorReductionLevel > 0)
                    {
                        MatColorAdjustment.SetFloat("_Brightness", brightness);
                        Graphics.Blit(CapturedTexture, rt, MatColorAdjustment);

                        // RenderTexture to Texture2D
                        ProcessCapturedTexture();
                    }
                    else { StartCoroutine(EncodeBytes()); }
                }
                else
                {
                    // cleanup
                    if (Screenshot != null) Destroy(Screenshot);
                    Screenshot = ScreenCapture.CaptureScreenshotAsTexture();

                    if (ColorReductionLevel > 0)
                    {
                        MatColorAdjustment.SetFloat("_Brightness", brightness);
                        Graphics.Blit(Screenshot, rt, MatColorAdjustment);
                    }
                    else { Graphics.Blit(Screenshot, rt); }

                    // RenderTexture to Texture2D
                    ProcessCapturedTexture();
                }
            }

            if (_CaptureMode == GameViewCaptureMode.Desktop)
            {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX)
                FMDesktopMonitorCount = FMDesktop.Manager.monitorCount;
                if (FMDesktopMonitorID >= (FMDesktopMonitorCount - 1)) FMDesktopMonitorID = FMDesktopMonitorCount - 1;
                if (FMDesktopMonitorID < 0) FMDesktopMonitorID = 0;

                if (FMDesktop.Manager.GetMonitor(FMDesktopMonitorID) != null)
                {
                    if (MatFMDesktop == null) MatFMDesktop = new Material(Shader.Find("Hidden/FMDesktopMask"));
                    
                    //Correct the rotation of screen
                    if (FMDesktopCorrectRotation)
                    {
                        switch (FMDesktop.Manager.GetMonitor(FMDesktopMonitorID).rotation)
                        {
                            case FMDesktop.MonitorRotation.Identity:
                                FMDesktopRotation = 0f;
                                break;
                            case FMDesktop.MonitorRotation.Rotate90:
                                FMDesktopRotation = 90f;
                                break;
                            case FMDesktop.MonitorRotation.Rotate180:
                                FMDesktopRotation = 180f;
                                break;
                            case FMDesktop.MonitorRotation.Rotate270:
                                FMDesktopRotation = 270f;
                                break;
                            case FMDesktop.MonitorRotation.Unspecified:
                                FMDesktopRotation = 0f;
                                break;
                        }
                        MatFMDesktop.SetFloat("_RotationAngle", FMDesktopRotation);
                    }
                    else { MatFMDesktop.SetFloat("_RotationAngle", 0f); }
                    
                    FMDesktop.Manager.GetMonitor(FMDesktopMonitorID).shouldBeUpdated = true;
                    DesktopTexture = FMDesktop.Manager.GetMonitor(FMDesktopMonitorID).texture;

                    MatFMDesktop.SetFloat("_FlipX", FMDesktopFlipX ? 0f : 1f);
                    MatFMDesktop.SetFloat("_FlipY", FMDesktopFlipY ? 0f : 1f);

                    MatFMDesktop.SetFloat("_RangeX", FMDesktopRangeX);
                    MatFMDesktop.SetFloat("_RangeY", FMDesktopRangeY);

                    MatFMDesktop.SetFloat("_OffsetX", FMDesktopOffsetX);
                    MatFMDesktop.SetFloat("_OffsetY", FMDesktopOffsetY);

                    MatFMDesktop.SetFloat("_Brightness", brightness);
                    Graphics.Blit(DesktopTexture, rt, MatFMDesktop);

                    //RenderTexture to Texture2D
                    ProcessCapturedTexture();
                }
                else { EncodingTexture = false; }
#else
                EncodingTexture = false;
#endif
            }
        }
    }

    public void Action_UpdateTexture() { RequestTextureUpdate(); }

    void RequestTextureUpdate()
    {
        if (EncodingTexture) return;
        NeedUpdateTexture = true;
        if (_CaptureMode != GameViewCaptureMode.MainCam) StartCoroutine(RenderTextureRefresh());
    }

    IEnumerator SenderCOR()
    {
        while (!stop)
        {
            if (Time.realtimeSinceStartup > next)
            {
                if (StreamFPS > 0)
                {
                    interval = 1f / StreamFPS;
                    next = Time.realtimeSinceStartup + interval;

                    RequestTextureUpdate();
                }
            }
            yield return null;
        }
    }

    IEnumerator EncodeBytes(byte[] RawTextureData = null)
    {
        if (CapturedTexture != null || RawTextureData != null)
        {
            //==================getting byte data==================
#if UNITY_IOS && !UNITY_EDITOR
            FastMode = true;
#endif

#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_WIN || UNITY_IOS || UNITY_ANDROID || WINDOWS_UWP || UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            if (FastMode)
            {
                //try AsyncMode, on supported platform
                if (AsyncMode && Loom.numThreads < Loom.maxThreads)
                {
                    //has spare thread
                    bool AsyncEncoding = true;
                    if (RawTextureData == null)
                    {
                        RawTextureData = CapturedTexture.GetRawTextureData();
                        streamWidth = CapturedTexture.width;
                        streamHeight = CapturedTexture.height;
                    }

                    Loom.RunAsync(() =>
                    {
                        dataByte = RawTextureData.FMRawTextureDataToJPG(streamWidth, streamHeight, Quality, ChromaSubsampling);
                        AsyncEncoding = false;
                    });
                    while (AsyncEncoding) yield return null;
                }
                else
                {
                    //need yield return, in order to fix random error "coroutine->IsInList()"
                    yield return dataByte = RawTextureData == null ? CapturedTexture.FMEncodeToJPG(Quality, ChromaSubsampling) : RawTextureData.FMRawTextureDataToJPG(streamWidth, streamHeight, Quality, ChromaSubsampling);
                }
            }
            else { dataByte = RawTextureData == null ? CapturedTexture.EncodeToJPG(Quality) : RawTextureData.FMRawTextureDataToJPG(streamWidth, streamHeight, Quality, ChromaSubsampling); }
#else
            dataByte = RawTextureData == null ? CapturedTexture.EncodeToJPG(Quality) : RawTextureData.FMRawTextureDataToJPG(streamWidth, streamHeight, Quality, ChromaSubsampling);
#endif

            if (ignoreSimilarTexture)
            {
                float diff = Mathf.Abs(lastRawDataByte - dataByte.Length);
                if (diff < similarByteSizeThreshold)
                {
                    EncodingTexture = false;
                    yield break;
                }
            }
            lastRawDataByte = dataByte.Length;

            if (GZipMode) dataByte = dataByte.FMZipBytes();

            dataLength = dataByte.Length;
            //==================getting byte data==================
            int _length = dataByte.Length;
            int _offset = 0;

            byte[] _meta_label = BitConverter.GetBytes(label);
            byte[] _meta_id = BitConverter.GetBytes(dataID);
            byte[] _meta_length = BitConverter.GetBytes(_length);

            int chunks = Mathf.RoundToInt(dataByte.Length / chunkSize);
            for (int i = 0; i <= chunks; i++)
            {
                int SendByteLength = (i == chunks) ? (_length % chunkSize + 18) : (chunkSize + 18);
                byte[] _meta_offset = BitConverter.GetBytes(_offset);
                byte[] SendByte = new byte[SendByteLength];

                Buffer.BlockCopy(_meta_label, 0, SendByte, 0, 4);
                Buffer.BlockCopy(_meta_id, 0, SendByte, 4, 4);
                Buffer.BlockCopy(_meta_length, 0, SendByte, 8, 4);

                Buffer.BlockCopy(_meta_offset, 0, SendByte, 12, 4);
                SendByte[16] = (byte)(GZipMode ? 1 : 0);
                SendByte[17] = (byte)ColorReductionLevel;

                Buffer.BlockCopy(dataByte, _offset, SendByte, 18, SendByte.Length - 18);
                OnDataByteReadyEvent.Invoke(SendByte);
                _offset += chunkSize;
            }

            dataID++;
            if (dataID > maxID) dataID = 0;
        }

        EncodingTexture = false;
        yield break;
    }

    void OnEnable() { StartAll(); }
    void OnDisable() { StopAll(); }
    void OnApplicationQuit() { StopAll(); }
    void OnDestroy() { StopAll(); }

    void StopAll()
    {
        stop = true;
        StopAllCoroutines();

        lastRawDataByte = 0;

#if FMETP_URP
        RemoveRenderPipelineListeners();
#endif
    }

    void StartAll()
    {
#if FMETP_URP
        StartCoroutine(DelayAddRenderPipelineListenersCOR(2f));
#endif
        if (Time.realtimeSinceStartup < 3f) return;
        stop = false;
        StartCoroutine(SenderCOR());

        NeedUpdateTexture = false;
        EncodingTexture = false;
    }
}