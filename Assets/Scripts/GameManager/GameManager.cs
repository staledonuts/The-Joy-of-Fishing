using DonutPackage.EventBus;
using UnityEngine;
using Unity.Cinemachine;

public sealed class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
    public static GameManager Instance 
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("GameManager_Singleton");
                    instance = obj.AddComponent<GameManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    [Header("Cinemachine Virtual Cameras")]
    [SerializeField] private CinemachineVirtualCameraBase vcamPlayer;
    [SerializeField] private CinemachineVirtualCameraBase vcamShop;
    [SerializeField] private CinemachineVirtualCameraBase vcamHook;

    [Header("Camera Targets")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform shoppeBoatTransform;
    [SerializeField] private Transform m_HookTransform;

    private const int ACTIVE_VCAM_PRIORITY = 10;
    private const int INACTIVE_VCAM_PRIORITY = 0;
    
    private bool _isPaused = false;

    public Transform HookTransform
    {
        get => m_HookTransform;
        set => m_HookTransform = value;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<PauseGameEvent>(HandlePauseGame);
        EventBus.Subscribe<ShopStateChangedEvent>(HandleShopStateChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PauseGameEvent>(HandlePauseGame);
        EventBus.Unsubscribe<ShopStateChangedEvent>(HandleShopStateChanged);
    }

    private void HandleShopStateChanged(ShopStateChangedEvent e)
    {
        CameraSwitcher(e.IsShopOpen ? CameraModes.Shop : CameraModes.Hook);
    }

    void Start()
    {
        // Assign camera targets if they are set
        if (shoppeBoatTransform != null && vcamShop != null) 
        {
            vcamShop.Follow = shoppeBoatTransform;
            vcamShop.LookAt = shoppeBoatTransform;
        }
        else Debug.LogError("ShoppeBoat Transform or vcamShop is not assigned in GameManager!");

        if (playerTransform != null && vcamPlayer != null) 
        {
            vcamPlayer.Follow = playerTransform;
            vcamPlayer.LookAt = playerTransform;
        }
        else Debug.LogError("Player Transform or vcamPlayer is not assigned in GameManager!");
        
        if (m_HookTransform != null && vcamHook != null) 
        {
            vcamHook.Follow = m_HookTransform;
            vcamHook.LookAt = m_HookTransform;
        }
        // No error needed for hook, it might be assigned later. The CameraSwitcher handles this.

        if (playerTransform != null && vcamPlayer != null)
        {
            CameraSwitcher(CameraModes.Player);
        }
        else if (vcamPlayer == null)
        {
            Debug.LogError("Player Virtual Camera (vcamPlayer) is not assigned in GameManager!");
        }
    }

    public void CameraSwitcher(CameraModes cameraMode)
    {
        // Reset all VCam priorities to inactive
        if (vcamPlayer != null) vcamPlayer.Priority = INACTIVE_VCAM_PRIORITY;
        if (vcamShop != null) vcamShop.Priority = INACTIVE_VCAM_PRIORITY;
        if (vcamHook != null) vcamHook.Priority = INACTIVE_VCAM_PRIORITY;

        // Activate the desired VCam by setting its priority higher
        // and manage UI visibility
        switch (cameraMode)
        {
            case CameraModes.Player:
            {
                if (vcamPlayer != null) vcamPlayer.Priority = ACTIVE_VCAM_PRIORITY;
                else Debug.LogError("vcamPlayer not assigned!");
                break;
            }
            
            case CameraModes.Hook:
            {
                if (vcamHook != null)
                {
                    if (m_HookTransform == null)
                    {
                        GameObject hookObj = CustomRopeSolver.Instance.GetHook().gameObject;
                        if (hookObj != null) m_HookTransform = hookObj.transform;
                    }

                    if (m_HookTransform != null)
                    {
                        vcamHook.Follow = m_HookTransform; 
                        vcamHook.LookAt = m_HookTransform;
                        vcamHook.Priority = ACTIVE_VCAM_PRIORITY;
                    }
                    else Debug.LogError("Hook Transform not found or assigned for vcamHook!");
                }
                else Debug.LogError("vcamHook not assigned!");
                break;
            }

            case CameraModes.Shop:
            {
                if (vcamShop != null) vcamShop.Priority = ACTIVE_VCAM_PRIORITY;
                else Debug.LogError("vcamShop not assigned!");
                break;
            }
            
            default:
            {
                Debug.LogWarning("Unknown CameraMode specified: " + cameraMode);
                if (vcamPlayer != null) vcamPlayer.Priority = ACTIVE_VCAM_PRIORITY;
                break;
            }
        }
    }        
    
    private void HandlePauseGame(PauseGameEvent e)
    {
        Pause();
    }
    
    //============================ PauseScreen ============================
    public void Pause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = _isPaused ? 0f : 1f;
        EventBus.Publish(new PauseStateChangedEvent { IsPaused = _isPaused });
    }
}

public enum CameraModes
{
    Shop,
    Player,
    Hook
}
