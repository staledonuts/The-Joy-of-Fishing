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

    public PlayerData playerData; 

    [Header("Lure Configuration")]
    public List<Lure> allAvailableLures = new List<Lure>(); 

    [Header("Fish Catch Feedback")]
    [SerializeField] private FishRanking[] fishRanking; 

    public static event Action OnInventoryChanged; 
    public static event Action OnMoneyChanged;
    public static event Action OnEquipmentChanged; 

    private string _savePath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            _savePath = Path.Combine(Application.persistentDataPath, "playerData.json");
            LoadData(); 
        }
        else if (instance != this)
        {
            Debug.LogWarning("Another instance of Inventory found, destroying this new one.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (allAvailableLures.Count == 0)
        {
            // Ensure the string IDs here match what PlayerData constructor hashes for defaults
            allAvailableLures.Add(new Lure("BasicLureID", "Basic Lure", "A simple, reliable lure.", 0));
            allAvailableLures.Add(new Lure("ShinySpinnerID", "Shiny Spinner", "Attracts fish with its sparkle.", 100));
            allAvailableLures.Add(new Lure("DeepDiverID", "Deep Diver", "Gets to the bottom quickly.", 150));
        }
    }

    public uint Money => playerData.Money;
    public List<CaughtFishData> CaughtFishes => playerData.CaughtFishes;
    public uint CurrentMaxLineLength => playerData.CurrentMaxLineLength;
    public uint EquippedLureID => playerData.EquippedLureID;
    public List<uint> OwnedLureIDs => playerData.OwnedLureIDs;

    public void AddCaughtFish(FishStats fishCaught) 
    {
        if (fishCaught != null) 
        {
            string typeID = fishCaught.FishName;
            uint value = fishCaught.Value;
            float size = fishCaught.transform.localScale.x; 
            float weight = size * 10f; 

            CaughtFishData newFishData = new CaughtFishData(typeID, size, weight, value);
            playerData.CaughtFishes.Add(newFishData);
            Debug.Log($"Caught a {typeID} (Value: {value})! Added to inventory.");
            
            PlayFishCatchFeedback(fishCaught.transform); 

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
        if (fishRanking != null && fishRanking.Length > 0)
        {
            fishRanking[0].Play(fishTransform); 
        }
    }

    public void AddMoney(uint amount)
    {
        playerData.Money += amount;
        OnMoneyChanged?.Invoke();
        OnInventoryChanged?.Invoke();
        SaveData();
    }

    public bool SpendMoney(uint amount)
    {
        if (playerData.Money >= amount)
        {
            playerData.Money -= amount;
            OnMoneyChanged?.Invoke();
            OnInventoryChanged?.Invoke();
            SaveData();
            return true;
        }
        return false;
    }

    public void SellFish(CaughtFishData fishToSell)
    {
        if (playerData.CaughtFishes.Remove(fishToSell))
        {
            AddMoney(fishToSell.value); 
            OnInventoryChanged?.Invoke();
            SaveData();
        }
    }
    
    public void SellFishByIndex(int index)
    {
        if (index >= 0 && index < playerData.CaughtFishes.Count)
        {
            SellFish(playerData.CaughtFishes[index]);
        }
    }

    public bool UpgradeLineLength(uint additionalLength, uint cost)
    {
        if (SpendMoney(cost))
        {
            playerData.CurrentMaxLineLength += additionalLength;
            OnEquipmentChanged?.Invoke();
            OnInventoryChanged?.Invoke();
            SaveData();
            return true;
        }
        return false;
    }

    // --- Lure Management (Using Hashed IDs) ---
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

    public bool IsLureOwned(uint hashedLureID)
    {
        if (hashedLureID == 0) return false;
        return playerData.OwnedLureIDs.Contains(hashedLureID);
    }

    // BuyLure now takes the Lure object directly, or you can keep a string ID version
    // that finds the lure in allAvailableLures first.
    public bool BuyLure(Lure lureToBuy)
    {
        if (lureToBuy == null)
        {
            Debug.LogError("Lure to buy is null.");
            return false;
        }
        
        uint hashedLureID = lureToBuy.HashedID;
        if (hashedLureID == 0) {
             Debug.LogError($"Lure '{lureToBuy.lureName}' has an invalid HashedID (0). Cannot buy.");
             return false;
        }

        if (IsLureOwned(hashedLureID))
        {
            Debug.Log($"Already own lure: {lureToBuy.lureName} (ID: {hashedLureID})");
            return false;
        }

        if (SpendMoney(lureToBuy.cost))
        {
            playerData.OwnedLureIDs.Add(hashedLureID);
            Debug.Log($"Bought lure: {lureToBuy.lureName} (Hashed ID: {hashedLureID})");
            OnEquipmentChanged?.Invoke(); 
            OnInventoryChanged?.Invoke();
            SaveData();
            return true;
        }
        return false;
    }
    
    // Overload to buy by string ID (which then gets hashed)
    public bool BuyLureByStringID(string stringLureID)
    {
        Lure lureToBuy = allAvailableLures.Find(l => l.lureID == stringLureID);
        if (lureToBuy == null)
        {
            Debug.LogError($"Lure with string ID '{stringLureID}' not found in allAvailableLures.");
            return false;
        }
        return BuyLure(lureToBuy); // Calls the other BuyLure method
    }


    public bool EquipLure(uint hashedLureID)
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
        Debug.Log($"Equipped lure: {equippedLure?.lureName} (HashedID: {hashedLureID})");
        OnEquipmentChanged?.Invoke();
        OnInventoryChanged?.Invoke();
        SaveData();
        return true;
    }
    
    // Overload to equip by string ID
    public bool EquipLureByStringID(string stringLureID)
    {
        Lure lureToEquip = allAvailableLures.Find(l => l.lureID == stringLureID);
        if (lureToEquip == null)
        {
            Debug.LogError($"Lure with string ID '{stringLureID}' not found for equipping.");
            return false;
        }
        return EquipLure(lureToEquip.HashedID);
    }


    public void SaveData()
    {
        try
        {
            string json = JsonUtility.ToJson(playerData, true); 
            File.WriteAllText(_savePath, json);
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
                }
                else
                {
                    playerData = new PlayerData(); 
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load player data: {e.Message}. Creating new PlayerData.");
                playerData = new PlayerData(); 
            }
        }
        else
        {
            playerData = new PlayerData(); 
        }
        OnMoneyChanged?.Invoke();
        OnEquipmentChanged?.Invoke();
        OnInventoryChanged?.Invoke();
    }
}

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