using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct Foo
{
    public int x;
    public int y;
 
    public override string ToString() { return "x: " + x + ", y: " + y; }
}

public class SaveData : MonoBehaviour
{
    //private string fileName;
    private FileInfo fi_Log;
    private StreamWriter sw_Log;
    
    /*string fname = System.DateTime.Now.ToString("HH-mm-ss") + ".csv";
    string path = Path.Combine(Application.persistentDataPath, fname);
    file = new StreamWriter(path);*/
    
    //string fileName = Application.streamingAssetsPath + "/XML/ObjectData.xml";
    public TextMeshProUGUI eyeData;

    private void Awake()
    {
        //fileName  = Path.Combine(Application.persistentDataPath, "/ObjectData.csv"); 
        eyeData.SetText(eyeData.text + $"{Application.persistentDataPath}/SaveData/Log_1.csv"+"\n");
        eyeData.SetText(eyeData.text + $"{Application.dataPath}/SaveData/Log_1.csv"+"\n");
        try {
            fi_Log = new FileInfo($"{Application.persistentDataPath}/SaveData/Log_1.csv");
            sw_Log = fi_Log.AppendText();
            sw_Log.WriteLine($"hahahahha");
        }
        catch (Exception e) {
            eyeData.SetText(eyeData.text + e+"\n");
        }  
        
        eyeData.SetText(eyeData.text +"Path found"+"\n");
    }

    private void Start()
    {
        sw_Log.Flush();
        sw_Log.Close();
        eyeData.SetText(eyeData.text + "Close Complete."+"\n");
    }

    /*public void Save()
    {
        var foo = new Foo()
        {
            x = 5,
            y = 10
        };
 
        XmlSerializer serializer = new XmlSerializer(typeof(Foo));
 
        using (FileStream stream = new FileStream(fileName, FileMode.Create))
        {
            serializer.Serialize(stream, foo);
        }
    }
 
    public void Load()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Foo));
 
        using (FileStream stream = new FileStream(fileName, FileMode.Open))
        {
            var foo = serializer.Deserialize(stream);
 
            Debug.Log(foo);
        }
    }*/

    private void OnApplicationQuit()
    {
        
        
    }
}
