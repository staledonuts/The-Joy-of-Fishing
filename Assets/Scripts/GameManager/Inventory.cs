using System;
using System.Collections.Generic;
using System.IO;
using Ami.BroAudio;
using UnityEngine;

public sealed class Inventory : MonoBehaviour
{
    private static Inventory instance = null;
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

    public PlayerData playerData; // This will hold all player-specific data

    // --- Lure Management ---
    // This would ideally come from a game data manager or ScriptableObjects
    [Header("Lure Configuration")]
    public List<Lure> allAvailableLures = new List<Lure>();

    // --- Fish Ranking (from your original script) ---
    [Header("Fish Catch Feedback")]
    [SerializeField] private FishRanking[] fishRanking;

    // --- Events for UI updates ---
    public static event Action OnInventoryChanged; // For general changes
    public static event Action OnMoneyChanged;
    public static event Action OnEquipmentChanged; // For line length or lure changes

    private string _savePath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            _savePath = Path.Combine(Application.persistentDataPath, "playerData.json");
            LoadData(); // Load data when the inventory is initialized
        }
        else if (instance != this)
        {
            Debug.LogWarning("Another instance of Inventory found, destroying this new one.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialize lure list if empty (example, better to use ScriptableObjects)
        if (allAvailableLures.Count == 0)
        {
            allAvailableLures.Add(new Lure("BasicLureID", "Basic Lure", "A simple, reliable lure.", 0));
            allAvailableLures.Add(new Lure("ShinySpinnerID", "Shiny Spinner", "Attracts fish with its sparkle.", 100));
            allAvailableLures.Add(new Lure("DeepDiverID", "Deep Diver", "Gets to the bottom quickly.", 150));
        }
    }


    public uint Money => playerData.money;
    public List<CaughtFishData> CaughtFishes => playerData.caughtFishes;
    public uint CurrentMaxLineLength => playerData.currentMaxLineLength;
    public string EquippedLureID => playerData.equippedLureID;
    public List<string> OwnedLureIDs => playerData.ownedLureIDs;


    /// <summary>
    /// Adds a caught fish to the inventory.
    /// Assumes FishStats has a way to provide FishTypeID, Value, and dynamic properties like size/weight.
    /// </summary>
    public void AddCaughtFish(FishStats fishCaught) // FishStats is your MonoBehaviour on the fish GameObject
    {
        if (fishCaught != null) // Assuming fishStats is your ScriptableObject Fish
        {
            string typeID = fishCaught.FishName;
            uint value = fishCaught.Value;

            // Dynamic properties might come from FishStats itself or be calculated
            float size = fishCaught.transform.localScale.x; // Example: get size
            float weight = size * 10f; // Example: calculate weight

            CaughtFishData newFishData = new CaughtFishData(typeID, size, weight, value);
            playerData.caughtFishes.Add(newFishData);
            Debug.Log($"Caught a {typeID} (Size: {size}, Weight: {weight}, Value: {value})! Added to inventory.");
            
            // Play fish ranking audio/particles if applicable
            // This part depends on your FishRanking setup and how it correlates to the caught fish
            // For example, find a matching FishRanking based on value or typeID
            PlayFishCatchFeedback(fishCaught.transform); // Pass transform for particle/audio position

            OnInventoryChanged?.Invoke();
            SaveData();
        }
        else
        {
            Debug.LogWarning("Attempted to add a null or incomplete fish to inventory.");
        }
    }
    
    private void PlayFishCatchFeedback(Transform fishTransform)
    {
        // Example: Play the first ranking's feedback. You'll need more specific logic.
        if (fishRanking != null && fishRanking.Length > 0)
        {
            fishRanking[0].Play(fishTransform); // Or select based on fish rarity/value
        }
    }


    public void AddMoney(uint amount)
    {
        playerData.money += amount;
        Debug.Log($"Added {amount} money. Total money: {playerData.money}");
        OnMoneyChanged?.Invoke();
        OnInventoryChanged?.Invoke();
        SaveData();
    }

    public bool SpendMoney(uint amount)
    {
        if (playerData.money >= amount)
        {
            playerData.money -= amount;
            Debug.Log($"Spent {amount} money. Remaining money: {playerData.money}");
            OnMoneyChanged?.Invoke();
            OnInventoryChanged?.Invoke();
            SaveData();
            return true;
        }
        else
        {
            Debug.LogWarning($"Attempted to spend {amount} money, but only have {playerData.money}. Transaction failed.");
            return false;
        }
    }

    public void SellFish(CaughtFishData fishToSell)
    {
        if (playerData.caughtFishes.Contains(fishToSell))
        {
            playerData.caughtFishes.Remove(fishToSell);
            AddMoney(fishToSell.value); 
            Debug.Log($"Sold {fishToSell.fishTypeID} for {fishToSell.value}.");
            OnInventoryChanged?.Invoke();
            SaveData();
        }
        else
        {
            Debug.LogWarning($"Attempted to sell {fishToSell.fishTypeID}, but it's not in the inventory.");
        }
    }
    
    // Example: Sell fish by index if you have a UI list
    public void SellFishByIndex(int index)
    {
        if (index >= 0 && index < playerData.caughtFishes.Count)
        {
            CaughtFishData fishToSell = playerData.caughtFishes[index];
            SellFish(fishToSell); // Uses the method above
        }
        else
        {
            Debug.LogWarning($"Attempted to sell fish at invalid index: {index}");
        }
    }
    public bool UpgradeLineLength(uint additionalLength, uint cost)
    {
        if (SpendMoney(cost))
        {
            playerData.currentMaxLineLength += additionalLength;
            Debug.Log($"Line length upgraded by {additionalLength}. New max length: {playerData.currentMaxLineLength}");
            OnEquipmentChanged?.Invoke();
            OnInventoryChanged?.Invoke();
            SaveData();
            return true;
        }
        return false;
    }

    // --- Lure Management ---
    public Lure GetLureByID(string lureID)
    {
        return allAvailableLures.Find(lure => lure.lureID == lureID);
    }
    
    public Lure GetEquippedLure()
    {
        if (string.IsNullOrEmpty(playerData.equippedLureID)) return null;
        return GetLureByID(playerData.equippedLureID);
    }

    public bool IsLureOwned(string lureID)
    {
        return playerData.ownedLureIDs.Contains(lureID);
    }

    public bool BuyLure(string lureID)
    {
        if (IsLureOwned(lureID))
        {
            Debug.Log($"Already own lure: {lureID}");
            return false;
        }

        Lure lureToBuy = GetLureByID(lureID);
        if (lureToBuy == null)
        {
            Debug.LogError($"Lure with ID '{lureID}' not found in allAvailableLures.");
            return false;
        }

        if (SpendMoney(lureToBuy.cost))
        {
            playerData.ownedLureIDs.Add(lureID);
            Debug.Log($"Bought lure: {lureToBuy.lureName} (ID: {lureID})");
            OnEquipmentChanged?.Invoke(); // Or a specific OnLuresChanged event
            OnInventoryChanged?.Invoke();
            SaveData();
            return true;
        }
        return false;
    }

    public bool EquipLure(string lureID)
    {
        if (!IsLureOwned(lureID))
        {
            Debug.LogWarning($"Cannot equip lure '{lureID}'. Player does not own it.");
            return false;
        }

        if (playerData.equippedLureID == lureID)
        {
            Debug.Log($"Lure '{lureID}' is already equipped.");
            return true; // Or false if you want to indicate no change happened
        }
        
        Lure lureToEquip = GetLureByID(lureID);
        if (lureToEquip == null)
        {
             Debug.LogError($"Cannot equip lure: Lure with ID '{lureID}' not found in allAvailableLures, even though it was marked as owned. Data inconsistency?");
            return false;
        }


        playerData.equippedLureID = lureID;
        Debug.Log($"Equipped lure: {GetLureByID(lureID)?.lureName}");
        OnEquipmentChanged?.Invoke();
        OnInventoryChanged?.Invoke();
        SaveData();
        return true;
    }

    // --- Data Persistence (Basic Example) ---
    public void SaveData()
    {
        try
        {
            string json = JsonUtility.ToJson(playerData, true); // true for pretty print
            File.WriteAllText(_savePath, json);
            Debug.Log($"Player data saved to {_savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save player data: {e.Message}");
        }
    }

    public void LoadData()
    {
        if (File.Exists(_savePath))
        {
            try
            {
                string json = File.ReadAllText(_savePath);
                PlayerData loadedData = JsonUtility.FromJson<PlayerData>(json);
                if (loadedData != null)
                {
                    playerData = loadedData;
                    Debug.Log("Player data loaded successfully.");
                }
                else
                {
                     Debug.LogWarning("Failed to parse player data from JSON. Using new PlayerData.");
                    playerData = new PlayerData(); // Fallback to new data
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load player data: {e.Message}. Creating new PlayerData.");
                playerData = new PlayerData(); // Fallback to new data
            }
        }
        else
        {
            Debug.Log("No save file found. Creating new PlayerData.");
            playerData = new PlayerData(); // Create new data if no save file
        }
        // Invoke events to update UI after loading
        OnMoneyChanged?.Invoke();
        OnEquipmentChanged?.Invoke();
        OnInventoryChanged?.Invoke();
    }
}

// FishRanking class from your original script (ensure it's defined or move it here)
// If FishRanking is in another file and accessible, you don't need to redefine it.
[System.Serializable]
public class FishRanking
{
    public SoundID FishGetAudio;
    public SoundID CoinGetAudio;
    public ParticleSystem CoinParticle;

    public void Play(Transform transform)
    {
        CoinGetAudio.Play(transform);
        FishGetAudio.Play(transform);
        if(CoinParticle != null)
        {
            CoinParticle.Play();
        }
    }
}
