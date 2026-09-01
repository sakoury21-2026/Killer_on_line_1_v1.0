using UnityEngine;

public class PlayerStealthState : MonoBehaviour
{
    // פתיחת גוף המחלקה PlayerStealthState.
    [SerializeField] private PlayerMovement movement; // שומר חיבור מפורש לרכיב התנועה שממנו נקרא את מצב הכריעה.
    [SerializeField] private bool logStateChangesToConsole = true; // מאפשר להציג שינויי הסתרה בזמן הבדיקה בלי להפוך את הלוגיקה לתלויה ב-Debug.Log.

    public bool IsHidden { get; private set; } // מאפשר למערכות אחרות לקרוא את מצב ההסתרה אך מאפשר לשנות אותו רק מתוך המחלקה הזאת.
    public bool IsCrouching => movement != null && movement.IsCrouching; // מחזיר את מצב הכריעה מהמקור האמיתי ומגן מפני חיבור חסר.

    private void Awake() // פועל פעם אחת כאשר השחקן נטען ומשלים את החיבור המקומי במקרה הצורך.
    { // פתיחת המתודה Awake.
        if (movement == null) // בודק אם PlayerMovement לא חובר דרך ה-Inspector.
        { // פתיחת תנאי השלמת החיבור.
            movement = GetComponent<PlayerMovement>(); // מחפש PlayerMovement על אותו GameObject של השחקן.
        } // סיום תנאי השלמת החיבור.

        if (movement == null) // בודק אם החיבור עדיין חסר למרות RequireComponent והחיפוש המקומי.
        { // פתיחת תנאי השגיאה.
            Debug.LogError("PlayerStealthState לא מצא PlayerMovement על אותו GameObject", this); // מציג שגיאת הגדרה ומקשר אותה לרכיב הבעייתי.
        } // סיום תנאי השגיאה.

        IsHidden = false; // מאתחל את השחקן כגלוי בתחילת המשחק עד שייכנס לאזור מסתור.
    } // סיום המתודה Awake.

    public void SetHidden(bool hidden) // מספק ל-HideZone מתודה ציבורית אחת לעדכון מצב הכניסה או היציאה ממחבוא.
    { // פתיחת המתודה SetHidden.
        if (IsHidden == hidden) // בודק אם המצב המבוקש כבר פעיל.
        { // פתיחת תנאי מניעת העדכון הכפול.
            return; // מונע שמירה והדפסה חוזרות כשלא התרחש שינוי אמיתי.
        } // סיום תנאי מניעת העדכון הכפול.

        IsHidden = hidden; // שומר את המצב החדש בתוך מקור האמת היחיד של ההסתרה.

        if (logStateChangesToConsole) // בודק אם הודעות הבדיקה מופעלות ב-Inspector.
        { // פתיחת תנאי הודעת הבדיקה.
            Debug.Log("Player hidden: " + IsHidden, this); // מציג ב-Console את מצב ההסתרה החדש ומקשר את ההודעה לשחקן.
        } // סיום תנאי הודעת הבדיקה.
    } // סיום המתודה SetHidden.
} // סיום המחלקה PlayerStealthState.


