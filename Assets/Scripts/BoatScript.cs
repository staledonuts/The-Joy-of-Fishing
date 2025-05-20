using UnityEngine;
using UnityEngine.InputSystem;
public class BoatScript : MonoBehaviour
{
    //Privates
    [Min(0.02f)] [SerializeField] private float rampUpTime = 1f;
    private float triggerValue;
    private Vector2 moveInput, hookInput;
    private bool BoatCanMove = true;
    private GameObject curHook;
    private Rigidbody2D rig2d;
    private AnimationCurve moveAnimCurve;
    public int maxLineLength;
    public bool currentlyReelingUp;

    //Publics
    [Header("Movement stuff")]
    [SerializeField] public float boatSpeed = 1f, BoatSpeedForce = 10f, forcetoAdd = 100;

    //public static event System.Action<bool> IsFishing;
    public static event System.Action DoneFishing;
    [SerializeField] private CustomRopeSolver customRope;

    public Transform baitpoint, baitTransform, rodpoint;

    //holds whether rope is active or not
    [HideInInspector] public bool ropeActive, boostbool;

    //current hook on the scene

    private void Awake()
    {
        rig2d = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        //MakeAnimationCurve();
        //GetKey = new TheJoyofFishing();
        //GetKey.Enable();
    }

    #region Make a animation curve

    private void MakeAnimationCurve()
    {
        moveAnimCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(rampUpTime, 1f));
        moveAnimCurve.preWrapMode = WrapMode.PingPong;
        moveAnimCurve.postWrapMode = WrapMode.PingPong;
    }

    #endregion Make a animation curve

    private void FixedUpdate()
    {
        MoveLeftRight();
        moveHook();
        ReelRope();
    }


//=============================================Input Stuff========================================

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
            bool playerhashorizontalspeed = Mathf.Abs(rig2d.linearVelocity.x) > Mathf.Epsilon;
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

    public void moveHook()
    {
        if(ropeActive && GameManager.Instance.MindcontrolActive)
        {
            if (CustomRopeSolver.Instance == null) 
            {  
                return;
            }
            else if (CustomRopeSolver.Instance != null)
            {
                CustomRopeSolver.Instance.MoveHook(hookInput * forcetoAdd * Time.deltaTime);
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

            if (rightTrigger > 0 && CustomRopeSolver.Instance.maxNodes <= maxLineLength)
            {
                CustomRopeSolver.Instance.ReelOut(rightTrigger);
            }
            else if (leftTrigger > 0)
            {
                CustomRopeSolver.Instance.ReelIn(leftTrigger);
            }   
        }

    
    }



//========================================Rope Stuff======================================================
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
}