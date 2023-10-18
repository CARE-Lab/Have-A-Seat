using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Oculus.Interaction;

public class TextScroller : MonoBehaviour
{
    RectTransform m_RectTransform;
    float speed = 5f;

    void Start()
    {
        m_RectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // this is either 1 for up or -1 for down
        var joyStickDirection = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;
        if (joyStickDirection != 0)
        {
            var multiplier = joyStickDirection * speed;

            // You want to invert the direction since scrolling down actually means
            // moving the content up
            m_RectTransform.position -= Vector3.up * multiplier * Time.deltaTime;
        }
    }

    public void scrollDown()
    {
        if(m_RectTransform.sizeDelta.y > -0.0264f) {
            m_RectTransform.position += new Vector3(0, 0.34f, 0);
        }
    }
}
