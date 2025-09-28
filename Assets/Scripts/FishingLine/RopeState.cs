using System.Collections.Generic;
using UnityEngine;

public class RopeState
{
    public List<RopeNode> nodes = new List<RopeNode>();
    
    // Hook data
    public Vector2 hookPhysicsTargetPosition;
    public Vector2 hookVisualPosition;
    public Vector2 prevHookPhysicsTargetPosition;
    public Quaternion hookVisualRotation;
}
