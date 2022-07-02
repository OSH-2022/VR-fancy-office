using Dummiesman;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;
using System.Net.Sockets;
using System.Runtime.InteropServices;

public class RemoteDesktopMainScript : MonoBehaviour
{
    public GameObject IPInputBox;
    public GameObject PortInputBox;
    public GameObject Arch;
    public GameObject NetworkToolkit;
    public GameObject KeyPad;
    private string IP;
    private int port;
    private Vector3 InitializedPosition = new Vector3(-0.3f,1.5f,0.3f); //To be modified
    private Socket socket, fileSocket;
	private byte[] fileRecvBuf = new byte[1024];
	private uint fileRecvBufIt = 0, fileRecvBufLen = 0;
    private string click_msg = string.Empty;
    private string drag_msg = string.Empty;
    private string release_msg = string.Empty;
	private long keyboard_msg = 0;
    private byte upos = 0;
	[StructLayoutAttribute(LayoutKind.Explicit)]
	private struct cUnion {
		[FieldOffsetAttribute(0)] public long ull;
		[FieldOffsetAttribute(0)] public double d;
	}
	private cUnion xpos, ypos;
    public int connected=0;
    // Start is called before the first frame update
    void Start()
    {
        //InitializedPosition=...
        transform.position = InitializedPosition;
        PortInputBox.SetActive(false);
        Arch.SetActive(false);
        NetworkToolkit.SetActive(false);
        KeyPad.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
	{
        if(connected==1)
		{
            List <bool> KeyState=GameObject.Find("GlobalScripts").GetComponent<GlobalVar>().KeyState;
            for (int i = 0; i < 62; ++i)
				if (KeyState[i])
					keyboard_msg |= 1l << i;
				else
					keyboard_msg &= (-1l << i) - 1l;
			send(xpos.ull, ypos.ull, keyboard_msg);
            upos = 0;
        }
    }
    public void SetIP()
    {
        GameObject loadedObject;
        IP=IPInputBox.transform.Find("InputBox").GetComponent<InputBoxProcessor>().buffer;
        NetworkToolkit.transform.Find("FMNetworkManager").GetComponent<FMNetworkManager>().ClientSettings.ServerIP=IP;
        PortInputBox.SetActive(true);
        Destroy(IPInputBox);
    }
    public void run()
    {
        port=int.Parse(PortInputBox.transform.Find("InputBox").GetComponent<InputBoxProcessor>().buffer);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(IP,port);
        connected=1;
        Destroy(PortInputBox);
        NetworkToolkit.SetActive(true);
        Arch.SetActive(true);
        KeyPad.SetActive(true);
    }
    public void recvFile(string fileIP, int filePort, string srcPath, string dstPath) {
        fileSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        fileSocket.Connect(fileIP, filePort);
		byte[] srcPathBytes = Encoding.UTF8.GetBytes(srcPath);
		byte[] srcPathBytesLength = new byte[4] { (byte)(srcPathBytes.Length >> 24), (byte)(srcPathBytes.Length >> 16), (byte)(srcPathBytes.Length >> 8), (byte)srcPathBytes.Length };
		fileSocket.Send(srcPathBytesLength);
        fileSocket.Send(srcPathBytes);
		fileRecvBufIt = 0; fileRecvBufLen = 0;
		recvFile(dstPath);
		fileSocket.Close();
	}
	private void recvFile(string dstPath) {
		Console.WriteLine("Current path is " + dstPath);
		if (!Directory.Exists(dstPath))
			Directory.CreateDirectory(dstPath);
		while (true) {
			int op = getFileByte();
			switch (op) {
				case 1: {
					int len = getFileByte();
					byte[] fileName = new byte[len];
					for (int i = 0; i < len; ++i)
						fileName[i] = (byte)getFileByte();
					string dstFileName = Path.Combine(dstPath, Encoding.UTF8.GetString(fileName));
					recvFile(dstFileName);
					break;
				}
				case 2: {
					int len = getFileByte();
					byte[] fileName = new byte[len];
					for (int i = 0; i < len; ++i)
						fileName[i] = (byte)getFileByte();
					string dstFileName = Path.Combine(dstPath, Encoding.UTF8.GetString(fileName));
					FileStream fs = new FileStream(dstFileName, FileMode.Create, FileAccess.Write, FileShare.None);
					len = 0;
					for (int i = 0; i < 4; ++i)
						len = len << 8 | getFileByte();
					byte[] outputMessage = new byte[len];
					for (int i = 0; i < len; ++i)
						outputMessage[i] = (byte)getFileByte();
					fs.Write(outputMessage, 0, len);
					fs.Close();
					break;
				}
				case 3:
					return;
			}
		}
    }
	private int getFileByte() {
		if (fileRecvBufIt >= fileRecvBufLen) {
			fileRecvBufIt = 0;
			fileRecvBufLen = fileSocket.Receive(fileRecvBuf);
			if (fileRecvBufLen == 0)
				return -1;
		}
		return ((int)fileRecvBuf[fileRecvBufIt++]) & 255;
	}
    private void send(long x, long y, long msg) {
        byte[] bytes = new byte[25]{ 
			upos, (byte)(x >> 56), (byte)(x >> 48), (byte)(x >> 40), (byte)(x >> 32), (byte)(x >> 24), (byte)(x >> 16), (byte)(x >> 8), (byte)(x),
			(byte)(y >> 56), (byte)(y >> 48), (byte)(y >> 40), (byte)(y >> 32), (byte)(y >> 24), (byte)(y >> 16), (byte)(y >> 8), (byte)(y),
			(byte)(msg >> 56), (byte)(msg >> 48), (byte)(msg >> 40), (byte)(msg >> 32), (byte)(msg >> 24), (byte)(msg >> 16), (byte)(msg >> 8), (byte)(msg) };
        socket.Send(bytes);
    }
    public void LeftButtonDown() { keyboard_msg |= 0x4000000000000000l; }
    public void LeftButtonUp() { keyboard_msg &= -0x4000000000000001l; }
	public void RightButtonDown() { keyboard_msg |= -0x8000000000000000l; }
	public void RightButtonUp() { keyboard_msg &= 0x7fffffffffffffffl; }
    public void Move(double x, double y) { xpos.d = x; ypos.d = y; upos = 1; }
}
