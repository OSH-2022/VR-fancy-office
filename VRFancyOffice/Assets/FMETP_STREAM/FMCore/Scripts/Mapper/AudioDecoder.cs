using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class AudioDecoder : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;
		DeviceSampleRate = AudioSettings.GetConfiguration().sampleRate;

        if (Audio == null) Audio = GetComponent<AudioSource>();
        Audio.volume = volume;
    }

    private bool ReadyToGetFrame = true;
    public int label = 2001;
    private int dataID = 0;
    private int dataLength = 0;
    private int receivedLength = 0;

    private byte[] dataByte;
    public bool GZipMode = false;


    public UnityEventFloatArray OnPCMFloatReadyEvent = new UnityEventFloatArray();

    public void Action_ProcessData(byte[] _byteData)
    {
        if (!enabled) return;
        if (_byteData.Length <= 18) return;

        int _label = BitConverter.ToInt32(_byteData, 0);
        if (_label != label) return;

        int _dataID = BitConverter.ToInt32(_byteData, 4);
        //if (_dataID < dataID) return;

        if (_dataID != dataID) receivedLength = 0;
        dataID = _dataID;
        dataLength = BitConverter.ToInt32(_byteData, 8);
        int _offset = BitConverter.ToInt32(_byteData, 12);

        GZipMode = _byteData[16] == 1;

        if (receivedLength == 0) dataByte = new byte[dataLength];
        receivedLength += _byteData.Length - 18;
        Buffer.BlockCopy(_byteData, 18, dataByte, _offset, _byteData.Length - 18);

        if (ReadyToGetFrame)
        {
            if (receivedLength == dataLength)
            {
                if (this.isActiveAndEnabled) StartCoroutine(ProcessAudioData(dataByte));
            }
        }
    }

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;
    public float Volume
    {
        get { return volume; }
        set
        {
            volume = Mathf.Clamp(value, 0f, 1f);
            if (Audio == null) Audio = GetComponent<AudioSource>();
            Audio.volume = volume;
        }
    }

    public int SourceChannels = 1;
    public double SourceSampleRate = 48000;
    public double DeviceSampleRate = 48000;

    private Queue<float> ABufferQueue = new Queue<float>();
    private object _asyncLock = new object();

    IEnumerator ProcessAudioData(byte[] receivedAudioBytes)
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            ReadyToGetFrame = false;
            if (GZipMode) receivedAudioBytes = receivedAudioBytes.FMUnzipBytes();

            if (receivedAudioBytes.Length >= 8 + 1024)
            {
                byte[] _sampleRateByte = new byte[4];
                byte[] _channelsByte = new byte[4];
                byte[] _audioByte = new byte[1];
                lock (_asyncLock)
                {
                    _audioByte = new byte[receivedAudioBytes.Length - 8];
                    Buffer.BlockCopy(receivedAudioBytes, 0, _sampleRateByte, 0, _sampleRateByte.Length);
                    Buffer.BlockCopy(receivedAudioBytes, 4, _channelsByte, 0, _channelsByte.Length);
                    Buffer.BlockCopy(receivedAudioBytes, 8, _audioByte, 0, _audioByte.Length);
                }

                SourceSampleRate = BitConverter.ToInt32(_sampleRateByte, 0);
                SourceChannels = BitConverter.ToInt32(_channelsByte, 0);

                float[] ABuffer = ToFloatArray(_audioByte);

                for (int i = 0; i < ABuffer.Length; i++)
                {
                    ABufferQueue.Enqueue(ABuffer[i]);
                }

                CreateClip();

                OnPCMFloatReadyEvent.Invoke(ABuffer);
            }
            ReadyToGetFrame = true;
        }
        yield return null;
    }

    private int position = 0;
    private int samplerate = 44100;
    private int channel = 2;

    private AudioClip audioClip;
    private AudioSource Audio;
    void CreateClip()
    {
        if (samplerate != (int)SourceSampleRate || channel != SourceChannels)
        {
            samplerate = (int)SourceSampleRate;
            channel = SourceChannels;

            if (Audio != null) Audio.Stop();
            if (audioClip != null) DestroyImmediate(audioClip);

            audioClip = AudioClip.Create("StreamingAudio", samplerate * SourceChannels, SourceChannels, samplerate, true, OnAudioRead, OnAudioSetPosition);
            Audio = GetComponent<AudioSource>();
            Audio.clip = audioClip;
            Audio.loop = true;
            Audio.Play();
        }

    }

    void OnAudioRead(float[] data)
    {
        int count = 0;
        while (count < data.Length)
        {
            if (ABufferQueue.Count > 0)
            {
                lock (_asyncLock) data[count] = ABufferQueue.Dequeue();
            }
            else { data[count] = 0f; }

            position++;
            count++;
        }
    }

    void OnAudioSetPosition(int newPosition)
    {
        position = newPosition;
    }

    private float[] ToFloatArray(byte[] byteArray)
    {
        int len = byteArray.Length / 2;
        float[] floatArray = new float[len];
        for (int i = 0; i < byteArray.Length; i += 2)
        {
            floatArray[i / 2] = ((float)BitConverter.ToInt16(byteArray, i)) / 32767f;
        }
        return floatArray;
    }
}


