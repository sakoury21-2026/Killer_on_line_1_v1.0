using UnityEngine;
using UnityEngine.AI;

public class LaurenAI : MonoBehaviour
{
    //private enum AIState { Patrol, Investigate, Search, Chase }

    //[Header("References")]
    //[SerializeField] private NavMeshAgent agent;
    //[SerializeField] private Transform player;
    //[SerializeField] private Transform[] patrolPoints;
    //[SerializeField] private NoiseSystem noiseSystem;
    //[SerializeField] private PlayerStealthState stealthState;
    //[SerializeField] private GameFlow gameFlow;

    //[Header("Movement")]
    //[SerializeField] private float arrivalDistance = 0.4f;
    //[SerializeField] private float searchDuration = 3f;
    //[SerializeField] private float chaseRefreshInterval = 0.2f;

    //[Header("Vision")]
    //[SerializeField] private float viewDistance = 10f;
    //[SerializeField] private float viewAngle = 80f;
    //[SerializeField] private float eyeHeight = 1.6f;
    //[SerializeField] private float catchDistance = 1.2f;
    //[SerializeField] private LayerMask visionMask = ~0;

    //private AIState state = AIState.Patrol;
    //private int patrolIndex;
    //private float searchTimer;
    //private float chaseRefreshTimer;

    //private void Awake()
    //{
    //    if (agent == null) agent = GetComponent<NavMeshAgent>();
    //    if (noiseSystem == null)
    //        noiseSystem = FindFirstObjectByType<NoiseSystem>();
    //    if (stealthState == null && player != null)
    //        stealthState = player.GetComponent<PlayerStealthState>();
    //}

    //private void OnEnable()
    //{
    //    if (noiseSystem != null)
    //        noiseSystem.NoiseReported += HandleNoiseReported;
    //}

    //private void OnDisable()
    //{
    //    if (noiseSystem != null)
    //        noiseSystem.NoiseReported -= HandleNoiseReported;
    //}

    //private void Start()
    //{
    //    GoToNextPatrolPoint();
    //}

    //private void Update()
    //{
    //    if (agent == null || !agent.isOnNavMesh || player == null) return;

    //    bool seesPlayer = CanSeePlayer();
    //    if (seesPlayer)
    //    {
    //        state = AIState.Chase;
    //    }
    //    else if (state == AIState.Chase)
    //    {
    //        state = AIState.Search;
    //        searchTimer = searchDuration;
    //        agent.ResetPath();
    //    }

    //    switch (state)
    //    {
    //        case AIState.Patrol: UpdatePatrol(); break;
    //        case AIState.Investigate: UpdateInvestigate(); break;
    //        case AIState.Search: UpdateSearch(); break;
    //        case AIState.Chase: UpdateChase(); break;
    //    }
    //}

    //private void HandleNoiseReported(Vector3 position, float radius)
    //{
    //    if (agent == null || !agent.isOnNavMesh) return;
    //    if (Vector3.Distance(transform.position, position) > radius) return;
    //    if (state == AIState.Chase) return;

    //    state = AIState.Investigate;
    //    agent.SetDestination(position);
    //}

    //private void UpdatePatrol()
    //{
    //    if (ReachedDestination()) GoToNextPatrolPoint();
    //}

    //private void GoToNextPatrolPoint()
    //{
    //    if (agent == null || !agent.isOnNavMesh) return;
    //    if (patrolPoints == null || patrolPoints.Length == 0) return;
    //    agent.SetDestination(patrolPoints[patrolIndex].position);
    //    patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    //}

    //private void UpdateInvestigate()
    //{
    //    if (!ReachedDestination()) return;
    //    state = AIState.Search;
    //    searchTimer = searchDuration;
    //}

    //private void UpdateSearch()
    //{
    //    searchTimer -= Time.deltaTime;
    //    if (searchTimer > 0f) return;
    //    state = AIState.Patrol;
    //    GoToNextPatrolPoint();
    //}

    //private void UpdateChase()
    //{
    //    chaseRefreshTimer -= Time.deltaTime;
    //    if (chaseRefreshTimer <= 0f)
    //    {
    //        agent.SetDestination(player.position);
    //        chaseRefreshTimer = chaseRefreshInterval;
    //    }

    //    if (gameFlow != null &&
    //        Vector3.Distance(transform.position, player.position) <= catchDistance &&
    //        CanSeePlayer())
    //    {
    //        gameFlow.PlayerCaught();
    //    }
    //}

    //private bool ReachedDestination()
    //{
    //    return !agent.pathPending &&
    //           agent.remainingDistance <= arrivalDistance;
    //}

    //private bool CanSeePlayer()
    //{
    //    if (stealthState != null && stealthState.IsHidden) return false;

    //    Vector3 eye = transform.position + Vector3.up * eyeHeight;
    //    Vector3 target = player.position + Vector3.up;
    //    Vector3 toPlayer = target - eye;
    //    float distance = toPlayer.magnitude;

    //    if (distance > viewDistance) return false;
    //    if (Vector3.Angle(transform.forward, toPlayer) > viewAngle * 0.5f)
    //        return false;

    //    if (!Physics.Raycast(eye, toPlayer.normalized, out RaycastHit hit,
    //                         distance, visionMask, QueryTriggerInteraction.Ignore))
    //        return false;

    //    return hit.transform.root == player.root;
    //}
}
