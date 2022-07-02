using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlePos : MonoBehaviour
{
    public Vector3 pos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        pos=transform.localPosition;
    }
    public void Replace()
    {
        transform.localPosition=new Vector3(0,2,0);
    }
}
