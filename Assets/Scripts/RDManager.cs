using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using Unity.VisualScripting;
using UnityEngine.Serialization;

public enum Redirector_condition
{
    APF_only = 0,
    APF_Saccadic =1,
    Saccadic_only =2
}
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
    
    public Transform VirtualTarget;

    public GameObject Env;

    public Redirector_condition condition;
 
    [HideInInspector]
    public Vector2 currPos, prevPos, currDir, prevDir; //cur pos of user w.r.t the OVR rig which is aligned with the (0,0,0)

    [HideInInspector]
    public Vector2 deltaPos;

    [HideInInspector]
    public float deltaDir;

    [HideInInspector]
    public int desiredSteeringDirection;
    
    [HideInInspector]
    public Vector2 totalForce; 
    
    [SerializeField] GameObject userDirVector;
    [SerializeField] GameObject ngArrow; // Arrow prefab
    
    private const float CURVATURE_GAIN_CAP_DEGREES_PER_SECOND = 15;  // degrees per second
    private const float ROTATION_GAIN_CAP_DEGREES_PER_SECOND = 30;  // degrees per second

    PathTrail pathTrail;
    GameManager gameManager;
    APF_Resetter ApfResetter;
    private SaveData logger;
    GameObject totalForcePointer;//visualization of totalForce
    
    float sumOfRealDistanceTravelled;
    int resetsPerTrial;
    
    bool alignmentState = false;//alignmentState == true: only use attractive force，alignmentState == false: only use repulsive force
    bool inReset;
    bool ifJustEndReset = false;//if just finishes reset, if true, execute redirection once then judge if reset, Prevent infinite loops

    public TextMeshProUGUI Text1;
    public TextMeshProUGUI Text2;
    public TextMeshProUGUI Text3;
    
    private void Start()
    {
        pathTrail = gameObject.GetComponent<PathTrail>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        ApfResetter = GetComponent<APF_Resetter>();
        logger = GetComponent<SaveData>();
        
    }

    void StartTrial()
    {
        sumOfRealDistanceTravelled = 0;
        resetsPerTrial = 0;
        logger.StartCondition(condition.ToString());
    }
    

    void Update()
    {
        if (Time.timeScale == 0 || !gameManager.ready || condition == Redirector_condition.Saccadic_only)
            return;

        UpdateCurrentUserState();
        CalculateDelta();
        GetNegativeGradient(out float rf, out Vector2 ng, out bool collisionhappens);
        if (collisionhappens && !inReset && !ifJustEndReset)
        {
            ApfResetter.InitializeReset();
            inReset = true;
            resetsPerTrial += 1;
        }
        if(inReset)
            ApfResetter.InjectResetting();
        else
        {
            ApplyRedirectionByNegativeGradient(ng);
            ifJustEndReset = false;
        }
            
        
        UpdatePreviousUserState();
    }
    
    public void UpdateTotalForcePointer(Vector2 forceT)
    {

        //record this new force
        totalForce = forceT;
        
        if (!totalForcePointer && gameManager.debugMode)
        {
            totalForcePointer = Instantiate(ngArrow);
            totalForcePointer.transform.position = new Vector3(0,0.06f,0);

            Vector3 newRot = Utilities.UnFlatten(forceT);
            Quaternion currentQ = new Quaternion();
            currentQ.eulerAngles = newRot;
            totalForcePointer.transform.localRotation = currentQ;
        }

        //totalForcePointer.SetActive(gameManager.debugMode);

        if (totalForcePointer != null && totalForcePointer.activeInHierarchy)
        {     
            totalForcePointer.transform.position = Utilities.UnFlatten(currPos)+ new Vector3(0, 0.06f, 0); ;

            if (forceT.magnitude > 0)
                totalForcePointer.transform.forward = transform.rotation * Utilities.UnFlatten(forceT);
        }
    }
    public void GetNegativeGradient(out float rf, out Vector2 ng, out bool collisionhappens)
    {
        var nearestPosList = new List<Vector2>();
        var currPosReal = Utilities.UnFlatten(currPos);
        
        //physical borders' contributions
        var walls =MRUK.Instance.GetCurrentRoom().WallAnchors;
        foreach (var wall in walls)
        {
            Vector3 nearestPoint;
            wall.GetClosestSurfacePosition(currPosReal, out nearestPoint);
            Vector2 nearestPoint_2d = Utilities.FlattenedPos2D(nearestPoint);
            nearestPosList.Add(nearestPoint_2d);
            
        }
        
        IfChangeAlignmentState();
        rf = 0; // currently not used
        ng = Vector2.zero;
        
        ng = RepulsiveNegativeGradient(nearestPosList, currPos) + AttractiveNegativeGradient(currPos);
        //ng = RepulsiveNegativeGradient(nearestPosList, currPosReal_2d);
        ng = ng.normalized;
        UpdateTotalForcePointer(ng);

        collisionhappens = IfCollisionHappens(nearestPosList, currPos);
    }

    private Vector2 RepulsiveNegativeGradient(List<Vector2> nearestPosList, Vector2 currPosReal)
    {
        float rf = 0; //total force
        var ng = Vector2.zero;
        foreach (var obPos in nearestPosList)
        {
            rf += 1 / (currPosReal - obPos).magnitude;

            //get gradient contributions
            var gDelta = -Mathf.Pow(Mathf.Pow(currPosReal.x - obPos.x, 2) + Mathf.Pow(currPosReal.y - obPos.y, 2), -3f / 2) * (currPosReal - obPos);
            ng += -gDelta;//negtive gradient
        }
        return ng;
    }

    private Vector2 AttractiveNegativeGradient(Vector2 currPosReal)
    {
        var physicalTargetPos = Utilities.FlattenedPos2D(gameManager.physicalChair.transform.position);
        var gDelta = 2 * (new Vector2(currPosReal.x - physicalTargetPos.x, currPosReal.y -physicalTargetPos.y));
        return -gDelta;//NegtiveGradient
    }
    
    public void IfChangeAlignmentState(){
        if (alignmentState)
            return;

        //position and direction in physical tracking space
        var objVirtualPos = Utilities.FlattenedPos2D(VirtualTarget.transform.position);
        var objPhysicalPos = Utilities.FlattenedPos2D(gameManager.physicalChair.transform.position);

        //the virtual distance from the user to the alignment target
        var Dv = (objVirtualPos - currPos).magnitude;

        //the physical distance from the user to the alignment target
        var Dp = (objPhysicalPos -currPos).magnitude;

        var gt = MIN_TRANS_GAIN + 1;
        var Gt = MAX_TRANS_GAIN + 1;
        //the physical rotational oﬀset
        var phiP = Vector2.Angle(currDir, objPhysicalPos - currPos) * Mathf.Deg2Rad;
        if (gt * Dp < Dv && Dv < Gt * Dp) {
            if (phiP < Mathf.Asin((Dp * 1 / CURVATURE_RADIUS) / 2))
            {
                alignmentState = true;
            }
        }
    }

    public void ApplyRedirectionByNegativeGradient(Vector2 ng)
    {
        float g_c = 0;//curvature
        float g_r = 0;//rotation
        float g_t = 0;//translation

        var physical_target = Utilities.FlattenedPos2D(gameManager.physicalChair.transform.position);
        var dist_to_physical_target = Vector2.Distance(currPos, physical_target);
        var dist_to_virtual_target = Vector2.Distance(currPos,
            Utilities.FlattenedPos2D(VirtualTarget.position));
        
        //calculate translation Gain
        var dotProduct = Vector2.Dot(ng, currDir);
        if (dotProduct < -0.3)
        {
            g_t = MAX_TRANS_GAIN;
        }else if (dist_to_virtual_target > dist_to_physical_target)
            g_t = MIN_TRANS_GAIN;
        
        /*Text2.SetText(g_t.ToString());
        Text1.SetText(dotProduct.ToString());*/
        
        
        var maxRotationFromCurvatureGain = CURVATURE_GAIN_CAP_DEGREES_PER_SECOND * Time.deltaTime;
        var maxRotationFromRotationGain = ROTATION_GAIN_CAP_DEGREES_PER_SECOND * Time.deltaTime;

        var desiredFacingDirection = Utilities.UnFlatten(ng);//vector of negtive gradient in physical space
        desiredSteeringDirection = (int)Mathf.Sign(Utilities.GetSignedAngle(Utilities.UnFlatten(currDir), desiredFacingDirection));

        //calculate rotation by curvature gain
        var rotationFromCurvatureGain = Mathf.Rad2Deg * (deltaPos.magnitude / CURVATURE_RADIUS);

        g_c = desiredSteeringDirection * Mathf.Min(rotationFromCurvatureGain, maxRotationFromCurvatureGain);

        
        if (deltaDir * desiredSteeringDirection < 0)
        {//rotate away from negtive gradient, so we make them rotate more in VE and less in PE
            g_r = desiredSteeringDirection * Mathf.Min(Mathf.Abs(deltaDir * MAX_ROT_GAIN), maxRotationFromRotationGain);
        }
        else
        {//rotate towards negtive gradient, so we make them rotate more in PE and less in VE
            g_r = desiredSteeringDirection * Mathf.Min(Mathf.Abs(deltaDir * MIN_ROT_GAIN), maxRotationFromRotationGain);
        }

        // Translation Gain
        var translation = Utilities.UnFlatten(g_t * deltaPos);
        if (translation.magnitude > 0)
        {
            Env.transform.Translate(-1*translation, Space.World);
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
        Env.transform.RotateAround(Utilities.UnFlatten(currPos), Vector3.up, finalRotation);
        
        if (gameManager.debugMode)
            pathTrail.virtualTrail.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, finalRotation);
        
    }

    bool IfCollisionHappens(List<Vector2> nearestPosList, Vector2 currPosReal)
    {
        float minDist = Single.MaxValue;
        
        foreach (var obsPos in nearestPosList)
        {
            var dist = Vector2.Distance(obsPos, currPosReal);
            if (dist < minDist)
                minDist = dist;
        }
        
        var dotProduct = Vector2.Dot(totalForce, currDir);
        
        Text1.SetText(minDist.ToString());
        Text2.SetText(dotProduct.ToString());
        
        return dotProduct < -0.3 && minDist < 0.3;
    }

    public void OnResetEnd()
    {
        inReset = false;
        ifJustEndReset = true;
    }

    /*private void OnDrawGizmos()
    {
        #if UNITY_EDITOR
        if (gameManager.ready)
        {
            Handles.color = Color.cyan;
            var currPosReal = Utilities.FlattenedPos3D(currPos);
            var walls =MRUK.Instance.GetCurrentRoom().WallAnchors;
            foreach (var wall in walls)
            {
                Vector3 nearestPoint;
                wall.GetClosestSurfacePosition(currPosReal, out nearestPoint);
                nearestPoint = Utilities.FlattenedPos3D(nearestPoint);
                Handles.DrawLine(currPosReal, nearestPoint);
            }

        }
        #endif
        
    }*/


    void UpdateCurrentUserState()
    {
        currPos = Utilities.FlattenedPos2D(headTransform.position);
        currDir = Utilities.FlattenedDir2D(headTransform.forward);
        
    }

    void UpdatePreviousUserState()
    {
        prevPos = Utilities.FlattenedPos2D(headTransform.position);
        prevDir = Utilities.FlattenedDir2D(headTransform.forward);
    }

    void CalculateDelta()
    {
        deltaPos = currPos - prevPos;
        deltaDir = Utilities.GetSignedAngle(Utilities.UnFlatten(prevDir), Utilities.UnFlatten(currDir));
        float dirMag = Mathf.Round(deltaDir * 100f) / 100f;
        float distMag = Mathf.Round(deltaPos.magnitude * 100f) / 100f;
        sumOfRealDistanceTravelled += distMag;
    }





}
