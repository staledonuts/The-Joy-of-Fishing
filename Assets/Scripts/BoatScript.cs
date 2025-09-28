using System;
using DonutPackage.EventBus;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks; 
public class BoatScript : MonoBehaviour
{
    private float triggerValue;
    private Vector2 moveInput, hookInput;
    private Rigidbody2D rig2d;
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
            EventBus.Publish(new ToggleShopEvent());
        }
    }

    private void MoveLeftRight()
    {
        //bool playerHasHorizontalSpeed = Mathf.Abs(rig2d.linearVelocity.x) > Mathf.Epsilon;
        Vector2 playerVelocity = new Vector2(moveInput.x * BoatSpeedForce, rig2d.linearVelocity.y);
        rig2d.linearVelocity = playerVelocity;
    }

    public void PauseButton(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            EventBus.Publish(new PauseGameEvent());
        }
    }

    public void MoveHook()
    {
        if(Inventory.Instance.playerData.RadioControlLure)
        {
            EventBus.Publish(new MoveHookEvent { MoveInput = hookInput * forcetoAdd * Time.deltaTime });
        }
    }
    public void ReelRope()
    {
        float rightTrigger = Mathf.Clamp(triggerValue, 0, 1);
        float leftTrigger = Mathf.Abs(Mathf.Clamp(triggerValue, -1, 0));

        if (rightTrigger > Mathf.Epsilon)
        {
            EventBus.Publish(new ReelOutEvent { InputValue = rightTrigger });
        }
        else if (leftTrigger > Mathf.Epsilon)
        {
            EventBus.Publish(new ReelInEvent { InputValue = leftTrigger });
        }   
    }

    public void OnCastOut()
    {
        GameManager.Instance.HookTransform = CustomRopeSolver.Instance.GetHook();
        GameManager.Instance.CameraSwitcher(CameraModes.Hook);
    }
}