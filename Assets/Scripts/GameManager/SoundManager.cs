using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using UnityEngine;


public class SoundManager : MonoBehaviour
{
    private static SoundManager instance = null;

    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Find singleton of this type in the scene
                instance = FindFirstObjectByType<SoundManager>();
                // If there is no singleton object in the scene, we have to add one
                if (instance == null)
                {
                    GameObject obj = new GameObject("SoundManager Singelton");
                    instance = obj.AddComponent<SoundManager>();
                    // The singleton object shouldn't be destroyed when we switch between scenes
                    DontDestroyOnLoad(obj);
                }
            }

            return instance;
        }
    }
    private float musicVolume = 0.5f;
    private float sfxVolume = 0.5f;
    private float masterVolume = 1f;
    private float ambienceVolume = 0.05f;
    // Any sliders connected to these values must on start have the same in insepctor component.
    // Slider component's "Value" setting Must match these float values.

    [SerializeField] private SoundID _musicTracks;
    [SerializeField] private SoundID _radioTrack;
    private IAudioPlayer _musicPlayer;

    private IAudioPlayer _shopRadioAmbience;

    private void Start()
    {
        _musicPlayer = (IAudioPlayer)_musicTracks.Play().AsBGM();
        GameObject shoppeBoatObj = GameObject.Find("ShoppeBoat");
        _shopRadioAmbience = _radioTrack.Play(shoppeBoatObj.transform);
        _shopRadioAmbience.SetVolume(0);
    }

    public void NextMusicTrack()
    {
        _musicPlayer = (IAudioPlayer)_musicTracks.Play().AsBGM();
    }

    public void TransitionToShopMusic(bool shopActive)
    {
        if(shopActive)
        {
            _shopRadioAmbience.TweenVolume(1f, 2f, null, Tween.Easing.EaseInQuad);
            _musicPlayer.TweenVolume(0f, 2f, null, Tween.Easing.EaseOutQuad);
        }
        else
        {
            _musicPlayer.TweenVolume(1f, 2f, null, Tween.Easing.EaseInQuad);
            _shopRadioAmbience.TweenVolume(0f, 2f, null, Tween.Easing.EaseOutQuad);
        }
    }



    private void Awake()
    {
        BroAudio.SetVolume(BroAudioType.All, masterVolume);
        BroAudio.SetVolume(BroAudioType.Music, musicVolume);
        BroAudio.SetVolume(BroAudioType.SFX, sfxVolume);
        BroAudio.SetVolume(BroAudioType.Ambience, ambienceVolume);
    }

    // Update is called once per frame
    private void Update()
    {    
    }

    public void MasterVolumeLevel(float newMasterVolume)
    {
        masterVolume = newMasterVolume;
        BroAudio.SetVolume(BroAudioType.All, masterVolume);
    }

    public void MusicVolumeLevel(float newMusicVolume)
    {
        musicVolume = newMusicVolume;
        BroAudio.SetVolume(BroAudioType.Music, musicVolume);
    }

    public void SfxVolumeLevel(float newSfxVolume)
    {
        sfxVolume = newSfxVolume;
        BroAudio.SetVolume(BroAudioType.SFX, sfxVolume);        
    }

    public void AmbienceVolumeLevel(float newAmbienceVolume)
    {
        ambienceVolume = newAmbienceVolume;
        BroAudio.SetVolume(BroAudioType.Ambience, ambienceVolume);
    }
    

}
