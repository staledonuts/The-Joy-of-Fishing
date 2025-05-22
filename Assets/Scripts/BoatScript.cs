using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class BoatScript : MonoBehaviour
{
    private float triggerValue;
    private bool CanCatchFish = false;
    private bool BoatCanMove = true;
    private Vector2 moveInput, hookInput;
    private GameObject curHook;
    private Rigidbody2D rig2d;
    private Transform fishInventory;
    private GameObject fishCollective;
    public static event Action DoneCollecting;
    public static event Action DoneFishing;
    public bool currentlyReelingUp;

    [Header("Movement stuff")]
    [SerializeField] public float boatSpeed = 1f, BoatSpeedForce = 10f, forcetoAdd = 100;
    [SerializeField] private CustomRopeSolver customRope;
    public Transform baitpoint, baitTransform, rodpoint;
    [HideInInspector] public bool ropeActive, boostbool;


    private void Awake()
    {
        rig2d = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        MoveLeftRight();
        MoveHook();
        ReelRope();
    }

    public void GetInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void GetTriggerValues(InputAction.CallbackContext context)
    {
        triggerValue = context.ReadValue<float>();
    }

    public void GetRightStickInput(InputAction.CallbackContext context)
    {
        hookInput = context.ReadValue<Vector2>();
    }
    public void CastOutButton(InputAction.CallbackContext context)
    {
        if(context.performed && ropeActive == false)
        {
            OnCastOut();
        }
    }

    public void CallShop(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GameManager.Instance.onShopSwitch();
        }
    }
    private void MoveLeftRight()
    {
        if (!BoatCanMove)
        {
            return;
        }
        else
        {
            bool playerHasHorizontalSpeed = Mathf.Abs(rig2d.linearVelocity.x) > Mathf.Epsilon;
            Vector2 playerVelocity = new Vector2(moveInput.x * BoatSpeedForce, rig2d.linearVelocity.y);
            rig2d.linearVelocity = playerVelocity;
        }
    }
    public void PauseButton(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            GameManager.Instance.Pause();
        }
    }

    public void MoveHook()
    {
        if(ropeActive && GameManager.Instance.MindcontrolActive)
        {
            if (CustomRopeSolver.Instance == null) 
            {  
                return;
            }
            else if (CustomRopeSolver.Instance != null)
            {
                CustomRopeSolver.Instance.ApplyMovementToLastNode(hookInput * forcetoAdd * Time.deltaTime);
            }
        }
    }
    public void ReelRope()
    {
        if (!CustomRopeSolver.Instance) 
        {  
            return;   
        }
        else
        {
            float rightTrigger = Mathf.Clamp(triggerValue, 0, 1);
            float leftTrigger = Mathf.Abs(Mathf.Clamp(triggerValue, -1, 0));

            if (rightTrigger > Mathf.Epsilon)
            {
                CustomRopeSolver.Instance.ReelOut(rightTrigger);
            }
            else if (leftTrigger > Mathf.Epsilon)
            {
                CustomRopeSolver.Instance.ReelIn(leftTrigger);
            }   
        }
    }

    public void OnCastOut()
    {
        if (ropeActive == false && GameManager.Instance.moveCam == 1)
        {
            GameManager.Instance.Hook = CustomRopeSolver.Instance.GetHook();
            GameManager.Instance.baitCam = true;
            GameManager.Instance.moveCam = 3;
            //sets rope to enabled
            ropeActive = true;
        }
    }

    public void DeleteRope()
    {
        Destroy(curHook);
        GameManager.Instance.baitCam = false;
        GameManager.Instance.moveCam = 1;
        //sets rope to disabled
        ropeActive = false;

        //Sends out a message for other scripts to listen
        DoneFishing?.Invoke();
    }

    private void OnEnable()
    {
        BaitScript.BaitIsOut += FindHookAndInventory;
        MoneyEffect.DeleteFish += ClearFishCollection;
        DoneFishing += AddFishToInventory;
    }

    private void OnDisable()
    {
        BaitScript.BaitIsOut -= FindHookAndInventory;
        MoneyEffect.DeleteFish -= ClearFishCollection;
        DoneFishing -= AddFishToInventory;
    }

    private void FindHookAndInventory(bool bait)
    {
        if (bait)
        {
            try
            {
                fishInventory = GameObject.FindGameObjectWithTag("FishInventory").transform;
            }
            catch
            {
                GameObject fishGameObject = new GameObject();
                fishGameObject.tag = "FishInventory";
                fishGameObject.name = "FishCollection";
                fishInventory = fishGameObject.transform;
            }
            CanCatchFish = bait;
        }
        else
        {
            CanCatchFish = bait;
        }
    }

    private void AddFishToInventory()
    {
        fishCollective = GameObject.FindGameObjectWithTag("FishCollective");
        for (int i = 0; i < fishCollective.transform.childCount; i++)
        {
            if (fishCollective.transform.GetChild(i).CompareTag("Fish"))
            {
                fishCollective.transform.GetChild(i).gameObject.SetActive(false);
                fishCollective.transform.GetChild(i).parent = fishInventory;
                BaitScript.FishOfHook?.Invoke();
            }
        }
        DoneCollecting?.Invoke();
    }

    private void ClearFishCollection()
    {
        for (int i = 0; i < fishInventory.childCount; i++)
        {
            Destroy(fishInventory.GetChild(i).gameObject);
        }
    }
}