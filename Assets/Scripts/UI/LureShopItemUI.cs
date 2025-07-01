using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class LureShopItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lureNameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Button _buyButton;
    [SerializeField] private Button _equipButton;
    [SerializeField] private GameObject _ownedIndicator;
    [SerializeField] private GameObject _equippedIndicator;

    private Lure _lure;

    public void Setup(Lure lureData)
    {
        _lure = lureData;

        _lureNameText.text = _lure.LureName;
        _descriptionText.text = _lure.Description;
        _costText.text = $"{_lure.Cost}g";

        _buyButton.onClick.AddListener(OnBuyButtonPressed);
        _equipButton.onClick.AddListener(OnEquipButtonPressed);

        UpdateStatus();
    }

    public void UpdateStatus()
    {
        bool isOwned = Inventory.Instance.IsLureOwned(_lure.HashedID);
        bool isEquipped = Inventory.Instance.playerData.EquippedLureID == _lure.HashedID;

        _buyButton.gameObject.SetActive(!isOwned);
        _costText.gameObject.SetActive(!isOwned);
        
        _equipButton.gameObject.SetActive(isOwned);
        _ownedIndicator.SetActive(isOwned);

        _equippedIndicator.SetActive(isEquipped);
        _equipButton.interactable = !isEquipped;
    }

    private async void OnBuyButtonPressed()
    {
        _buyButton.interactable = false;
        bool success = await Inventory.Instance.BuyLure(_lure);
        if (success)
        {
            // FIX: The method was renamed to PopulateShopPanel in UIManager.
            // We also need to refresh the inventory panel.
            UIManager.Instance.PopulateShopPanel();
            UIManager.Instance.PopulateInventoryPanel();
        }
        else
        {
            _buyButton.interactable = true; // Re-enable if purchase failed
        }
    }

    private async void OnEquipButtonPressed()
    {
        // FIX: The 'await' keyword was missing here. Adding it fixes
        // the CS0029 and CS1998 errors.
        bool success = await Inventory.Instance.EquipLure(_lure.HashedID);
        if (success)
        {
            // FIX: We also call the correct method here.
            // We only need to refresh the inventory when equipping.
            UIManager.Instance.PopulateInventoryPanel();
        }
    }
}