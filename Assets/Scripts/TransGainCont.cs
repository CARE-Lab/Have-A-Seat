using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

public class TransGainCont : MonoBehaviour
{
    [Tooltip("The game object that is being physically tracked (probably user's head)")]
    public Transform headTransform;
    public Transform XRTransform;
    public GameObject furn;
    public GameObject Questionnair;
    public GameObject resultUI;
    public TextMeshProUGUI resText;


    bool prev_state_button_one = false;
    float[] max_gains = { 1.2f, 1.25f, 1.3f };
    float[] min_gains = { 0.825f, 0.86f, 0.925f };
    string[] let = { "L_gain", "M_gain", "H_gain" };
    int[,] latin_square_order = { { 1, 2, 0 }, {1,0,2},{0,1,2},{ 0,2,1},{ 2,0,1},{ 2,1,0} };
    int order_index = 0;
    int gain_index = 0;
    float curr_max_gain;
    float curr_min_gain;
    int current_status = 1;
    Vector3 currPos, prevPos; //cur pos of user w.r.t the OVR rig which is aligned with the (0,0,0)
    Vector3 deltaPos;
    bool prev_state_button_two = false;
    ArrayList[]res = new ArrayList[3];
    
    void Start()
    {
        curr_max_gain = max_gains[latin_square_order[order_index,gain_index]];
        curr_min_gain = min_gains[latin_square_order[order_index,gain_index]];
    }

    // Update is called once per frame
    void Update()
    {
        bool button_one_pressed = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
        if (button_one_pressed != prev_state_button_one)
        {
            if (button_one_pressed)
            {
                furn.SetActive(!furn.activeInHierarchy);

            }
            prev_state_button_one = button_one_pressed;
        }


        bool button_two_pressed = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
        if (button_two_pressed != prev_state_button_two)
        {
            if (button_two_pressed)
            {
                current_status++;

            }
            prev_state_button_two = button_two_pressed;
        }

        UpdateCurrentUserState();
        CalculateDelta();

        if (current_status == 4)
        {
            // pop-up question
            Questionnair.SetActive(true);
            gain_index++;
            if(gain_index == 3)
            {
                order_index++;
                gain_index = 0;
            }
            current_status = 1;
        }

        if(order_index == 6)
        {
            string r = "";
            //end of experiment
            for(int i=0; i<res.Length; i++)
            {
                r += let[i]+": ";
                for(int j=0; i < res[i].Count; j++)
                {
                    r+= res[i][j]+" ,";
                }
                r += "\n";
            }
            resultUI.SetActive(true);
            resText.SetText(r);
        }
        else
        {
            curr_max_gain = max_gains[latin_square_order[order_index, gain_index]];
            curr_min_gain = min_gains[latin_square_order[order_index, gain_index]];
        }

        float g_t;
        switch (current_status)
        {
            case 1: g_t = curr_max_gain;
                break;
            case 2:
                g_t = curr_min_gain;
                break;
            case 3:
                g_t = (Mathf.Pow(curr_max_gain, 2)+ Mathf.Pow(curr_min_gain, 2)) / 2;
                break;
            default: g_t = 1;
                break;
        }

        var translation = g_t * deltaPos;
        if (translation.magnitude > 0)
        {
            XRTransform.Translate(translation, Space.World);
        }

        UpdatePreviousUserState();
    }

    public void onSmallerClicked()
    {
        res[latin_square_order[order_index, gain_index]].Add(0);
        Questionnair.SetActive(false);
    }

    public void onLargerClicked()
    {
        res[latin_square_order[order_index, gain_index]].Add(1);
        Questionnair.SetActive(false);
    }

    void UpdateCurrentUserState()
    {
        currPos = Utilities.FlattenedPos3D(headTransform.position);
    }

    void UpdatePreviousUserState()
    {
        prevPos = Utilities.FlattenedPos3D(headTransform.position);
    }

    void CalculateDelta()
    {
        deltaPos = currPos - prevPos;
    }
}
