using System;
using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using UnityEngine;

public class BoatEmitter : MonoBehaviour
{
    private SoundID inst;

    private SoundID instUnderwater;

    private IAudioPlayer player;

    private void Awake()
    {
        player = inst.Play(transform);
    }

    private void Update()
    {
        
    }

    public void UnderWater()
    {
        player.Stop();
        player = instUnderwater.Play();
    }

    public void AboveWater()
    {
        player.Stop();
        player = inst.Play();
    }
}