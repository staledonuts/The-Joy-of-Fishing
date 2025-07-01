using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(TooltipTrigger))] // Ensures the component is always on the GameObject
public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lureNameText;
    [SerializeField] private Image _lureImage;
    [SerializeField] private Button _equipButton;
    [SerializeField] private GameObject _equippedIndicator;
    private Lure _lure;
    private TooltipTrigger _tooltipTrigger;

     private void Awake()
    {
        _tooltipTrigger = GetComponent<TooltipTrigger>();
        _equipButton.onClick.AddListener(OnEquipButtonPressed);
    }

    public void Setup(Lure lureData)
    {
        _lure = lureData;

        _lureNameText.text = _lure.LureName;

        _tooltipTrigger.header = lureData.LureName;
        _tooltipTrigger.body = lureData.Description;

        UpdateStatus();
    }

    public void UpdateStatus()
    {
        bool isEquipped = Inventory.Instance.playerData.EquippedLureID == _lure.HashedID;
        _equippedIndicator.SetActive(isEquipped);
        _equipButton.interactable = !isEquipped;
    }

    private async void OnEquipButtonPressed()
    {
        await Inventory.Instance.EquipLure(_lure.HashedID);
    }
}