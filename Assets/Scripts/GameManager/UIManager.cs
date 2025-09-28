using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
using DonutPackage.BTween;
using DonutPackage.EventBus;
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

public enum InventoryMode { Gear, Fish }
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
    private bool _shopbool = false;
    private InventoryMode _currentInventoryMode = InventoryMode.Gear;

    [Header("Shop Panel")]
    [SerializeField] private RectTransform _shopPanel;
    [SerializeField] private GameObject _shopItemPrefab; // The new universal prefab
    [SerializeField] private Transform _shopContainer;

    [Header("Inventory & Fish Bag Panel")]
    [SerializeField] private RectTransform _inventoryPanel;
    [SerializeField] private Transform _inventoryContainer;
    [SerializeField] private GameObject _inventoryItemPrefab;
    [SerializeField] private GameObject _fishBagItemPrefab;
    [SerializeField] private Button _gearTabButton;
    [SerializeField] private Button _fishTabButton;

    [Header("Upgrade Tiers")]
    [SerializeField] private List<LineUpgradeTier> _lineUpgradeTiers = new List<LineUpgradeTier>();
    
    [Header("Max Level State")]
    [SerializeField] private Sprite _maxLevelImage;
    
    [Header("Other UI")]
    [SerializeField] private GameObject _pauseCanvas;
    [SerializeField] private GameObject _callShopCanvas;
    [SerializeField] private GameObject _goFishCanvas;
    [SerializeField] private SoundID _buySfx;
    [SerializeField] private Image _GameStartLogo;
    
    [Header("Asset Databases")]
    [SerializeField] private FishDatabase _fishDatabase;

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
        _gearTabButton.onClick.AddListener(() => SwitchInventoryTab(InventoryMode.Gear));
        _fishTabButton.onClick.AddListener(() => SwitchInventoryTab(InventoryMode.Fish));

        // Set the initial state
        SwitchInventoryTab(InventoryMode.Gear);
        PopulateShopPanel();
        StartGameFadeIN();
        _shopPanel.TweenAnchoredPosition(SHOPPANELOFFPOS, 0.2f, () => { Debug.Log("Panel move complete!"); }, BTween.Ease.OutQuad);
    }

private void OnEnable()
    {
        // Subscribe to events to keep UI up-to-date
        EventBus.Subscribe<MoneyChangedEvent>(HandleMoneyChanged);
        EventBus.Subscribe<EquipmentChangedEvent>(HandleEquipmentChanged);
        EventBus.Subscribe<InventoryChangedEvent>(HandleInventoryChanged);
        EventBus.Subscribe<PauseStateChangedEvent>(HandlePauseState);
        EventBus.Subscribe<ToggleShopEvent>(HandleToggleShop);
        EventBus.Subscribe<FadeOutScreenEvent>(HandleFadeOutScreen);
    }

    private void OnDisable()
    {
        // Always unsubscribe
        EventBus.Unsubscribe<MoneyChangedEvent>(HandleMoneyChanged);
        EventBus.Unsubscribe<EquipmentChangedEvent>(HandleEquipmentChanged);
        EventBus.Unsubscribe<InventoryChangedEvent>(HandleInventoryChanged);
        EventBus.Unsubscribe<PauseStateChangedEvent>(HandlePauseState);
        EventBus.Unsubscribe<ToggleShopEvent>(HandleToggleShop);
        EventBus.Unsubscribe<FadeOutScreenEvent>(HandleFadeOutScreen);
    }

    private void HandleFadeOutScreen(FadeOutScreenEvent e)
    {
        UIScreenfadeout();
    }

    private void HandleMoneyChanged(MoneyChangedEvent e)
    {
        PopulateShopPanel();
    }

    private void HandleInventoryChanged(InventoryChangedEvent e)
    {
        if (_currentInventoryMode == InventoryMode.Fish)
        {
            PopulateFishBagPanel();
        }
        else
        {
            UpdateGearPanel();
        }
    }

    private void HandleEquipmentChanged(EquipmentChangedEvent e)
    {
        PopulateShopPanel();
        UpdateGearPanel();
    }

    private void HandlePauseState(PauseStateChangedEvent e)
    {
        if (_pauseCanvas != null) _pauseCanvas.SetActive(e.IsPaused);
    }

    private void HandleToggleShop(ToggleShopEvent e)
    {
        ToggleShop();
    }

    /// <summary>
    /// Switches the view of the right-side panel between Gear and Fish.
    /// </summary>
    public void SwitchInventoryTab(InventoryMode mode)
    {
        _currentInventoryMode = mode;
        if (mode == InventoryMode.Gear)
        {
            UpdateGearPanel();
            // Optional: Visually change tab colors to show active state
            _gearTabButton.interactable = false;
            _fishTabButton.interactable = true;
        }
        else // mode == InventoryMode.Fish
        {
            PopulateFishBagPanel();
            _gearTabButton.interactable = true;
            _fishTabButton.interactable = false;
        }
    }

    public void PopulateShopPanel()
    {
        foreach (Transform child in _shopContainer) Destroy(child.gameObject);

        // 1. Add the next available line upgrade
        uint currentLength = Inventory.Instance.CurrentMaxLineLength;
        LineUpgradeTier nextTier = _lineUpgradeTiers.FirstOrDefault(tier => tier.NewLength > currentLength);
        if (nextTier != null)
        {
            GameObject tierGO = Instantiate(_shopItemPrefab, _shopContainer);
            tierGO.GetComponent<ShopItemUI>().SetupAsLineUpgrade(nextTier);
        }

        // 2. Add all unowned lures
        List<Lure> allLures = Inventory.Instance.allAvailableLures;
        foreach (Lure lure in allLures)
        {
            if (!Inventory.Instance.IsLureOwned(lure.HashedID))
            {
                GameObject lureGO = Instantiate(_shopItemPrefab, _shopContainer);
                lureGO.GetComponent<ShopItemUI>().SetupAsLure(lure);
            }
        }
    }

    /// <summary>
    /// Populates the inventory panel with the player's owned gear (lures).
    /// This is now the single method responsible for this panel.
    /// </summary>
    private void UpdateGearPanel()
    {
        // Only run if this tab is active
        if (_currentInventoryMode != InventoryMode.Gear) return;

        foreach (Transform child in _inventoryContainer) Destroy(child.gameObject);

        List<uint> ownedLureIDs = new List<uint>(Inventory.Instance.playerData.OwnedLureIDs);
        foreach (uint lureID in ownedLureIDs)
        {
            Lure lureData = Inventory.Instance.GetLureByHashedID(lureID);
            if (lureData != null)
            {
                GameObject itemGO = Instantiate(_inventoryItemPrefab, _inventoryContainer);
                itemGO.GetComponent<InventoryItemUI>().Setup(lureData);
            }
        }
    }

    private void PopulateFishBagPanel()
    {
        // Only run if this tab is active
        if (_currentInventoryMode != InventoryMode.Fish) return;

        foreach (Transform child in _inventoryContainer) Destroy(child.gameObject);

        var caughtFishes = Inventory.Instance.playerData.CaughtFishes;
        foreach (CaughtFishData fishData in caughtFishes)
        {
            GameObject itemGO = Instantiate(_fishBagItemPrefab, _inventoryContainer);
            itemGO.GetComponent<FishBagItemUI>().Setup(fishData);
        }
    }

    /// <summary>
    /// Gets a fish sprite from the database by its name.
    /// This method now passes the request to the FishDatabase asset.
    /// </summary>
    public Sprite GetFishSprite(string fishName)
    {
        if (_fishDatabase == null)
        {
            Debug.LogError("FishDatabase has not been assigned in the UIManager inspector!");
            return null;
        }
        return _fishDatabase.GetSprite(fishName);
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
    /// Toggles the shop panel's visibility and associated game state.
    /// </summary>
    public void ToggleShop()
    {
        _shopbool = !_shopbool;

        if (_shopbool)
        {
            _shopPanel.TweenAnchoredPosition(SHOPPANELONPOS, UITWEENSPEED, null, BTween.Ease.OutCirc);
        }
        else
        {
            _shopPanel.TweenAnchoredPosition(SHOPPANELOFFPOS, UITWEENSPEED, null, BTween.Ease.OutCirc);
        }
        EventBus.Publish(new ShopStateChangedEvent { IsShopOpen = _shopbool });
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
