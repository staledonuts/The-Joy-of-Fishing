using System.Collections;
using UnityEngine;
using Ami.BroAudio;
public class AmbientSoundPlayer : MonoBehaviour
{
    [Header("Sound Settings")]
    [Tooltip("The SoundID from your BroAudio Library to play.")]
    [SerializeField] private SoundID soundToPlay;

    [Tooltip("Minimum delay in seconds before the sound plays again.")]
    [SerializeField] private float minDelay = 5.0f;

    [Tooltip("Maximum delay in seconds before the sound plays again.")]
    [SerializeField] private float maxDelay = 15.0f;

    [Header("Playback Control")]
    [Tooltip("Should the ambient sound start playing automatically when the script starts?")]
    [SerializeField] private bool playOnStart = true;
    
    [Tooltip("Should the sound play at the GameObject's position? If false, plays as a 2D sound.")]
    [SerializeField] private bool playAtPosition = true;


    private Coroutine _loopingCoroutine;
    private bool _isCurrentlyLooping = false;

    void Start()
    {
        if (soundToPlay == SoundID.Invalid)
        {
            Debug.LogWarning($"AmbientSoundPlayer on {gameObject.name} has no SoundID assigned. It will not play anything.", this);
            return;
        }

        if (minDelay < 0)
        {
            minDelay = 0;
        }

        if (maxDelay < minDelay)
        {
            maxDelay = minDelay;
        }

        if (playOnStart)
        {
            StartLoopingSound();
        }
    }

    /// <summary>
    /// Starts the looping ambient sound effect. If it's already playing, it will restart.
    /// </summary>
    public void StartLoopingSound()
    {
        if (soundToPlay == SoundID.Invalid) return;

        StopLoopingSound(); // Stop any existing loop before starting a new one

        _isCurrentlyLooping = true;
        _loopingCoroutine = StartCoroutine(PlaySoundWithRandomDelayLoop());
        Debug.Log($"Ambient sound loop started for '{soundToPlay}' on {gameObject.name}.");
    }

    /// <summary>
    /// Stops the looping ambient sound effect.
    /// </summary>
    public void StopLoopingSound()
    {
        _isCurrentlyLooping = false;
        if (_loopingCoroutine != null)
        {
            StopCoroutine(_loopingCoroutine);
            _loopingCoroutine = null;
            Debug.Log($"Ambient sound loop stopped for '{soundToPlay}' on {gameObject.name}.");
        }
    }

    private IEnumerator PlaySoundWithRandomDelayLoop()
    {
        yield return new WaitForSeconds(Random.Range(minDelay, maxDelay)); 

        while (_isCurrentlyLooping)
        {
            if (playAtPosition)
            {
                BroAudio.Play(soundToPlay, transform.position);
            }
            else
            {
                BroAudio.Play(soundToPlay);
            }
            
            float randomDelay = Random.Range(minDelay, maxDelay);
            
            if (randomDelay > 0)
            {
                yield return new WaitForSeconds(randomDelay);
            }
            else
            {
                yield return null;
            }

            if (!_isCurrentlyLooping)
            {
                yield break;
            }
        }
    }

    private void OnDisable()
    {
        StopLoopingSound();
    }

    private void OnDestroy()
    {
        StopLoopingSound();
    }
}
