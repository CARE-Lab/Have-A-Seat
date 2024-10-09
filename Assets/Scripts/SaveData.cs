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
    private FileInfo fi_Log;
    private StreamWriter sw_Log;
    private bool isClosed;
    
    //public TextMeshProUGUI eyeData;
    
    public void StartCondition(string conditionName, int subjectNumber)
    {
        string date = DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString();
        try
        {
            //This PC\Quest Pro\Internal shared storage\Android\data\com.UnityTechnologies.com.unity.template.urpblank\files
            fi_Log = new FileInfo($"{Application.persistentDataPath}/Subject_No_{subjectNumber}_{conditionName}_Log_{date}.csv");
            sw_Log = fi_Log.AppendText();
            sw_Log.WriteLine($"Difficulty Level, PDE, AE, Resets Per Path, Distance Traveled, Average distance traveled between resets");
            isClosed = false;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
       
    }

    public void EndTrial(int difficultyLvl, float PDE, float AE, int ResetsPerPath, float distanceTraveled)
    {
        float avgDistTravBetResets = distanceTraveled / (ResetsPerPath + 1);
        sw_Log.WriteLine($"{difficultyLvl},{PDE},{AE},{ResetsPerPath},{distanceTraveled},{avgDistTravBetResets}");
    }

    public void EndCondition()
    {
        CloseFile();
    }

     void CloseFile()
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
