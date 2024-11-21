using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    private FileInfo fi_Log;
    private FileInfo raw_log;
    private FileInfo PDE_log;
    private FileInfo AE_log;
    private FileInfo RPP_log;
    private FileInfo AVG_dist_log;
    
    private StreamWriter sw_Log;
    private StreamWriter sw_raw_Log;
    private StreamWriter sw_PDE_Log;
    private StreamWriter sw_AE_Log;
    private StreamWriter sw_RPP_Log;
    private StreamWriter sw_AVG_dist_Log;

    private bool isClosed = true;
    private bool isClosed_ANOVA = true;
    private float[,] values = new float[4,3];

    private int partNo;
    private string RDWCondition;
    private string lvlCond;
    
    //public TextMeshProUGUI eyeData;
    
    public void StartCondition(string conditionName, int Difflvl, int subjectNo)
    {
        try
        {
            //This PC\Quest Pro\Internal shared storage\Android\data\com.UnityTechnologies.com.unity.template.urpblank\files
            lvlCond = Difflvl == 0 ? "EasyLvl" : "HardLvl";
            RDWCondition = conditionName;
            partNo = subjectNo;
            
            //eyeData.SetText($"partNo. {partNo}, subjNo {subjectNo}");
            
            fi_Log = new FileInfo($"{Application.persistentDataPath}/{conditionName}_{lvlCond}_Log.csv");
            raw_log = new FileInfo($"{Application.persistentDataPath}/{conditionName}_{lvlCond}_raw_data.csv");

            sw_Log = fi_Log.AppendText();
            sw_raw_Log = raw_log.AppendText();
            
            if (subjectNo == 1)
            {
                sw_Log.WriteLine($"ParticipantNo., PDE, AE, ResetsPerPath, Average_distance_traveled_between_resets");
                sw_raw_Log.WriteLine($"ParticipantNo., PDE, AE, ResetsPerPath, Distance_Traveled, Average_distance_traveled_between_resets, success, Rot_inducedSacc");
                
            }
            
            if (isClosed_ANOVA)
            {
                PDE_log = new FileInfo($"{Application.persistentDataPath}/PDE_data.csv");
                AE_log = new FileInfo($"{Application.persistentDataPath}/AE_data.csv");
                RPP_log = new FileInfo($"{Application.persistentDataPath}/RPP_data.csv");
                AVG_dist_log = new FileInfo($"{Application.persistentDataPath}/AVG_Dist_data.csv");
                
                sw_PDE_Log = PDE_log.AppendText();
                sw_AE_Log = AE_log.AppendText();
                sw_RPP_Log = RPP_log.AppendText();
                sw_AVG_dist_Log = AVG_dist_log.AppendText();

                if (subjectNo == 1)
                {
                    sw_PDE_Log.WriteLine("ParticipantNo., Redirector, DifficultyLevel, Measurement");
                    sw_AE_Log.WriteLine("ParticipantNo., Redirector, DifficultyLevel, Measurement");
                    sw_RPP_Log.WriteLine("ParticipantNo., Redirector, DifficultyLevel, Measurement");
                    sw_AVG_dist_Log.WriteLine("ParticipantNo., Redirector, DifficultyLevel, Measurement");
                }
            }
            
            isClosed = false;
            isClosed_ANOVA = false;
            values = new float[4,3];
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
       
    }

    public void EndTrial(int trialNo, float PDE, float AE, int ResetsPerPath, float distanceTraveled, int success, float rotSacc)
    {
        float avgDistTravBetResets = distanceTraveled / (ResetsPerPath + 1);
        sw_raw_Log.WriteLine($"{partNo},{PDE},{AE},{ResetsPerPath},{distanceTraveled},{avgDistTravBetResets},{success}, {rotSacc}");
        values[0,trialNo] = PDE;
        values[1,trialNo] = AE;
        values[2,trialNo] = ResetsPerPath;
        values[3,trialNo] = avgDistTravBetResets;
        
    }

    public void EndCondition()
    {
        int count = 0;
        var averages = values.Cast<float>()
            .GroupBy(x => count++ / values.GetLength(1))
            .Select(g => g.Average())
            .ToArray();
        
        sw_Log.WriteLine($"{partNo},{averages[0]},{averages[1]},{averages[2]},{averages[3]}");
        sw_PDE_Log.WriteLine($"{partNo},{RDWCondition},{lvlCond},{averages[0]}");
        sw_AE_Log.WriteLine($"{partNo},{RDWCondition},{lvlCond},{averages[1]}");
        sw_RPP_Log.WriteLine($"{partNo},{RDWCondition},{lvlCond},{averages[2]}");
        sw_AVG_dist_Log.WriteLine($"{partNo},{RDWCondition},{lvlCond},{averages[3]}");
        CloseFile();
    }

     void CloseFile()
    {
        sw_Log.Flush();
        sw_raw_Log.Flush();
        sw_Log.Close();
        sw_raw_Log.Close();
        
        isClosed = true;
    }

    void CloseFileANOVA()
    {
        sw_PDE_Log.Flush();
        sw_AE_Log.Flush();
        sw_RPP_Log.Flush();
        sw_AVG_dist_Log.Flush();
        
        sw_PDE_Log.Close();
        sw_AE_Log.Close();
        sw_RPP_Log.Close();
        sw_AVG_dist_Log.Close();

        isClosed_ANOVA = true;
    }

    private void OnApplicationQuit()
    {
        if (!isClosed)
            CloseFile();
        if(!isClosed_ANOVA)
            CloseFileANOVA();
    }
}
