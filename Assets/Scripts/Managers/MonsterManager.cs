using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;

public enum HuntingState
{
    Idle, //Just stays in a location
    Wandering, //Randomly walks around the available area
    Prowling, //If the player is close enough to detect but there is no path to it. (It just exists as close to the player as it can get)
    Approaching, //It detects a route closer to the player and it moves towards the player.
    Dashing, //It has a straight shot at the player and it runs.
    Attacking, //When it reaches the player
    Retreating //It suddenly exists in an area with light and it retreats into the darkness.
}

public class MonsterManager : Singleton<MonsterManager>
{
    [Header("Monster Ai")]
    [SerializeField] private HuntingState currentHuntingState;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] private float prowlingDistance = 10f;
    [SerializeField] private float dashingDistance = 5f;
    [SerializeField] private float attackingDistance = 10f;
    [SerializeField] private float speed = 3.5f;

    [Header("Wandering")] //Borrowed from Josh's script
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderTimer = 5f;
    private float wanderTimerCounter;

    [Header("Animation")]
    [SerializeField] private Animator anim;
    private AttackCollider atkCollider;

    private void Start()
    {
        //Brendans sanity check
        if (GetComponent<NavMeshAgent>() == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        wanderTimerCounter = wanderTimer;

        if (GetComponentInChildren<AttackCollider>() != null)
        {
            atkCollider = GetComponentInChildren<AttackCollider>();
        }
        else
        {
            Debug.LogError("Couldn't find weapon");
            return;
        }
    }

    /// <summary>
    /// ChatGpt helped me with this
    /// </summary>
    /// <returns></returns>
    // Call this to get the current speed
    public float GetCurrentSpeed()
    {
        return agent.velocity.magnitude;
    }

    private void Update()
    {
        anim.SetFloat("Speed", GetCurrentSpeed());

        //Get the distance between us and the player
        float distToPlayer = Vector3.Distance(transform.position, _PLAYER.transform.position);
        if (distToPlayer < prowlingDistance && currentHuntingState != HuntingState.Retreating && currentHuntingState != HuntingState.Attacking)
        {
            currentHuntingState = HuntingState.Prowling;
        }
        if (distToPlayer < dashingDistance && currentHuntingState != HuntingState.Attacking) //Change this to approaching later
        {
            currentHuntingState = HuntingState.Dashing;
        }
        //if (currentHuntingState != HuntingState.Dashing)
        //agent.speed = speed;

        NavMeshHit hit;

        // Check if player position is on the navmesh
        bool playerIsReachable = NavMesh.SamplePosition(_PLAYER.transform.position, out hit, prowlingDistance, NavMesh.AllAreas);

        switch (currentHuntingState)
        {
            case HuntingState.Idle:
                agent.speed = 0;
                int rndwaittime = Random.Range(3, 6);
                StartCoroutine(Idling(rndwaittime));
                break;

            case HuntingState.Wandering:
                agent.speed = speed;
                wanderTimerCounter += Time.deltaTime;
                if (wanderTimerCounter >= wanderTimer)
                {
                    Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                    agent.SetDestination(newPos);
                    wanderTimerCounter = 0f;
                }
                float wanderRnd = Random.Range(0f, 1f);
                if (wanderRnd <= 50f) currentHuntingState = HuntingState.Idle;
                break;

            case HuntingState.Prowling:

                if (playerIsReachable)
                {
                    // Player is on a reachable NavMesh point, move to them and switch to Approaching
                    agent.SetDestination(hit.position);
                    currentHuntingState = HuntingState.Approaching;
                }
                else
                {
                    // Player is in an unwalkable area, move as close as possible
                    if (NavMesh.SamplePosition(_PLAYER.transform.position, out hit, prowlingDistance * 2f, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                    }
                    else
                    {
                        // No reachable point found - could stand still or return to patrol
                        agent.SetDestination(transform.position);
                        currentHuntingState = HuntingState.Idle;
                    }
                }
                agent.speed = speed;
                break;
            case HuntingState.Approaching:
                if (distToPlayer > prowlingDistance)
                    currentHuntingState = HuntingState.Wandering;
                agent.SetDestination(_PLAYER.transform.position);
                break;
            case HuntingState.Dashing:
                agent.speed = speed * 2;
                agent.SetDestination(_PLAYER.transform.position);
                if (distToPlayer < attackingDistance)
                {
                    agent.speed = 0;
                    StartCoroutine(Attack());
                }
                break;
            case HuntingState.Attacking:
                agent.speed = 0;
                if (_PLAYER.isAlive == true)
                    currentHuntingState = HuntingState.Idle;
                break;
            case HuntingState.Retreating:
                break;

        }
    }

    private IEnumerator Attack()
    {
        currentHuntingState = HuntingState.Attacking;
        PlayAnimation("Attack", 3);
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator Idling(int waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        float idleRnd = Random.Range(0f, 1f);
        if (idleRnd <= 50f) currentHuntingState = HuntingState.Wandering;
    }


    /// <summary>
    /// Idk what this does, Josh just said it makes the wandering work
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="dist"></param>
    /// <param name="layermask"></param>
    /// <returns></returns>
   private static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection.y = 0f; // keep it flat on ground
        randDirection += origin;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randDirection, out navHit, dist, layermask))
        {
            return navHit.position;
        }

        return origin;
    }

    private void PlayAnimation(string _animationName, int _animationCount)
    {
        int rnd = Random.Range(1, _animationCount);
        anim.SetTrigger(_animationName + rnd);
    }

    public void EnableCollider()
    {
        atkCollider.SetCollider(true);
    }

    public void DisableCollider()
    {
        atkCollider.SetCollider(false);
    }
}
