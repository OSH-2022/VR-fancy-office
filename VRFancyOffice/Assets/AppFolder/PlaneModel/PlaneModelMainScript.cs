using Dummiesman;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;

public class PlaneModelMainScript : MonoBehaviour
{
    public GameObject AileronLeft; //左副翼
    public GameObject AileronRight; //右副翼
    public GameObject Rubber; //转向舵
    public GameObject Elevators; //升降舵
    public GameObject Handle1,Handle2;
    public GameObject IPInputbox;
    public GameObject PortInputbox;
    public GameObject Panel1,Panel2;
    public GameObject Voltige;
    public GameObject CloseButton;
    public GameObject InfoScreenGameObject;
    public GameObject PlaneModelGameObject;
    private int height=new int();
    private int velocity=new int();
    private int AileronLeftRot=new int();
    private int AileronRightRot=new int();
    private int ElevatorsRot=new int();
    private int RubberRot=new int();
    private int Accelerate=new int();
    private Vector3 InitializedPosition = new Vector3(-0.3f,1.5f,0.3f); //To be modified
    private string IP;
    private int port;
    private int connected=0;
    public Socket socket;
    public TMP_Text InfoScreen;
    Thread childThread = null;
    Vector3 RotationToUpdate=new Vector3(0,0,0);
    // Start is called before the first frame update
    void Start()
    {
        //InitializedPosition=...
        transform.position = InitializedPosition;
        PortInputbox.SetActive(false);
        Panel1.SetActive(false);
        Panel2.SetActive(false);
        Voltige.SetActive(false);
        CloseButton.SetActive(false);
        InfoScreenGameObject.SetActive(false);
        AileronLeftRot=0;
        AileronRightRot=0;
        ElevatorsRot=0;
    }

    // Update is called once per frame
    void Update()
    {
        //Voltige.transform.localRotation=Quaternion.Euler(RotationToUpdate);
        Voltige.transform.localRotation=Quaternion.Euler(new Vector3(0,0,0));
        Voltige.transform.Rotate(RotationToUpdate[0],0,0);
        Voltige.transform.Rotate(0,RotationToUpdate[1],0);
        Voltige.transform.Rotate(0,0,RotationToUpdate[2]);
        if(connected==1)
        {
            Vector3 pos=Handle1.GetComponent<HandlePos>().pos;
            float roll=9*pos.x; //翻滚角
            float pitch=9*pos.z; //俯仰角
            AileronLeft.transform.localRotation = Quaternion.Euler(new Vector3(0,roll,0));
            AileronRight.transform.localRotation = Quaternion.Euler(new Vector3(0,-roll,0));
            AileronLeftRot=(int)(roll*(1000));
            AileronRightRot=-(int)(roll*(1000));
            Elevators.transform.localRotation=Quaternion.Euler(new Vector3(0,pitch,0));
            ElevatorsRot=(int)(pitch*(1000));
            pos=Handle2.GetComponent<HandlePos>().pos;
            float accel=9*pos.z;//加速度
            float yaw=9*pos.x; //偏航角
            Accelerate=(int)(1000*accel);
            Rubber.transform.localRotation = Quaternion.Euler(new Vector3(0,0,yaw));
            RubberRot=(int)(yaw*(1000));
            string text="Height:"+Convert.ToString(height)+"\nVelocity:"+Convert.ToString(velocity)+"\nRoll:"+Convert.ToString((int)(Voltige.transform.localRotation[0]*180))+"\nPitch:"+Convert.ToString((int)(180*Voltige.transform.localRotation[1]))
            +"\nYaw:"+Convert.ToString((int)(180*Voltige.transform.localRotation[2]));
            InfoScreen.SetText(text);
        }
    }
    void FixedUpdate()
    {
        if(connected==1)
        {
            string msg=Convert.ToString(AileronLeftRot)+" "+Convert.ToString(AileronRightRot)+" "+Convert.ToString(ElevatorsRot)+" "+Convert.ToString(Accelerate)+" "+Convert.ToString(RubberRot);
            send(msg);
        }
    }
    public void SetIP()
    {
        IP=IPInputbox.transform.Find("InputBox").GetComponent<InputBoxProcessor>().buffer;
        Destroy(IPInputbox);
        PortInputbox.SetActive(true);
    }
    public void SetPort()
    {
        port=int.Parse(PortInputbox.transform.Find("InputBox").GetComponent<InputBoxProcessor>().buffer);
        Destroy(PortInputbox);
        Panel1.SetActive(true);
        Panel2.SetActive(true);
        Voltige.SetActive(true);
        InfoScreenGameObject.SetActive(true);
        CloseButton.SetActive(true);
        Voltige.transform.localScale=new Vector3(0.1f,0.1f,0.1f);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(IP, port);
        CloseThread();
        ThreadStart childRef = new ThreadStart(DataReceiveFunction);
        childThread = new Thread(childRef);
        childThread.Start();
        connected=1;
    }
    void send(string msg)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(msg);
        socket.Send(bytes,bytes.Length,SocketFlags.None);
    }
    public void DataReceiveFunction()
    {
        while(true)
        {
            byte[] readBuff = new byte[1024]; 
            int count = socket.Receive(readBuff);
            string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
            char[] chs = { ' ' };
            string[] res = str.Split(chs, options: StringSplitOptions.RemoveEmptyEntries);
            List<string> msg=new List<string>();
            foreach (var item in res)
            {
                msg.Add(item); //翻转角、旋转角，乘以一千取整，空格隔开
            }
            float recvroll=(float)(int.Parse(msg[0]))/1000;
            float recvpitch=(float)(int.Parse(msg[1]))/1000;
            float recvyaw=(float)(int.Parse(msg[2]))/1000;
            velocity=int.Parse(msg[3])/1000;
            height=int.Parse(msg[4])/1000;
            RotationToUpdate=new Vector3(recvroll,recvyaw,-recvpitch);
        }
    }
    public void CloseThread()
    {
        if (childThread != null)
        {
            childThread.Abort();
        }
    }
    private void OnDestroy()
    {
        CloseThread();
    }
    public void Close()
    {
        Destroy(PlaneModelGameObject);
    }
}
