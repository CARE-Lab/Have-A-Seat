using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


//[RequireComponent(typeof(LineRenderer))]

public class EyeTracker : MonoBehaviour
{
    public float rayDistance = 1.0f;
    public float rayWidth = 0.01f;
    public LayerMask layersToinclude;

    public Color rayColor = Color.red;
    public GameObject rectilePrefab;
    GameObject rectile;

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
        //InvokeRepeating("UpdateLog", 1f, 1f);
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

    private void UpdateLog()
    {
        string s = eye == Eye.Left ? "L" : "R";
        eyeData.SetText(eyeData.text + s + ": " + transform.rotation.eulerAngles.ToString()+"\n") ;
        textScroller.scrollDown();
    }

    private void FixedUpdate()
    {
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
