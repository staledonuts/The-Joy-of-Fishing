using System.Collections.Generic;
using UnityEngine;

public class CustomRopeSolver : MonoBehaviour
{
    public Transform rodTip; // Attach your rod tip here
    public GameObject nodePrefab; // Prefab with Kinematic Rigidbody2D and small collider
    public int maxNodes = 50;
    public float segmentLength = 0.3f;
    public float tightenFactor = 0.5f; // Higher = snappier rope
    public LineRenderer ropeLine;

    private List<Rigidbody2D> nodes = new List<Rigidbody2D>();
    private Transform hook;

    private void Start()
    {
        // Create initial rope
        Vector2 startPos = rodTip.position;
        for (int i = 0; i < maxNodes; i++)
        {
            Vector2 pos = startPos - new Vector2(0, i * segmentLength);
            GameObject node = Instantiate(nodePrefab, pos, Quaternion.identity, transform);
            var rb = node.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            nodes.Add(rb);
        }

        // Hook is the last node
        hook = nodes[nodes.Count - 1].transform;
    }

    private void FixedUpdate()
    {
        ApplyConstraints();
        DrawRope();
    }

    private void ApplyConstraints()
    {
        // First node follows the rod tip
        nodes[0].position = rodTip.position;

        for (int i = 1; i < nodes.Count; i++)
        {
            var prev = nodes[i - 1];
            var cur = nodes[i];

            Vector2 dir = cur.position - prev.position;
            float dist = dir.magnitude;
            float error = dist - segmentLength;
            Vector2 correction = dir.normalized * error * tightenFactor;

            // Only apply to current node (prev is fixed unless first frame)
            cur.position -= correction;
        }
    }

    private void DrawRope()
    {
        if (ropeLine == null) return;

        ropeLine.positionCount = nodes.Count;
        for (int i = 0; i < nodes.Count; i++)
        {
            ropeLine.SetPosition(i, nodes[i].position);
        }
    }

    public void ReelIn()
    {
        if (nodes.Count > 2)
        {
            Destroy(nodes[nodes.Count - 1].gameObject);
            nodes.RemoveAt(nodes.Count - 1);
        }
    }

    public void ReelOut()
    {
        if (nodes.Count >= maxNodes) return;

        Vector2 lastDir = nodes[nodes.Count - 1].position - nodes[nodes.Count - 2].position;
        Vector2 newPos = nodes[nodes.Count - 1].position + lastDir.normalized * segmentLength;

        GameObject node = Instantiate(nodePrefab, newPos, Quaternion.identity, transform);
        var rb = node.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        nodes.Add(rb);
    }

    public Transform GetHook()
    {
        return hook;
    }
}
