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

    private const int ACTIVE_VCAM_PRIORITY = 10;
    private const int INACTIVE_VCAM_PRIORITY = 0;
    
    [Header("UI Control Center")]
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private GameObject callShopCanvas;
    [HideInInspector] public Transform ShoppeBoat, Player, Hook;
    [HideInInspector] public float CMcamOrthoSize;

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

    void Start()
    {
        GameObject shoppeBoatObj = GameObject.Find("ShoppeBoat");
        if (shoppeBoatObj != null) 
        {
            ShoppeBoat = shoppeBoatObj.transform;
            vcamShop.Follow = ShoppeBoat;
            vcamShop.LookAt = ShoppeBoat;
        }
        else Debug.LogError("ShoppeBoat GameObject not found!");

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null) 
        {
            Player = playerObj.transform;
            vcamPlayer.Follow = Player;
            vcamPlayer.LookAt = Player;
        }
        else Debug.LogError("Player GameObject not found!");
        
        GameObject hookObj = GameObject.Find("Hook");
        if (hookObj != null) 
        {
            Hook = hookObj.transform;
            vcamHook.Follow = Hook;
            vcamHook.LookAt = Hook;
        }

        if (Player != null && vcamPlayer != null)
        {
            CameraSwitcher(CameraModes.Player);
        }
        else if (vcamPlayer == null)
        {
            Debug.LogError("Player Virtual Camera (vcamPlayer) is not assigned in GameManager!");
        }
    }

    // Removed Update() as it was empty

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
                    if (Hook == null)
                    {
                        GameObject hookObj = CustomRopeSolver.Instance.GetHook().gameObject;
                        if (hookObj != null) Hook = hookObj.transform;
                    }

                    if (Hook != null)
                    {
                        vcamHook.Follow = Hook; 
                        vcamHook.LookAt = Hook;
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
    
    //============================ PauseScreen ============================
    private bool _setPause;
    public void Pause()
    {
        if (Time.timeScale == 1)
        {
            Time.timeScale = 0;
            if(pauseCanvas != null) pauseCanvas.SetActive(true);
            _setPause = true;
        } 
        else 
        {
            Time.timeScale = 1; 
            if(pauseCanvas != null) pauseCanvas.SetActive(false);
            _setPause = false; 
        }
    }
}
public enum CameraModes
{
    Shop,
    Player,
    Hook
}
