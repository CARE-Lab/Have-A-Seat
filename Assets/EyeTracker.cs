using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting;


//[RequireComponent(typeof(LineRenderer))]

public class EyeTracker : MonoBehaviour
{
    public GameObject rectilePrefab;
    public enum Eye
    {
        Left,
        Right
    }
    public Eye eye;
    public TextMeshProUGUI eyeData;

    [Tooltip("The game object that is being physically tracked (probably user's head)")]
    public Transform headTransform;

    public Transform XRTransform;

    [Tooltip("Rotation per Saccade in degrees")]
    [Range(0, 5)]
    public float rotPerSaccade;

    [Tooltip("Y angular velocity (degress/sec) above which a saccade is detected")]
    [Range(180, 1000)]
    public int horizontalSaccadeThres;

    [Tooltip("Time duration to wait before detecting another saccade")]
    [Range(0, 1)]
    public float downTime;

    float secCounter = 0;
    bool saccdetected = true;

    TextScroller textScroller;
    GameObject rectile;
    OVRFaceExpressions userFace;

    bool paused = false;
    bool prev_state_pause = false;
    bool prev_state_clear = false;
    Quaternion currDir, prevDir;

    

    void Start()
    {
        rectile = null;
        textScroller = eyeData.GetComponent<TextScroller>();
        userFace = GameObject.Find("User").GetComponent<OVRFaceExpressions>();
        eyeData.SetText("");
    }

 
    private void Update()
    {
        bool button_pressed = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
        if (button_pressed != prev_state_pause)
        {
            if (button_pressed)
            {
                paused = !paused;

            }
            prev_state_pause = button_pressed;
        }

        bool clear_pressed = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
        if (clear_pressed != prev_state_clear)
        {
            if (clear_pressed)
            {
                //eyeData.SetText("");
                XRTransform.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, rotPerSaccade);

            }
            prev_state_clear = clear_pressed;
        }

        float eyeClosedL = userFace.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedL);
        float eyeClosedR = userFace.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedR);

        UpdateCurrentGazeDirection();
        Vector3 vel = CalculateAngularVelocity();

        if(Mathf.Abs(vel.y) > horizontalSaccadeThres && !saccdetected)
        {
            XRTransform.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, rotPerSaccade);
            saccdetected = true;
            eyeData.SetText("detected");
        }

        secCounter += Time.deltaTime;
        if(secCounter >= downTime)
        {
            saccdetected = false;
            eyeData.SetText("");
            secCounter = 0;
        }
           

       /* if (!paused)
        {
            if (eye == Eye.Left)
            {
                eyeData.SetText(eyeData.text + "L: " + vel.ToString() + "\n");
                //eyeData.SetText(eyeData.text + "L: " + eyeClosedL + ", R: "+ eyeClosedR + "\n");

            }
            textScroller.scrollDown();
        }*/


    }

    private void FixedUpdate()
    {

       /* if (paused)
        {
            Destroy(rectile);
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            if(rectile == null)
            {
                rectile = Instantiate(rectilePrefab, hit.point, rectilePrefab.transform.rotation);
            }
            else
            {
                rectile.transform.position = hit.point;
            }
            
        }*/

       
    }

    void UpdateCurrentGazeDirection()
    {
        currDir = transform.rotation;
    }

    Vector3 CalculateAngularVelocity()
    {
        Quaternion deltaRotation = currDir * Quaternion.Inverse(prevDir);
        prevDir = currDir;
        deltaRotation.ToAngleAxis(out var angle, out var axis);
        Vector3 angularVelocity = (1.0f / Time.deltaTime) * angle * axis;
        return angularVelocity;

    }

}
