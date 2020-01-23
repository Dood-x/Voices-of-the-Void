using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnobLights : MonoBehaviour
{
    public Controller controller;

    public GameObject left;
    public GameObject right;
    public GameObject buttonLight;

    private void Update()
    {
        if(controller.selectedKnob == Controller.Knob.Confirm)
        {
            left.SetActive(false);
            right.SetActive(false);
            buttonLight.SetActive(true);
        }

        else if(controller.selectedKnob == Controller.Knob.Vertical)
        {
            left.SetActive(true);
            right.SetActive(false);
            buttonLight.SetActive(false);
        }

        else if (controller.selectedKnob == Controller.Knob.Horizontal)
        {
            left.SetActive(false);
            right.SetActive(true);
            buttonLight.SetActive(false);
        }

    }
}
