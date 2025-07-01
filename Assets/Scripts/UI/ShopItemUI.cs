using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public enum ShopItemType { LineUpgrade, Lure }

[RequireComponent(typeof(TooltipTrigger))] // Ensures the component is always on the GameObject
public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Image _itemImage;
    [SerializeField] private Button _buyButton;
    private ShopItemType _itemType;
    private Lure _lureData;
    private LineUpgradeTier _lineData;
    private TooltipTrigger _tooltipTrigger;

    private void Awake()
    {
        // Get the tooltip component on this object
        _tooltipTrigger = GetComponent<TooltipTrigger>();
        _buyButton.onClick.AddListener(OnPurchaseButtonPressed);
    }

    public void SetupAsLure(Lure lure)
    {
        _itemType = ShopItemType.Lure;
        _lureData = lure;

        _nameText.text = lure.LureName;
        _descriptionText.text = lure.Description;
        _costText.text = $"{lure.Cost}g";

        // --- Configure the tooltip ---
        _tooltipTrigger.header = lure.LureName;
        _tooltipTrigger.body = lure.Description;

        UpdatePurchaseButton();
    }

    public void SetupAsLineUpgrade(LineUpgradeTier tier)
    {
        _itemType = ShopItemType.LineUpgrade;
        _lineData = tier;

        _nameText.text = tier.Description;
        _descriptionText.text = $"Increases line length to {tier.NewLength}m";
        _costText.text = $"{tier.Cost}g";
        _itemImage.sprite = tier.TierImage;

        // --- Configure the tooltip ---
        _tooltipTrigger.header = tier.Description;
        _tooltipTrigger.body = $"Increases line length to {tier.NewLength}m.";

        UpdatePurchaseButton();
    }

    public void UpdatePurchaseButton()
    {
        uint cost = (_itemType == ShopItemType.Lure) ? _lureData.Cost : _lineData.Cost;
        _buyButton.interactable = Inventory.Instance.Money >= cost;
    }

    private async void OnPurchaseButtonPressed()
    {
        _buyButton.interactable = false;
        bool success = false;

        if (_itemType == ShopItemType.Lure)
        {
            success = await Inventory.Instance.BuyLure(_lureData);
        }
        else if (_itemType == ShopItemType.LineUpgrade)
        {
            success = await Inventory.Instance.UpgradeLineLength(_lineData.NewLength, _lineData.Cost);
        }

        if (!success)
        {
            _buyButton.interactable = true;
        }
    }
}