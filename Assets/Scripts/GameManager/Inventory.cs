using System;
using System.Collections.Generic;
using Ami.BroAudio;
using UnityEngine;

// Placeholder for your Fish class/ScriptableObject.
// Ensure you have this defined in your project. It's the type that FishStats.fishStats refers to.
/*
[CreateAssetMenu(fileName = "New FishData", menuName = "Fish/Fish Data")]
public class Fish : ScriptableObject // Or 'public class Fish' if not a ScriptableObject
{
    public string fishName = "Unnamed Fish";
    public Sprite sprite; // Used by FishStats
    public Color fishColor = Color.white; // Used by FishStats
    public RuntimeAnimatorController animatorController; // Used by FishStats
    public uint value = 10; // Example: value of the fish
    // Add other properties like rarity, description, etc.
}
*/

public sealed class Inventory : MonoBehaviour
{
    private static Inventory instance = null;
    public static Inventory Instance
    {
        get
        {
            if (instance == null)
            {
                // Find singleton of this type in the scene
                instance = FindFirstObjectByType<Inventory>();

                // If there is no singleton object in the scene, create one
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("Inventory_Singleton");
                    instance = singletonObject.AddComponent<Inventory>();
                }
            }
            return instance;
        }
    }

    // Current amount of money the player has
    private uint _money = 0;

    public uint Money
    {
        get => _money;
    }
    [SerializeField] private FishRanking[] fishRanking;
    // List to store all fish caught by the player
    // Make sure the 'Fish' type matches the type of 'fishStats.fishStats' in your FishStats script
    private List<FishStats> _caughtFishes = new List<FishStats>();

    private void Awake()
    {   
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Debug.LogWarning("Another instance of Inventory found, destroying this new one.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Adds a caught fish to the inventory.
    /// </summary>
    /// <param name="fishToAdd">The Fish object (e.g., from FishStats.fishStats) to add to the inventory.</param>
    public void AddCaughtFish(FishStats fishToAdd)
    {
        if (fishToAdd != null)
        {
            _caughtFishes.Add(fishToAdd);
            Debug.Log($"Caught a {fishToAdd.name}! Added to inventory.");
            // Optionally, you could add money here if catching a fish directly gives money
            // AddMoney(fishToAdd.value); 
        }
        else
        {
            Debug.LogWarning("Attempted to add a null fish to inventory.");
        }
    }

    /// <summary>
    /// Adds a specified amount of money to the player's total.
    /// </summary>
    /// <param name="amount">The amount of money to add.</param>
    public void AddMoney(uint amount)
    {
        _money += amount;
        Debug.Log($"Added {amount} money. Total money: {_money}");
        // Here you might want to trigger a UI update for the money display
    }

    /// <summary>
    /// Attempts to spend a specified amount of money.
    /// </summary>
    /// <param name="amount">The amount of money to spend.</param>
    /// <returns>True if the money was successfully spent, false otherwise (e.g., insufficient funds).</returns>
    public bool SpendMoney(uint amount)
    {
        if (_money >= amount)
        {
            _money -= amount;
            Debug.Log($"Spent {amount} money. Remaining money: {_money}");
            // Here you might want to trigger a UI update for the money display
            return true;
        }
        else
        {
            Debug.LogWarning($"Attempted to spend {amount} money, but only have {_money}. Transaction failed.");
            return false;
        }
    }

    
    public void SellFish(FishStats fishToSell)
    {
        if (_caughtFishes.Contains(fishToSell))
        {
            _caughtFishes.Remove(fishToSell);
            AddMoney(fishToSell.Value); // Assuming Fish has a 'value' property
            Debug.Log($"Sold {fishToSell.FishName} for {fishToSell.Value}.");
        }
        else
        {
            Debug.LogWarning($"Attempted to sell {fishToSell.name}, but it's not in the inventory.");
        }
    }
}

[Serializable]
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
