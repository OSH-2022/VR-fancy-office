using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuOptionProcess : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject ObjImporterPrefab;
    public GameObject RemoteDesktopPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ThreeDObjImport()
    {
        Instantiate(ObjImporterPrefab);
        Destroy(MainMenu);
    }
    public void RemoteDesktop()
    {
        Instantiate(RemoteDesktopPrefab);
        Destroy(MainMenu);
    }
}
