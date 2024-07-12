using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    //private string fileName;
    private FileInfo fi_Log;
    private StreamWriter sw_Log;
    
    public TextMeshProUGUI eyeData;

    private void Awake()
    {
        try {
            fi_Log = new FileInfo($"{Application.persistentDataPath}/SaveData/Log_1.csv");
            sw_Log = fi_Log.AppendText();
            sw_Log.WriteLine($"hahahahha");
        }
        catch (Exception e) {
            sw_Log.WriteLine(e);
        }  
        
    }

    private void Start()
    {
        sw_Log.Flush();
        sw_Log.Close();
        //eyeData.SetText(eyeData.text + "Close Complete."+"\n");
    }
    
    private void OnApplicationQuit()
    {
        
        
    }
}
