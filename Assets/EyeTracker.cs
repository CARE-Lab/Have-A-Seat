using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting;


//[RequireComponent(typeof(LineRenderer))]

public class EyeTracker : MonoBehaviour
{
    public float rayDistance = 1.0f;
    public float rayWidth = 0.01f;
    public LayerMask layersToinclude;

    public Color rayColor = Color.red;
    public GameObject rectilePrefab;
    GameObject rectile;
    OVRFaceExpressions userFace;

    bool paused = false;
    bool prev_state_pause = false;

    bool prev_state_clear = false;
    public enum Eye
    {
        Left,
        Right
    }
    public Eye eye;
    public TextMeshProUGUI eyeData;

    TextScroller textScroller;
    

    void Start()
    {
        rectile = null;
        textScroller = eyeData.GetComponent<TextScroller>();
        userFace = GameObject.Find("User").GetComponent<OVRFaceExpressions>();
      //  InvokeRepeating("UpdateLog", 0.03f, 0.03f);
    }

 
    private void Update()
    {
        bool button_pressed = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
        if (button_pressed != prev_state_pause)
        {
            if (button_pressed)
            {
                paused = !paused;

            }
            prev_state_pause = button_pressed;
        }

        bool clear_pressed = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
        if (clear_pressed != prev_state_clear)
        {
            if (clear_pressed)
            {
                eyeData.SetText("");

            }
            prev_state_clear = clear_pressed;
        }

        float eyeClosedL = userFace.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedL);
        float eyeClosedR = userFace.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedR);

        if (!paused)
        {
            if (eye == Eye.Left)
            {
                eyeData.SetText(eyeData.text + "L: " + transform.rotation.eulerAngles.ToString() + "\n");
                //eyeData.SetText(eyeData.text + "L: " + eyeClosedL + ", R: "+ eyeClosedR + "\n");

            }
            textScroller.scrollDown();
        }

        

    }
    

    private void FixedUpdate()
    {

        if (paused)
        {
            Destroy(rectile);
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layersToinclude))
        {
            if(rectile == null)
            {
                rectile = Instantiate(rectilePrefab, hit.point, rectilePrefab.transform.rotation);
            }
            else
            {
                rectile.transform.position = hit.point;
            }
            
        }
    }


}
