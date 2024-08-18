using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using TMPro;
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
    public GameObject debug_UI;
    public TextMeshProUGUI eyeData;
    public bool debugMode = true;
    
    [HideInInspector] public GameObject trackedArea; 
    [HideInInspector] public List<GameObject> trackingSpacePoints = new List<GameObject>();
    [HideInInspector] public bool ready = false;
    [HideInInspector] public GameObject physicalChair;
    
    [SerializeField] private AnchorPrefabSpawner _couchSpawner;
   

    [SerializeField] TextMeshProUGUI text1;
    [SerializeField] TextMeshProUGUI text2;
    [SerializeField] TextMeshProUGUI text3;
    [SerializeField] GameObject axis_ref;
    
    void Start()
    {
        red_manager = GameObject.Find("Redirection Manager").GetComponent<RDManager>();
        pathTrail = GameObject.Find("Redirection Manager").GetComponent<PathTrail>();
        Setup();
    }

    void Setup()
    {
        float angleY = startPos.rotation.eulerAngles.y - red_manager.headTransform.rotation.eulerAngles.y;
        red_manager.Env.transform.RotateAround(red_manager.currPos, -angleY);
        //red_manager.XRTransform.Rotate(0, angleY, 0);

        Vector3 distDiff = startPos.position - red_manager.headTransform.position;
        distDiff = Utilities.FlattenedPos3D(distDiff);
        red_manager.Env.transform.position -= distDiff;
    }

    public void FindChair()
    {
        physicalChair = new GameObject();
        Pose p = MRUK.Instance.GetCurrentRoom().GetSeatPoses()[0];
        physicalChair.transform.position = p.position;
        physicalChair.transform.forward = p.right;
        ready = true;
    }
    
    private void Update()
    {
       /* bool button_one_pressed = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
        if (button_one_pressed != prev_state_button_one)
        {
            if (button_one_pressed)
            {
               
            }
            prev_state_button_one = button_one_pressed;
        }*/

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
