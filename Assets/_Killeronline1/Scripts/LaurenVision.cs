using UnityEngine;

public class LaurenVision : MonoBehaviour
{
    [Header("Player References")]
    // יוצר כותרת ב-Inspector עבור חיבורי השחקן.
    [SerializeField] private Transform player;
    // שומר את ה-Transform הראשי של השחקן כדי לחשב כיוון ומרחק.
    [SerializeField] private PlayerStealthState playerStealthState;
    // שומר את מצב ההתגנבות כדי לא לראות שחקן שמוסתר כחוק.
    [SerializeField] private RoomTracker playerRoomTracker;
    // שומר את החדר הנוכחי של השחקן.

    [Header("Lauren References")]
    // יוצר כותרת ב-Inspector עבור חיבורי Lauren.
    [SerializeField] private Transform eyePoint;
    // קובע את הנקודה שממנה יוצאת קרן הראייה בגובה העיניים.
    [SerializeField] private RoomTracker laurenRoomTracker;
    // שומר את החדר הנוכחי של Lauren.

    [Header("Vision Settings")]
    // יוצר כותרת ב-Inspector עבור חוקי הראייה.
    [SerializeField] private float visionRange = 12f;
    // קובע את המרחק המרבי שבו Lauren יכולה לראות את השחקן.
    [SerializeField, Range(1f, 180f)] private float visionAngle = 80f;
    // קובע את רוחב חרוט הראייה במעלות כדי למנוע ראייה מאחור.
    [SerializeField] private LayerMask visionMask = ~0;
    // קובע אילו שכבות יכולות להיפגע מהקרן וצריך לכלול את השחקן ואת קירות הסביבה.

    public bool CanSeePlayer()
    // מחזיר true רק אם כל חוקי הראייה עברו בהצלחה.
    {
        // פתיחת המתודה CanSeePlayer.
        if (player == null || playerStealthState == null)
        // בודק שקיימים גם השחקן וגם מקור האמת של ההתגנבות.
        {
            // פתיחת תנאי חיבורי השחקן החסרים.
            return false;
            // מחזיר false כי אסור לנחש כאשר חסר חיבור חשוב.
        }
        // סיום תנאי חיבורי השחקן החסרים.

        if (laurenRoomTracker == null || playerRoomTracker == null)
        // בודק ששני האובייקטים מחוברים למערכת החדרים.
        {
            // פתיחת תנאי חיבורי החדר החסרים.
            return false;
            // מחזיר false כי אין דרך בטוחה לבדוק אם הם באותו חדר.
        }
        // סיום תנאי חיבורי החדר החסרים.

        RoomVolume laurenRoom = laurenRoomTracker.CurrentRoom;
        // קורא פעם אחת את החדר הנוכחי של Lauren.
        RoomVolume playerRoom = playerRoomTracker.CurrentRoom;
        // קורא פעם אחת את החדר הנוכחי של השחקן.

        if (laurenRoom == null || playerRoom == null)
        // בודק שאף אחד מהם אינו נמצא בשטח שלא כוסה על ידי RoomVolume.
        {
            // פתיחת תנאי החדר הלא ידוע.
            return false;
            // מחזיר false כדי ש-null מול null לא ייחשב בטעות לאותו חדר.
        }
        // סיום תנאי החדר הלא ידוע.

        if (laurenRoom != playerRoom)
        // בודק אם Lauren והשחקן נמצאים בחדרים שונים.
        {
            // פתיחת תנאי החדרים השונים.
            return false;
            // מחזיר false כי לפי חוק המשחק Lauren רואה רק בתוך אותו חדר.
        }
        // סיום תנאי החדרים השונים.

        if (playerStealthState.IsHidden)
        // בודק אם השחקן נמצא במחבוא וגם כורע.
        {
            // פתיחת תנאי ההסתרה.
            return false;
            // מחזיר false כי המחבוא מסתיר את השחקן מ-Lauren.
        }
        // סיום תנאי ההסתרה.

        Vector3 eyePosition = eyePoint != null ? eyePoint.position : transform.position + Vector3.up * 1.6f;
        // בוחר נקודת עיניים מחוברת או גובה גיבוי מעל Lauren.
        Vector3 targetPosition = player.position + Vector3.up;
        // מכוון את הקרן בערך למרכז הגוף ולא לכפות הרגליים.
        Vector3 directionToPlayer = targetPosition - eyePosition;
        // יוצר וקטור מנקודת העיניים אל השחקן.
        float distanceToPlayer = directionToPlayer.magnitude;
        // מחשב את אורך הווקטור שהוא המרחק אל השחקן.

        if (distanceToPlayer > visionRange)
        // בודק אם השחקן רחוק יותר מטווח הראייה.
        {
            // פתיחת תנאי המרחק.
            return false;
            // מחזיר false כי Lauren אינה רואה למרחק בלתי מוגבל.
        }
        // סיום תנאי המרחק.

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        // מחשב את הזווית בין הפנים של Lauren לבין הכיוון לשחקן.

        if (angleToPlayer > visionAngle * 0.5f)
        // בודק אם השחקן נמצא מחוץ לחצי הזווית בכל צד של מרכז המבט.
        {
            // פתיחת תנאי הזווית.
            return false;
            // מחזיר false כדי ש-Lauren לא תראה מאחור או רחוק מדי לצדדים.
        }
        // סיום תנאי הזווית.

        if (!Physics.Raycast(eyePosition, directionToPlayer.normalized, out RaycastHit hit, distanceToPlayer, visionMask, QueryTriggerInteraction.Ignore))
        // יורה קרן ומתעלם מ-Trigger כמו RoomVolume ו-HideZone.
        {
            // פתיחת תנאי הקרן שלא פגעה.
            return false;
            // מחזיר false כי הקרן חייבת לפגוע בשחקן עצמו כדי להוכיח קו ראייה.
        }
        // סיום תנאי הקרן שלא פגעה.

        PlayerStealthState hitPlayer = hit.collider.GetComponentInParent<PlayerStealthState>();
        // בודק אם הדבר הראשון שהקרן פגעה בו שייך לשחקן.
        return hitPlayer == playerStealthState;
        // מחזיר true רק אם הפגיעה הראשונה היא בשחקן ולא בקיר או ברהיט.
    }
    // סיום המתודה CanSeePlayer.

    private void OnDrawGizmosSelected()
    // מצייר עזר בסצנה רק כאשר Lauren מסומנת ב-Editor.
    {
        // פתיחת המתודה OnDrawGizmosSelected.
        Vector3 eyePosition = eyePoint != null ? eyePoint.position : transform.position + Vector3.up * 1.6f;
        // מחשב את אותה נקודת עיניים שמשמשת בזמן המשחק.
        Gizmos.color = Color.yellow;
        // בוחר צבע צהוב לקו שמציג את טווח הראייה.
        Gizmos.DrawRay(eyePosition, transform.forward * visionRange);
        // מצייר קו קדימה באורך טווח הראייה כדי להקל על הכיוון ב-Scene.
    }
    // סיום המתודה OnDrawGizmosSelected.

}
