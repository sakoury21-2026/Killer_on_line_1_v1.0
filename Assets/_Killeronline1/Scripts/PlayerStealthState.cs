using UnityEngine;
// נותן גישה ל-MonoBehaviour, ל-SerializeField ול-Debug של Unity.

[DisallowMultipleComponent]
// מונע שני מקורות שונים שיחזיקו מצב התגנבות על אותו שחקן.
[RequireComponent(typeof(PlayerMovement))]
// מבטיח שעל אותו GameObject יהיה PlayerMovement שמחזיק את מצב הכריעה האמיתי.
public sealed class PlayerStealthState : MonoBehaviour
// מרכז עבור Lauren את המידע אם השחקן נמצא במחבוא וגם כורע.
{
    // פתיחת גוף המחלקה PlayerStealthState.
    [SerializeField] private PlayerMovement movement;
    // שומר חיבור לרכיב התנועה שממנו נקרא את מצב הכריעה.
    [SerializeField] private bool logStateChangesToConsole = true;
    // מאפשר להציג הודעות בדיקה כאשר נכנסים למחבוא או יוצאים ממנו.

    private int hideZoneContactCount;
    // סופר כמה אזורי מחבוא חופפים נוגעים בשחקן כדי שיציאה מאחד לא תבטל אזור אחר.

    public bool IsCrouching => movement != null && movement.IsCrouching;
    // מחזיר true רק כאשר רכיב התנועה קיים והשחקן כורע.
    public bool IsInsideHideZone => hideZoneContactCount > 0;
    // מחזיר true כאשר השחקן נמצא לפחות באזור מחבוא אחד.
    public bool IsHidden => IsInsideHideZone && IsCrouching;
    // מחזיר true רק כאשר שני התנאים מתקיימים יחד: בתוך מחבוא וגם בכריעה.

    private void Awake()
    // פועל פעם אחת כאשר השחקן נטען ומכין את החיבור המקומי.
    {
        // פתיחת המתודה Awake.
        if (movement == null)
        // בודק אם PlayerMovement לא חובר דרך ה-Inspector.
        {
            // פתיחת תנאי השלמת החיבור.
            movement = GetComponent<PlayerMovement>();
            // מחפש PlayerMovement על אותו GameObject של השחקן.
        }
        // סיום תנאי השלמת החיבור.

        if (movement == null)
        // בודק אם החיבור עדיין חסר למרות RequireComponent והחיפוש המקומי.
        {
            // פתיחת תנאי השגיאה.
            Debug.LogError("PlayerStealthState לא מצא PlayerMovement על אותו GameObject", this);
            // מציג שגיאת הגדרה ומקשר אותה לשחקן הבעייתי.
        }
        // סיום תנאי השגיאה.

        hideZoneContactCount = 0;
        // מאתחל את השחקן מחוץ לכל אזור מחבוא בתחילת המשחק.
    }
    // סיום המתודה Awake.

    public void SetHidden(bool insideHideZone)
    // נשארת בשם הקיים כדי לשמור על החיבור ל-HideZone אך מעדכנת כניסה או יציאה מאזור מחבוא.
    {
        // פתיחת המתודה SetHidden.
        bool wasHidden = IsHidden;
        // שומר את תוצאת ההסתרה לפני העדכון כדי לדעת אם היא השתנתה.

        if (insideHideZone)
        // בודק אם התקבלה הודעת כניסה לאזור מחבוא.
        {
            // פתיחת תנאי הכניסה.
            hideZoneContactCount++;
            // מוסיף מגע אחד עם אזור מחבוא.
        }
        // סיום תנאי הכניסה.
        else
        // מופעל כאשר התקבלה הודעת יציאה מאזור מחבוא.
        {
            // פתיחת תנאי היציאה.
            hideZoneContactCount = Mathf.Max(0, hideZoneContactCount - 1);
            // מפחית מגע אחד ולא מאפשר למונה להפוך לשלילי.
        }
        // סיום תנאי היציאה.

        if (logStateChangesToConsole && wasHidden != IsHidden)
        // בודק אם מצב ההסתרה האמיתי השתנה והדפסת הבדיקה מופעלת.
        {
            // פתיחת תנאי הודעת הבדיקה.
            Debug.Log("Player hidden: " + IsHidden, this);
            // משרשר טקסט עם ערך אמת או שקר ומציג אותו ב-Console.
        }
        // סיום תנאי הודעת הבדיקה.
    }
    // סיום המתודה SetHidden.

    private void OnDisable()
    // פועל אם השחקן או הסקריפט נכבים בזמן שהשחקן בתוך מחבוא.
    {
        // פתיחת המתודה OnDisable.
        hideZoneContactCount = 0;
        // מנקה את מגעי המחבוא כדי שהשחקן לא יישאר מוסתר בהפעלה הבאה.
    }
    // סיום המתודה OnDisable.
}
// סיום גוף המחלקה PlayerStealthState
