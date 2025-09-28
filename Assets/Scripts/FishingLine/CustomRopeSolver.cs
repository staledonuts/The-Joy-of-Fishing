using DonutPackage.EventBus;
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

    // State is now in a separate class
    private RopeState ropeState = new RopeState();
    private RopeProcessor ropeProcessor = new RopeProcessor();

    [Header("Configuration")]
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

    private Transform hook;
    private LureType _lure; 
    private float _worldGravity = -9.81f; 
    private bool _isBusy = false;
    private float maxReelOutInterval = 0.4f, minReelOutInterval = 0.05f;
    private float maxReelInInterval = 0.4f, minReelInInterval = 0.10f;
    private float lastInstanceTime = 0;

    private float currentReelOutSettleSpeedFactor = 1.0f;
    private float reelSettleSpeedEffectTimer = 0f;
    private Vector2 _hookExternalForce;

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
        
        ropeState.nodes.Add(new RopeNode
        {
            position = startPos,
            prevPosition = startPos,
            transform = null,
            visualPosition = startPos 
        });
        
        for (int i = 0; i < 2; i++) 
        {
            if (ropeState.nodes.Count >= maxNodes) break; 

            RopeNode previousNode = ropeState.nodes[ropeState.nodes.Count - 1];
            Vector2 physPos = previousNode.position - new Vector2(0, segmentLength);
            
            GameObject nodeObj = null;
            if (nodePrefab != null) 
            {
                nodeObj = Instantiate(nodePrefab, physPos, Quaternion.identity, transform);
                Rigidbody2D nodeRb = nodeObj.GetComponent<Rigidbody2D>();
                if (nodeRb != null) nodeRb.bodyType = RigidbodyType2D.Kinematic;
            }
            
            ropeState.nodes.Add(new RopeNode
            {
                position = physPos,
                prevPosition = physPos,
                transform = nodeObj?.transform,
                visualPosition = physPos 
            });
        }

        ropeState.hookPhysicsTargetPosition = ropeState.nodes.Count > 0 ? ropeState.nodes[ropeState.nodes.Count - 1].position + Vector2.down * segmentLength : startPos + Vector2.down * segmentLength;
        ropeState.prevHookPhysicsTargetPosition = ropeState.hookPhysicsTargetPosition;
        ropeState.hookVisualPosition = ropeState.hookPhysicsTargetPosition;

        GameObject hookObj = Instantiate(hookPrefab, ropeState.hookVisualPosition, Quaternion.identity, transform);
        _lure = hookObj.GetComponent<LureType>();
        hook = hookObj.transform;
        ropeState.hookVisualRotation = hook.rotation; 
    }

    private void FixedUpdate()
    {
        if (rodTip == null || hook == null) return; 
        if (ropeState.nodes.Count == 0 && rodTip == null) return; 

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
        SimulateHook(Time.fixedDeltaTime);
        ropeProcessor.SolveConstraints(ropeState, segmentLength, rodTip.position, reelSettleSpeedEffectTimer, currentReelOutSettleSpeedFactor);
        CorrectCollisionsAfterConstraints();
    }

    private void SimulateHook(float dt)
    {
        if (hook == null) return;

        Vector2 velocity = ropeState.hookPhysicsTargetPosition - ropeState.prevHookPhysicsTargetPosition;
        ropeState.prevHookPhysicsTargetPosition = ropeState.hookPhysicsTargetPosition;

        Vector2 acceleration = (Vector2.up * _worldGravity * gravityScale) + _hookExternalForce;
        ropeState.hookPhysicsTargetPosition += velocity + acceleration * (dt * dt);

        _hookExternalForce = Vector2.zero;
    }
    
    private void Update() 
    {
        if (rodTip == null || hook == null) return;
        if (ropeState.nodes.Count == 0) return;

        RopeNode rootNode = ropeState.nodes[0];
        rootNode.visualPosition = rodTip.position;
        if (rootNode.transform != null) rootNode.transform.position = rootNode.visualPosition;
        ropeState.nodes[0] = rootNode;

        for (int i = 1; i < ropeState.nodes.Count; i++)
        {
            RopeNode node = ropeState.nodes[i];
            if (node.transform != null)
            {
                node.visualPosition = Vector2.Lerp(node.visualPosition, node.position, Time.deltaTime * visualInterpolationSpeed);
                node.transform.position = node.visualPosition;
                ropeState.nodes[i] = node; 
            }
            else 
            {
                 node.visualPosition = node.position;
                 ropeState.nodes[i] = node;
            }
        }

        ropeState.hookVisualPosition = Vector2.Lerp(ropeState.hookVisualPosition, ropeState.hookPhysicsTargetPosition, Time.deltaTime * visualInterpolationSpeed);
        hook.position = ropeState.hookVisualPosition;

        if (ropeState.nodes.Count > 0)
        {
            Vector2 lastNodeVisual = ropeState.nodes[ropeState.nodes.Count - 1].visualPosition;
            Vector2 directionToHookVisual = (ropeState.hookVisualPosition - lastNodeVisual).normalized;

            if (directionToHookVisual != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(directionToHookVisual.y, directionToHookVisual.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.AngleAxis(targetAngle - 90f, Vector3.forward);
                ropeState.hookVisualRotation = Quaternion.Lerp(ropeState.hookVisualRotation, targetRotation, Time.deltaTime * visualInterpolationSpeed);
                hook.rotation = ropeState.hookVisualRotation;
            } else if (ropeState.nodes.Count == 1) { 
                 Quaternion targetRotation = Quaternion.Euler(0,0,-90f);
                 ropeState.hookVisualRotation = Quaternion.Lerp(ropeState.hookVisualRotation, targetRotation, Time.deltaTime * visualInterpolationSpeed);
                 hook.rotation = ropeState.hookVisualRotation;
            }
        } else if (rodTip != null) { 
             Quaternion targetRotation = Quaternion.Euler(0,0,-90f);
             ropeState.hookVisualRotation = Quaternion.Lerp(ropeState.hookVisualRotation, targetRotation, Time.deltaTime * visualInterpolationSpeed);
             hook.rotation = ropeState.hookVisualRotation;
        }

        DrawRope(); 
    }

    private void Simulate(float dt)
    {
        for (int i = 1; i < ropeState.nodes.Count; i++) 
        {
            RopeNode n = ropeState.nodes[i];
            Vector2 velocity = n.position - n.prevPosition; 

            if (i == ropeState.nodes.Count - 1 && ropeState.nodes.Count > 1) 
            {
                velocity *= lastNodeVelocityRetention;
            }
            
            Vector2 currentPositionForCast = n.position; 
            n.prevPosition = currentPositionForCast;     

            Vector2 tentativePosition = currentPositionForCast + velocity + (Vector2.up * _worldGravity * gravityScale * dt * dt);
            
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
            ropeState.nodes[i] = n;
        }
    }



    private void CorrectCollisionsAfterConstraints()
    {
        for (int i = 1; i < ropeState.nodes.Count; i++) 
        {
            RopeNode n = ropeState.nodes[i];
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
                Vector2 newPos = closestPointOnCollider + directionFromCollider * (nodeRadius + 0.001f);
                Vector2 correction = newPos - n.position;
                n.position += correction;
                n.prevPosition += correction;
                ropeState.nodes[i] = n; 
            }
        }

        if (hook != null)
        {
            Collider2D hit = Physics2D.OverlapCircle(ropeState.hookPhysicsTargetPosition, nodeRadius, obstacleMask);
            if (hit != null)
            {
                Vector2 closestPointOnCollider = hit.ClosestPoint(ropeState.hookPhysicsTargetPosition);
                Vector2 directionFromCollider = (ropeState.hookPhysicsTargetPosition - closestPointOnCollider).normalized;
                if (directionFromCollider.sqrMagnitude < 0.0001f)
                {
                    directionFromCollider = (ropeState.hookPhysicsTargetPosition - (Vector2)hit.bounds.center).normalized;
                    if (directionFromCollider.sqrMagnitude < 0.0001f)
                        directionFromCollider = Vector2.up;
                }
                Vector2 newPos = closestPointOnCollider + directionFromCollider * (nodeRadius + 0.001f);
                Vector2 correction = newPos - ropeState.hookPhysicsTargetPosition;

                ropeState.hookPhysicsTargetPosition += correction;
                ropeState.prevHookPhysicsTargetPosition += correction;
            }
        }
    }

    private void DrawRope() 
    {
        if (!ropeLine) return;
        if (ropeState.nodes.Count == 0) { 
             ropeLine.positionCount = 0;
             return;
        }

        int pointCount = ropeState.nodes.Count + 1;
        ropeLine.positionCount = pointCount;
        
        ropeLine.SetPosition(0, ropeState.nodes[0].visualPosition);

        for (int i = 1; i < ropeState.nodes.Count; i++) 
        {
            ropeLine.SetPosition(i, ropeState.nodes[i].visualPosition); 
        }
        ropeLine.SetPosition(ropeState.nodes.Count, ropeState.hookVisualPosition); 
    }

    public void ReelIn(float inputAmount)
    {
        if (_isBusy) return;

        float interval = Mathf.Lerp(maxReelInInterval, minReelInInterval, inputAmount);
        if (Time.time < lastInstanceTime + interval) return;

        _isBusy = true;
        try
        {
            if (ropeState.nodes.Count > 1)
            {
                lastInstanceTime = Time.time;
                RopeNode removedNode = ropeState.nodes[1];
                if (removedNode.transform != null)
                {
                    Destroy(removedNode.transform.gameObject);
                }
                ropeState.nodes.RemoveAt(1);
            }
            else
            {
                TryCatchFish();
            }
        }
        finally
        {
            _isBusy = false;
        }
    }

    public async UniTask ReelOut(float inputAmount)
    {
        if (_isBusy) return;
        
        if (ropeState.nodes.Count >= maxNodes) return;

        float interval = Mathf.Lerp(maxReelOutInterval, minReelOutInterval, inputAmount);
        if (Time.time < lastInstanceTime + interval) return;
        
        if (ropeState.nodes.Count > 0 && ropeState.nodes.Count < maxNodes) 
        {
            _isBusy = true;
            try
            {
                lastInstanceTime = Time.time;
                await UniTask.Yield();
                
                RopeNode rodTipNode = ropeState.nodes[0];
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

                ropeState.nodes.Insert(1, newNode);

                currentReelOutSettleSpeedFactor = Mathf.Lerp(minSettleSpeedFactor, maxSettleSpeedFactor, inputAmount);
                reelSettleSpeedEffectTimer = settleSpeedEffectDuration;
            }
            finally
            {
                _isBusy = false;
            }
        }
    }

    public Transform GetHook() => hook;

    public LureType GetLure() => _lure;

    public void TryCatchFish()
    {
        FishStats caughtFish = _lure.GetCurrentCatch();
        if (caughtFish != null)
        {
            EventBus.Publish(new FishCaughtEvent { Fish = caughtFish });
            
            _lure.DestroyCatch();
        }
    }

    public void ApplyMovementToHook(Vector2 displacementThisFrame)
    {
        _hookExternalForce += displacementThisFrame;
    }
}
