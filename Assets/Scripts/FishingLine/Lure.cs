using UnityEngine;

[System.Serializable]
public class Lure
{
    public string lureID; // Unique identifier, e.g., "BasicLure", "ShinySpinner"
    public string lureName;
    public string description;
    public uint cost; // Cost to buy this lure
    // Add other properties like +catchRate, specific fish attraction, visual prefab, etc.

    public Lure(string id, string name, string desc, uint itemCost)
    {
        lureID = id;
        lureName = name;
        description = desc;
        cost = itemCost;
    }
}