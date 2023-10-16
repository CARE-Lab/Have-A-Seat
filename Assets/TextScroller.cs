using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Oculus.Interaction;

public class TextScroller : MonoBehaviour
{
    RectTransform m_RectTransform;

    void Start()
    {
        m_RectTransform = GetComponent<RectTransform>();
    }

    public void scrollDown()
    {
        if(m_RectTransform.sizeDelta.y > -0.0264f) {
            Vector3 pos = m_RectTransform.position;
            pos.y = pos.y + 0.34f;
            m_RectTransform.position = pos;
        }
    }
}
