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
    
    public TextMeshProUGUI Text1;
    public TextMeshProUGUI Text2;
    public TextMeshProUGUI Text3;
    
    public override void InitializeReset()
    {
        var currDir = _rdManager.currDir;
        var currPos = _rdManager.currPos;
        totalF = _rdManager.totalForce;
        
        var APFangle = Vector2.Angle(currDir, totalF); //smaller angle
       
        
        //var virtualResetDir = FindVirtualResetDir(currDir, currPos);

        // new resetter option1
        //rotate in the direction of the larger angle which is the opposite of the sign of the smaller angle
        /*int rotateDir = -(int)Mathf.Sign(Utilities.GetSignedAngle(currDir, Utilities.UnFlatten(virtualResetDir)));
        var beta = Vector2.Angle(currDir, virtualResetDir); //required angle to rotate in virtual env (not 360 anymore)
        
        
        speedRatio = Mathf.Abs((360-beta-APFangle) / APFangle); */
        
        
        //option 2
        var Vfor = _rdManager.virtual_for;
        var Pfor = _rdManager.physical_for;
        int signAPF = (int)Mathf.Sign(Vector2.SignedAngle(currDir, totalF));
        
        int rotateDir = (int)Mathf.Sign(Utilities.GetSignedAngle(Vfor, Pfor)); // rotate in the direction of alpha
        
        if (rotateDir * signAPF < 0)
            requiredRotateSteerAngle = 360 - APFangle;//required rotation angle in real world (the greater angle)
        else
            requiredRotateSteerAngle = APFangle;
        
        
        speedRatio = Vector3.Angle(Vfor, Pfor) / requiredRotateSteerAngle;
        
        Text1.SetText($"{speedRatio}");
        
        setHUD((int)rotateDir);
    }

    private Vector2 FindVirtualResetDir(Vector2 userDir, Vector2 userPos)
    {
        var currDir = Utilities.UnFlatten(userDir);
        var currPos = Utilities.UnFlatten(userPos, 1f);
        
        var label_list = new List<String> { "WALL_FACE"};
        
        totalF = Utilities.UnFlatten(_rdManager.totalForce);
        var APFray = new Ray(currPos, totalF);
        MRUK.Instance.GetCurrentRoom()
            .Raycast(APFray, 100f, LabelFilter.Included(label_list), out RaycastHit hit);
        //Text2.SetText($"ph-dist: {hit.distance}");
        
        Vector3 finalResetDir = currDir;
        float minDistDiff = 100000f;
        
        for (int angle = 0; angle < 360; angle += 45)
        {
            var dir = Quaternion.Euler(0,angle,0) * Vector3.forward;
            Ray r = new Ray(currPos, dir);
            if (Physics.Raycast(r, out RaycastHit hit_v, 100f, 1 << 8))
            {

                var diff = Math.Abs(hit_v.distance - hit.distance);
                if (diff < minDistDiff)
                {
                    minDistDiff = diff;
                    finalResetDir = dir;
                    /*Text1.SetText($"direction angle: {angle}");
                    Text3.SetText($"Virtual_dist: {hit_v.distance}, {hit_v.transform.gameObject.name}");*/
                }
            }
        }
        
        GameObject VirtualDirPointer = Instantiate(ResetDir);
        VirtualDirPointer.transform.position = currPos - new Vector3(0,0.1f,0);

        Vector3 newRot = Utilities.FlattenedDir3D(finalResetDir);
        Quaternion currentQ = new Quaternion();
        currentQ.eulerAngles = newRot;
        VirtualDirPointer.transform.localRotation = currentQ;
        Destroy(VirtualDirPointer, 5f);

        return Utilities.FlattenedDir2D(finalResetDir);
    }

    
    public override void InjectResetting()
    {        
        var steerRotation = speedRatio * _rdManager.deltaDir;  
        
        var smallerAngle = Vector2.Angle(totalF, _rdManager.currDir);
        
        if ( smallerAngle < 5)
        {//meet the rotation requirement
            /*_rdManager.Env.transform.RotateAround(Utilities.UnFlatten(_rdManager.currPos), Vector3.up, requiredRotateSteerAngle);
            
            if (gameManager.debugMode)
                pathTrail.virtualTrail.RotateAround(Utilities.UnFlatten(_rdManager.currPos), Vector3.up, requiredRotateSteerAngle);
                */

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
