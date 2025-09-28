using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using DonutPackage.BTween;
using DonutPackage.EventBus;
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
    private float sfxVolume = 1f;
    private float masterVolume = 1f;
    private float ambienceVolume = 0.75f;
    // Any sliders connected to these values must on start have the same in insepctor component.
    // Slider component's "Value" setting Must match these float values.

    [SerializeField] private SoundID _musicTracks;
    [SerializeField] private SoundID _radioTrack;
    [Tooltip("The transform where the shop radio sound should emanate from.")]
    [SerializeField] private Transform _shopRadioSource;

    private IMusicPlayer _musicPlayer;
    private IAudioPlayer _shopRadioAmbience;

    private void OnEnable()
    {
        EventBus.Subscribe<ShopStateChangedEvent>(HandleShopStateChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ShopStateChangedEvent>(HandleShopStateChanged);
    }

    private void Start()
    {
        _musicPlayer = _musicTracks.Play().AsBGM();
        if (_shopRadioSource != null)
        {
            _shopRadioAmbience = _radioTrack.Play(_shopRadioSource);
            _shopRadioAmbience.SetVolume(0);
        }
        else
        {
            Debug.LogError("Shop Radio Source Transform is not assigned in the SoundManager inspector!", this);
        }
    }

    private void HandleShopStateChanged(ShopStateChangedEvent e)
    {
        TransitionToShopMusic(e.IsShopOpen);
    }

    public void NextMusicTrack()
    {
        _musicPlayer = _musicTracks.Play().AsBGM();
    }

    public void TransitionToShopMusic(bool shopActive)
    {
        if(shopActive)
        {
            _shopRadioAmbience.TweenVolume(1f, 2f, null, BTween.Ease.InQuad);
            (_musicPlayer as IAudioPlayer).TweenVolume(0f, 2f, null, BTween.Ease.OutQuad);
        }
        else
        {
            (_musicPlayer as IAudioPlayer).TweenVolume(1f, 2f, null, BTween.Ease.InQuad);
            _shopRadioAmbience.TweenVolume(0f, 2f, null, BTween.Ease.OutQuad);
        }
    }



    private void Awake()
    {
        BroAudio.SetVolume(BroAudioType.All, masterVolume);
        BroAudio.SetVolume(BroAudioType.Music, musicVolume);
        BroAudio.SetVolume(BroAudioType.SFX, sfxVolume);
        BroAudio.SetVolume(BroAudioType.Ambience, ambienceVolume);
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
