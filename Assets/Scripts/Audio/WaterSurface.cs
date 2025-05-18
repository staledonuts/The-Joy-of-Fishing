using System;
using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using DG.Tweening;
using UnityEngine;

public class WaterSurface : MonoBehaviour
{
    [SerializeField] private bool hookSubmerged = false; //Hook is under the watersurface

    private SoundID splashEvent;
    private BoatEmitter boatEmitter;

    private IAudioPlayer audioPlayer;

    //water ambience MOVE ME TO BETTER PLACE LATER?
    private SoundID lakeAmbienceEvent;

    private void Awake()
    {
        boatEmitter = FindAnyObjectByType<BoatEmitter>();

    }

    private void Update()
    {
        /*if (hookSubmerged)
        {
            lakeAmbienceEvent.("music_duck", 1);
            boatEmitter.UnderWater();
        }
        else
        {
            lakeAmbienceEvent.setParameterByName("music_duck", 0); //not paused
            boatEmitter.AboveWater();
        }*/
    }

    private void OnTriggerEnter2D(Collider2D other) //WaterSurface need to be BaitLayer. This ok?
    {
        if (other.gameObject.CompareTag("Bait"))
        {
            if (other.transform.position.y > gameObject.transform.position.y)
            {
                hookSubmerged = true;
                if(audioPlayer == null)
                {
                    audioPlayer = splashEvent.Play(other.transform);
                }
                else if(!audioPlayer.IsPlaying)
                {
                    audioPlayer = splashEvent.Play(other.transform);
                }
            }
            else if (other.transform.position.y < gameObject.transform.position.y)
            {
                hookSubmerged = false;
            }
        }
    }
}