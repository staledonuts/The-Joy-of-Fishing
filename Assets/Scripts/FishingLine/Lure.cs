using UnityEngine;

[System.Serializable]
public class Lure
{
    [Tooltip("Unique string identifier for this lure, e.g., 'BasicLure', 'ShinySpinner'. This will be hashed.")]
    public string lureID; // This is the string ID that will be hashed.
    public string lureName;
    public string description;
    public uint cost; 

    private uint _hashedID = 0;
    private bool _isHashedIDCalculated = false;

    /// <summary>
    /// Gets the FNV1a hashed ID of the lureID string. Calculated on first access.
    /// </summary>
    public uint HashedID
    {
        get
        {
            if (!_isHashedIDCalculated)
            {
                if (string.IsNullOrEmpty(lureID))
                {
                    Debug.LogError($"Lure '{lureName}' has an empty or null lureID. Cannot generate HashedID.");
                    _hashedID = 0; // Or some other default error value
                }
                else
                {
                    _hashedID = FNV1aHash.Calculate(lureID);
                }
                _isHashedIDCalculated = true;
            }
            return _hashedID;
        }
    }

    public Lure(string id, string name, string desc, uint itemCost)
    {
        lureID = id; // Set the string ID
        lureName = name;
        description = desc;
        cost = itemCost;
        // HashedID will be calculated on first access
    }

    // Default constructor for serialization if needed
    public Lure() { }
}
