using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction.PoseDetection;
using TMPro;
using UnityEngine;

public class Alignment_Resetter : Resetter
{
    [SerializeField] private GameObject ResetDir;
    
    
    public override void InitializeReset()
    {
        var currDir = _rdManager.currDir;
        var currPos = _rdManager.currPos;
        var virtualResetDir = FindVirtualResetDir(currDir, currPos);

        //rotate in the direction of the larger angle which is the opposite of the sign of the smaller angle
        rotateDir = -(int)Mathf.Sign(Utilities.GetSignedAngle(currDir, Utilities.UnFlatten(totalF)));
        
        var theta = Vector2.Angle(totalF, currDir); //smaller angle
        var beta = Vector2.Angle(currDir, virtualResetDir); //required angle to rotate in virtual env (not 360 anymore)
        
        requiredRotateSteerAngle = 360 - theta;//required rotation angle in real world (the greater angle)
        
        speedRatio = Mathf.Abs((beta + theta - 360) / requiredRotateSteerAngle); //new resetter option1

        /*rotateDir = (int)Mathf.Sign(_rdManager.AlphaSignedAngle); //option 2
        speedRatio = (Mathf.Abs(_rdManager.AlphaSignedAngle) + theta - 360) / 360 - theta;*/
        
        setHUD((int)rotateDir);
    }

    private Vector2 FindVirtualResetDir(Vector2 userDir, Vector2 userPos)
    {
        var currDir = Utilities.UnFlatten(userDir);
        var currPos = Utilities.UnFlatten(userPos, 0.2f);
        
        var label_list = new List<String> { "WALL_FACE"};
        
        totalF = Utilities.UnFlatten(_rdManager.totalForce);
        var APFray = new Ray(currPos, totalF);
        float APFDist;
        MRUK.Instance.GetCurrentRoom()
            .Raycast(APFray, 1000f, LabelFilter.Included(label_list), out RaycastHit hit);
        
        Vector3 finalResetDir = currDir;
        float minDistDiff = 100000f;
        
        for (int angle = 0; angle < 360; angle += 45)
        {
            var dir = Quaternion.Euler(0,angle,0) * Vector3.forward;
            Ray r = new Ray(currPos, dir);
            if (Physics.Raycast(r, out RaycastHit hit_v, 1000f, 1 << 8))
            {

                var diff = Math.Abs(hit_v.distance - hit.distance);
                if (diff < minDistDiff)
                {
                    minDistDiff = diff;
                    finalResetDir = dir;
                }
            }
        }
        
        GameObject VirtualDirPointer = Instantiate(ResetDir);
        VirtualDirPointer.transform.position = currPos - new Vector3(0,0.1f,0);

        Vector3 newRot = Utilities.FlattenedDir3D(finalResetDir);
        Quaternion currentQ = new Quaternion();
        currentQ.eulerAngles = newRot;
        VirtualDirPointer.transform.localRotation = currentQ;

        return Utilities.FlattenedDir2D(finalResetDir);
    }

    
    public override void InjectResetting()
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
    
}
