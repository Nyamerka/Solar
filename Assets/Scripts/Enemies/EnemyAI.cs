using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public enum EnemyState
{
    Patrol,
    Chase,
    Search,
    Stunned,
    Fleeing,
    Dying
}

public enum EnemyType
{
    Patrol,
    Sentry,
    Wraith,
    Lurker
}

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Type")]
    [SerializeField] private EnemyType enemyType = EnemyType.Patrol;

    [Header("Speeds")]
    [SerializeField] private float patrolSpeed = 1.8f;
    [SerializeField] private float chaseSpeed = 3.5f;

    [Header("Chase")]
    [SerializeField] private float chaseDuration = 5f;
    [SerializeField] private float searchDuration = 3f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private int contactDamage = 1;

    [Header("Sentry Rotation")]
    [SerializeField] private float sentryRotationSpeed = 45f;

    [Header("Flee (Lurker)")]
    [SerializeField] private float fleeDistance = 5f;

    [Header("Death")]
    [SerializeField] private GameObject lightEssencePrefab;

    public EnemyState CurrentState { get; private set; } = EnemyState.Patrol;
    public EnemyType Type => enemyType;

    public event Action OnDied;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private EnemyPatrol patrol;
    private EnemyVision vision;
    private EnemyHearing hearing;
    private EnemyLightHealth lightHealth;

    private Vector3 lastKnownPlayerPos;
    private float stateTimer;
    private float sentryAngle;
    private bool isDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        patrol = GetComponent<EnemyPatrol>();
        vision = GetComponent<EnemyVision>();
        hearing = GetComponent<EnemyHearing>();
        lightHealth = GetComponent<EnemyLightHealth>();
    }

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        agent.speed = patrolSpeed;

        if (enemyType == EnemyType.Sentry)
            sentryAngle = transform.eulerAngles.y;
    }

    private void Update()
    {
        if (isDead) return;

        switch (CurrentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Search:
                UpdateSearch();
                break;
            case EnemyState.Stunned:
                UpdateStunned();
                break;
            case EnemyState.Fleeing:
                UpdateFleeing();
                break;
        }
    }

    private void UpdatePatrol()
    {
        agent.speed = patrolSpeed;

        if (enemyType == EnemyType.Sentry)
        {
            agent.isStopped = true;
            sentryAngle += sentryRotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, sentryAngle, 0);
        }
        else if (patrol != null)
        {
            patrol.UpdatePatrol(agent);
        }

        if (CheckPlayerDetection())
            TransitionTo(EnemyState.Chase);
    }

    private void UpdateChase()
    {
        if (playerTransform == null) { TransitionTo(EnemyState.Patrol); return; }

        agent.speed = chaseSpeed;
        lastKnownPlayerPos = playerTransform.position;
        agent.SetDestination(lastKnownPlayerPos);

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distToPlayer <= attackRange)
            AttackPlayer();

        bool canSee = (vision != null && vision.CanSeePlayer(playerTransform))
                   || (hearing != null && hearing.CanHearPlayer(playerTransform));

        if (!canSee)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0)
                TransitionTo(EnemyState.Search);
        }
        else
        {
            stateTimer = chaseDuration;
        }
    }

    private void UpdateSearch()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0)
                TransitionTo(EnemyState.Patrol);
        }

        if (CheckPlayerDetection())
            TransitionTo(EnemyState.Chase);
    }

    private void UpdateStunned()
    {
        agent.isStopped = true;
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            agent.isStopped = false;
            TransitionTo(EnemyState.Patrol);
        }
    }

    private void UpdateFleeing()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0 || (agent.remainingDistance < 0.5f && !agent.pathPending))
        {
            TransitionTo(EnemyState.Patrol);
        }
    }

    private void TransitionTo(EnemyState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case EnemyState.Chase:
                stateTimer = chaseDuration;
                agent.isStopped = false;
                break;
            case EnemyState.Search:
                stateTimer = searchDuration;
                agent.isStopped = false;
                agent.SetDestination(lastKnownPlayerPos);
                break;
            case EnemyState.Patrol:
                agent.isStopped = false;
                break;
        }
    }

    private bool CheckPlayerDetection()
    {
        if (playerTransform == null) return false;

        if (enemyType == EnemyType.Wraith)
            return hearing != null && hearing.CanHearPlayer(playerTransform);

        return (vision != null && vision.CanSeePlayer(playerTransform))
            || (hearing != null && hearing.CanHearPlayer(playerTransform));
    }

    private void AttackPlayer()
    {
        var health = playerTransform.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(contactDamage);
    }

    public void AlertToPlayer()
    {
        if (isDead) return;
        if (playerTransform != null)
            lastKnownPlayerPos = playerTransform.position;
        TransitionTo(EnemyState.Chase);
    }

    public void Stun(float duration)
    {
        if (isDead) return;
        stateTimer = duration;
        TransitionTo(EnemyState.Stunned);
    }

    public void Flee(Vector3 fromPosition)
    {
        if (isDead) return;
        Vector3 fleeDir = (transform.position - fromPosition).normalized;
        Vector3 fleeTo = transform.position + fleeDir * fleeDistance;

        if (NavMesh.SamplePosition(fleeTo, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
            stateTimer = 3f;
            CurrentState = EnemyState.Fleeing;
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        CurrentState = EnemyState.Dying;
        agent.isStopped = true;
        agent.enabled = false;

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        OnDied?.Invoke();

        SpawnLightEssence();

        Destroy(gameObject, 1f);
    }

    private void SpawnLightEssence()
    {
        if (lightEssencePrefab == null) return;

        int count = enemyType == EnemyType.Wraith ? 2 : 1;
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = UnityEngine.Random.insideUnitSphere * 0.5f;
            offset.y = 0;
            Instantiate(lightEssencePrefab, transform.position + offset, Quaternion.identity);
        }
    }
}
