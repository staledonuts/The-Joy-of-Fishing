using UnityEngine;

public sealed class HookScript : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    //[SerializeField] private Collider2D collider2D;

    public void MoveHook(Vector2 vec2)
    {
        body.AddForce(vec2);
    }

}