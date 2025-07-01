using UnityEngine;

[System.Serializable]
public class Lure
{
    [Tooltip("Unique string identifier for this lure, e.g., 'BasicLure', 'ShinySpinner'. This will be hashed.")]
    [SerializeField] private string _lureID; // This is the string ID that will be hashed.
    [SerializeField] private string _lureName;
    [SerializeField] private string _description;
    [SerializeField] private uint _cost;
    [SerializeField] private LureID _gameplayLureType;

    private uint _hashedID = 0;
    private bool _isHashedIDCalculated = false;

    public string LureID => _lureID;
    public string LureName => _lureName;
    public string Description => _description;
    public uint Cost => _cost;
    public LureID GameplayLureType => _gameplayLureType;

    /// <summary>
    /// Gets the FNV1a hashed ID of the lureID string. Calculated on first access.
    /// </summary>
    public uint HashedID
    {
        get
        {
            if (!_isHashedIDCalculated)
            {
                if (string.IsNullOrEmpty(_lureID))
                {
                    Debug.LogError($"Lure '{_lureName}' has an empty or null lureID. Cannot generate HashedID.");
                    _hashedID = 0; // Or some other default error value
                }
                else
                {
                    _hashedID = FNV1aHash.Calculate(_lureID);
                }
                _isHashedIDCalculated = true;
            }
            return _hashedID;
        }
    }

    public Lure(string id, string name, string desc, uint itemCost)
    {
        _lureID = id; // Set the string ID
        _lureName = name;
        _description = desc;
        _cost = itemCost;
        // HashedID will be calculated on first access
    }

    // Default constructor for serialization if needed
    public Lure() { }
}
