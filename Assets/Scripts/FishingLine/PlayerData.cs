using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public uint money;
    public List<CaughtFishData> caughtFishes;

    // Fishing line and lure data
    public uint currentMaxLineLength;
    public string equippedLureID;
    public List<string> ownedLureIDs; // Stores the lureID strings

    // Default values for a new game
    public PlayerData()
    {
        money = 50; // Starting money
        caughtFishes = new List<CaughtFishData>();
        currentMaxLineLength = 300; // Default starting line length
        
        // Assuming a "BasicLure" exists and is owned by default
        equippedLureID = "BasicLureID"; // Make sure this ID matches a Lure defined in your game
        ownedLureIDs = new List<string> { "BasicLureID" };
    }
}
