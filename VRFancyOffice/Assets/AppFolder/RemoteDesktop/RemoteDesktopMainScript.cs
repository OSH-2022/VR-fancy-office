using Dummiesman;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net.Sockets;
using System.Runtime.InteropServices

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
    private Socket socket;
    private string click_msg = string.Empty;
    private string drag_msg = string.Empty;
    private string release_msg = string.Empty;
	private ulong keyboard_msg = 0;
	[StructLayoutAttribute(LayoutKind.Explicit)]
	private struct cUnion {
		[FieldOffsetAttribute(0)] public ulong ull;
		[FiledOffsetAttribute(0)] public double d;
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
					keyboard_msg |= 1ul << i;
				else
					keyboard_msg &= -1ul << i;
			send(xpos.ull, ypos.ull, keyboard_msg);
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
    private void send(ulong x, ulong y, ulong msg) {
        byte[] bytes = new byte[24]{ 
			x >> 56, x >> 48 & 255, x >> 40 & 255, x >> 32 & 255, x >> 24 & 255, x >> 16 & 255, x >> 8 & 255, x & 255,
			y >> 56, y >> 48 & 255, y >> 40 & 255, y >> 32 & 255, y >> 24 & 255, y >> 16 & 255, y >> 8 & 255, y & 255,
			msg >> 56, msg >> 48 & 255, msg >> 40 & 255, msg >> 32 & 255, msg >> 24 & 255, msg >> 16 & 255, msg >> 8 & 255, msg & 255 };
        socket.Send(bytes);
    }
    public void LeftButtonDown() { keyboard_msg |= 1ul << 62; }
    public void LeftButtonUp() { keyboard_msg &= -1ul << 62; }
	public void RightButtonDown() { keyboard_msg |= 1ul << 63; }
	public void RightButtonUp() { keyboard_msg &= -1ul << 63; }
    public void Move(double x, double y) { xpos.d = x; ypos.d = y; }
}
