using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public uint Money;
    public List<CaughtFishData> CaughtFishes;
    public uint CurrentMaxLineLength;
    public bool RadioControlLure;
    public uint EquippedLureID;
    public List<uint> OwnedLureIDs;
    public List<uint> DestroyedBlockerIDs;

    public PlayerData()
    {
        Money = 50;
        CaughtFishes = new List<CaughtFishData>();
        CurrentMaxLineLength = 300;
        RadioControlLure = false;
        EquippedLureID = 0;
        OwnedLureIDs = new List<uint>();
        DestroyedBlockerIDs = new List<uint>();
    }
}
