using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenTouch : MonoBehaviour
{
    public GameObject RemoteDesktopGameObject;
    public float LastExitTime = 0f;
    public float LastEnterTime = 0f;
    public int exitted=1;
    public int exit_event=0;
    public int StartCounting=0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(StartCounting==1&&Time.time-LastExitTime>0.1) exitted=1;
        if(exitted==1&&exit_event==0)
        {
            RemoteDesktopGameObject.GetComponent<RemoteDesktopMainScript>().LeftButtonUp();
            exit_event=1;
        }
    }
    void OnCollisionEnter(Collision collisionInfo)
　　{
        if(exitted==0)
        {
            StartCounting=0;
            return;
        }
        LastEnterTime=Time.time;
        exitted=0;
        exit_event=0;
        StartCounting=0;
        ContactPoint contact = collisionInfo.contacts[0];
        Vector3 pos = transform.InverseTransformPoint(contact.point);
        double x=-pos.x+0.5;
        double y=-pos.y+0.5;
        string output=string.Format("({0},{1})",x,y);
        RemoteDesktopGameObject.GetComponent<RemoteDesktopMainScript>().Move(x,y);
        RemoteDesktopGameObject.GetComponent<RemoteDesktopMainScript>().LeftButtonDown();
        print(output);
    }
    void OnCollisionExit(Collision collisionInfo)
    {
        LastExitTime=Time.time;
        StartCounting=1;
    }
    void OnCollisionStay(Collision collisionInfo)
    {
        if(Time.time-LastEnterTime<1.5) return;
        ContactPoint contact = collisionInfo.contacts[0];
        Vector3 pos = transform.InverseTransformPoint(contact.point);
        double x=-pos.x+0.5;
        double y=-pos.y+0.5;
        string output=string.Format("({0},{1})",x,y);
        RemoteDesktopGameObject.GetComponent<RemoteDesktopMainScript>().Move(x,y);
    }
}
