using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
                instance = FindFirstObjectByType<CustomRopeSolver>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("CustomRopeSolver_Singleton");
                    instance = obj.AddComponent<CustomRopeSolver>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    [System.Serializable]
    private struct RopeNode
    {
        public Vector2 position; // Physics target position
        public Vector2 prevPosition;
        public Transform transform; // Visual representation of the node
        public Vector2 visualPosition; // Current visual position for interpolation
    }

    public Transform rodTip;
    public GameObject nodePrefab; 
    public GameObject hookPrefab;
    public uint maxNodes => Inventory.Instance.CurrentMaxLineLength;
    public float segmentLength = 0.3f;
    public float gravityScale = 1f; 
    public LineRenderer ropeLine;
    public LayerMask obstacleMask;
    public float nodeRadius = 0.05f; 
    public float collisionBounceFactor = 0.0f; 
    [Range(0.8f, 1.0f)] 
    public float lastNodeVelocityRetention = 0.95f; 

    public float minSettleSpeedFactor = 1.0f; 
    public float maxSettleSpeedFactor = 1.5f; 
    public float settleSpeedEffectDuration = 0.25f; 

    [Header("Visual Interpolation")]
    public float visualInterpolationSpeed = 15f; // Speed for Lerp

    private List<RopeNode> nodes = new List<RopeNode>();
    private Transform hook; 

    private LureType _lure;
    private Rigidbody2D hookRb; 
    
    private float worldGravity = -9.81f; 

    private float maxReelOutInterval = 0.4f, minReelOutInterval = 0.05f;
    private float maxReelInInterval = 0.4f, minReelInInterval = 0.10f;
    private float lastInstanceTime = 0;

    private float currentReelOutSettleSpeedFactor = 1.0f;
    private float reelSettleSpeedEffectTimer = 0f;

    // For hook interpolation
    private Vector2 hookPhysicsTargetPosition;
    private Vector2 hookVisualPosition;
    private Quaternion hookVisualRotation;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("Another instance of CustomRopeSolver found, destroying this one.");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (rodTip == null)
        {
            Debug.LogError("RodTip transform is not assigned in CustomRopeSolver!");
            enabled = false; 
            return;
        }
        if (hookPrefab == null)
        {
            Debug.LogError("Hook Prefab is not assigned in CustomRopeSolver!");
            enabled = false;
            return;
        }

        Vector2 startPos = rodTip.position;
        
        nodes.Add(new RopeNode
        {
            position = startPos,
            prevPosition = startPos,
            transform = null,
            visualPosition = startPos 
        });
        
        for (int i = 0; i < 2; i++) 
        {
            if (nodes.Count >= maxNodes) break; 

            RopeNode previousNode = nodes[nodes.Count - 1];
            Vector2 physPos = previousNode.position - new Vector2(0, segmentLength);
            
            GameObject nodeObj = null;
            if (nodePrefab != null) 
            {
                nodeObj = Instantiate(nodePrefab, physPos, Quaternion.identity, transform);
                Rigidbody2D nodeRb = nodeObj.GetComponent<Rigidbody2D>();
                if (nodeRb != null) nodeRb.bodyType = RigidbodyType2D.Kinematic;
            }
            
            nodes.Add(new RopeNode
            {
                position = physPos,
                prevPosition = physPos,
                transform = nodeObj?.transform,
                visualPosition = physPos 
            });
        }


        hookPhysicsTargetPosition = nodes.Count > 0 ? nodes[nodes.Count - 1].position + Vector2.down * segmentLength : startPos + Vector2.down * segmentLength;
        hookVisualPosition = hookPhysicsTargetPosition;


        GameObject hookObj = Instantiate(hookPrefab, hookVisualPosition, Quaternion.identity, transform);
        _lure = hookObj.GetComponent<LureType>();
        hookRb = hookObj.GetComponent<Rigidbody2D>();
        if (hookRb == null)
        {
            Debug.LogError("Hook prefab must have a Rigidbody2D component!", hookObj);
            hookRb = hookObj.AddComponent<Rigidbody2D>(); 
        }
        hookRb.bodyType = RigidbodyType2D.Kinematic; 
        hook = hookObj.transform;
        hookVisualRotation = hook.rotation; 
    }

    private void FixedUpdate()
    {
        if (rodTip == null || hook == null) return; 
        // Ensure there's at least the rodTip anchor node before proceeding
        if (nodes.Count == 0 && rodTip == null) return; 

        if (reelSettleSpeedEffectTimer > 0)
        {
            reelSettleSpeedEffectTimer -= Time.fixedDeltaTime;
            if (reelSettleSpeedEffectTimer <= 0)
            {
                currentReelOutSettleSpeedFactor = 1.0f; 
                reelSettleSpeedEffectTimer = 0f; 
            }
        }

        Simulate(Time.fixedDeltaTime);
        SolveConstraints();
        CorrectCollisionsAfterConstraints(); 
        
        UpdateHookPhysicsTargetPosition();
    }

    void Update() 
    {
        if (rodTip == null || hook == null) return;
        if (nodes.Count == 0) return;

        RopeNode rootNode = nodes[0];
        rootNode.visualPosition = rodTip.position;
        if (rootNode.transform != null) rootNode.transform.position = rootNode.visualPosition; // Should be null
        nodes[0] = rootNode;

        for (int i = 1; i < nodes.Count; i++)
        {
            RopeNode node = nodes[i];
            if (node.transform != null)
            {
                node.visualPosition = Vector2.Lerp(node.visualPosition, node.position, Time.deltaTime * visualInterpolationSpeed);
                node.transform.position = node.visualPosition;
                nodes[i] = node; 
            }
            else 
            {
                 node.visualPosition = node.position;
                 nodes[i] = node;
            }
        }

        // Interpolate hook's visual position and rotation
        hookVisualPosition = Vector2.Lerp(hookVisualPosition, hookPhysicsTargetPosition, Time.deltaTime * visualInterpolationSpeed);
        hook.position = hookVisualPosition;

        if (nodes.Count > 0) // Check if there are any nodes to determine hook rotation
        {
            // Hook rotation should be based on the last *segment's* visual orientation
            // If only nodes[0] (rodTip) exists, there's no segment yet to orient from, use default.
            Vector2 lastSegmentStartVisual = (nodes.Count > 1) ? nodes[nodes.Count - 2].visualPosition : nodes[0].visualPosition;
            Vector2 lastNodeVisual = nodes[nodes.Count - 1].visualPosition;

            // The direction for the hook should be from the last node to the hook itself
            Vector2 directionToHookVisual = (hookVisualPosition - lastNodeVisual).normalized;

            if (directionToHookVisual != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(directionToHookVisual.y, directionToHookVisual.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.AngleAxis(targetAngle - 90f, Vector3.forward);
                hookVisualRotation = Quaternion.Lerp(hookVisualRotation, targetRotation, Time.deltaTime * visualInterpolationSpeed);
                hook.rotation = hookVisualRotation;
            } else if (nodes.Count == 1) { // Only rodTip node, hook hangs directly below
                 Quaternion targetRotation = Quaternion.Euler(0,0,-90f);
                 hookVisualRotation = Quaternion.Lerp(hookVisualRotation, targetRotation, Time.deltaTime * visualInterpolationSpeed);
                 hook.rotation = hookVisualRotation;
            }
        } else if (rodTip != null) { // No nodes at all (should not happen if Start initializes one)
             Quaternion targetRotation = Quaternion.Euler(0,0,-90f);
             hookVisualRotation = Quaternion.Lerp(hookVisualRotation, targetRotation, Time.deltaTime * visualInterpolationSpeed);
             hook.rotation = hookVisualRotation;
        }


        DrawRope(); 
    }


    private void Simulate(float dt)
    {
        // Node 0 is the rod tip anchor, its physics position is set in SolveConstraints.
        // So simulation starts from node 1.
        for (int i = 1; i < nodes.Count; i++) 
        {
            RopeNode n = nodes[i];
            Vector2 velocity = n.position - n.prevPosition; 

            if (i == nodes.Count - 1 && nodes.Count > 1) // Apply drag only if it's the actual last segment end 
            {
                velocity *= lastNodeVelocityRetention;
            }
            
            Vector2 currentPositionForCast = n.position; 
            n.prevPosition = currentPositionForCast;     

            Vector2 tentativePosition = currentPositionForCast + velocity + (Vector2.up * worldGravity * gravityScale * dt * dt);
            
            Vector2 movementVector = tentativePosition - currentPositionForCast;
            float movementDistance = movementVector.magnitude;

            if (movementDistance > 0.0001f) 
            {
                RaycastHit2D hit = Physics2D.CircleCast(currentPositionForCast, nodeRadius, movementVector.normalized, movementDistance, obstacleMask);

                if (hit.collider != null)
                {
                    Vector2 collisionNormal = hit.normal; 
                    if (hit.distance < 0.001f) 
                    {
                        n.position = currentPositionForCast + collisionNormal * (nodeRadius * 1.01f + 0.001f); 
                    }
                    else
                    {
                        n.position = hit.point + collisionNormal * (nodeRadius * 0.05f); 
                    }

                    float vn_scalar = Vector2.Dot(velocity, collisionNormal); 
                    Vector2 v_normalComponent = vn_scalar * collisionNormal;    
                    Vector2 v_tangentialComponent = velocity - v_normalComponent; 
                    Vector2 velocityAfterCollision = v_tangentialComponent - (v_normalComponent * collisionBounceFactor);
                    n.prevPosition = n.position - velocityAfterCollision; 
                }
                else
                {
                    n.position = tentativePosition;
                }
            }
            else 
            {
                 n.position = tentativePosition; 
            }
            nodes[i] = n;
        }
    }

    private void SolveConstraints()
    {
        if (nodes.Count == 0) return;

        RopeNode rootNode = nodes[0];
        rootNode.position = rodTip.position;
        rootNode.prevPosition = rodTip.position; 
        nodes[0] = rootNode;


        for (int iter = 0; iter < 15; iter++) 
        {
            for (int i = 0; i < nodes.Count - 1; i++) 
            {
                RopeNode nodeA = nodes[i];
                RopeNode nodeB = nodes[i + 1];

                Vector2 delta = nodeB.position - nodeA.position;
                float currentDistance = delta.magnitude;
                float error = 0f;

                if (currentDistance > 0.0001f) 
                   error = (currentDistance - segmentLength) / currentDistance;
                else 
                {
                     error = (currentDistance - segmentLength) / (segmentLength + 0.0001f); 
                }

                Vector2 correction = delta * 0.5f * error;

                if (i == 0 && reelSettleSpeedEffectTimer > 0 && nodes.Count > 1)
                {
                    if (currentDistance < segmentLength) 
                    {
                        correction *= currentReelOutSettleSpeedFactor;
                    }
                }

                if (i != 0) 
                {
                    nodeA.position += correction;
                    nodes[i] = nodeA;
                }
                nodeB.position -= correction;
                nodes[i + 1] = nodeB;
            }
        }
    }

    private void CorrectCollisionsAfterConstraints()
    {
        // Start from 1, node 0 is rod tip and its position is sacred
        for (int i = 1; i < nodes.Count; i++) 
        {
            RopeNode n = nodes[i];
            Collider2D hit = Physics2D.OverlapCircle(n.position, nodeRadius, obstacleMask);

            if (hit != null) 
            {
                Vector2 closestPointOnCollider = hit.ClosestPoint(n.position);
                Vector2 directionFromCollider = (n.position - closestPointOnCollider).normalized;
                if (directionFromCollider.sqrMagnitude < 0.0001f)
                {
                    directionFromCollider = (n.position - (Vector2)hit.bounds.center).normalized;
                    if (directionFromCollider.sqrMagnitude < 0.0001f) 
                    {
                        directionFromCollider = Vector2.up; 
                    }
                }
                n.position = closestPointOnCollider + directionFromCollider * (nodeRadius + 0.001f); 
                nodes[i] = n; 
            }
        }
    }
    
    private void UpdateHookPhysicsTargetPosition() 
    {
        if (nodes.Count > 0) // Check if there's at least the rodTip node
        {
            RopeNode lastPhysicsNode = nodes[nodes.Count - 1]; 
            Vector2 directionFromPreviousPhysicsNode;

            if (nodes.Count > 1) // If there's an actual segment before the last node
            {
                RopeNode secondLastPhysicsNode = nodes[nodes.Count - 2];
                if (Vector2.SqrMagnitude(lastPhysicsNode.position - secondLastPhysicsNode.position) > 0.00001f)
                {
                    directionFromPreviousPhysicsNode = (lastPhysicsNode.position - secondLastPhysicsNode.position).normalized;
                }
                else 
                {
                    // Fallback: direction from rodTip to last node if secondLast and last are coincident
                    directionFromPreviousPhysicsNode = (lastPhysicsNode.position - nodes[0].position).normalized; 
                    if (directionFromPreviousPhysicsNode == Vector2.zero) 
                    {
                        directionFromPreviousPhysicsNode = Vector2.down; // Absolute fallback
                    }
                }
            }
            else // Only one node (nodes[0], the rodTip anchor), hook hangs directly below it
            {
                directionFromPreviousPhysicsNode = Vector2.down; 
            }
            
            hookPhysicsTargetPosition = lastPhysicsNode.position + directionFromPreviousPhysicsNode * segmentLength;
        }
        else if (rodTip != null) // Should not be reached if Start() guarantees nodes[0]
        {
            hookPhysicsTargetPosition = rodTip.position;
        }
    }

    private void DrawRope() 
    {
        if (!ropeLine) return;
        if (nodes.Count == 0) { // Need at least the anchor for drawing to hook
             ropeLine.positionCount = 0;
             return;
        }


        // LineRenderer uses visual positions
        int pointCount = nodes.Count + 1; // nodes[0]...nodes[N-1] + hook

        ropeLine.positionCount = pointCount;
        
        // First point of LineRenderer is always nodes[0].visualPosition (rodTip)
        ropeLine.SetPosition(0, nodes[0].visualPosition);

        for (int i = 1; i < nodes.Count; i++) // Draw segments from nodes[1] onwards
        {
            ropeLine.SetPosition(i, nodes[i].visualPosition); 
        }
        // Last point of LineRenderer is the hook's visual position
        ropeLine.SetPosition(nodes.Count, hookVisualPosition); 
    }

    public async UniTask ReelIn(float inputAmount) // Changed to async UniTask
    {
        float interval = Mathf.Lerp(maxReelInInterval, minReelInInterval, inputAmount);
        if (Time.time < lastInstanceTime + interval) return;

        if (nodes.Count > 1)
        {
            lastInstanceTime = Time.time;
            RopeNode removedNode = nodes[1];
            if (removedNode.transform != null)
            {
                Destroy(removedNode.transform.gameObject);
            }
            nodes.RemoveAt(1);
        }
        else
        {
            // Await the TryCatchFish method
            await TryCatchFish();
        }
    }

    public async UniTask ReelOut(float inputAmount) // Changed to async UniTask
    {
        if (nodes.Count >= maxNodes) return;

        float interval = Mathf.Lerp(maxReelOutInterval, minReelOutInterval, inputAmount);
        if (Time.time < lastInstanceTime + interval) return;
        
        if (nodes.Count > 0 && nodes.Count < maxNodes) 
        {
            // Yield to make the method truly async and prevent compiler warnings inside this method
            await UniTask.Yield(); 
            
            lastInstanceTime = Time.time;
            
            RopeNode rodTipNode = nodes[0];
            Vector2 initialNewNodePhysicsPos = rodTipNode.position; 
            Vector2 initialVisualPos = rodTipNode.visualPosition; 

            GameObject nodeObj = null;
            if (nodePrefab != null)
            {
                nodeObj = Instantiate(nodePrefab, initialVisualPos, Quaternion.identity, transform);
                Rigidbody2D nodeRb = nodeObj.GetComponent<Rigidbody2D>();
                if (nodeRb != null) nodeRb.bodyType = RigidbodyType2D.Kinematic;
            }

            RopeNode newNode = new RopeNode
            {
                position = initialNewNodePhysicsPos, 
                prevPosition = initialNewNodePhysicsPos,
                transform = nodeObj?.transform,
                visualPosition = initialVisualPos 
            };

            nodes.Insert(1, newNode);

            currentReelOutSettleSpeedFactor = Mathf.Lerp(minSettleSpeedFactor, maxSettleSpeedFactor, inputAmount);
            reelSettleSpeedEffectTimer = settleSpeedEffectDuration;
        }
    }

    public Transform GetHook() => hook;

    public LureType GetLure() => _lure;

    
    public async UniTask TryCatchFish() // Changed to async UniTask
    {
        FishStats caughtFish = _lure.GetCurrentCatch();
        if (caughtFish != null)
        {
            // Now we properly await the async operation
            await Inventory.Instance.AddCaughtFish(caughtFish);
            
            _lure.DestroyCatch();
        }
    }



    public void ApplyMovementToLastNode(Vector2 displacementThisFrame)
    {
        if (nodes.Count > 0) // Check if there's at least the rodTip node
        {
            // Apply movement to the actual last node in the list
            RopeNode lastNode = nodes[nodes.Count - 1];
            lastNode.position += displacementThisFrame; 
            nodes[nodes.Count - 1] = lastNode;
        }
    }
}
