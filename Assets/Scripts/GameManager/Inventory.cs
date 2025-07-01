using System;
using System.Collections.Generic;
using Ami.BroAudio; 
using UnityEngine;
using Cysharp.Threading.Tasks;

public sealed class Inventory : MonoBehaviour
{
    // --- Singleton Pattern (unchanged) ---
    private static Inventory instance;
    public static Inventory Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<Inventory>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("Inventory_Singleton");
                    instance = singletonObject.AddComponent<Inventory>();
                }
            }
            return instance;
        }
    }

    public PlayerData playerData { get; private set; }

    [Header("Configuration")]
    public List<Lure> allAvailableLures = new List<Lure>();
    //[SerializeField] private FishRanking[] fishRanking;

    public static event Action OnInventoryChanged;
    public static event Action OnMoneyChanged;
    public static event Action OnEquipmentChanged;

    // `Awake` becomes `async void` to allow for awaiting the load operation.
    // This is a primary entry point for async logic in Unity's lifecycle.
    private async void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Await the asynchronous loading of data
            playerData = await SaveLoadManager.Instance.LoadDataAsync();
            
            // After data is loaded, notify all listeners to update UI etc.
            OnMoneyChanged?.Invoke();
            OnEquipmentChanged?.Invoke();
            OnInventoryChanged?.Invoke();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    // --- Public Properties (unchanged) ---
    public uint Money => playerData.Money;
    public List<CaughtFishData> CaughtFishes => playerData.CaughtFishes;
    public uint CurrentMaxLineLength => playerData.CurrentMaxLineLength;

    // --- Data Modification Methods ---

    public async UniTask AddCaughtFish(FishStats fishCaught)
    {
        // ... (logic for adding fish is the same)
        
        OnInventoryChanged?.Invoke();
        await SaveAsync(); // Await the save operation
    }

    public async UniTask AddMoney(uint amount)
    {
        playerData.Money += amount;
        OnMoneyChanged?.Invoke();
        await SaveAsync();
    }

    public async UniTask<bool> SpendMoney(uint amount)
    {
        if (playerData.Money < amount) return false;
        
        playerData.Money -= amount;
        OnMoneyChanged?.Invoke();
        await SaveAsync();
        return true;
    }

    public async UniTask SellFish(CaughtFishData fishToSell)
    {
        if (playerData.CaughtFishes.Remove(fishToSell))
        {
            // `AddMoney` is now async, so we await it.
            await AddMoney(fishToSell.value);
            OnInventoryChanged?.Invoke();
        }
    }

    public async UniTask<bool> UpgradeLineLength(uint newLength, uint cost)
    {
        // `SpendMoney` is now async, so we await the result.
        if (!await SpendMoney(cost)) return false;
        
        playerData.CurrentMaxLineLength = newLength;
        OnEquipmentChanged?.Invoke();
        // `SpendMoney` already saves, so no need to save again here.
        return true;
    }

    public async UniTask<bool> BuyLure(Lure lureToBuy)
    {
        if (lureToBuy == null || IsLureOwned(lureToBuy.HashedID)) return false;
        if (!await SpendMoney(lureToBuy.Cost)) return false;

        playerData.OwnedLureIDs.Add(lureToBuy.HashedID);
        OnEquipmentChanged?.Invoke();
        return true;
    }

    public bool IsLureOwned(uint hashedLureID)
    {
        return playerData.OwnedLureIDs.Contains(hashedLureID);
    }

    public async UniTask<bool> EquipLure(uint hashedLureID)
    {
        if (hashedLureID == 0) {
            Debug.LogWarning("Cannot equip lure with HashedID 0.");
            return false;
        }

        if (!IsLureOwned(hashedLureID))
        {
            Debug.LogWarning($"Cannot equip lure with HashedID '{hashedLureID}'. Player does not own it.");
            return false;
        }

        if (playerData.EquippedLureID == hashedLureID)
        {
            Debug.Log($"Lure with HashedID '{hashedLureID}' is already equipped.");
            return true; 
        }
        
        playerData.EquippedLureID = hashedLureID;
        Lure equippedLure = GetLureByHashedID(hashedLureID);
        if (equippedLure != null && CustomRopeSolver.Instance != null)
        {
            CustomRopeSolver.Instance.GetLure()?.ChangeBait(equippedLure.GameplayLureType);
            Debug.Log($"Equipped lure: {equippedLure.LureName} and changed gameplay bait to {equippedLure.GameplayLureType}");
        }
        
        // Invoke events to notify UI
        OnEquipmentChanged?.Invoke();
        OnInventoryChanged?.Invoke();
        
        // Await the asynchronous save operation
        await SaveAsync();
        
        return true;
    }

    public Lure GetLureByHashedID(uint hashedLureID)
    {
        if (hashedLureID == 0) return null; // Assuming 0 is not a valid hashed ID
        return allAvailableLures.Find(lure => lure.HashedID == hashedLureID);
    }
    
    public Lure GetEquippedLure()
    {
        if (playerData.EquippedLureID == 0) return null; // Assuming 0 means no lure equipped
        return GetLureByHashedID(playerData.EquippedLureID);
    }
    
    // Overload to equip by string ID
    public async UniTask<bool> EquipLureByStringID(string stringLureID)
    {
        Lure lureToEquip = allAvailableLures.Find(l => l.LureID == stringLureID);
        if (lureToEquip == null)
        {
            Debug.LogError($"Lure with string ID '{stringLureID}' not found for equipping.");
            return false;
        }
        // Await the result of the async EquipLure method before returning it.
        return await EquipLure(lureToEquip.HashedID);
    }

    // --- Private Helper Methods ---

    private void PlayFishCatchFeedback(Transform fishTransform) { /* ... */ }

    /// <summary>
    /// A single, private async method to trigger a save.
    /// </summary>
    private async UniTask SaveAsync()
    {
        await SaveLoadManager.Instance.SaveDataAsync(playerData);
    }
}