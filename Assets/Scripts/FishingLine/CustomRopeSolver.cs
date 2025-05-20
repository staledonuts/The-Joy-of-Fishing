using System.Collections.Generic;
using UnityEngine;

public sealed class CustomRopeSolver : MonoBehaviour
{
    private static CustomRopeSolver instance = null;

    public static CustomRopeSolver Instance
    {
        get
        {
            if (instance == null)
            {
                // Find singleton of this type in the scene
                instance = FindFirstObjectByType<CustomRopeSolver>();
                // If there is no singleton object in the scene, we have to add one
                if (instance == null)
                {
                    GameObject obj = new GameObject("GameManager Singelton");
                    instance = obj.AddComponent<CustomRopeSolver>();
                    // The singleton object shouldn't be destroyed when we switch between scenes
                    DontDestroyOnLoad(obj);
                }
            }

            return instance;
        }
    }

    [System.Serializable]
    private struct RopeNode
    {
        public Vector2 position;
        public Vector2 prevPosition;
        public Transform transform;
    }
    public Transform rodTip;
    public GameObject nodePrefab;
    public GameObject hookPrefab;
    public int maxNodes = 50;
    public float segmentLength = 0.3f;
    public float tightenFactor = 0.5f;
    public float gravityScale = 1f;
    public LineRenderer ropeLine;
    public LayerMask obstacleMask;
    public float nodeRadius = 0.05f;

    private List<RopeNode> nodes = new List<RopeNode>();
    private Transform hook;
    private Rigidbody2D hookRb;
    private float gravity = -9.81f;
    private HashSet<(int, int)> wrappedSegments = new();

    private float maxReelOutInterval = 0.4f, minReelOutInterval = 0.05f;
    private float maxReelInInterval = 0.4f, minReelInInterval = 0.10f;
    private float lastInstanceTime = 0;



    private void Start()
    {
        Vector2 startPos = rodTip.position;

        // Initialize static nodes
        for (int i = 0; i < 3; i++)
        {
            Vector2 pos = startPos - new Vector2(0, i * segmentLength);
            GameObject nodeObj = Instantiate(nodePrefab, pos, Quaternion.identity, transform);
            nodeObj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

            nodes.Add(new RopeNode
            {
                position = pos,
                prevPosition = pos,
                transform = nodeObj.transform
            });
        }

        // Add the hook
        GameObject hookObj = Instantiate(hookPrefab, startPos - new Vector2(0, 3 * segmentLength), Quaternion.identity, transform);
        hookRb = hookObj.GetComponent<Rigidbody2D>();
        hookRb.bodyType = RigidbodyType2D.Dynamic;
        hook = hookObj.transform;
    }

    private void FixedUpdate()
    {
        Simulate(Time.fixedDeltaTime);
        SolveConstraints();
        CheckWrapping();
        UpdateTransforms();
        DrawRope();
    }

    private void Simulate(float dt)
    {
        for (int i = 1; i < nodes.Count; i++)
        {
            RopeNode n = nodes[i];
            Vector2 velocity = n.position - n.prevPosition;
            n.prevPosition = n.position;
            n.position += velocity;
            n.position += Vector2.up * gravity * gravityScale * dt * dt;

            Collider2D hit = Physics2D.OverlapCircle(n.position, nodeRadius, obstacleMask);
            if (hit)
            {
                Vector2 closest = hit.ClosestPoint(n.position);
                Vector2 pushDir = (n.position - closest).normalized;
                n.position = closest + pushDir * nodeRadius;
            }

            nodes[i] = n;
        }
    }

private void SolveConstraints()
{
    RopeNode root = nodes[0];
    root.position = rodTip.position;
    nodes[0] = root;

    for (int iter = 0; iter < 5; iter++)
    {
        for (int i = 1; i < nodes.Count; i++)
        {
            Vector2 a = nodes[i - 1].position;
            Vector2 b = nodes[i].position;
            float dist = Vector2.Distance(a, b);
            Vector2 dir = (b - a).normalized;
            float error = dist - segmentLength;
            Vector2 correction = dir * error * 0.5f;

            if (i != 1)
            {
                RopeNode prevNode = nodes[i - 1];
                prevNode.position += correction;
                nodes[i - 1] = prevNode;
            }

            RopeNode node = nodes[i];
            node.position -= correction;
            nodes[i] = node;
        }
    }

    // Final constraint: hook
    Vector2 endDir = (Vector2)hook.position - nodes[nodes.Count - 1].position;
    float hookDist = endDir.magnitude;
    if (hookDist > segmentLength)
    {
        Vector2 correction = endDir.normalized * (hookDist - segmentLength);
        hookRb.AddForce(-correction * 500f);
    }
}
private void CheckWrapping()
    {
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            Vector2 a = nodes[i].position;
            Vector2 b = nodes[i + 1].position;
            float dist = Vector2.Distance(a, b);
            Vector2 dir = (b - a).normalized;

            if (!wrappedSegments.Contains((i, i + 1)))
            {
                RaycastHit2D hit = Physics2D.Raycast(a, dir, dist, obstacleMask);
                if (hit.collider)
                {
                    Vector2 hitPoint = hit.point;
                    GameObject wrapObj = Instantiate(nodePrefab, hitPoint, Quaternion.identity, transform);
                    wrapObj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

                    RopeNode wrapNode = new RopeNode
                    {
                        position = hitPoint,
                        prevPosition = hitPoint,
                        transform = wrapObj.transform
                    };

                    nodes.Insert(i + 1, wrapNode);
                    wrappedSegments.Add((i, i + 1));
                    break;
                }
            }
        }
    }

    private void UpdateTransforms()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].transform.position = nodes[i].position;
        }
    }

    private void DrawRope()
    {
        if (!ropeLine) return;

        ropeLine.positionCount = nodes.Count + 1;
        for (int i = 0; i < nodes.Count; i++)
        {
            ropeLine.SetPosition(i, nodes[i].position);
        }
        ropeLine.SetPosition(nodes.Count, hook.position);
    }

    public void ReelIn(float inputAmount)
    {
        float nextTimeStamp = lastInstanceTime + Mathf.Lerp(maxReelInInterval, minReelInInterval, inputAmount);
        if (Time.time > nextTimeStamp && nodes.Count > 2)
        {
            lastInstanceTime = Time.time;
            Destroy(nodes[^1].transform.gameObject);
            nodes.RemoveAt(nodes.Count - 1);
        }
    }

    public void ReelOut(float inputAmount)
    {
        if (nodes.Count >= maxNodes) return;

        float nextTimeStamp = lastInstanceTime + Mathf.Lerp(maxReelOutInterval, minReelOutInterval, inputAmount);
        if (Time.time > nextTimeStamp)
        {
            lastInstanceTime = Time.time;
            Vector2 lastDir = nodes[^1].position - nodes[^2].position;
            Vector2 newPos = nodes[^1].position + lastDir.normalized * segmentLength;

            GameObject nodeObj = Instantiate(nodePrefab, newPos, Quaternion.identity, transform);
            nodeObj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

            RopeNode newNode = new RopeNode
            {
                position = newPos,
                prevPosition = newPos,
                transform = nodeObj.transform
            };

            nodes.Add(newNode);
        }
    }

    public Transform GetHook() => hook;

    public void MoveHook(Vector2 vec2)
    {
        if (hookRb != null)
        {
            hookRb.AddForce(vec2 * 50f);
        }
    }
}