
using UnityEngine;

public sealed class CatchPoint : MonoBehaviour
{
    private CircleCollider2D _circleCollider2D;
    void Start()
    {
        _circleCollider2D = GetComponent<CircleCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Fish")
        {
            CustomRopeSolver.Instance.TryCatchFish();
        }
    }
}