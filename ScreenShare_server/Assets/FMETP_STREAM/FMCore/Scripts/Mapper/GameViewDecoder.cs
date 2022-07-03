using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;

public class GameViewDecoder : MonoBehaviour
{
    public bool FastMode = false;
    public bool AsyncMode = false;

    [Range(0f, 10f)]
    public float DecoderDelay = 0f;
    private float DecoderDelay_old = 0f;

    public Texture ReceivedTexture { get { return (ColorReductionLevel > 0 ? (Texture)ReceivedRenderTexture : (Texture)ReceivedTexture2D); } }
    public Texture2D ReceivedTexture2D;
    public RenderTexture ReceivedRenderTexture;
    public int ColorReductionLevel = 0;

    public GameObject TestQuad;
    public RawImage TestImg;

    public UnityEventTexture OnReceivedTexture;

    [Tooltip("Mono return texture format R8, otherwise it's RGB24 by default")]
    public bool Mono = false;
    public FilterMode DecodedFilterMode = FilterMode.Bilinear;
    public TextureWrapMode DecodedWrapMode = TextureWrapMode.Clamp;

    [HideInInspector] public Material MatColorAdjustment;
    void Reset() { MatColorAdjustment = new Material(Shader.Find("Hidden/FMETPColorAdjustment")); }

    // Use this for initialization
    void Start()
    {
        if(MatColorAdjustment) MatColorAdjustment = new Material(Shader.Find("Hidden/FMETPColorAdjustment"));
        Application.runInBackground = true;
    }

    private bool ReadyToGetFrame = true;

    //[Header("Pair Encoder & Decoder")]
    public int label = 1001;
    private int dataID = 0;
    //int maxID = 1024;
    private int dataLength = 0;
    private int receivedLength = 0;

    private byte[] dataByte;
    public bool GZipMode = false;

    public void Action_ProcessImageData(byte[] _byteData)
    {
        if (!enabled) return;
        if (_byteData.Length <= 18) return;

        int _label = BitConverter.ToInt32(_byteData, 0);
        if (_label != label) return;
        int _dataID = BitConverter.ToInt32(_byteData, 4);

        if (_dataID != dataID) receivedLength = 0;
        dataID = _dataID;
        dataLength = BitConverter.ToInt32(_byteData, 8);
        int _offset = BitConverter.ToInt32(_byteData, 12);

        GZipMode = _byteData[16] == 1;
        ColorReductionLevel = (int)_byteData[17];

        if (receivedLength == 0) dataByte = new byte[dataLength];
        receivedLength += _byteData.Length - 18;
        Buffer.BlockCopy(_byteData, 18, dataByte, _offset, _byteData.Length - 18);

        if (ReadyToGetFrame)
        {
            if (receivedLength == dataLength)
            {
                if (DecoderDelay_old != DecoderDelay)
                {
                    StopAllCoroutines();
                    DecoderDelay_old = DecoderDelay;
                }

                if (this.isActiveAndEnabled) StartCoroutine(ProcessImageData(dataByte));
            }
        }
    }

    IEnumerator ProcessImageData(byte[] _byteData)
    {
        yield return new WaitForSeconds(DecoderDelay);
        ReadyToGetFrame = false;

        if (GZipMode)
        {
            try { _byteData = _byteData.FMUnzipBytes(); }
            catch(Exception e)
            {
                Debug.LogException(e);
                ReadyToGetFrame = true;
                yield break;
            }
        }

        //check is Mono
        if (ReceivedTexture2D != null)
        {
            if (ReceivedTexture2D.format != (Mono ? TextureFormat.R8 : TextureFormat.RGB24))
            {
                Destroy(ReceivedTexture2D);
                ReceivedTexture2D = null;
            }
        }
        if (ReceivedTexture2D == null) ReceivedTexture2D = new Texture2D(0, 0, Mono ? TextureFormat.R8 : TextureFormat.RGB24, false);
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
                bool AsyncDecoding = true;
                byte[] RawTextureData = new byte[1];
                int _width = 0;
                int _height = 0;

                Loom.RunAsync(() =>
                {
                    try { _byteData.FMJPGToRawTextureData(ref RawTextureData, ref _width, ref _height, Mono ? TextureFormat.R8 : TextureFormat.RGB24); }
                    catch { }
                    AsyncDecoding = false;

                });
                while (AsyncDecoding) yield return null;

                if (RawTextureData.Length <= 8)
                {
                    ReadyToGetFrame = true;
                    yield break;
                }

                try
                {
                    //check resolution
                    ReceivedTexture2D.FMMatchResolution(ref ReceivedTexture2D, _width, _height);
                    ReceivedTexture2D.LoadRawTextureData(RawTextureData);
                    ReceivedTexture2D.Apply();
                }
                catch
                {
                    Destroy(ReceivedTexture2D);
                    GC.Collect();

                    ReadyToGetFrame = true;
                    yield break;
                }
            }
            else
            {
                //no spare thread, run in main thread
                try { ReceivedTexture2D.FMLoadJPG(ref ReceivedTexture2D, _byteData); }
                catch
                {
                    Destroy(ReceivedTexture2D);
                    GC.Collect();

                    ReadyToGetFrame = true;
                    yield break;
                }
            }
        }
        else { ReceivedTexture2D.LoadImage(_byteData); }
#else
        ReceivedTexture2D.LoadImage(_byteData);
#endif
        if (ReceivedTexture2D.width <= 8)
        {
            //throw new Exception("texture is smaller than 8 x 8, wrong data");
            Debug.LogError("texture is smaller than 8 x 8, wrong data");
            ReadyToGetFrame = true;
            yield break;
        }

        if (ReceivedTexture2D.filterMode != DecodedFilterMode) ReceivedTexture2D.filterMode = DecodedFilterMode;
        if (ReceivedTexture2D.wrapMode != DecodedWrapMode) ReceivedTexture2D.wrapMode = DecodedWrapMode;

        if(ColorReductionLevel > 0)
        {
            //check is Mono
            if (ReceivedRenderTexture != null)
            {
                if (ReceivedRenderTexture.format != (Mono ? RenderTextureFormat.R8 : RenderTextureFormat.ARGB32))
                {
                    Destroy(ReceivedRenderTexture);
                    ReceivedRenderTexture = null;
                }
            }
            if (ReceivedRenderTexture == null) ReceivedRenderTexture = new RenderTexture(ReceivedTexture2D.width, ReceivedTexture2D.height, 0, Mono ? RenderTextureFormat.R8 : RenderTextureFormat.ARGB32);
            if (ReceivedRenderTexture.filterMode != DecodedFilterMode) ReceivedRenderTexture.filterMode = DecodedFilterMode;
            if (ReceivedRenderTexture.wrapMode != DecodedWrapMode) ReceivedRenderTexture.wrapMode = DecodedWrapMode;

            float brightness = Mathf.Pow(2, ColorReductionLevel);
            MatColorAdjustment.SetFloat("_Brightness", brightness);
            Graphics.Blit(ReceivedTexture2D, ReceivedRenderTexture, MatColorAdjustment);
        }


        if (TestQuad != null) TestQuad.GetComponent<Renderer>().material.mainTexture = ReceivedTexture;
        if (TestImg != null) TestImg.texture = ReceivedTexture;
        OnReceivedTexture.Invoke(ReceivedTexture);

        ReadyToGetFrame = true;
        yield return null;
    }

    private void OnDisable() { StopAllCoroutines(); }

    //Motion JPEG: frame buffer
    private byte[] frameBuffer = new byte[300000];
    private const byte picMarker = 0xFF;
    private const byte picStart = 0xD8;
    private const byte picEnd = 0xD9;

    private int frameIdx = 0;
    private bool inPicture = false;
    private byte previous = (byte)0;
    private byte current = (byte)0;

    private int idx = 0;
    private int streamLength = 0;

    public void Action_ProcessMJPEGData(byte[] _byteData) { parseStreamBuffer(_byteData); }
    void parseStreamBuffer(byte[] streamBuffer)
    {
        idx = 0;
        streamLength = streamBuffer.Length;

        while (idx < streamLength)
        {
            if (inPicture) { parsePicture(streamBuffer); }
            else { searchPicture(streamBuffer); }
        }
    }

    //look for a jpeg frame(begin with FF D8)
    void searchPicture(byte[] streamBuffer)
    {
        do
        {
            previous = current;
            current = streamBuffer[idx++];

            // JPEG picture start ?
            if (previous == picMarker && current == picStart)
            {
                frameIdx = 2;
                frameBuffer[0] = picMarker;
                frameBuffer[1] = picStart;
                inPicture = true;
                return;
            }
        } while (idx < streamLength);
    }


    //fill the frame buffer, until FFD9 is reach.
    void parsePicture(byte[] streamBuffer)
    {
        do
        {
            previous = current;
            current = streamBuffer[idx++];

            frameBuffer[frameIdx++] = current;

            // JPEG picture end ?
            if (previous == picMarker && current == picEnd)
            {
                // Using a memorystream thissway prevent arrays copy and allocations
                using (MemoryStream s = new MemoryStream(frameBuffer, 0, frameIdx))
                {
                    if (ReadyToGetFrame)
                    {
                        if (DecoderDelay_old != DecoderDelay)
                        {
                            StopAllCoroutines();
                            DecoderDelay_old = DecoderDelay;
                        }
                        StartCoroutine(ProcessImageData(s.ToArray()));
                    }
                }

                inPicture = false;
                return;
            }
        } while (idx < streamLength);
    }
}
