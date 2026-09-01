using UnityEngine;
using UnityEngine.AI;

public class EnemeAI : MonoBehaviour
{
    // 1. הגדרת המצבים
    public enum AIState { Patrol, Investigate, Search, Chase }

    [Header("State Management")]
    [SerializeField] private AIState currentState = AIState.Patrol;

    [Header("AI Settings")]
    [SerializeField] private float searchDuration = 5f;

    private Animator animator;
    private NavMeshAgent agent;
    private float searchTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 2. הרצת הלוגיקה של המצב הנוכחי בלבד
        switch (currentState)
        {
            case AIState.Patrol: UpdatePatrol(); break;
            case AIState.Investigate: UpdateInvestigate(); break;
            case AIState.Search: UpdateSearch(); break;
            case AIState.Chase: UpdateChase(); break;
        }
    }

    // 3. הפונקציה החשובה ביותר: מנהלת את המעבר בצורה נקייה ומסודרת
    public void ChangeState(AIState newState)
    {
        currentState = newState;
        if (animator != null)
        {
            switch (currentState)
            {
                case AIState.Patrol: animator.SetTrigger("Patrol"); break;
                case AIState.Investigate: animator.SetTrigger("Investigate"); break;
                case AIState.Search: animator.SetTrigger("Search"); break;
                case AIState.Chase: animator.SetTrigger("Chase"); break;

            }
        }
    }


    // --- לוגיקת המצבים והמעברים ביניהם ---

    private void UpdatePatrol()
    {
        // קוד סיור...
        if (SeesPlayer())
        {
            ChangeState(AIState.Chase);
            Debug.Log("שחקן נראה! מתחילים מרדף!");
        }
    }

    private void UpdateChase()
    {
        // קוד מרדף...
        if (!SeesPlayer())
        {
            ChangeState(AIState.Search); // הפונקציה תטפל אוטומטית באיפוס ה-Path והטיימר!
        }
    }

    private void UpdateSearch()
    {
        // קוד חיפוש (ספירה לאחור של הטיימר)
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0)
        {
            ChangeState(AIState.Patrol);
        }
    }

    private void UpdateInvestigate()
    {
        // קוד חקירת רעש...
        if (agent.remainingDistance < 0.5f)
        {
            ChangeState(AIState.Patrol);
            Debug.Log("Patroling...");
        }
    }

    // 4. טיפול באירוע שמע (הקוד בתחתית התמונה שלך)
    public void HandleNoiseReported(Vector3 position, float radius)
    {
        if (agent == null || !agent.isOnNavMesh) return;
        if (Vector3.Distance(transform.position, position) > radius) return;
        if (currentState == AIState.Chase) return; // מתעלמים מרעש אם כבר רודפים אחרי השחקן

        ChangeState(AIState.Investigate);
        agent.SetDestination(position);
    }

    private bool SeesPlayer() => false; // פונקציית עזר לראיית שחקן
}
