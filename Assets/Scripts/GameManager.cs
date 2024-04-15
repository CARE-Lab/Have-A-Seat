using System.Collections;
using System.Collections.Generic;
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
    bool configured;
    PathTrail pathTrail;
    bool prev_state_button_one = false;
    bool prev_state_button_two = false;
    bool paused = false;
    
    public Transform startPos;
    public GameObject debug_UI;
    
    [HideInInspector] public GameObject trackedArea;
    [HideInInspector] public bool debugMode = true;
    [HideInInspector] public List<GameObject> trackingSpacePoints = new List<GameObject>();
    [HideInInspector] public bool ready = false;

    [SerializeField] GameObject wallMarker;
    [SerializeField] GameObject dirMarker;
    [SerializeField] GameObject realPlanePrefab;

    [SerializeField] TextMeshProUGUI text1;
    [SerializeField] TextMeshProUGUI text2;
    [SerializeField] TextMeshProUGUI text3;

    void Start()
    {
        red_manager = GameObject.Find("Redirection Manager").GetComponent<RDManager>();
        pathTrail = GameObject.Find("Redirection Manager").GetComponent<PathTrail>();

        //Check if the boundary is configured
        configured = OVRManager.boundary.GetConfigured();
        
        StartCoroutine(SetupCorotuine());
    }
    IEnumerator SetupCorotuine()
    {
        yield return new WaitForSeconds(5f);
        /*float angleY = startPos.rotation.eulerAngles.y - red_manager.headTransform.rotation.eulerAngles.y;
        red_manager.XRTransform.Rotate(0, angleY, 0);
        for (int i = 0; i < trackingSpacePoints.Count; i++)
            trackingSpacePoints[i].transform.RotateAround(Utilities.FlattenedPos3D(red_manager.XRTransform.transform.position), Vector3.up, angleY);
        
        Vector3 distDiff = startPos.position - red_manager.headTransform.position;
        distDiff = new Vector3(distDiff.x, 0, distDiff.z);
        red_manager.XRTransform.transform.position += distDiff;

        for (int i = 0; i < trackingSpacePoints.Count; i++)
            trackingSpacePoints[i].transform.position += distDiff;

        ready = true;*/

        if (configured)
        {
            IntializeArea();
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

    public void IntializeArea()
    {
        //Grab all the boundary points. Setting BoundaryType to OuterBoundary is necessary
        Vector3[] boundaryPoints = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);
        Vector3 boundrydim = OVRManager.boundary.GetDimensions(OVRBoundary.BoundaryType.PlayArea);

        Vector3 p1 = boundaryPoints[0];
        Vector3 p2 = boundaryPoints[1];
        Vector3 p3 = boundaryPoints[2];
        Vector3 p4 = boundaryPoints[3];

        for(int i = 0; i < boundaryPoints.Length; i++)
        {
            GameObject pi = Instantiate(wallMarker, boundaryPoints[i], Quaternion.identity);
            trackingSpacePoints.Add(pi);
        }

      /*  Vector3 p1Diff = p3 - p1;
        Vector3 p2Diff = p4 - p2;
        Vector3 center; // Center of the tracked physical area
        if (LineIntersection(out center, p1, p1Diff, p2, p2Diff))
        {
            // if OVRRig/XR Origin is not aligned with world origin, then must shift and rotate by the diffrence.
            center += red_manager.XRTransform.position;
            center = Utilities.FlattenedPos3D(center);
            
          *//*  Vector3 forwardDir = (p1 - p2).normalized;
            float angle = Vector3.Angle(Vector3.forward, Utilities.FlattenedDir3D(forwardDir));
            if (forwardDir.x < 0.0f)
            {
                angle = -angle;
                angle += 360;
            }*//*

            if (trackedArea == null)
            {
                trackedArea = Instantiate(realPlanePrefab, center + new Vector3(0, 0.05f, 0), Quaternion.identity);
                trackedArea.transform.localScale = new Vector3(boundrydim.x / 10, 1, boundrydim.z / 10);
                trackedArea.transform.localRotation = startPos.localRotation;
                //trackedArea.transform.Rotate(0, angle, 0);

            }

        }*/
    }

    public static bool LineIntersection(out Vector3 intersection, Vector3 linePoint1,
 Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parallel
        if (Mathf.Abs(planarFactor) < 0.0001f
                && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2)
                    / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }

}
