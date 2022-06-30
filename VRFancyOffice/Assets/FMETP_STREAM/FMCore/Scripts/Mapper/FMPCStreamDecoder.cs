using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

using System;
public class FMPCStreamDecoder : MonoBehaviour
{
    public int PCWidth = 0;
    public int PCHeight = 0;
    public int PCCount = 0;

    public Color MainColor = Color.white;
    [Range(0.000001f, 100f)]
    public float PointSize = 0.04f;
    public bool ApplyDistance = true;

    public Mesh PMesh;
    public Material MatFMPCStreamDecoder;

    //init when added component, or reset component
    void Reset() { MatFMPCStreamDecoder = new Material(Shader.Find("Hidden/FMPCStreamDecoder")); }

    public bool FastMode = false;
    public bool AsyncMode = false;

    public Texture2D ReceivedTexture;

    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;
        this.gameObject.AddComponent<MeshRenderer>().hideFlags = HideFlags.HideInInspector;
        this.gameObject.AddComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;
    }

    private void Update()
    {
        if (PCCount > 0)
        {
            MatFMPCStreamDecoder.color = MainColor;
            MatFMPCStreamDecoder.SetFloat("_PointSize", PointSize);
            MatFMPCStreamDecoder.SetFloat("_ApplyDistance", ApplyDistance ? 1f : 0f);
        }
    }

    private bool ReadyToGetFrame = true;

    //[Header("Pair Encoder & Decoder")]
    public int label = 4001;
    private int dataID = 0;
    //int maxID = 1024;
    private int dataLength = 0;
    private int receivedLength = 0;

    private byte[] dataByte;
    public bool GZipMode = false;

    public void Action_ProcessPointCloudData(byte[] _byteData)
    {
        if (!enabled) return;
        if (_byteData.Length <= 8) return;

        int _label = BitConverter.ToInt32(_byteData, 0);
        if (_label != label) return;
        int _dataID = BitConverter.ToInt32(_byteData, 4);

        if (_dataID != dataID) receivedLength = 0;
        dataID = _dataID;
        dataLength = BitConverter.ToInt32(_byteData, 8);
        int _offset = BitConverter.ToInt32(_byteData, 12);

        GZipMode = _byteData[16] == 1;

        if (receivedLength == 0) dataByte = new byte[dataLength];
        receivedLength += _byteData.Length - 17;
        Buffer.BlockCopy(_byteData, 17, dataByte, _offset, _byteData.Length - 17);

        if (ReadyToGetFrame)
        {
            if (receivedLength == dataLength)
            {
                StopAllCoroutines();
                if (this.isActiveAndEnabled) StartCoroutine(ProcessImageData(dataByte));
            }
        }
    }


    private float camNearClipPlane = 0f;
    private float camFarClipPlane = 0f;
    private float camFOV = 60f;
    private float camAspect = 1f;
    private float camOrthographicProjection = 0f;
    private float camOrthographicSize = 1f;

    IEnumerator ProcessImageData(byte[] _byteData)
    {
        ReadyToGetFrame = false;

        if (GZipMode)
        {
            try { _byteData = _byteData.FMUnzipBytes(); }
            catch (Exception e)
            {
                Debug.LogException(e);
                ReadyToGetFrame = true;
                yield break;
            }
        }

        //read camera meta data
        camNearClipPlane = BitConverter.ToSingle(_byteData, 0);
        camFarClipPlane = BitConverter.ToSingle(_byteData, 4);
        camFOV = BitConverter.ToSingle(_byteData, 8);
        camAspect = BitConverter.ToSingle(_byteData, 12);
        camOrthographicProjection = BitConverter.ToSingle(_byteData, 16);
        camOrthographicSize = BitConverter.ToSingle(_byteData, 20);

        byte[] _byteDataTmp = new byte[_byteData.Length - 24];
        Buffer.BlockCopy(_byteData, 24, _byteDataTmp, 0, _byteData.Length - 24);
        _byteData = _byteDataTmp;

        if (ReceivedTexture == null) ReceivedTexture = new Texture2D(0, 0, TextureFormat.RGB24, false);
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
                    _byteData.FMJPGToRawTextureData(ref RawTextureData, ref _width, ref _height, TextureFormat.RGB24);
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
                    ReceivedTexture.FMMatchResolution(ref ReceivedTexture, _width, _height);
                    ReceivedTexture.LoadRawTextureData(RawTextureData);
                    ReceivedTexture.Apply();
                }
                catch
                {
                    Destroy(ReceivedTexture);
                    ReadyToGetFrame = true;
                    yield break;
                }
            }
            else
            {
                //no spare thread, run in main thread
                try
                {
                    ReceivedTexture.FMLoadJPG(ref ReceivedTexture, _byteData);
                }
                catch
                {
                    Destroy(ReceivedTexture);
                    ReadyToGetFrame = true;
                    yield break;
                }
            }
        }
        else
        {
            if (ReceivedTexture == null) ReceivedTexture = new Texture2D(0, 0);
            ReceivedTexture.LoadImage(_byteData);
        }
#else
            if (ReceivedTexture == null) ReceivedTexture = new Texture2D(0, 0);
            ReceivedTexture.LoadImage(_byteData);
#endif
        if (ReceivedTexture.width <= 8)
        {
            //throw new Exception("texture is smaller than 8 x 8, wrong data");
            Debug.LogError("texture is smaller than 8 x 8, wrong data");
            ReadyToGetFrame = true;
            yield break;
        }

        ReceivedTexture.filterMode = FilterMode.Point;
        Action_ProcessImage(ReceivedTexture);

        ReadyToGetFrame = true;

        yield return null;
    }

    private void OnDisable() { StopAllCoroutines(); }

    public void Action_ProcessImage(Texture2D inputTexture)
    {
        if (inputTexture.width != PCWidth || inputTexture.height != PCHeight)
        {
            PCWidth = inputTexture.width / 2;
            PCHeight = inputTexture.height;
            PCCount = PCWidth * PCHeight;

            if (PMesh != null) DestroyImmediate(PMesh);
            PMesh = new Mesh();
            PMesh.name = "PMesh_" + PCWidth + "x" + PCHeight;

            PMesh.indexFormat = PCCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;

            Vector3[] vertices = new Vector3[PCCount];
            for(int j = 0; j< PCHeight; j++)
            {
                for(int i = 0; i<PCWidth; i++)
                {
                    int index = (j * PCWidth) + i;
                    vertices[index].x = ((float)i / (float)PCWidth);
                    vertices[index].y = ((float)j / (float)PCHeight);
                    vertices[index].z = 0f;
                }
            }

            PMesh.vertices = vertices;

            PMesh.SetIndices(Enumerable.Range(0, PMesh.vertices.Length).ToArray(), MeshTopology.Points, 0);
            PMesh.UploadMeshData(false);

            PMesh.bounds = new Bounds(Vector3.zero, new Vector3(2, 2, 2) * camFarClipPlane);
            GetComponent<MeshFilter>().sharedMesh = PMesh;
            GetComponent<MeshRenderer>().sharedMaterial = MatFMPCStreamDecoder;
        }

        if(MatFMPCStreamDecoder != null) MatFMPCStreamDecoder.mainTexture = inputTexture;

        MatFMPCStreamDecoder.SetFloat("_NearClipPlane", camNearClipPlane);
        MatFMPCStreamDecoder.SetFloat("_FarClipPlane", camFarClipPlane);

        MatFMPCStreamDecoder.SetFloat("_VerticalFOV", camFOV);
        MatFMPCStreamDecoder.SetFloat("_Aspect", camAspect);

        MatFMPCStreamDecoder.SetFloat("_OrthographicProjection", camOrthographicProjection);
        MatFMPCStreamDecoder.SetFloat("_OrthographicSize", camOrthographicSize);
    }
}
