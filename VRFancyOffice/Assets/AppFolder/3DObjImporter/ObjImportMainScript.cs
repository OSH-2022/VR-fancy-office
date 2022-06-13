using Dummiesman;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjImportMainScript : MonoBehaviour
{
    public GameObject InputBoxWithKeyPad;
    public GameObject ImportedObjectsParent;
    public GameObject ControlPanel;
    public GameObject ObjImporterInst;
    private GameObject Legend; //图例
    public Transform LegendBox; //包含图例的“盒子”
    private Transform ImportedObjectsParentTrans;
    private Vector3 InitializedPosition = new Vector3(-0.3f,1.5f,0.3f); //To be modified
    private string dir;
    private string FullName;
    private string error = string.Empty;
    private List<GameObject> parts = new List<GameObject>();
    private List<int> shown = new List<int>();
    private List<string> NameList=new List<string>();
    private FileInfo[] files;
    private List<xmlParser> xmlParserList;
    private int index = new int();
    public TMP_Text Name;
    public TMP_Text Visibility;
    // Start is called before the first frame update
    void Start()
    {
        //InitializedPosition=...
        transform.position = InitializedPosition;
        ImportedObjectsParentTrans = ImportedObjectsParent.transform;
        ControlPanel.SetActive(false);
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
                NameList.Add(_part.fileName);
                error = string.Empty;
            }
            SetModelCenterEvent(ImportedObjectsParentTrans,1f);
        }
        Destroy(InputBoxWithKeyPad);
        index=0;
        ControlPanel.SetActive(true);
        Name.SetText(NameList[index]);
        CreateLegend();
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
    private void SetModelCenterEvent(Transform tran,float size)
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
        tran.localScale = new Vector3(scale[0]/MaxVec3(bounds.size),scale[1]/MaxVec3(bounds.size),scale[2]/MaxVec3(bounds.size))*size;
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
            item.position = item.position - center + tran.position;
        }
    }
    private void CreateLegend()
    {
        LegendBox.localPosition=new Vector3(0,0,0);
        LegendBox.localScale=new Vector3(1f,1f,1f);
        Legend=Instantiate(parts[index]);
        Legend.transform.SetParent(LegendBox);
        Legend.transform.localPosition=new Vector3(0,0,0);
        SetModelCenterEvent(LegendBox,0.15f);
    }
    private float MaxVec3(Vector3 v)
    {
        if(v[0]>v[1]&&v[0]>v[2]) return v[0];
        else if(v[1]>v[2]&&v[1]>v[2]) return v[1];
        else return v[2];
    }
    public void DecIndex()
    {
        if(index>0) index=index-1;
        SetVisButton();
        Name.SetText(NameList[index]);
        DestroyImmediate(Legend);
        CreateLegend();
    }
    public void IncIndex()
    {
        if(index<NameList.Count-1) index=index+1;
        SetVisButton();
        Name.SetText(NameList[index]);
        DestroyImmediate(Legend);
        CreateLegend();
    }
    public void ChangeVisibility()
    {
        shown[index]=shown[index]==1?0:1;
        if(shown[index]==0) parts[index].SetActive(false);
        else parts[index].SetActive(true);
        SetVisButton();
    }
    public void _Destroy()
    {
        Destroy(ObjImporterInst);
    }
    private void SetVisButton()
    {
        if(shown[index]==1) Visibility.SetText("Hide");
        else Visibility.SetText("Show");
    }
}
