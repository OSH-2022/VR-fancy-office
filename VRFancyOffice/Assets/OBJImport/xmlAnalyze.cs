using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

public class xmlParser {
    public string filePath = "";
    public string fileName = "";
    public string fileType = "";
    public string nodeType = "";
    public string nodeModified = "";
    public string check = "";
    public string opacity = "";
    public string visible = "";
    public string name = "";
    public string index = "";
    public string color = "";
    public string value = "";
    public string originalType = "";
    public string volume = "";
    public string surface = "";
    public string modify = "";
    public string selected = "";
    public string typeSelected = "";
}

public class xmlAnalyze : MonoBehaviour {
    [Header("Set Dynamically")]
    public List<xmlParser> xmlParserList = new List<xmlParser>();
    public string path=string.Empty;
    //public XmlDocument xmlDoc = new XmlDocument();
    //public List<XmlElement> listxml = new List<XmlElement>();
    //public List<string> names = new List<string>();
    void Start()
    {

    }
    public List<xmlParser> Load(string path) {
        StreamReader fileStream = new StreamReader(path, System.Text.Encoding.GetEncoding("utf-8"));
        string fileLine;
        while ((fileLine = fileStream.ReadLine()) != null) {
            //foreach(Match match in Regex.Matches(fileLine, @"(?<=itemList itemCount="")\d{2}(?="")")) {
            //    xmlParser[int.Parse(match.Value)] xmlParserList;
            //    idx = 0;
            //}

            if (Regex.IsMatch(fileLine, @"<item ")) {
                xmlParser tmpXmlParser = new xmlParser();
                while ((fileLine = fileStream.ReadLine()) != "</item>") {
                    if (Regex.IsMatch(fileLine, @"(?<=filePath="")[\s\S]*(?="" fileN)")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=filePath="")[\s\S]*(?="" fileN)")) {
                            if (match.Value != "") {
                                tmpXmlParser.filePath = match.Value;
                            }
                        }
                    }
                    if (Regex.IsMatch(fileLine, @"(?<=fileName="")[\s\S]*(?="" fileT)")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=fileName="")[\s\S]*(?="" fileT)")) {
                            if (match.Value != "") {
                                tmpXmlParser.fileName = match.Value;
                            }
                        }
                    }
                    if (Regex.IsMatch(fileLine, @"(?<=fileType="")[\s\S]*(?="" read)")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=fileType="")[\s\S]*(?="" read)")) {
                            if (match.Value != "") {
                                tmpXmlParser.fileType = match.Value;
                            }
                        }
                    } else if (Regex.IsMatch(fileLine, @"name =""node_type""")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=[>])[\s\S]*(?=[<])")) {
                            if (match.Value != "") {
                                tmpXmlParser.nodeType = match.Value;
                            }
                        }
                    } else if (Regex.IsMatch(fileLine, @"name=""node_modified""")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=[>])[\s\S]*(?=[<])")) {
                            if (match.Value != "") {
                                tmpXmlParser.nodeModified = match.Value;
                            }
                        }
                    } else if (Regex.IsMatch(fileLine, @"name=""check""")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=[>])[\s\S]*(?=[<])")) {
                            if (match.Value != "") {
                                tmpXmlParser.check = match.Value;
                            }
                        }
                    } 
                    else if (Regex.IsMatch(fileLine, @"name=""opacity""")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=[>])[\s\S]*(?=[<])")) {
                            if (match.Value != "") {
                                tmpXmlParser.opacity = match.Value;
                            }
                        }
                    }else if (Regex.IsMatch(fileLine, @"name=""visible""")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=[>])[\s\S]*(?=[<])")) {
                            if (match.Value != "") {
                                tmpXmlParser.visible = match.Value;
                            }
                        }
                    } else if (Regex.IsMatch(fileLine, @"name=""name""")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=[>])[\s\S]*(?=[<])")) {
                            if (match.Value != "") {
                                tmpXmlParser.name = match.Value;
                            }
                        }
                    } else if (Regex.IsMatch(fileLine, @"name=""index""")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=[>])[\s\S]*(?=[<])")) {
                            if (match.Value != "") {
                                tmpXmlParser.index = match.Value;
                            }
                        }
                    } else if (Regex.IsMatch(fileLine, @"name=""color""")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=[>])[\s\S]*(?=[<])")) {
                            if (match.Value != "") {
                                tmpXmlParser.color = match.Value;
                            }
                        }
                    } else if (Regex.IsMatch(fileLine, @"name=""value""")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=[>])[\s\S]*(?=[<])")) {
                            if (match.Value != "") {
                                tmpXmlParser.value = match.Value;
                            }
                        }
                    } else if (Regex.IsMatch(fileLine, @"name=""original_type""")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=[>])[\s\S]*(?=[<])")) {
                            if (match.Value != "") {
                                tmpXmlParser.originalType = match.Value;
                            }
                        }
                    } else if (Regex.IsMatch(fileLine, @"name=""volume""")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=[>])[\s\S]*(?=[<])")) {
                            if (match.Value != "") {
                                tmpXmlParser.volume = match.Value;
                            }
                        }
                    } else if (Regex.IsMatch(fileLine, @"name=""surface""")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=[>])[\s\S]*(?=[<])")) {
                            if (match.Value != "") {
                                tmpXmlParser.surface = match.Value;
                            }
                        }
                    } else if (Regex.IsMatch(fileLine, @"name=""modify""")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=[>])[\s\S]*(?=[<])")) {
                            if (match.Value != "") {
                                tmpXmlParser.modify = match.Value;
                            }
                        }
                    } else if (Regex.IsMatch(fileLine, @"name=""selected""")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=[>])[\s\S]*(?=[<])")) {
                            if (match.Value != "") {
                                tmpXmlParser.selected = match.Value;
                            }
                        }
                    } else if (Regex.IsMatch(fileLine, @"name=""type_selected""")) {
                        foreach (Match match in Regex.Matches(fileLine, @"(?<=[>])[\s\S]*(?=[<])")) {
                            if (match.Value != "") {
                                tmpXmlParser.typeSelected = match.Value;
                            }
                        }
                    }

                }
                if(tmpXmlParser.fileType!="dicom")
                xmlParserList.Add(tmpXmlParser);
            }
        }
        fileStream.Close();
        fileStream.Dispose();
        return xmlParserList;
    }

    void Update() {

    }
}
