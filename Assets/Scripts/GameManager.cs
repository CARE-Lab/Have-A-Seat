using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.XR.Oculus;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;

public class GameManager : MonoBehaviour
{
    RDManager red_manager;
    PathTrail pathTrail;
    bool prev_state_button_one = false;
    bool prev_state_button_two = false;
    bool paused = false;
    
    public Transform startPos;
    
    public bool debugMode = true;
    
    [HideInInspector] public bool ready = false;
    
    [SerializeField] private AnchorPrefabSpawner _couchSpawner;
    
    [SerializeField] private Transform easyLvl;
    [SerializeField] private Transform mediumLvl;
    [SerializeField] private Transform hardLvl;
    
    [Header("Debugging")]
    [SerializeField] TextMeshProUGUI text1;
    [SerializeField] TextMeshProUGUI text2;
    [SerializeField] TextMeshProUGUI text3;
    [SerializeField] GameObject axis_ref;
    public GameObject debug_UI;
    public TextMeshProUGUI eyeData;
    
    
    
    void Start()
    {
        red_manager = GameObject.Find("Redirection Manager").GetComponent<RDManager>();
        pathTrail = GameObject.Find("Redirection Manager").GetComponent<PathTrail>();
    }

    public void Setup()
    {
        //spawn and find physical chair
        _couchSpawner.SpawnPrefabs();
        FindChair();
        ready = true;
    }
    

    private void FindChair()
    {
        foreach (var an in MRUK.Instance.GetCurrentRoom().Anchors)
        {
            if (an.HasLabel("COUCH"))
            {
                Transform parent = an.gameObject.transform;
                foreach (Transform child in parent.transform)
                {
                    if (child.tag == "PhysicalChair")
                    {
                        red_manager.PhysicalTarget = child;
                    }
                        
                }
            }
        }
    }
    
    private void Update()
    {
        bool button_two_pressed = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
        if (button_two_pressed != prev_state_button_two)
        {
            if (button_two_pressed)
            {
                //debug_UI.SetActive(!debug_UI.activeInHierarchy);
                Time.timeScale = paused ? 1 : 0;
                paused = !paused;
            }
            prev_state_button_two = button_two_pressed;
        }
    }
    
}
