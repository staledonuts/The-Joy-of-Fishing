using System;
using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIManager : MonoBehaviour
{
    private static UIManager instance = null;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Find singleton of this type in the scene
                instance = FindFirstObjectByType<UIManager>();

                // If there is no singleton object in the scene, create one
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("UIManager_Singleton");
                    instance = singletonObject.AddComponent<UIManager>();
                }
            }
            return instance;
        }
    }
    private readonly Vector2 SHOPPANELOFFPOS = new Vector2(-1000f, 0f);
    private readonly Vector2 SHOPPANELONPOS = new Vector2(-0f, 0f);
    private readonly Color FADEOUTCOLOR = new(0,0,0,1);
    private readonly Color FADEINCOLOR = new(0,0,0,0);
    private const float UITWEENSPEED = 0.8f;
    [SerializeField] private GameObject _pauseCanvas;
    [SerializeField] private GameObject _callShopCanvas;
    [SerializeField] private RectTransform _shopPanel;
    [SerializeField] private GameObject _goFishCanvas;

    [SerializeField] private SoundID _buySfx;
    [SerializeField] private Image _GameStartLogo;
    [SerializeField] private Button _mindcontrol; 
    [SerializeField] private Button _lvl1Line; 
    [SerializeField] private Button _lvl2Line; 
    [SerializeField] private Button _lvl3Line;
    [SerializeField] private uint _mindControlCost; 
    [SerializeField] private uint _lvl1Cost; 
    [SerializeField] private uint _lvl2Cost; 
    [SerializeField] private uint _lvl3Cost;
    [SerializeField] private uint _lvl1Length; 
    [SerializeField] private uint _lvl2Length;
    [SerializeField] private uint _lvl3Lenght;

    private void Awake()
    {   
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            //Debug.LogWarning("Another instance of UIManager found, destroying this new one.");
            Destroy(gameObject);
        }
        _fadeimage.color = FADEOUTCOLOR;
    }

    void Start()
    {
        StartGameFadeIN();
        _shopPanel.TweenAnchoredPosition(SHOPPANELOFFPOS, 0.2f, () => { Debug.Log("Panel move complete!"); }, Tween.Easing.EaseOutQuad);
    }

    public void BuyMindControlLure()
    {
        if (Inventory.Instance.SpendMoney(_mindControlCost))
        {
            _buySfx.Play();
            _mindcontrol.interactable = false;
            Inventory.Instance.playerData.RadioControlLure = true;
        }
        else
        {
            Debug.Log("not enough money!");
        }
    }

    public void BuyLineLVL1()
    {
        if (Inventory.Instance.UpgradeLineLength(_lvl1Length, _lvl1Cost))
        {
            _buySfx.Play();
            _lvl1Line.interactable = false;
        }
        else
        {
            Debug.Log("not enough money!");
        }
    }

    public void BuyLineLVL2()
    {
        if (Inventory.Instance.UpgradeLineLength(_lvl2Length, _lvl2Cost))
        {
            _buySfx.Play();
            _lvl2Line.interactable = false;
        }
        else
        {
            Debug.Log("not enough money!");
        }
    }

    public void BuyLineLVL3()
    {
        if (Inventory.Instance.UpgradeLineLength(_lvl3Lenght, _lvl3Cost))
        {
            _buySfx.Play();
            _lvl3Line.interactable = false;
        }
        else
        {
            Debug.Log("not enough money!");
        }
    }

    public void TweenLogo()
    {
        if(_GameStartLogo != null)
        {
            if(_GameStartLogo.material.GetInstanceID() == _GameStartLogo.materialForRendering.GetInstanceID())
            {
                _GameStartLogo.material = Instantiate(_GameStartLogo.material);
            }
            _GameStartLogo.material.TweenMaterialFloat("_Dissolve", 0f, 2f, () => { _fadeCanvas.SetActive(false); }, Tween.Easing.EaseInOutQuad);
        }
    }
    private bool _shopbool = false;
    private bool OnShopSwitch()
    {
        if (!_shopbool)
        {
            _shopbool = true;
            _shopPanel.TweenAnchoredPosition(SHOPPANELONPOS, UITWEENSPEED, () => { Debug.Log("Panel move complete!"); }, Tween.Easing.EaseOutCirc);
            SoundManager.Instance.TransitionToShopMusic(true);
        }
        else
        {
            _shopbool = false;
            _shopPanel.TweenAnchoredPosition(SHOPPANELOFFPOS, UITWEENSPEED, () => { Debug.Log("Panel move complete!"); }, Tween.Easing.EaseOutCirc);
            SoundManager.Instance.TransitionToShopMusic(false);
        }
        return _shopbool;
    }

    public void CallShop()
    {            
        if(OnShopSwitch())
        {

            SoundManager.Instance.TransitionToShopMusic(true);
            GameManager.Instance.CameraSwitcher(CameraModes.Shop);
        }
        else
        {
            SoundManager.Instance.TransitionToShopMusic(false);
            GameManager.Instance.CameraSwitcher(CameraModes.Hook);
        }
    }

    [SerializeField] private Image _fadeimage;
    [SerializeField] private GameObject _fadeCanvas;
    

    public void UIScreenfadeout() 
    {
        if(!_fadeCanvas.activeSelf)
        {
            _fadeCanvas.SetActive(true);
        }
        _fadeimage.TweenImageColor(FADEOUTCOLOR, UITWEENSPEED, null, Tween.Easing.EaseInOutQuad);
    }
    public void UIScreenfadein() 
    {
        _fadeimage.TweenImageColor(FADEINCOLOR, UITWEENSPEED, () => _fadeCanvas.SetActive(false), Tween.Easing.EaseInQuad);
    }

    public void StartGameFadeIN()
    {
        _fadeimage.TweenImageColor(FADEINCOLOR, 1.2f, () =>  TweenLogo() , Tween.Easing.EaseInQuad);
    }
}
