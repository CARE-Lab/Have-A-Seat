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
    
    [HideInInspector] public GameObject trackedArea;
    [HideInInspector] public bool debugMode = true;
    [HideInInspector] public List<GameObject> trackingSpacePoints = new List<GameObject>();
    [HideInInspector] public bool ready = false;
    [HideInInspector] public GameObject physicalChair;

    [SerializeField] GameObject wallMarker;
    [SerializeField] GameObject dirMarker;
    [SerializeField] GameObject realPlanePrefab;
    [SerializeField] private AnchorPrefabSpawner _couchSpawner;
   

    [SerializeField] TextMeshProUGUI text1;
    [SerializeField] TextMeshProUGUI text2;
    [SerializeField] TextMeshProUGUI text3;

    void Start()
    {
        red_manager = GameObject.Find("Redirection Manager").GetComponent<RDManager>();
        pathTrail = GameObject.Find("Redirection Manager").GetComponent<PathTrail>();
    }

    public void Setup()
    {
        StartCoroutine(SetupCorotuine());
    }
    
    IEnumerator SetupCorotuine()
    {
        yield return new WaitForSeconds(.5f);
        eyeData.SetText(eyeData.text + "Setting up\n");
        //Check if the boundary is configured
        bool configured = OVRManager.boundary.GetConfigured();
        if (configured)
        {
            eyeData.SetText(eyeData.text + "is configured\n");
            float angleY = startPos.rotation.eulerAngles.y - red_manager.headTransform.rotation.eulerAngles.y;
            red_manager.XRTransform.Rotate(0, angleY, 0);
           
            Vector3 distDiff = startPos.position - red_manager.headTransform.position;
            distDiff = new Vector3(distDiff.x, 0, distDiff.z);
            red_manager.XRTransform.transform.position += distDiff;
            
            _couchSpawner.SpawnPrefabs();
            physicalChair = new GameObject();
            physicalChair.transform.position = MRUK.Instance.GetCurrentRoom().GetSeatPoses()[0].position;
            ready = true;
        }
    }

    private void Update()
    {
       /* bool button_one_pressed = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
        if (button_one_pressed != prev_state_button_one)
        {
            if (button_one_pressed)
            {
                debugMode = !debugMode;
                trackedArea.GetComponent<Renderer>().enabled = debugMode;
                for (int i = 0; i < trackingSpacePoints.Count; i++)
                {
                    trackingSpacePoints[i].GetComponent<Renderer>().enabled = debugMode;
                }
                if (debugMode)
                    pathTrail.BeginTrailDrawing();
                else
                {
                    pathTrail.ClearTrail(PathTrail.REAL_TRAIL_NAME);
                    pathTrail.ClearTrail(PathTrail.VIRTUAL_TRAIL_NAME);
                }

            }
            prev_state_button_one = button_one_pressed;
        }*/

        bool button_two_pressed = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
        if (button_two_pressed != prev_state_button_two)
        {
            if (button_two_pressed)
            {
                debug_UI.SetActive(!debug_UI.activeInHierarchy);
                Time.timeScale = paused ? 1 : 0;
                paused = !paused;
            }
            prev_state_button_two = button_two_pressed;
        }
    }
    
}
