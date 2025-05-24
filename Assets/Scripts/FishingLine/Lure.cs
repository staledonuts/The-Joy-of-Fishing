using UnityEngine;

[System.Serializable]
public class Lure
{
    public string lureID;
    public string lureName;
    public string description;
    public uint cost;

    public Lure(string id, string name, string desc, uint itemCost)
    {
        lureID = id;
        lureName = name;
        description = desc;
        cost = itemCost;
    }
}