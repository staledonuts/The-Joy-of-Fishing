using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use TextMeshPro for better text rendering

public class LureShopItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lureNameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Button _buyButton;
    [SerializeField] private Button _equipButton;
    [SerializeField] private GameObject _ownedIndicator; // An icon or text saying "Owned"
    [SerializeField] private GameObject _equippedIndicator; // An icon or text showing this is equipped

    private Lure _lure;

    /// <summary>
    /// Configures the UI element with the data from a Lure object.
    /// </summary>
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

    /// <summary>
    /// Updates the visibility of buttons and indicators based on player's inventory.
    /// </summary>
    public void UpdateStatus()
    {
        bool isOwned = Inventory.Instance.IsLureOwned(_lure.HashedID);
        bool isEquipped = Inventory.Instance.playerData.EquippedLureID == _lure.HashedID;

        _ownedIndicator.SetActive(isOwned);
        _buyButton.gameObject.SetActive(!isOwned);
        _equipButton.gameObject.SetActive(isOwned);

        _equippedIndicator.SetActive(isEquipped);
        _equipButton.interactable = !isEquipped;
    }

    private async void OnBuyButtonPressed()
    {
        _buyButton.interactable = false; // Prevent double-clicks
        bool success = await Inventory.Instance.BuyLure(_lure);
        if (success)
        {
            // Successfully bought, refresh the entire shop UI
            UIManager.Instance.PopulateLureShop();
        }
        else
        {
            Debug.Log("Could not buy lure. Not enough money?");
            _buyButton.interactable = true; // Re-enable if purchase failed
        }
    }

    private async void OnEquipButtonPressed()
    {
        // EquipLure now returns a UniTask<bool>
        bool success = Inventory.Instance.EquipLure(_lure.HashedID);
        if (success)
        {
            // Refresh the entire shop UI to show the new "Equipped" status
            UIManager.Instance.PopulateLureShop();
        }
    }
}