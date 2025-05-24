    using UnityEngine;

    [System.Serializable]
    public class CaughtFishData
    {
        public string fishTypeID;
        public float size;
        public float weight;
        public uint value;

        public CaughtFishData(string typeId, float fishSize, float fishWeight, uint fishValue)
        {
            fishTypeID = typeId;
            size = fishSize;
            weight = fishWeight;
            value = fishValue;
        }
    }
    