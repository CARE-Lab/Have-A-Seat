using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.PoseDetection;
using TMPro;
using UnityEngine;

public class APF_Resetter : Resetter
{
    
    public override void InitializeReset()
    {
        var currDir = _rdManager.currDir;
        totalF = _rdManager.totalForce;
        
        //rotate in the direction of the larger angle which is the opposite of the sign of the smaller angle, but we're rotating the env so 
        // reverse again
        rotateDir = Mathf.Sign(Utilities.GetSignedAngle(Utilities.UnFlatten(_rdManager.currDir), Utilities.UnFlatten(totalF)));
        var smallerAngle = Vector2.Angle(totalF, currDir);
        requiredRotateSteerAngle = 360 - smallerAngle;//required rotation angle in real world (the greater angle)
        speedRatio = smallerAngle / requiredRotateSteerAngle;
        setHUD((int)rotateDir);
    }
    
    public override void InjectResetting()
    {        
        var steerRotation = speedRatio * _rdManager.deltaDir;  
        
        var smallerAngle = Vector2.Angle(totalF, _rdManager.currDir);
        
        if (smallerAngle < 5)
        {
            //reset end
            EndReset();
        }
        else
        {//rotate the rotation calculated by ratio
            _rdManager.Env.transform.RotateAround(Utilities.UnFlatten(_rdManager.currPos), Vector3.up, steerRotation);
        
            if (gameManager.debugMode)
                pathTrail.virtualTrail.RotateAround(Utilities.UnFlatten(_rdManager.currPos), Vector3.up, steerRotation);
        }
    }
    
}
