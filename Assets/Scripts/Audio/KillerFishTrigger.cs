using System;
using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class KillerFishTrigger : MonoBehaviour
{
    private SoundID statueEvent;
    private bool triggered = false;
    private Light2D[] eyeLight;
    private float lightValue;
    private float changePerSecond = 0.1f;
    private bool goingUp = true;
    [SerializeField] private bool done = false;

    private void Awake()
    {
        eyeLight = GetComponentsInChildren<Light2D>();
    }

    private void Update()
    {
        if (triggered && !done) // Gradually change eye light up and down
        {
            foreach (Light2D light in eyeLight)
            {
                if (goingUp)
                {
                    light.intensity += changePerSecond * Time.deltaTime;
                    if (light.intensity > 0.8)
                    {
                        goingUp = false;
                    }
                }
                else
                {
                    light.intensity -= changePerSecond * Time.deltaTime;
                    if (light.intensity < 0)
                    {
                        done = true;
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bait") && !triggered)
        {
            triggered = true;
            statueEvent.Play();
        }
    }
}