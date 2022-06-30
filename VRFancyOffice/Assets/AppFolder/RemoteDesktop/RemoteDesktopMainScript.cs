using Dummiesman;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net.Sockets;

public class RemoteDesktopMainScript : MonoBehaviour
{
    public GameObject IPInputBox;
    public GameObject PortInputBox;
    public GameObject Arch;
    public GameObject NetworkToolkit;
    private string IP;
    private int port;
    private Vector3 InitializedPosition = new Vector3(-0.3f,1.5f,0.3f); //To be modified
    private Socket socket;
    private string click_msg = string.Empty;
    private string drag_msg = string.Empty;
    private string release_msg = string.Empty;
    public int connected=0;
    // Start is called before the first frame update
    void Start()
    {
        //InitializedPosition=...
        transform.position = InitializedPosition;
        PortInputBox.SetActive(false);
        Arch.SetActive(false);
        NetworkToolkit.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(connected==1)
        {
            if(drag_msg!="") send(drag_msg);
            if(click_msg!="") send(click_msg);
            if(release_msg!="") send(release_msg);
            click_msg="";
            drag_msg="";
            release_msg="";
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
    }
    private void send(string msg)
    {
        byte[] bytes = System.Text.Encoding.Default.GetBytes(msg);
        socket.Send(bytes);
    }
    public void LeftButtonDown()
    {
        click_msg="2 ";
    }
    public void LeftButtonUp()
    {
        release_msg="3 ";
    }
    public void Move(double x, double y)
    {
        int _x=(int)(x*1000);
        int _y=(int)(y*1000);
        string msg=string.Format("1 {0} {1} ",_x,_y);
        drag_msg=msg;
    }
}
