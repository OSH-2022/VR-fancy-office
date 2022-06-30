using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignTexture : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Assign(Texture a)
    {
        Renderer m_Renderer;
        m_Renderer=GetComponent<Renderer>();
        m_Renderer.material.SetTexture("_MainTex",a);
    }
}
