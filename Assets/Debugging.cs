using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugging : MonoBehaviour
{
    public GameObject open_env;
    public GameObject close_env;

    bool prev_state_button_one = false;

    // Update is called once per frame
    void Update()
    {
        bool button_one_pressed = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
        if (button_one_pressed != prev_state_button_one)
        {
            if (button_one_pressed)
            {
                open_env.SetActive(!open_env.activeInHierarchy);
                close_env.SetActive(!close_env.activeInHierarchy);

            }
            prev_state_button_one = button_one_pressed;
        }
    }
}
