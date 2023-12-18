using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Debugging : MonoBehaviour
{
    public GameObject open_env;
    public GameObject close_env;
    public GameObject debug_UI;

    bool prev_state_button_one = false;
    bool prev_state_button_two = false;

    bool paused = false;
 

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

        bool button_two_pressed = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
        if (button_two_pressed != prev_state_button_two)
        {
            if (button_two_pressed)
            {
                debug_UI.SetActive(!debug_UI.activeInHierarchy);
                Time.timeScale = paused ? 1 : 0;
                paused = !paused;
            }
            prev_state_button_two = button_two_pressed;
        }
    }
}
