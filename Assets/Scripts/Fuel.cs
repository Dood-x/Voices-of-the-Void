using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fuel : MonoBehaviour
{
    public AnimationCurve Fuelmeter;

    public float timer;

    float realtimer;

    float omjer;

    bool IsItempty = false;

    bool Izvrši = false;

    private void Start()
    {
        realtimer = timer;
        Time.timeScale = 1;
    }

    private void Update()
    {
        if(!IsItempty)       
        realtimer -= Time.deltaTime;


        if (realtimer > 0)
        {
            omjer = realtimer / timer;
            transform.localScale = new Vector3(transform.localScale.x, Fuelmeter.Evaluate(omjer), transform.localScale.z);
        }

        else

            IsItempty = true;

        if(IsItempty && !Izvrši)
        {
            Izvrši = true;
            Time.timeScale = 0;
        }



    }

}
