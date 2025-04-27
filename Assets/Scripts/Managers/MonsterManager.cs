using UnityEngine;
using UnityEngine.AI;

public enum HuntingState
{
    Idle, //Just stays in a location
    Wandering, //Randomly walks around the available are.
    Dashing, //It has a straight shot at the player and it runs.
    Attacking, //When it reaches the player
    Retreating //It suddenly exists in an area with light and it retreats into the darkness.
}

public class MonsterManager : Singleton<MonsterManager>
{
    
    public enum State { Idle, Wander, Dash, Attack, Retreating }
    [SerializeField] private State currentState;

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;

    [Header("Animation")]
    [SerializeField] private Animator anim;
    private AttackCollider atkCollider;

    [Header("Variables")]
    [SerializeField] private float speed = 3.5f;
    public float wanderRadius = 10f;
    public float attackRange = 2f;
    public float dashRange = 15f;
    public float idleTime = 2f;
    private float idleTimer;

    private float navMeshCheckTimer = 0f;
    private float navMeshCheckInterval = 2f;


    void Start()
    {
        //Brendans sanity check
        if (GetComponent<NavMeshAgent>() == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (GetComponentInChildren<AttackCollider>() != null)
        {
            atkCollider = GetComponentInChildren<AttackCollider>();
        }
        else
        {
            Debug.LogError("Couldn't find weapon");
            return;
        }

        ChangeState(State.Idle);
    }


    /// <summary>
    /// ChatGpt helped me with this
    /// </summary>
    
    // Call this to get the current speed
    public float GetCurrentSpeed()
    {
        return agent.velocity.magnitude;
    }
    void Update()
    {
        navMeshCheckTimer += Time.deltaTime;
        if (navMeshCheckTimer >= navMeshCheckInterval)
        {
            navMeshCheckTimer = 0f;
            CheckNavMeshPosition();
        }

        if (!CanReachPlayer())
        {
            Debug.LogWarning("Monster can't reach the player! Teleporting...");

            NavMeshHit playerHit;
            if (NavMesh.SamplePosition(player.position, out playerHit, 2.0f, NavMesh.AllAreas))
            {
                // Increase the offset for more distance from player
                Vector3 offset = (transform.position - player.position).normalized * 3f;  // Increased offset

                Vector3 safeTeleportPos = playerHit.position + offset;

                NavMeshHit safeHit;
                if (NavMesh.SamplePosition(safeTeleportPos, out safeHit, 2.0f, NavMesh.AllAreas))
                {
                    transform.position = safeHit.position;
                    agent.Warp(safeHit.position); // Sync the agent with the new position
                }
            }
        }


        anim.SetFloat("Speed", GetCurrentSpeed());

        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Wander:
                Wander();
                break;
            case State.Dash:
                Dash();
                break;
            case State.Attack:
                Attack();
                break;
            case State.Retreating:
                Retreating();
                break;
        }

        // Global transitions
        if (_FLM._flashLightOn)
        {
            ChangeState(State.Retreating);
        }
        else if (currentState != State.Retreating && currentState != State.Attack && PlayerInDashRange())
        {
            ChangeState(State.Dash);
        }
    }

    void CheckNavMeshPosition()
    {
        NavMeshHit hit;

        // Check if the monster is still on a NavMesh
        if (!NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            Debug.LogWarning("Monster off NavMesh! Attempting to recover...");

            // Call the new method to teleport the monster to the farthest point from the player
            TeleportToFarthestNavMeshPoint();
        }
    }


    void ChangeState(State newState)
    {
        currentState = newState;
        idleTimer = 0;
    }

    void Idle()
    {
        agent.speed = 0;
        idleTimer += Time.deltaTime;
        agent.isStopped = true;

        if (idleTimer >= idleTime)
        {
            ChangeState(State.Wander);
        }
    }

    //Borrowed from Josh's script
    void Wander()
    {
        agent.speed = speed;

        if (!agent.pathPending && (agent.remainingDistance <= agent.stoppingDistance))
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                float rndIdle = Random.Range(0f, 1f);
                if (rndIdle < 0.5f)
                {
                    ChangeState(State.Idle);
                }
                else
                {
                    // We arrived or are stuck, pick a new spot
                    Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                    if (NavMesh.SamplePosition(newPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                        agent.isStopped = false;
                    }
                    else
                    {
                        Debug.LogWarning("Couldn't find valid wander position!");
                        ChangeState(State.Idle); // Fall back to idle if wander fails
                    }
                }
            }
        }
    }


    void Dash()
    {
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            ChangeState(State.Attack);
            return;
        }

        agent.speed = speed;
        agent.SetDestination(player.position);
        agent.isStopped = false;
    }

    void Attack()
    {
        agent.speed = 0;
        agent.isStopped = true;
        transform.LookAt(player);

        // Replace this with your animation system call
        PlayAnimation("Attack", 3);

        if (Vector3.Distance(transform.position, player.position) > attackRange)
        {
            ChangeState(State.Dash);
        }
    }

    void Retreating()
    {
        agent.speed = speed;
        if (!_FLM._flashLightOn)
        {
            ChangeState(State.Idle);
            return;
        }

        Vector3 dirAwayFromPlayer = (transform.position - player.position).normalized;
        Vector3 retreatPos = transform.position + dirAwayFromPlayer * 5f;

        agent.SetDestination(retreatPos);
        agent.isStopped = false;
    }

    bool PlayerInDashRange()
    {
        if (player == null) return false;

        // First, check if the player is near a NavMesh (expand radius to be forgiving)
        NavMeshHit hit;
        bool playerOnNavMesh = NavMesh.SamplePosition(player.position, out hit, 3.0f, NavMesh.AllAreas);

        if (!playerOnNavMesh)
        {
            // Player is NOT reachable, no dash
            return false;
        }

        // Now check distance normally
        float dist = Vector3.Distance(transform.position, player.position);
        return dist <= dashRange;
    }


    // Helper for wander
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
    bool CanReachPlayer()
    {
        if (player == null) return false;

        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(player.position, path))
        {
            return path.status == NavMeshPathStatus.PathComplete;
        }

        return false;
    }

    void TeleportToFarthestNavMeshPoint()
    {
        // First, check if the player is on a valid NavMesh
        NavMeshHit playerHit;
        if (NavMesh.SamplePosition(player.position, out playerHit, 2.0f, NavMesh.AllAreas))
        {
            Vector3 playerPos = playerHit.position;

            // Define search parameters
            float searchRadius = 15f;  // The radius to search around the player
            int maxSearchAttempts = 10; // Number of attempts to find a valid point

            Vector3 farthestPoint = playerPos;
            float maxDistance = 0f;

            // Try finding the farthest point within the radius
            for (int i = 0; i < maxSearchAttempts; i++)
            {
                // Pick a random point within the search radius
                Vector3 randomDirection = Random.insideUnitSphere * searchRadius;
                randomDirection += playerPos;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, 2f, NavMesh.AllAreas))
                {
                    // Calculate distance to the player
                    float distanceToPlayer = Vector3.Distance(hit.position, playerPos);

                    // We want the farthest point
                    if (distanceToPlayer > maxDistance)
                    {
                        maxDistance = distanceToPlayer;
                        farthestPoint = hit.position;
                    }
                }
            }

            // Now, teleport the monster to the farthest point found
            if (maxDistance > 0f)  // Ensure we found a valid position
            {
                transform.position = farthestPoint;
                agent.Warp(farthestPoint);  // Warp the agent to the new position
                Debug.Log($"Teleporting to farthest point: {farthestPoint}");
            }
            else
            {
                Debug.LogWarning("Could not find a valid farthest point.");
            }
        }
        else
        {
            Debug.LogWarning("Player is not on a valid NavMesh.");
        }
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
