using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//[RequireComponent(typeof(AudioListener))]
public class AudioEncoder : MonoBehaviour
{
    //----------------------------------------------
    AudioListener[] AudioListenerObject;
    private Queue<byte> AudioBytes = new Queue<byte>();
    private Queue<float> AudioBuffer = new Queue<float>();

    //[Header("[Capture In-Game Sound]")]
    public bool StreamGameSound = true;
    public int OutputSampleRate = 48000;
    public int OutputChannels = 2;
    public bool ForceMono = true;

    private object _asyncLockAudio = new object();
    private object _asyncLockFilter = new object();
    //----------------------------------------------

    [Range(1f, 60f)]
    public float StreamFPS = 20f;
    private float interval = 0.05f;

    public bool GZipMode = false;

    public UnityEventByteArray OnDataByteReadyEvent = new UnityEventByteArray();

    //[Header("Pair Encoder & Decoder")]
    public int label = 2001;
    private int dataID = 0;
    private int maxID = 1024;
    private int chunkSize = 8096; //32768;
    private float next = 0f;
    private bool stop = false;

    public int dataLength;

    // Use this for initialization
    void Start ()
    {
        Application.runInBackground = true;

        if (GetComponent<AudioListener>() == null) this.gameObject.AddComponent<AudioListener>();
        OutputSampleRate = AudioSettings.GetConfiguration().sampleRate;
        AudioListenerObject = FindObjectsOfType<AudioListener>();
        for (int i = 0; i < AudioListenerObject.Length; i++)
        {
            if (AudioListenerObject[i] != null) AudioListenerObject[i].enabled = (AudioListenerObject[i].gameObject == this.gameObject);
        }
        StartCoroutine(SenderCOR());
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (StreamGameSound)
        {
            OutputChannels = channels;

            int step = ForceMono ? channels : 1;
            int i = 0;
            lock (_asyncLockFilter)
            {
                do { AudioBuffer.Enqueue(data[i]); i+= step; } while (i < data.Length);
            }
        }
    }

    IEnumerator SenderCOR()
    {
        while (!stop)
        {
            if (Time.realtimeSinceStartup > next)
            {
                interval = 1f / StreamFPS;
                next = Time.realtimeSinceStartup + interval;
                EncodeBytes();
            }
            yield return null;
        }
    }

    Int16 FloatToInt16(float inputFloat)
    {
        inputFloat *= 32767;
        if (inputFloat < -32768) inputFloat = -32768;
        if (inputFloat > 32767) inputFloat = 32767;
        return Convert.ToInt16(inputFloat);
    }

    void EncodeBytes()
    {
        if (AudioBuffer.Count > 0)
        {
            Queue<float> _DataQueue = new Queue<float>();
            lock (_asyncLockFilter)
            {
                do { AudioBuffer.Dequeue(); } while (AudioBuffer.Count > 5120);
                do { _DataQueue.Enqueue(AudioBuffer.Dequeue()); } while (AudioBuffer.Count > 0);
            }

            lock (_asyncLockAudio)
            {
                do
                {
                    byte[] byteData = BitConverter.GetBytes(FloatToInt16(_DataQueue.Dequeue()));
                    foreach (byte _byte in byteData) AudioBytes.Enqueue(_byte);
                }
                while (_DataQueue.Count > 0);
            }
        }

        //==================getting byte data==================
        byte[] dataByte;

        byte[] _samplerateByte = BitConverter.GetBytes(OutputSampleRate);
        byte[] _channelsByte = BitConverter.GetBytes(ForceMono ? 1 : OutputChannels);
        lock (_asyncLockAudio)
        {
            dataByte = new byte[AudioBytes.Count + _samplerateByte.Length + _channelsByte.Length];

            Buffer.BlockCopy(_samplerateByte, 0, dataByte, 0, _samplerateByte.Length);
            Buffer.BlockCopy(_channelsByte, 0, dataByte, 4, _channelsByte.Length);
            Buffer.BlockCopy(AudioBytes.ToArray(), 0, dataByte, 8, AudioBytes.Count);
            AudioBytes.Clear();
        }

        if (GZipMode) dataByte = dataByte.FMZipBytes();
        //==================getting byte data==================

        dataLength = dataByte.Length;
        int _length = dataByte.Length;
        int _offset = 0;

        byte[] _meta_label = BitConverter.GetBytes(label);
        byte[] _meta_id = BitConverter.GetBytes(dataID);
        byte[] _meta_length = BitConverter.GetBytes(_length);
        
        int chunks = Mathf.FloorToInt(dataByte.Length / chunkSize);
        for (int i = 0; i <= chunks; i++)
        {
            byte[] _meta_offset = BitConverter.GetBytes(_offset);
            int SendByteLength = (i == chunks) ? (_length % chunkSize + 18) : (chunkSize + 18);
            byte[] SendByte = new byte[SendByteLength];

            Buffer.BlockCopy(_meta_label, 0, SendByte, 0, 4);
            Buffer.BlockCopy(_meta_id, 0, SendByte, 4, 4);
            Buffer.BlockCopy(_meta_length, 0, SendByte, 8, 4);
            Buffer.BlockCopy(_meta_offset, 0, SendByte, 12, 4);
            SendByte[16] = (byte)(GZipMode ? 1 : 0);
            SendByte[17] = (byte)0;//not used, but just keep one empty byte for standard

            Buffer.BlockCopy(dataByte, _offset, SendByte, 18, SendByte.Length - 18);
            OnDataByteReadyEvent.Invoke(SendByte);
            _offset += chunkSize;
        }

        dataID++;
        if (dataID > maxID) dataID = 0;
    }

    private void OnEnable()
    {
        if (Time.realtimeSinceStartup <= 3f) return;
        if (stop)
        {
            stop = false;
            StartCoroutine(SenderCOR());
        }

        if(AudioListenerObject != null)
        {
            for (int i = 0; i < AudioListenerObject.Length; i++)
            {
                if (AudioListenerObject[i] != null) AudioListenerObject[i].enabled = (AudioListenerObject[i].gameObject == this.gameObject);
            }
        }
    }
    private void OnDisable()
    {
        stop = true;
        StopCoroutine(SenderCOR());

        //reset listener
        if (AudioListenerObject != null)
        {
            for (int i = 0; i < AudioListenerObject.Length; i++)
            {
                if (AudioListenerObject[i] != null) AudioListenerObject[i].enabled = (AudioListenerObject[i].gameObject != this.gameObject);
            }
        }
    }
}
