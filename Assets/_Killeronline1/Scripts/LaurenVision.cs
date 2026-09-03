
using UnityEngine;
// נותן גישה ל-Transform, ל-LayerMask, ל-Ray ולשאר כלי הראייה של Unity.

[DisallowMultipleComponent]
// מונע התקנה כפולה של מערכת הראייה על Lauren.
public sealed class LaurenVision : MonoBehaviour
// אחראי רק לשאלה אם Lauren רואה את השחקן ברגע הנוכחי.
{

    [Header("Player References")]
    [SerializeField] private Transform player;
    // שומר את ה-Transform של השחקן.

    [SerializeField] private PlayerStealthState playerStealthState;
    // שומר את מצב ההתגנבות של השחקן.

    [SerializeField] private RoomTracker playerRoomTracker;
    // שומר את RoomTracker של השחקן.

    [Header("Lauren References")]
    [SerializeField] private Transform eyePoint;
    // נקודת העיניים שממנה יוצאת קרן הראייה.

    [SerializeField] private RoomTracker laurenRoomTracker;
    // RoomTracker של Lauren.

    [Header("Vision Settings")]
    [SerializeField] private float visionRange = 12f;
    // טווח הראייה המקסימלי.

    [SerializeField, Range(1f, 180f)] private float visionAngle = 80f;
    // רוחב חרוט הראייה.

    [SerializeField] private LayerMask visionMask = ~0;
    // השכבות שה-Ray יכול לפגוע בהן.

    private void Awake()
    // מחבר אוטומטית את כל ה-References בתחילת המשחק.
    {
        FindPlayerReferences();

        if (laurenRoomTracker == null)
        {
            laurenRoomTracker = GetComponent<RoomTracker>();
        }

        if (eyePoint == null)
        {
            Transform foundEyePoint = transform.Find("EyePoint");

            if (foundEyePoint != null)
            {
                eyePoint = foundEyePoint;
            }
        }

        DebugPlayerSetup();
    }

    private void FindPlayerReferences()
    // מוצא את ה-Player ואת הרכיבים הדרושים לו.
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject == null)
            {
                Debug.LogError(
                    "LaurenVision לא מצא Player. ודא של-Player יש Tag בשם 'Player'.",
                    this
                );

                return;
            }

            player = playerObject.transform;
        }

        if (playerStealthState == null)
        {
            playerStealthState =
                player.GetComponentInParent<PlayerStealthState>();
        }

        if (playerRoomTracker == null)
        {
            playerRoomTracker =
                player.GetComponentInParent<RoomTracker>();
        }
    }

    private void DebugPlayerSetup()
    // מציג ב-Console בדיוק מה LaurenVision הצליח למצוא.
    {
        if (player == null)
        {
            Debug.LogError("LaurenVision: Player חסר.", this);
        }
        else
        {
            Debug.Log("LaurenVision: Player נמצא -> " + player.name, this);
        }

        if (playerStealthState == null)
        {
            Debug.LogError(
                "LaurenVision: PlayerStealthState חסר על ה-Player או על אחד ההורים שלו.",
                this
            );
        }

        if (playerRoomTracker == null)
        {
            Debug.LogError(
                "LaurenVision: RoomTracker חסר על ה-Player או על אחד ההורים שלו.",
                this
            );
        }

        if (laurenRoomTracker == null)
        {
            Debug.LogError(
                "LaurenVision: RoomTracker של Lauren חסר.",
                this
            );
        }
    }

    public bool CanSeePlayer()
    // מחזיר true רק כאשר כל חוקי הראייה מתקיימים.
    {
        if (player == null ||
            playerStealthState == null ||
            laurenRoomTracker == null ||
            playerRoomTracker == null)
        {
            return false;
        }

        RoomVolume laurenRoom = laurenRoomTracker.CurrentRoom;
        RoomVolume playerRoom = playerRoomTracker.CurrentRoom;

        if (laurenRoom == null || playerRoom == null)
        {
            return false;
        }

        if (laurenRoom != playerRoom)
        {
            return false;
        }

        if (playerStealthState.IsHidden)
        {
            return false;
        }

        Vector3 eyePosition =
            eyePoint != null
                ? eyePoint.position
                : transform.position + Vector3.up * 1.6f;

        Vector3 targetPosition = player.position + Vector3.up;

        Vector3 directionToPlayer = targetPosition - eyePosition;

        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > visionRange)
        {
            return false;
        }

        float angleToPlayer =
            Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer > visionAngle * 0.5f)
        {
            return false;
        }

        if (!Physics.Raycast(
                eyePosition,
                directionToPlayer.normalized,
                out RaycastHit hit,
                distanceToPlayer,
                visionMask,
                QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        PlayerStealthState hitPlayer =
            hit.collider.GetComponentInParent<PlayerStealthState>();

        return hitPlayer == playerStealthState;
    }

    private void OnDrawGizmosSelected()
    // מציג את כיוון וטווח הראייה ב-Scene.
    {
        Vector3 eyePosition =
            eyePoint != null
                ? eyePoint.position
                : transform.position + Vector3.up * 1.6f;

        Gizmos.color = Color.yellow;

        Gizmos.DrawRay(eyePosition, transform.forward * visionRange);


        
    }
}


// סיום גוף המחלקה LaurenVision
