using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.PoseDetection;
using UnityEngine;

public class APF_Resetter : MonoBehaviour
{

    public bool isResetting;
    private RDManager _rdManager;

    private void Start()
    {
        _rdManager = GetComponent<RDManager>();
    }

    public void InitializeReset()
    {
        //rotate in the direction of the larger angle which is the opposite of the sign of the smaller angle
        var rotateDir = -(int)Mathf.Sign(Utilities.GetSignedAngle(Utilities.UnFlatten(_rdManager.currDir), Utilities.UnFlatten(_rdManager.totalForce)));
    }
}
