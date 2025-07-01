using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishBagItemUI : MonoBehaviour
{
    [SerializeField] private Image _fishImage; 
    [SerializeField] private TextMeshProUGUI _fishNameText;
    [SerializeField] private TextMeshProUGUI _fishInfoText; // For size and weight
    [SerializeField] private TextMeshProUGUI _fishValueText;
    [SerializeField] private Button _sellButton;

    private CaughtFishData _fishData;

    /// <summary>
    /// Configures the UI element with the data from a caught fish.
    /// </summary>
    public void Setup(CaughtFishData fishData)
    {
        _fishData = fishData;

        _fishNameText.text = _fishData.fishTypeID;
        _fishInfoText.text = $"Size: {_fishData.size:F1} | Weight: {_fishData.weight:F1}kg";
        _fishValueText.text = $"{_fishData.value}g";

        _sellButton.onClick.AddListener(OnSellButtonPressed);
        if (_fishImage != null)
        {
            // Get the sprite from the UIManager's database
            _fishImage.sprite = UIManager.Instance.GetFishSprite(fishData.fishTypeID);
        }
    }

    private async void OnSellButtonPressed()
    {
        // Disable button to prevent double-clicks
        _sellButton.interactable = false;

        // Call the SellFish method in the Inventory
        await Inventory.Instance.SellFish(_fishData);

        // The UIManager will automatically refresh the list because it's
        // listening to the OnInventoryChanged event.
    }
}