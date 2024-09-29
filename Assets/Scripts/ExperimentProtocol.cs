using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public enum Redirector_condition
{
    OriginalAPF = 0,
    AlignmentAPF =1
}

public class ExperimentProtocol : MonoBehaviour
{
    public TextAsset textAssetData;
    public String[][]latin_square = new String[6][];
    public RDManager rdManager;
    public GameObject TrialUI;
    public GameObject ConditionUI;
    public GameObject FinishUI;
    
    private SaveData logFile;
    
    private int conditionIndx = 0;
    private String[] condition_order;
    
    void Start()
    {
        ReadCSV();
        condition_order = latin_square[Random.Range(0, 6)];
        logFile = GetComponent<SaveData>();
    }

    public void StartCondition()
    {
        
        if (condition_order[conditionIndx] == "A" || condition_order[conditionIndx] == "C" || condition_order[conditionIndx] == "E")
        {
            rdManager.condition = Redirector_condition.OriginalAPF;
        }
        else
            rdManager.condition = Redirector_condition.AlignmentAPF;

        if (condition_order[conditionIndx] == "A" || condition_order[conditionIndx] == "B")
            rdManager.difficultyLvl = 0;
        else if (condition_order[conditionIndx] == "C" || condition_order[conditionIndx] == "D")
            rdManager.difficultyLvl = 1;
        else
            rdManager.difficultyLvl = 2;
       
        
        logFile.StartCondition(rdManager.condition.ToString(), rdManager.difficultyLvl);
        TrialUI.SetActive(true);
        
    }

    public void EndCondition()
    {
        logFile.EndCondition();
        conditionIndx++;
        if (conditionIndx == 6)
        {
            FinishUI.SetActive(true);
        }
        else
        {
            ConditionUI.SetActive(true);
        }
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    public void EndTrial(float PDE, float AE, int ResetsPerPath, float distanceTraveled)
    {
        logFile.EndTrial(PDE, AE, ResetsPerPath, distanceTraveled);
    }

    void ReadCSV()
    {
        string[] data = textAssetData.text.Split(new string[] { ",", "\n" }, StringSplitOptions.None);
       
        for (int i = 0; i < data.Length-1; i++)
        {
            int r = i / 6;
            latin_square[r] = new String[6];
            latin_square[r][i % 6] = data[i];
        }
        
    }
    
}
