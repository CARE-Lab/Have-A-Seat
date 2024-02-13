using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class RDManager : MonoBehaviour
{

    [Tooltip("Translation gain (dilate)")]
    [Range(0, 5)]
    public float MAX_TRANS_GAIN = 0.26F;

    [Tooltip("Translation gain (compress)")]
    [Range(-0.99F, 0)]
    public float MIN_TRANS_GAIN = -0.14F;

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

    [Tooltip("The game object that is being physically tracked (probably user's head)")]
    public Transform headTransform;

    public Transform XRTransform;

    public GameObject ngArrow; // Arrow prefab

    public GameObject VirtualTarget;

    GameObject physicalTarget;
 
    [HideInInspector]
    public Vector3 currPos, prevPos, currDir, prevDir; //cur pos of user w.r.t the OVR rig which is aligned with the (0,0,0)

    [HideInInspector]
    public Vector3 deltaPos;

    [HideInInspector]
    public float deltaDir;

    [HideInInspector]
    public int desiredSteeringDirection;

    [SerializeField] GameObject userDirVector;

    private const float CURVATURE_GAIN_CAP_DEGREES_PER_SECOND = 15;  // degrees per second
    private const float ROTATION_GAIN_CAP_DEGREES_PER_SECOND = 30;  // degrees per second

    PathTrail pathTrail;
    GameManager gameManager;
    GameObject totalForcePointer;//visualization of totalForce

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
            return;

        UpdateCurrentUserState();
        CalculateDelta();
        GetRepulsiveForceAndNegativeGradient(gameManager.trackingSpacePoints, out float rf, out Vector2 ng);
        ApplyRedirectionByNegativeGradient(ng);
        UpdatePreviousUserState();

    }
    public void UpdateTotalForcePointer(Vector2 forceT)
    {

        if (totalForcePointer == null && gameManager.debugMode)
        {
            totalForcePointer = Instantiate(ngArrow);
            totalForcePointer.transform.position = new Vector3(0,0.06f,0);
        }

        if (totalForcePointer != null && totalForcePointer.activeInHierarchy)
        {     
            totalForcePointer.transform.position = currPos + new Vector3(0, 0.06f, 0); ;

            if (forceT.magnitude > 0)
                totalForcePointer.transform.forward = transform.rotation * Utilities.UnFlatten(forceT);
        }

        totalForcePointer.SetActive(gameManager.debugMode);
    }
    public void GetRepulsiveForceAndNegativeGradient(List<GameObject> trackingSpacePoints, out float rf, out Vector2 ng)
    {
        var nearestPosList = new List<Vector2>();
        var currPosReal = Utilities.FlattenedPos2D(currPos);
    
        //physical borders' contributions
        for (int i = 0; i < trackingSpacePoints.Count; i++)
        {
            var p = trackingSpacePoints[i].transform.position;
            var q = trackingSpacePoints[(i + 1) % trackingSpacePoints.Count].transform.position;
            p = Utilities.FlattenedPos2D(p);
            q = Utilities.FlattenedPos2D(q);
            var nearestPos = Utilities.GetNearestPos(currPosReal, new List<Vector2> { p, q });
            nearestPosList.Add(nearestPos);
        }

        rf = 0;
        ng = Vector2.zero;
        foreach (var obPos in nearestPosList)
        {
            rf += 1 / (currPosReal - obPos).magnitude;

            //get gradient contributions
            var gDelta = -Mathf.Pow(Mathf.Pow(currPosReal.x - obPos.x, 2) + Mathf.Pow(currPosReal.y - obPos.y, 2), -3f / 2) * (currPosReal - obPos);
            ng += -gDelta;//negtive gradient
        }
        ng = ng.normalized;
        UpdateTotalForcePointer(ng);

    }

    private Vector2 AttractiveNegtiveGradient()
    {
        var gDelta = 2 * (new Vector2(currPos.x - physicalTarget.transform.position.x, currPos.y - physicalTarget.transform.position.y));
        return -gDelta;//NegtiveGradient
    }

    public void ApplyRedirectionByNegativeGradient(Vector2 ng)
    {
        float g_c = 0;//curvature
        float g_r = 0;//rotation
        float g_t = 0;//translation

        //calculate translation
        if (Vector2.Dot(ng, currDir) < 0)
        {
            g_t = MIN_TRANS_GAIN;
        }

        var maxRotationFromCurvatureGain = CURVATURE_GAIN_CAP_DEGREES_PER_SECOND * Time.deltaTime;
        var maxRotationFromRotationGain = ROTATION_GAIN_CAP_DEGREES_PER_SECOND * Time.deltaTime;

        var desiredFacingDirection = Utilities.UnFlatten(ng);//vector of negtive gradient in physical space
        desiredSteeringDirection = (int)Mathf.Sign(Utilities.GetSignedAngle(currDir, desiredFacingDirection));

        //calculate rotation by curvature gain
        var rotationFromCurvatureGain = Mathf.Rad2Deg * (deltaPos.magnitude / CURVATURE_RADIUS);

        g_c = desiredSteeringDirection * Mathf.Min(rotationFromCurvatureGain, maxRotationFromCurvatureGain);

        
        if (deltaDir * desiredSteeringDirection < 0)
        {//rotate away from negtive gradient
            g_r = desiredSteeringDirection * Mathf.Min(Mathf.Abs(deltaDir * MIN_ROT_GAIN), maxRotationFromRotationGain);
        }
        else
        {//rotate towards negtive gradient
            g_r = desiredSteeringDirection * Mathf.Min(Mathf.Abs(deltaDir * MAX_ROT_GAIN), maxRotationFromRotationGain);
        }

        // Translation Gain
        var translation = g_t * deltaPos;
        if (translation.magnitude > 0)
        {
            XRTransform.Translate(translation, Space.World);
        }

        float finalRotation;
        if (Mathf.Abs(g_r) > Mathf.Abs(g_c))
        {
            // Rotation Gain
            finalRotation = g_r;
            g_c = 0;
        }
        else
        {
            // Curvature Gain
            finalRotation = g_c;
            g_r = 0;
        }

        XRTransform.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, finalRotation);
        //gameManager.trackedArea.transform.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, finalRotation);
        for (int i = 0; i < gameManager.trackingSpacePoints.Count; i++)
        {
            gameManager.trackingSpacePoints[i].transform.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, finalRotation);
        }
        if (gameManager.debugMode)
            pathTrail.realTrail.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, finalRotation);

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
