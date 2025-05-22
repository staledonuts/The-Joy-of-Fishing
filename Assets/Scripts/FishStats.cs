using UnityEngine;

public class FishStats : MonoBehaviour
{
    internal SpriteRenderer sprRend;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private RuntimeAnimatorController _animatorController;
    [SerializeField] private string _fishName = "";
    [Header("The length and weight affects " +
        "excitementLevel, struggleCount, baitLevel")]
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
    public uint Value
    {
        get => _value;
    }

    public int BaitLevel
    {
        get => _baitLevel;
    }
    public float BaitAttractionRadius
    {
        get => _baitAttractionRadius;
    }
    public string FishName
    {
        get => _fishName;
    }

    private void Start()
    {
        sprRend = GetComponentInChildren<SpriteRenderer>();
        SetFishStats();
    }

    private void SetFishStats()
    {
        CheckValue();
        sprRend.sprite = _sprite;
        sprRend.color = _fishColor;
        GetComponentInChildren<Animator>().runtimeAnimatorController = _animatorController;
        GetComponentInChildren<Animator>().SetBool("Moveing", true);
    }

    private void CheckValue()
    {
        var wlValue = _weight + _length;
        //value = (uint)wlValue;
        switch (wlValue)
        {
            case <= 30f:

                _excitementLevel = 1;
                _struggleCount = Random.Range(0, 1);
                _baitLevel = 0;
                break;

            case > 30f when wlValue <= 60f:

                _excitementLevel = 1;
                _struggleCount = Random.Range(2, 3);
                _baitLevel = 1;
                break;

            case > 60f when wlValue <= 90f:

                _excitementLevel = 2;
                _struggleCount = 3;
                _baitLevel = 2;
                break;

            default:
                _excitementLevel = 2;
                _struggleCount = 4;
                _baitLevel = 3;
                break;
        }
    }
}