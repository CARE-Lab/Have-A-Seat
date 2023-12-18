using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class RDManager : MonoBehaviour
{
    [Tooltip("Maximum rotation gain applied")]
    [Range(0, 5)]
    public float MAX_ROT_GAIN = 0.49F;

    [Tooltip("Minimum rotation gain applied")]
    [Range(-0.99F, 0)]
    public float MIN_ROT_GAIN = -0.2F;

    [Tooltip("Radius applied by curvature gain")]
    [Range(1, 23)]
    public float CURVATURE_RADIUS = 7.5F;

    [Tooltip("Baseline rotation applied")]
    [Range(0, 1)]
    public float BASELINE_ROT = 0.1F;

    [Tooltip("Threshold Angle in degrees to apply rotational dampening if using the original is unckecked")]
    [Range(0, 160)]
    public int AngleThreshDamp = 45;  // TIMOFEY: 45.0f;

    [Tooltip("Threshold distance within which dampening is applied")]
    [Range(0, 5)]
    public float DistThreshDamp = 1.25F;  // TIMOFEY: 45.0f;

    [Tooltip("Smoothing between rotations per frame")]
    [Range(0, 1)]
    public float SMOOTHING_FACTOR = 0.125f;

    [Tooltip("Use Original dampening method as proposed by razzaque or use the new one by Hodgson")]
    public bool original_dampening = true;

    [Tooltip("The game object that is being physically tracked (probably user's head)")]
    public Transform headTransform;

    public Transform XRTransform;

    public Transform startPos;

    [HideInInspector]
    public Vector3 redirection_target;

    [HideInInspector]
    public GameObject center; // center of the tracking area

    [HideInInspector]
    public Vector3 currPos, prevPos, currDir, prevDir; //cur pos of user w.r.t the OVR rig which is aligned with the (0,0,0)

    [HideInInspector]
    public Vector3 deltaPos;

    [HideInInspector]
    public float deltaDir;

    [SerializeField] GameObject userDirVector;

    private const float S2C_BEARING_ANGLE_THRESHOLD_IN_DEGREE = 160;
    private const float S2C_TEMP_TARGET_DISTANCE = 4;

    private const float MOVEMENT_THRESHOLD = 0.2f; // meters per second
    private const float ROTATION_THRESHOLD = 1.5f; // degrees per second
    private const float CURVATURE_GAIN_CAP_DEGREES_PER_SECOND = 15;  // degrees per second
    private const float ROTATION_GAIN_CAP_DEGREES_PER_SECOND = 30;  // degrees per second

    private bool no_tmptarget = true;
    private Vector3 tmp_target;       // the curr redirection target

    // Auxiliary Parameters
    private float rotationFromCurvatureGain; //Proposed curvature gain based on user speed
    private float rotationFromRotationGain; //Proposed rotation gain based on head's yaw
    private float lastRotationApplied = 0f;

    PathTrail pathTrail;
    GameManager gameManager;

    float sumOfInjectedRotationFromCurvatureGain;
    float sumOfRealDistanceTravelled;
    float sumOfRealRot;
    float sumOfInjectedRotationFromRotationGain;


    private void Start()
    {
        pathTrail = gameObject.GetComponent<PathTrail>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        sumOfInjectedRotationFromCurvatureGain = 0;
        sumOfRealDistanceTravelled = 0;
        sumOfInjectedRotationFromRotationGain = 0;
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        UpdateCurrentUserState();
        CalculateDelta();
        ApplyRedirection();
        UpdatePreviousUserState();

        /*if (gameManager.debug)
        {
            LineRenderer lineRenderer2 = userDirVector.GetComponent<LineRenderer>();
            lineRenderer2.SetPosition(0, currPos);
            lineRenderer2.SetPosition(1, Utilities.FlattenedPos3D(headTransform.TransformPoint(Vector3.forward * 0.5f)));
        }*/
    }

    public void ApplyRedirection()
    {
        S2C_PickRedirectionTarget();

        rotationFromCurvatureGain = 0;

        //float distMag = Mathf.Round(deltaPos.magnitude * 1000f) / 1000f;

        if ((deltaPos.magnitude / Time.deltaTime) > MOVEMENT_THRESHOLD)
        {
            rotationFromCurvatureGain = Mathf.Rad2Deg * (deltaPos.magnitude / CURVATURE_RADIUS);
            rotationFromCurvatureGain = Mathf.Min(rotationFromCurvatureGain, CURVATURE_GAIN_CAP_DEGREES_PER_SECOND * Time.deltaTime);
        }

        //Compute desired facing vector for redirection
        Vector3 desiredFacingDirection = Utilities.FlattenedPos3D(redirection_target) - currPos;
        int signOfAngle = (int)Mathf.Sign(Utilities.GetSignedAngle(currDir, desiredFacingDirection));
        int desiredSteeringDirection = (-1) * signOfAngle;

        //Compute proposed rotation gain
        rotationFromRotationGain = 0;

        if (Mathf.Abs(deltaDir) / Time.deltaTime >= ROTATION_THRESHOLD)
        {
            //Determine if we need to rotate with or against the user
            if (deltaDir * desiredSteeringDirection < 0)
            {
                //Rotating against the user
                //text1.SetText("against");
                rotationFromRotationGain = Mathf.Min(Mathf.Abs(deltaDir * MIN_ROT_GAIN), ROTATION_GAIN_CAP_DEGREES_PER_SECOND * Time.deltaTime);
            }
            else
            {
                //Rotating with the user
                //text1.SetText("with");
                rotationFromRotationGain = Mathf.Min(Mathf.Abs(deltaDir * MAX_ROT_GAIN), ROTATION_GAIN_CAP_DEGREES_PER_SECOND * Time.deltaTime);
            }
        }

        float rotationProposed = desiredSteeringDirection * Mathf.Max(rotationFromRotationGain, rotationFromCurvatureGain);
        bool curvatureGainUsed = rotationFromCurvatureGain > rotationFromRotationGain;
        bool rotationGainUsed = rotationFromCurvatureGain < rotationFromRotationGain;

        //if user is stationary, apply baseline rotation
        if (Mathf.Approximately(rotationProposed, 0))
        {
            rotationProposed = desiredSteeringDirection * BASELINE_ROT * Time.deltaTime;
            curvatureGainUsed = false;
            rotationGainUsed = false;
        }



        //DAMPENING METHODS
        float bearingToTarget = Vector3.Angle(currDir, desiredFacingDirection);
        if (original_dampening)
        {
            // Razzaque et al.
            rotationProposed *= Mathf.Sin(Mathf.Deg2Rad * bearingToTarget);

        }
        else
        {
            // Hodgson et al.
            if (bearingToTarget <= AngleThreshDamp)
                rotationProposed *= Mathf.Sin(Mathf.Deg2Rad * 90 * bearingToTarget / AngleThreshDamp);
        }


        // MAHDI: Linearly scaling the rotation when the distance is near zero
        if (desiredFacingDirection.magnitude <= DistThreshDamp)
        {
            rotationProposed *= desiredFacingDirection.magnitude / DistThreshDamp;
        }

        // Implement additional rotation with smoothing
        float finalRotation = (1.0f - SMOOTHING_FACTOR) * lastRotationApplied + SMOOTHING_FACTOR * rotationProposed;
        lastRotationApplied = finalRotation;

        if (curvatureGainUsed)
        {
            sumOfInjectedRotationFromCurvatureGain += Mathf.Abs(finalRotation);
        }
        else if (rotationGainUsed)
        {
            sumOfInjectedRotationFromRotationGain += Mathf.Abs(finalRotation);
        }

        //text3.SetText("Injected rot so far: " + sumOfInjectedRotationFromRotationGain);


        XRTransform.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, finalRotation);
        center.transform.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, finalRotation);
        pathTrail.realTrail.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, finalRotation);
        gameManager.trackedArea.transform.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, finalRotation);
    }

    public void S2C_PickRedirectionTarget()
    {
        if (center != null)
        {
            Vector3 centerPos = Utilities.FlattenedPos3D(center.transform.position);
            Vector3 userToCenter = centerPos - currPos;
            float bearingToCenter = Vector3.Angle(currDir, userToCenter);
            float signedAngle = Utilities.GetSignedAngle(currDir, userToCenter);

            //text2.SetText("Angle to center: " + bearingToCenter + "\n Signed angle: " + signedAngle);

            if (bearingToCenter >= S2C_BEARING_ANGLE_THRESHOLD_IN_DEGREE)
            {
                if (no_tmptarget)
                {
                    tmp_target = currPos + S2C_TEMP_TARGET_DISTANCE * (Quaternion.Euler(0, (int)Mathf.Sign(signedAngle) * 90, 0) * currDir);
                    no_tmptarget = false;
                }
                redirection_target = tmp_target;
            }
            else
            {
                redirection_target = center.transform.position;
                no_tmptarget = true;
            }

        }
    }

    void UpdateCurrentUserState()
    {
        currPos = Utilities.FlattenedPos3D(headTransform.position);
        currDir = Utilities.FlattenedDir3D(headTransform.forward);
    }

    void UpdatePreviousUserState()
    {
        prevPos = Utilities.FlattenedPos3D(headTransform.position);
        prevDir = Utilities.FlattenedDir3D(headTransform.forward);
    }

    void CalculateDelta()
    {
        deltaPos = currPos - prevPos;
        deltaDir = Utilities.GetSignedAngle(prevDir, currDir);
        float dirMag = Mathf.Round(deltaDir * 100f) / 100f;
        float distMag = Mathf.Round(deltaPos.magnitude * 100f) / 100f;
        sumOfRealDistanceTravelled += distMag;
        sumOfRealRot += dirMag;
    }





}
