using Dummiesman;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjImportMainScript : MonoBehaviour
{
    public GameObject InputBoxWithKeyPad;
    public GameObject ImportedObjectsParent;
    private Transform ImportedObjectsParentTrans;
    private Vector3 InitializedPosition = new Vector3(-0.3f,1.5f,0.3f); //To be modified
    private string dir;
    private string FullName;
    private string error = string.Empty;
    private List<GameObject> parts = new List<GameObject>();
    private List<int> shown = new List<int>();
    private FileInfo[] files;
    private List<xmlParser> xmlParserList;
    // Start is called before the first frame update
    void Start()
    {
        //InitializedPosition=...
        transform.position = InitializedPosition;
        ImportedObjectsParentTrans = ImportedObjectsParent.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Load()
    {
        GameObject loadedObject;
        dir=InputBoxWithKeyPad.transform.Find("InputBox").GetComponent<InputBoxProcessor>().buffer;
        if (!Directory.Exists(dir))
        {
            error = "directory doesn't exist.";
        }
        else
        {
            FullName = GetFiles(dir);
            xmlParserList = new xmlAnalyze().Load(FullName);
            foreach(xmlParser _part in xmlParserList)
            {
                float r = Convert.ToSingle(_part.color.Split(',')[0]);
                float g = Convert.ToSingle(_part.color.Split(',')[1]);
                float b = Convert.ToSingle(_part.color.Split(',')[2]);
                float a = Convert.ToSingle(_part.opacity);
                Color color = new Color(r, g, b, a);
                string filename=FullName.Substring(0,FullName.LastIndexOf("\\")+1)+_part.fileName+".obj";
                loadedObject = new OBJLoader().Load(filename, color,new Vector3(0,0,0));
                loadedObject.transform.SetParent(ImportedObjectsParentTrans);
                parts.Add(loadedObject);
                shown.Add(1);
                error = string.Empty;
            }
            SetModelCenterEvent(ImportedObjectsParentTrans);
        }
        Destroy(InputBoxWithKeyPad);
    }
    private string GetFiles(string path)
    {
        string DSaaSPath = string.Empty;
        //获取指定路径下面的所有资源文件  
        if (Directory.Exists(path))
        {
            DirectoryInfo direction = new DirectoryInfo(path);
            files = direction.GetFiles("*.3DSaaS");
            DSaaSPath=files[0].FullName;
        }
        return DSaaSPath;
    }
    private void SetModelCenterEvent(Transform tran)
    {
        Vector3 scale = tran.localScale;
        Vector3 center = Vector3.zero;
        Renderer[] renders = tran.GetComponentsInChildren<Renderer>();
        foreach (Renderer child in renders)
        {
            center += child.bounds.center;
        }
        center /= tran.GetComponentsInChildren<Renderer>().Length;
        Bounds bounds = new Bounds(center, Vector3.zero);
        foreach (Renderer item in renders)
        {
            bounds.Encapsulate(item.bounds);
        }
        tran.localScale = new Vector3(scale[0]/MaxVec3(bounds.size),scale[1]/MaxVec3(bounds.size),scale[2]/MaxVec3(bounds.size));
        scale = tran.localScale;
        tran.localPosition = Vector3.zero;
        tran.rotation = Quaternion.Euler(Vector3.zero);
        center = Vector3.zero;
        renders = tran.GetComponentsInChildren<Renderer>();
        foreach (Renderer child in renders)
        {
            center += child.bounds.center;
        }
        center /= tran.GetComponentsInChildren<Renderer>().Length;
        foreach (Transform item in tran)
        {
            item.position = item.position - center + InitializedPosition;
        }
    }
    private float MaxVec3(Vector3 v)
    {
        if(v[0]>v[1]&&v[0]>v[2]) return v[0];
        else if(v[1]>v[2]&&v[1]>v[3]) return v[1];
        else return v[2];
    }
}
