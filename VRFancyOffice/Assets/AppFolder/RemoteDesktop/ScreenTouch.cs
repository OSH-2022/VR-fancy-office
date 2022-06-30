using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenTouch : MonoBehaviour
{
    private float LastExitTime = 0f;
    private float LastEnterTime = 0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision ctl)
　　{
        if(Time.time - LastExitTime<0.6) return;
        LastEnterTime = Time.time;
        ContactPoint contact = ctl.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = transform.InverseTransformPoint(contact.point);
        double x=-pos.x+0.5;
        double y=-pos.y+0.5;
        string output=string.Format("({0},{1})",x,y);
        print(output);
    }
    void OnCollisionExit(Collision collisionInfo)
    {
        if(Time.time - LastEnterTime<0.6) return;
        LastExitTime=Time.time;
    }
    void OnCollisionStay(Collision collisionInfo)
    {
        // Debug-draw all contact points and normals
        foreach (ContactPoint contact in collisionInfo.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal * 10, Color.white);
        }
    }
}
