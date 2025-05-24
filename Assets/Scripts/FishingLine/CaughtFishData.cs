    using UnityEngine;

    [System.Serializable]
    public class CaughtFishData
    {
        public string fishTypeID; // e.g., the name or an ID from your Fish ScriptableObject/data class
        public float size;      // Example: if you track size of caught fish
        public float weight;    // Example: if you track weight
        public uint value;      // Store the value at the time of catching or derive from fishTypeID

        // Constructor
        public CaughtFishData(string typeId, float fishSize, float fishWeight, uint fishValue)
        {
            fishTypeID = typeId;
            size = fishSize;
            weight = fishWeight;
            value = fishValue;
        }
    }
    