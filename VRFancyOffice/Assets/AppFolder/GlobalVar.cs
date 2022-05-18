using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVar : MonoBehaviour
{
    public List<bool> KeyState=new List<bool>(new bool[62]);
    // Start is called before the first frame update
    void Start()
    {
        for(int i=0;i<62;i++)
        {
            KeyState[i]=false;
        }   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
