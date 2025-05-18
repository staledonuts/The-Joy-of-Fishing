using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using UnityEngine;

public class ScaleSound : MonoBehaviour
{
    private SoundID inst;
    private bool triggered;


    void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.tag == "Bait" && !triggered)
        {
            
            inst.Play(transform);
            triggered = true;
        }
    }
}
