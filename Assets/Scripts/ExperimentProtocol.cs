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
    public Redirector_condition condition;
    public String[][]latin_square = new String[18][];
    public RDManager rdManager;
    public GameObject TrialUI;
    
    private SaveData logFile;
    
    void Start()
    {
        ReadCSV();
        rdManager.condition = condition;
        logFile = GetComponent<SaveData>();
    }

    public void StartCondition()
    {
        String[] trial_order = latin_square[Random.Range(0, 17)];
        for (int i = 0; i < trial_order.Length; i++)
        {
            if (trial_order[i] == "A" || trial_order[i] == "B" || trial_order[i] == "C")
                trial_order[i] = "L";
            if (trial_order[i] == "D" || trial_order[i] == "E" || trial_order[i] == "F")
                trial_order[i] = "M";
            if (trial_order[i] == "G" || trial_order[i] == "H" || trial_order[i] == "I")
                trial_order[i] = "H";
        }
        
        rdManager.trial_Order = trial_order;
        logFile.StartCondition(condition.ToString());
        TrialUI.SetActive(true);
    }

    void ReadCSV()
    {
        string[] data = textAssetData.text.Split(new string[] { ",", "\n" }, StringSplitOptions.None);
       
        for (int i = 0; i < data.Length-1; i++)
        {
            int r = i / 9;
            latin_square[r] = new String[9];
            latin_square[r][i % 9] = data[i];
        }
        
    }
    
}
