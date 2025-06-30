using UnityEngine;
using Ami.BroAudio;

public class ProgressionBlocker : MonoBehaviour
{
    [Header("Blocker Identification")]
    [Tooltip("Unique string ID for this blocker (e.g., its prefab name or a scene-unique name). This will be hashed.")]
    public string blockerStringID = "DefaultBlockerID_ChangeMe";

    [Header("Visuals & Effects")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem destructionParticles;
    [SerializeField] private SoundID destructionSound; 

    [Header("Interaction")]
    [SerializeField] private Collider2D mainCollider; 
    [SerializeField] private Collider2D triggerCollider; 

    private bool _isDestroyed = false;
    private uint _hashedID = 0;

    /// <summary>
    /// Gets the FNV1a hashed ID of the blockerStringID.
    /// </summary>
    public uint HashedID => _hashedID;

    void Awake()
    {
        if (string.IsNullOrEmpty(blockerStringID) || blockerStringID == "DefaultBlockerID_ChangeMe")
        {
            // Attempt to use GameObject name if string ID is not set, but warn.
            blockerStringID = gameObject.name + "_" + GetInstanceID(); // Make it more unique if just name
            Debug.LogWarning($"ProgressionBlocker on {gameObject.name} had a default/empty blockerStringID. Auto-set to '{blockerStringID}'. Please assign a persistent unique string ID in the Inspector.", gameObject);
        }
        _hashedID = FNV1aHash.Calculate(blockerStringID);
        if (_hashedID == 0 && !string.IsNullOrEmpty(blockerStringID)) // Hash 0 can be valid for empty string, but we check if stringID wasn't empty
        {
            Debug.LogError($"Generated HashedID is 0 for blocker '{blockerStringID}' on {gameObject.name}. This might indicate an issue or a hash collision if 0 is used as a 'none' value.", gameObject);
        }


        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (mainCollider == null) mainCollider = GetComponent<Collider2D>();
        if (destructionParticles != null) destructionParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void Start()
    {
        LoadDestroyedState();
    }

    void LoadDestroyedState()
    {
        if (Inventory.Instance != null && Inventory.Instance.playerData != null)
        {
            if (Inventory.Instance.playerData.DestroyedBlockerIDs.Contains(HashedID))
            {
                _isDestroyed = true;
                DeactivateBlockerVisualsAndCollision();
            }
            else
            {
                _isDestroyed = false;
                ActivateBlockerVisualsAndCollision();
            }
        }
        else
        {
            Debug.LogWarning("Inventory instance or playerData not available to load blocker state for: " + blockerStringID);
            ActivateBlockerVisualsAndCollision();
        }
    }

    private void ActivateBlockerVisualsAndCollision()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (mainCollider != null) mainCollider.enabled = true;
    }

    private void DeactivateBlockerVisualsAndCollision()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (mainCollider != null) mainCollider.enabled = false;
    }

    public void BlowUp()
    {
        if (_isDestroyed) return; 

        Debug.Log($"Progression Blocker '{blockerStringID}' (ID: {HashedID}) is being destroyed!");
        _isDestroyed = true;

        if (destructionParticles != null)
        {
           destructionParticles.Play();
        }
        destructionSound.Play(transform.position);
        DeactivateBlockerVisualsAndCollision();

        if (Inventory.Instance != null && Inventory.Instance.playerData != null)
        {
            if (!Inventory.Instance.playerData.DestroyedBlockerIDs.Contains(HashedID))
            {
                Inventory.Instance.playerData.DestroyedBlockerIDs.Add(HashedID);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isDestroyed && other.CompareTag("BombExplosion")) 
        {
            Debug.Log($"Blocker '{blockerStringID}' detected bomb explosion trigger from {other.name}.");
            BlowUp();
        }
    }
}
