using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LineUpgradeTier
{
    public string Description;
    public uint NewLength;
    public uint Cost;
    public Sprite TierImage;
}
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
    private LineUpgradeTier _nextAvailableTier;
    private bool _shopbool = false;
    
    [Header("Upgrade Line Button")]
    [SerializeField] private Button _upgradeLineButton;
    [SerializeField] private TextMeshProUGUI _upgradeButtonText;
    [SerializeField] private List<LineUpgradeTier> _lineUpgradeTiers = new List<LineUpgradeTier>();
    [SerializeField] private Image _upgradeButtonImage; 
    
    [Header("Max Level State")]
    [SerializeField] private Sprite _maxLevelImage;

    [Header("Shop Settings")]
    [SerializeField] private RectTransform _shopPanel;
    [SerializeField] private GameObject _lureShopItemPrefab;
    [SerializeField] private Transform _lureShopContainer;
    
    [Header("Other UI")]
    [SerializeField] private GameObject _pauseCanvas;
    [SerializeField] private GameObject _callShopCanvas;
    [SerializeField] private GameObject _goFishCanvas;
    [SerializeField] private SoundID _buySfx;
    [SerializeField] private Image _GameStartLogo;

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
        _shopPanel.TweenAnchoredPosition(SHOPPANELOFFPOS, 0.2f, () => { Debug.Log("Panel move complete!"); }, BTween.Ease.OutQuad);
    }

     private void OnEnable()
    {
        // Subscribe to events to keep the button state accurate
        Inventory.OnMoneyChanged += UpdateUpgradeButtonUI;
        Inventory.OnEquipmentChanged += UpdateUpgradeButtonUI;
    }

    private void OnDisable()
    {
        // Always unsubscribe
        Inventory.OnMoneyChanged -= UpdateUpgradeButtonUI;
        Inventory.OnEquipmentChanged -= UpdateUpgradeButtonUI;
    }

        /// <summary>
    /// This single method is called when the new upgrade button is pressed.
    /// </summary>
    public async void OnUpgradeLineButtonPressed()
    {
        if (_nextAvailableTier == null)
        {
            Debug.Log("Already at max level.");
            return;
        }

        // Disable button to prevent double-clicks during the transaction
        _upgradeLineButton.interactable = false;

        bool success = await Inventory.Instance.UpgradeLineLength(_nextAvailableTier.NewLength, _nextAvailableTier.Cost);

        if (success)
        {
            _buySfx.Play();
            Debug.Log($"Successfully upgraded line to {_nextAvailableTier.NewLength}m!");
        }
        
        // The UI will automatically update via the OnEquipmentChanged event,
        // which re-enables the button if another tier is available and affordable.
    }

    /// <summary>
    /// Updates the upgrade button's text and interactable state based on player progress.
    /// </summary>
    /// <summary>
    /// Updates the upgrade button's text, image, and interactable state.
    /// </summary>
    private void UpdateUpgradeButtonUI()
    {
        uint currentLength = Inventory.Instance.CurrentMaxLineLength;
        _nextAvailableTier = _lineUpgradeTiers.FirstOrDefault(tier => tier.NewLength > currentLength);

        if (_nextAvailableTier != null)
        {
            // A next tier is available
            _upgradeButtonText.text = $"{_nextAvailableTier.Description}\nCost: {_nextAvailableTier.Cost}g";
            _upgradeLineButton.interactable = Inventory.Instance.Money >= _nextAvailableTier.Cost;
            
            // Set the image for the current tier
            if (_upgradeButtonImage != null && _nextAvailableTier.TierImage != null)
            {
                _upgradeButtonImage.sprite = _nextAvailableTier.TierImage;
                _upgradeButtonImage.enabled = true;
            }
        }
        else
        {
            // Player is at the max level
            _upgradeButtonText.text = "Max Level";
            _upgradeLineButton.interactable = false;

            // Set the max level image, or disable the image if none is provided
            if (_upgradeButtonImage != null)
            {
                if (_maxLevelImage != null)
                {
                    _upgradeButtonImage.sprite = _maxLevelImage;
                    _upgradeButtonImage.enabled = true;
                }
                else
                {
                    _upgradeButtonImage.enabled = false;
                }
            }
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
            _GameStartLogo.material.TweenMaterialFloat("_Dissolve", 0f, 2f, () => { _fadeCanvas.SetActive(false); }, BTween.Ease.InOutQuad);
        }
    }

    /// <summary>
    /// Fills the lure shop with items based on the Inventory's available lures.
    /// </summary>
    public void PopulateLureShop()
    {
        // 1. Clear any old items from the list
        foreach (Transform child in _lureShopContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Get all lures defined in the Inventory
        List<Lure> allLures = Inventory.Instance.allAvailableLures;

        // 3. Create a UI element for each lure
        foreach (Lure lure in allLures)
        {
            GameObject itemGO = Instantiate(_lureShopItemPrefab, _lureShopContainer);
            LureShopItemUI itemUI = itemGO.GetComponent<LureShopItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(lure);
            }
        }
    }

    private bool OnShopSwitch()
    {
        if (!_shopbool)
        {
            _shopbool = true;
            _shopPanel.TweenAnchoredPosition(SHOPPANELONPOS, UITWEENSPEED, null, BTween.Ease.OutCirc);
            SoundManager.Instance.TransitionToShopMusic(true);
            
            // Populate the shop when it opens
            PopulateLureShop();
        }
        else
        {
            _shopbool = false;
            _shopPanel.TweenAnchoredPosition(SHOPPANELOFFPOS, UITWEENSPEED, null, BTween.Ease.OutCirc);
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
        _fadeimage.TweenImageColor(FADEOUTCOLOR, UITWEENSPEED, null, BTween.Ease.InOutQuad);
    }
    public void UIScreenfadein() 
    {
        _fadeimage.TweenImageColor(FADEINCOLOR, UITWEENSPEED, () => _fadeCanvas.SetActive(false), BTween.Ease.InQuad);
    }

    public void StartGameFadeIN()
    {
        _fadeimage.TweenImageColor(FADEINCOLOR, 1.2f, () =>  TweenLogo() , BTween.Ease.InQuad);
    }
}
