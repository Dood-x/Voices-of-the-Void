using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fuel : MonoBehaviour
{
    public AnimationCurve Fuelmeter;

   

    public float omjer;

   

   

    private void Update()
    {
             
            transform.localScale = new Vector3(transform.localScale.x, Fuelmeter.Evaluate(omjer), transform.localScale.z);
  
    }

}
