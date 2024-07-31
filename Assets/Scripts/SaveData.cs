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
    public int subjectNumber;
 
    private FileInfo fi_Log;
    private StreamWriter sw_Log;
    private bool isClosed;
    
    private void Start()
    {
        
    }

    public void StartCondition(string conditionName)
    {
        string date = DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString();
        fi_Log = new FileInfo($"{Application.persistentDataPath}SaveData/{subjectNumber}_{conditionName}_Log_{date}.csv");
        sw_Log = fi_Log.AppendText();
        sw_Log.WriteLine($"PDE, AE, Resets Per Path, Distance Traveled, Average distance traveled between resets, Rot_induced_Sacc");
        isClosed = false;
    }

    public void EndTrial(float PDE, float AE, int ResetsPerPath, float distanceTraveled, float rotInducedSacc)
    {
           
    }

    public void CloseFile()
    {
        sw_Log.Flush();
        sw_Log.Close();
        isClosed = true;
    }

    private void OnApplicationQuit()
    {
        if (isClosed) return;
        CloseFile();
    }
}
