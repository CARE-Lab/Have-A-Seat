using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.PoseDetection;
using TMPro;
using UnityEngine;

public class Alignment_Resetter : MonoBehaviour
{
    [SerializeField] private GameObject HUD;
    [SerializeField] private TextMeshProUGUI HUD_text;
    [SerializeField] private Transform VirtualRoomCenter;
    
    RDManager _rdManager;
    PathTrail pathTrail;
    GameManager gameManager;
    float rotateDir; //rotation direction, positive if rotate clockwise
    float speedRatio;
    private Vector2 totalF;
    private float requiredRotateSteerAngle;

    private void Start()
    {
        _rdManager = GetComponent<RDManager>();
        pathTrail = gameObject.GetComponent<PathTrail>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    public void InitializeReset()
    {
        var currDir = Utilities.UnFlatten(_rdManager.currDir);
        totalF = _rdManager.totalForce;
        Vector3 roomCenterDir = VirtualRoomCenter.position - Utilities.UnFlatten(_rdManager.currPos);
        
        //rotate in the direction of the larger angle which is the opposite of the sign of the smaller angle
        rotateDir = -(int)Mathf.Sign(Utilities.GetSignedAngle(currDir, Utilities.UnFlatten(totalF)));
        var smallerAngle = Vector2.Angle(totalF, currDir);
        requiredRotateSteerAngle = 360 - smallerAngle;//required rotation angle in real world (the greater angle)
        speedRatio = smallerAngle / requiredRotateSteerAngle;
        setHUD((int)rotateDir);
    }

    void setHUD(int rotateDir)
    {
        HUD.SetActive(true);
        if(rotateDir > 0)
            HUD_text.SetText("Spin slowly in Place till this massage disappears\n ->>");
        else
            HUD_text.SetText("Spin slowly in Place till this massage disappears\n <<-");
        
    }
    
    public void InjectResetting()
    {        
        var steerRotation = speedRatio * _rdManager.deltaDir;  
        
        var smallerAngle = Vector2.Angle(totalF, _rdManager.currDir);
        
        if (Mathf.Abs(requiredRotateSteerAngle) <= Mathf.Abs(steerRotation) || smallerAngle == 0)
        {//meet the rotation requirement
            _rdManager.Env.transform.RotateAround(Utilities.UnFlatten(_rdManager.currPos), Vector3.up, requiredRotateSteerAngle);
        
            if (gameManager.debugMode)
                pathTrail.virtualTrail.RotateAround(Utilities.UnFlatten(_rdManager.currPos), Vector3.up, requiredRotateSteerAngle);

            //reset end
            EndReset();
        }
        else
        {//rotate the rotation calculated by ratio
            _rdManager.Env.transform.RotateAround(Utilities.UnFlatten(_rdManager.currPos), Vector3.up, steerRotation);
        
            if (gameManager.debugMode)
                pathTrail.virtualTrail.RotateAround(Utilities.UnFlatten(_rdManager.currPos), Vector3.up, steerRotation);
            requiredRotateSteerAngle -= Mathf.Abs(steerRotation);            
        }
    }
    
    void EndReset()
    {
        HUD.SetActive(false);
        requiredRotateSteerAngle = 0;
        _rdManager.OnResetEnd();
    }
}
