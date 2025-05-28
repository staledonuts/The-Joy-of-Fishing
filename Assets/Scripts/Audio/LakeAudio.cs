using Ami.BroAudio;
using UnityEngine;

public class LakeAudio : MonoBehaviour
{
    [Header("Sound Settings")]
    [Tooltip("The SoundID from your BroAudio Library to play.")]
    [SerializeField] private SoundID soundToPlay;

    private IAudioPlayer _player;

    private void Start()
    {
        if (soundToPlay == SoundID.Invalid)
        {
            return;
        }

        _player = BroAudio.Play(soundToPlay);
    }

    private void OnDisable()
    {
        _player.Stop();
    }
}