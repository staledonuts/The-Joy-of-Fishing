using UnityEngine;

public class FishStats : MonoBehaviour
{
    internal SpriteRenderer sprRend;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private RuntimeAnimatorController _animatorController;
    [SerializeField] private string _fishName = "";
    [Header("The length and weight affects " + "excitementLevel, struggleCount, baitLevel")]
    [Tooltip("In kg")]
    [SerializeField] private float _weight;
    [Tooltip("In dm")]
    [SerializeField] private float _length;
    [Tooltip("The amount of money you get when the fish is sold")]
    [SerializeField] private uint _value = 10;
    [SerializeField] private float _baitAttractionRadius = 10f;
    [SerializeField] private Color _fishColor = Color.white;
    [SerializeField] private int _struggleCount;
    [SerializeField] private int _excitementLevel;
    [SerializeField] private int _baitLevel;
    [SerializeField] private LureID _lureAttractionType;
    public float Weight => _weight;
    public float Length => _length;
    public uint Value => _value;
    public LureID LureAttractionType => _lureAttractionType;
    public float BaitAttractionRadius => _baitAttractionRadius;
    public Sprite FishSprite => _sprite;
    public string FishName => _fishName;

    private void Start()
    {
        sprRend = GetComponentInChildren<SpriteRenderer>();
        SetFishStats();
    }

    private void SetFishStats()
    {
        sprRend.sprite = _sprite;
        sprRend.color = _fishColor;
        GetComponentInChildren<Animator>().runtimeAnimatorController = _animatorController;
        GetComponentInChildren<Animator>().SetBool("Moveing", true);
    }
}