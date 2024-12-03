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
    public RDManager rdManager;
    public GameObject TrialUI;
    public GameObject ConditionUI;
    public GameObject FinishUI;
    
    [Header("Experiment Conditions")]
    public int subjectNumber;
    public Redirector_condition Condition;
    public bool isTrial;
    
    private SaveData logFile;
    private int diffIndx = 0;
    private String[][]latin_square = new String[2][];
    private String[] diffOrder;
    
    [Header("Debugging")]
    public TextMeshProUGUI Text1;

    private void Awake()
    {
        rdManager.condition = Condition;
        rdManager.istrial = isTrial;
        logFile = GetComponent<SaveData>();
    }

    void Start()
    {
        ReadCSV();
        diffOrder = latin_square[Random.Range(0, 2)];
    }

    public void StartCondition()
    {
        if(isTrial)
            rdManager.difficultyLvl = 0;
        else
        {
            if (diffOrder[diffIndx] == "A")
                rdManager.difficultyLvl = 0;
            else if (diffOrder[diffIndx] == "B")
                rdManager.difficultyLvl = 1;
            
            logFile.StartCondition(Condition.ToString(), rdManager.difficultyLvl, subjectNumber);
        }
        
        if(TrialUI.activeInHierarchy)
            TrialUI.GetComponent<CloseMenu>().ActivateOpenMenu();
        else
            TrialUI.SetActive(true);
    }

    public void EndCondition()
    {
        if (isTrial)
        {
            FinishUI.SetActive(true);
            return;
        }
        
        logFile.EndCondition();
        diffIndx++;
        if (diffIndx == 2)
        {
            FinishUI.SetActive(true);
        }
        else
        {
            ConditionUI.GetComponent<CloseMenu>().ActivateOpenMenu();
        }
    }
    
    public void EndTrial(int trialNo, float PDE, float AE, int ResetsPerPath, float distanceTraveled, int success, float rotSacc)
    {
        logFile.EndTrial(trialNo, PDE, AE, ResetsPerPath, distanceTraveled, success, rotSacc);
    }

    void ReadCSV()
    {
        string[] data = textAssetData.text.Split(new string[] { ",", "\n" }, StringSplitOptions.None);
       
        for (int i=0; i< 2; i++)
            latin_square[i] = new String[2];
        
        for (int i = 0; i < data.Length-1; i++)
        {
            int r = i / 2;
            latin_square[r][i % 2] = data[i];
        }
        
    }
    
    public void QuitApplication()
    {
        Application.Quit();
    }
    
}
