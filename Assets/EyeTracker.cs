using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;


//[RequireComponent(typeof(LineRenderer))]

public class EyeTracker : MonoBehaviour
{
    public float rayDistance = 1.0f;
    public float rayWidth = 0.01f;
    public LayerMask layersToinclude;

    public Color rayColor = Color.red;
    public GameObject rectilePrefab;
    GameObject rectile;

    bool paused = false;
    bool prev_state_pause = false;
    public enum Eye
    {
        Left,
        Right
    }
    public Eye eye;
    public TextMeshProUGUI eyeData;

    TextScroller textScroller;
    

    //private LineRenderer lineRenderer;

    void Start()
    {
        //lineRenderer = GetComponent<LineRenderer>();
        //SetupRay();
        rectile = null;
        textScroller = eyeData.GetComponent<TextScroller>();
      //  InvokeRepeating("UpdateLog", 0.03f, 0.03f);
    }

    /* void SetupRay()
     {
         lineRenderer.useWorldSpace = false; // why?
         lineRenderer.positionCount = 2;
         lineRenderer.startWidth = rayWidth;
         lineRenderer.endWidth = rayWidth;
         lineRenderer.startColor = rayColor;
         lineRenderer.endColor = rayColor;
         lineRenderer.SetPosition(0, transform.position);
         lineRenderer.SetPosition(1, transform.position + new Vector3(0, 0, rayDistance));

     }*/

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

        if (!paused)
        {
            if (eye == Eye.Left)
            {
                eyeData.SetText(eyeData.text + "L: " + transform.rotation.eulerAngles.ToString() + "\n");

            }
            textScroller.scrollDown();
        }
        
        /*   var joyStickDirection = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;
           eyeData.SetText(eyeData.text + joyStickDirection + "\n");*/

    }
    private void UpdateLog()
    {
        if (paused)
        {
            return;
        }

        //string s = eye == Eye.Left ? "L" : "R";
        //eyeData.SetText(eyeData.text + s + ": " + transform.rotation.eulerAngles.ToString() + "\n");
        if (eye == Eye.Left)
        {
            eyeData.SetText(eyeData.text +"L: " + transform.rotation.eulerAngles.ToString() + "\n");
           
        }
        textScroller.scrollDown();
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
