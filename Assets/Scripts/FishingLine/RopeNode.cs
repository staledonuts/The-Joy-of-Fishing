using UnityEngine;

[System.Serializable]
public struct RopeNode
{
    public Vector2 position; // Physics target position
    public Vector2 prevPosition;
    public Transform transform; // Visual representation of the node
    public Vector2 visualPosition; // Current visual position for interpolation
}
