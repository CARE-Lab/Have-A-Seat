using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting;
using System;


//[RequireComponent(typeof(LineRenderer))]

public class SaccadicRedirector : MonoBehaviour
{
    public GameObject rectilePrefab;
   
    public TextMeshProUGUI eyeData;
    public TextMeshProUGUI timeData;
    public TextMeshProUGUI rotData;

    [Tooltip("The game object that is being physically tracked (probably user's head)")]
    public Transform headTransform;

    public Transform XRTransform;

    [Tooltip("Time duration to wait before detecting another saccade")]
    [Range(0, 1)]
    public float downTime;

    [Header("Saccade detection Settings")]
    [Tooltip("Rotation per Saccade in degrees")]
    [Range(0, 5)]
    public float rotPerSaccade;

    [Tooltip("Y angular velocity (degress/sec) above which a saccade is detected")]
    [Range(180, 1000)]
    public int horizontalSaccadeThres;

    [Tooltip("X angular velocity (degress/sec) above which a saccade is detected")]
    [Range(180, 1000)]
    public int VerticalSaccadeThres;


    [Header("Blink detection Settings")]
    [Tooltip("Value above which eyelids are considered closed")]
    [Range(0, 1)]
    public float blinkDetectionThreshold;

    [Tooltip("Rotations while blinking (degress/sec)")]
    [Range(0, 25)]
    public float rotPerBlink;

    [Tooltip("Translation while blinking Front axis (cm/sec)")]
    [Range(0, 10)]
    public float transFront;

    [Tooltip("Translation while blinking Right axis (cm/sec)")]
    [Range(0, 5)]
    public float transRight;

    float secCounter = 0;
    float blinkCounter = 0;
    bool saccdetected = true;
    bool blinkdetected = true;

    TextScroller textScroller;
    GameObject rectile;
    OVRFaceExpressions userFace;

    bool paused = false;
    bool prev_state_pause = false;
    bool prev_state_clear = false;
    Quaternion currDir, prevDir;

    
    float inducedRot = 0;

    

    void Start()
    {
        rectile = null;
        textScroller = eyeData.GetComponent<TextScroller>();
        userFace = GameObject.Find("User").GetComponent<OVRFaceExpressions>();
        //eyeData.SetText("");
    }

 
    private void Update()
    {
/*        bool button_pressed = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
        if (button_pressed != prev_state_button_two)
        {
            if (button_pressed)
            {
                paused = !paused;

            }
            prev_state_button_two = button_pressed;
        }

        bool clear_pressed = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
        if (clear_pressed != prev_state_button_one)
        {
            if (clear_pressed)
            {
                eyeData.SetText("");
               
            }
            prev_state_button_one = clear_pressed;
        }
*/
        float eyeClosedL = userFace.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedL);
        float eyeClosedR = userFace.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedR);

        float curRot = 0;
        if (!saccdetected)
        {
            if (eyeClosedL > blinkDetectionThreshold && eyeClosedR > blinkDetectionThreshold)
            {
                //eyeData.SetText(eyeData.text + "Blink detected\n");
                curRot = rotPerBlink * Time.deltaTime;
                XRTransform.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, rotPerBlink*Time.deltaTime);
                XRTransform.Translate(Vector3.forward * (transFront/100) * Time.deltaTime);
                XRTransform.Translate(Vector3.right * (transRight / 100) * Time.deltaTime);
                blinkdetected = true;
            }
        }
      
        UpdateCurrentGazeDirection();
        Vector3 vel = CalculateAngularVelocity();

        if (!blinkdetected)
        {
            if (Mathf.Abs(vel.y) > horizontalSaccadeThres && Mathf.Abs(vel.y) > Mathf.Abs(vel.x) && !saccdetected)
            {
                XRTransform.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, rotPerSaccade);
                saccdetected = true;
                //eyeData.SetText("Horizontal");
                curRot = rotPerSaccade;

            }
            else if (Mathf.Abs(vel.x) > VerticalSaccadeThres && Mathf.Abs(vel.x) > Mathf.Abs(vel.y) && !saccdetected)
            {
                XRTransform.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, rotPerSaccade);
                saccdetected = true;
                //eyeData.SetText("Veritcal");
                curRot = rotPerSaccade;
            }
        }

        inducedRot += curRot;

        if (saccdetected)
        {
            secCounter += Time.deltaTime;
            if (secCounter > downTime)
            {
                saccdetected = false;
                //eyeData.SetText("");
                secCounter = 0;
            }
        }

        if (blinkdetected)
        {
            blinkCounter += Time.deltaTime;
            if (blinkCounter > downTime)
            {
                blinkdetected = false;
                //eyeData.SetText("");
                blinkCounter = 0;
            }
        }

        timeData.SetText("Time elapsed: " + Time.realtimeSinceStartup.ToString());
        rotData.SetText("Rot induced: " + inducedRot);

       /* if (!paused)
        {

            eyeData.SetText(eyeData.text + vel.ToString() + "\n");
            //eyeData.SetText(eyeData.text + "L: " + eyeClosedL + ", R: " + eyeClosedR + "\n");

            textScroller.scrollDown();
        }*/


    }

    private void FixedUpdate()
    {

        /*if (paused)
        {
            Destroy(rectile);
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            if (rectile == null)
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
        //The Quaterion that you need to apply to prevDir to get to currDir. Basically quaternion subtraction
        Quaternion deltaRotation = currDir * Quaternion.Inverse(prevDir);
        prevDir = currDir;
        deltaRotation.ToAngleAxis(out var angle, out var axis);
        Vector3 angularVelocity = (1.0f / Time.deltaTime) * angle * axis;
        return angularVelocity;

    }

}
