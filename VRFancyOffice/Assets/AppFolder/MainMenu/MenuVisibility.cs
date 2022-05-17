using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuVisibility : MonoBehaviour
{
    public GameObject MainMenu;
    public Transform RightHandAnchor;
    private GameObject MenuInst;
    private int shown = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ShowMenu()
    {
        if(shown==0)
        {
            MenuInst = Instantiate(MainMenu);
            MenuInst.transform.SetParent(RightHandAnchor,false);
            shown=1;
        }
    }
    public void DestroyMenu()
    {
        if(shown==1)
        {
            Destroy(MenuInst);
            shown=0;
        }
    }
}
