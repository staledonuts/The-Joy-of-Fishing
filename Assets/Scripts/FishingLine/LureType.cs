using System;
using System.Collections.Generic;
using Ami.BroAudio;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class LureType : MonoBehaviour
{
    [SerializeField] private SoundID _fishHookedInstance;
    [SerializeField] private LureData[] _lureDatas;
    private Dictionary<LureID, LureData> _lureDict = new();


    private LureData _currentLure;
    private SpriteRenderer _spriteRenderer;
    private FishStats _hookedFish;

    public LureID CurretLureID => _currentLure.LureType;


    private void Start()
    {
        this.tag = "Bait";
        if(_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if(_lureDatas == null)
        {
            throw new Exception("You have no luredata assigned to the lure");
        }
        else
        {
            foreach(LureData ld in _lureDatas)
            {
                if(!_lureDict.ContainsKey(ld.LureType))
                {
                    _lureDict.TryAdd(ld.LureType, ld);
                }
            }
        }
        ChangeBait(LureID.Hook);
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Fish")) { return; }
        if(_hookedFish != null) { return; }
        FishStats fish = collision.transform.GetComponent<FishStats>();
        if (fish.LureAttractionType != _currentLure.LureType) { return; }
        AddFishToHook(fish);
    }

    private void AddFishToHook(FishStats fish)
    {
        _hookedFish = fish;

        fish.GetComponent<Collider2D>().enabled = false;
        fish.GetComponent<MoveAi>().enabled = false;
        fish.GetComponent<Pathfinding.AIPath>().enabled = false;
        fish.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        fish.GetComponentInChildren<Animator>().SetBool("Moveing", false);
        fish.transform.position = this.transform.position;
        fish.transform.rotation = Quaternion.Euler(0, 0, 0);
        fish.transform.parent = this.transform;
        PlaySound();
    }
    
    public FishStats GetCurrentCatch()
    {
        return _hookedFish;
    }

    public void DestroyCatch()
    {
        if(_hookedFish != null)
        {
            // Add .gameObject to destroy the entire fish object
            Destroy(_hookedFish.gameObject);
            _hookedFish = null;
        }
    }
    
    public void ChangeBait(LureID lureID)
    {
        _currentLure = _lureDict.GetValueOrDefault(lureID);
        if(_currentLure != null)
        {
            _spriteRenderer.sprite = _currentLure.LureSprite;
        }
    }

    private void PlaySound()
    {
        _fishHookedInstance.Play(transform);
    }

    [Serializable]
    private class LureData
    {
        [SerializeField] private Sprite _lureSprite;
        [SerializeField] private LureID _lureType;
        [SerializeField] private string _lureName;
        public Sprite LureSprite => _lureSprite;
        public LureID LureType => _lureType;
        public string LureName => _lureName;
    }
}

public enum LureID
{
    Hook,
    Guppy,
    Worm,
    TNT,
    
}

