using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public uint money;
    public List<CaughtFishData> caughtFishes;
    public uint currentMaxLineLength;
    public string equippedLureID;
    public List<string> ownedLureIDs;

    public PlayerData()
    {
        money = 50;
        caughtFishes = new List<CaughtFishData>();
        currentMaxLineLength = 300;
        
        equippedLureID = "BasicLureID";
        ownedLureIDs = new List<string> { "BasicLureID" };
    }
}
