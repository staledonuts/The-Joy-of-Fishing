using Ami.BroAudio;
using Pathfinding;
using UnityEngine;

/*
 * The fish Ai.
 * Takes care of all the fish movement.
 * Uses the AStar pathfinding prodject to get the walkable surfaces.
 */

[RequireComponent(typeof(Seeker), typeof(AIPath))]
public class MoveAi : FishStats
{
    private Seeker _agent;
    private AIPath _path;
    private Transform _player;
    private float dist = 0f;
    private bool CanFish = false;


    private Vector3 _wander = Vector3.zero;

    [SerializeField] private SoundID _fishMoveSFX;
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderJitter = 1f;



    private void Awake()
    {
        _agent = GetComponent<Seeker>();
        _path = GetComponent<AIPath>();
    }

    private void OnEnable()
    {
        Wander();
        BaitScript.BaitIsOut += delegate (bool theBait)
        {
            _player = FindAnyObjectByType<BaitScript>().transform;

            CanFish = theBait;
        };
    }
    private void OnDisable()
    {
        BaitScript.BaitIsOut -= delegate (bool theBait)
        {
            _player = FindAnyObjectByType<BaitScript>().transform;

            CanFish = theBait;
        };
    }

    private void LateUpdate()
    {
        if (CanFish)
        {
            HookOut();
        }
        else
        {
            HookIn();
        }
    }

    //Method that gets a random position in the world and sets the destination
    private void Wander()
    {
        _wander += new Vector3(Random.Range(-1f, 1f) * wanderJitter, Random.Range(-1f, 1f) * wanderJitter);

        _wander = _wander.normalized;
        _wander *= wanderRadius;

        Vector3 targetWorld = this.gameObject.transform.InverseTransformVector(_wander);

        Seek(targetWorld);
    }

    //Sets a position for the Ai to move towards
    private void Seek(Vector3 target)
    {
        Vector3 currTarget = this.transform.position + target;
        _agent.StartPath(this.transform.position, currTarget);
        _fishMoveSFX.Play(transform);
    }

    //Does the exact opposite of Seek()
    private void Flee(Vector3 position)
    {
        Vector3 fleeVector = position - this.transform.position;
        Vector3 fleePos = this.transform.position - fleeVector;
        _agent.StartPath(this.transform.position, fleePos);
    }

    private void HookIn()
    {
        if (_path.reachedEndOfPath)
        {
            Wander();
        }

        var x = transform.rotation.z < 0 ? base.transform.localScale = new Vector2(1,1) : base.transform.localScale = new Vector2(-1,1);
    }

    private void HookOut()
    {
        dist = Vector3.Distance(this.transform.position, _player.position);
        if (dist > BaitAttractionRadius)
        {
            if (_path.reachedEndOfPath)
            {
                Wander();
            }
        }
        if (dist < BaitAttractionRadius)
        {
            _path.canMove = true;
            if (BaitScript.BaitLevel() == BaitLevel)
            {
               _agent.StartPath(this.transform.position, _player.position);
            }
            else
            {
                Flee(_player.position);
            }
        }

        var x = transform.rotation.z < 0 ? base.transform.localScale = new Vector2(1,1) : base.transform.localScale = new Vector2(-1,1);
    }
}