using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using DonutPackage.EventBus;
using UnityEngine;

public class bffEventTrigger : MonoBehaviour
{

    public Animator bffanim;
    [SerializeField] private SoundID inst;

    private bool triggered;

    void Awake()
    {
    }


    void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.tag == "Bait" && !triggered)
        {
            inst.Play();
            Invoke("endgame", 5.5f);
            triggered = true;
        }
    }

    public void endgame()
    {
        EventBus.Publish(new FadeOutScreenEvent());
    }
}
