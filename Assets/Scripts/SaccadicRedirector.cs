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

    [Tooltip("Y angular velocity (degrees/sec) above which a saccade is detected")]
    [Range(180, 1000)]
    public int horizontalSaccadeThres;

    [Tooltip("X angular velocity (degrees/sec) above which a saccade is detected")]
    [Range(180, 1000)]
    public int VerticalSaccadeThres;


    [Header("Blink detection Settings")]
    [Tooltip("Value above which eyelids are considered closed")]
    [Range(0, 1)]
    public float blinkDetectionThreshold;

    [Tooltip("Rotations while blinking (degrees/sec)")]
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

 
    OVRFaceExpressions userFace;

    Quaternion currDir, prevDir;
  
    [HideInInspector]
    public float inducedRotSaccadic = 0;

    PathTrail pathTrail;
    GameManager gameManager;
    RDManager rdManager;



    void Start()
    {
       
        userFace = GameObject.Find("User").GetComponent<OVRFaceExpressions>();

        pathTrail = GameObject.Find("Redirection Manager").GetComponent<PathTrail>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        rdManager = GameObject.Find("Redirection Manager").GetComponent<RDManager>();
        //eyeData.SetText("");
    }

 
    private void Update()
    {

        if (Time.timeScale == 0 || !gameManager.ready)
            return;


        if (userFace.ValidExpressions) // blink rotations
        {
            float eyeClosedL = userFace.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedL);
            float eyeClosedR = userFace.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedR);

            if (!saccdetected)
            {
                if (eyeClosedL > blinkDetectionThreshold && eyeClosedR > blinkDetectionThreshold)
                {
                    //eyeData.SetText(eyeData.text + "Blink detected\n");
                    InduceRot(rotPerBlink * Time.deltaTime);
                    blinkdetected = true;
                }
            }
        }
        
      
        UpdateCurrentGazeDirection();
        Vector3 vel = CalculateAngularVelocity();

        if (!blinkdetected)
        {
            if ((Mathf.Abs(vel.y) > horizontalSaccadeThres && Mathf.Abs(vel.y) > Mathf.Abs(vel.x)) 
                || (Mathf.Abs(vel.x) > VerticalSaccadeThres && Mathf.Abs(vel.x) > Mathf.Abs(vel.y)) && !saccdetected)
            {
                InduceRot(rotPerSaccade);
                saccdetected = true;
                //eyeData.SetText(eyeData.text + "Sacc detected\n");

            }

        }

        

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


    }

    void InduceRot(float finalRotation)
    {
        if (rdManager.condition == Redirector_condition.AlignmentAPF)
        {
           
            finalRotation *= rdManager.SignTheta;
            rdManager.Env.transform.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, finalRotation);
            // XRTransform.Translate(Vector3.forward * (transFront / 100) * Time.deltaTime);
        

            if(gameManager.debugMode)
                pathTrail.virtualTrail.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, finalRotation);

            inducedRotSaccadic += Mathf.Abs(finalRotation);
        }
        /*else
        {
            if (rdManager.desiredSteeringDirection != 0)
                finalRotation *= rdManager.desiredSteeringDirection;
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
