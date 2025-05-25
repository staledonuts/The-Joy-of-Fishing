using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public uint Money;
    public List<CaughtFishData> CaughtFishes;
    public uint CurrentMaxLineLength;
    public bool RadioControlLure;
    public string EquippedLureID;
    public List<string> OwnedLureIDs;

    public PlayerData()
    {
        Money = 50;
        CaughtFishes = new List<CaughtFishData>();
        CurrentMaxLineLength = 300;
        RadioControlLure = false;
        EquippedLureID = "BasicLureID";
        OwnedLureIDs = new List<string> { "BasicLureID" };
    }
}
