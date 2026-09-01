using System.Collections;
using UnityEngine;
using UnityEngine.AI;
 [RequireComponent(typeof(NavMeshAgent))]
public class PatrolPoint : MonoBehaviour
{
    
        public enum State { Patrol, Chase }
        public State currentState = State.Patrol;

        [Header("Patrol")]
        public Transform[] waypoints;
        private int currentWaypoint = 0;
        public bool isOpen;

        [Header("Detection")]
        public Transform player;
        public float viewDistance = 15f;
        public float viewAngle = 60f; // זווית ראייה
        public float catchDistance = 1.2f; // מרחק שבו היא "תופסת" אותך
        public LayerMask obstacleMask; // קירות שחוסמים ראייה

        [Header("Speeds")]
        public float patrolSpeed = 2f;
        public float chaseSpeed = 5f;

        private NavMeshAgent agent;

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.autoBraking = false;
            GoToNextWaypoint();
        }

        void Update()
        {
            switch (currentState)
            {
                case State.Patrol:
                    PatrolUpdate();
                    if (CanSeePlayer()) currentState = State.Chase;
                    break;

                case State.Chase:
                    ChaseUpdate();
                    break;
            }
        }

        // ---------- PATROL ----------
        void PatrolUpdate()
        {
            agent.speed = patrolSpeed;
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                GoToNextWaypoint();
            }
        }

        void GoToNextWaypoint()
        {
            if (waypoints.Length == 0) return;
            agent.destination = waypoints[currentWaypoint].position;
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }

        // ---------- CHASE ----------
        void ChaseUpdate()
        {
            agent.speed = chaseSpeed;
            agent.destination = player.position;

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= catchDistance)
            {
                CatchPlayer();
            }
        }

        void CatchPlayer()
        {
            Debug.Log("המפלצת תפסה אותך!");
            // כאן תפעיל את מה שקורה כשאתה מת
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Die();
            }
        }

        // ---------- DETECTION ----------
        bool CanSeePlayer()
        {
            Vector3 dirToPlayer = (player.position - transform.position);
            float distance = dirToPlayer.magnitude;

            if (distance > viewDistance) return false;

            // בדיקת זווית ראייה
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            if (angle > viewAngle / 2f) return false;

            // בדיקת חסימה (קירות) - Raycast
            if (Physics.Raycast(transform.position, dirToPlayer.normalized, out RaycastHit hit, distance, obstacleMask))
            {
                return false; // יש קיר בדרך
            }

            return true; // רואה את השחקן!
        }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Door"))
        {
            isOpen = true;
        }
    }

}
