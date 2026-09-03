using UnityEngine;
// נותן גישה ל-MonoBehaviour, ל-Transform, ל-Quaternion, למערכים ול-Time של Unity.

[DisallowMultipleComponent]
// מונע שתי מערכות פתיחה שונות על אותו אובייקט של דלת היציאה.
public sealed class ExitDoorInteractable : MonoBehaviour, IInteractable
// מגדיר דלת יציאה מיוחדת שנפתחת בלחיצה רק כאשר המפתח במלאי.
{
    // פתיחת גוף המחלקה ExitDoorInteractable.
    [Header("Door Leaves")]
    // יוצר כותרת ב-Inspector עבור שתי כנפי דלת היציאה שנמצאו בסצנה הראשית.
    [SerializeField] private Transform[] doorPivots;
    // שומר מערך של כנף אחת או יותר שצריכות להיפתח יחד.
    [SerializeField] private float[] openAngles;
    // שומר לכל כנף זווית פתיחה באותו אינדקס; לדוגמה 90 לראשונה ומינוס 90 לשנייה.
    [SerializeField] private float openingSpeed = 120f;
    // קובע כמה מעלות בשנייה כל כנף נעה אל מצב הפתיחה.

    [Header("Required References")]
    // יוצר כותרת ב-Inspector עבור החיבורים למלאי ול-UI.
    [SerializeField] private PlayerInventory playerInventory;
    // שומר את המלאי כדי לבדוק אם מפתח היציאה נאסף.
    [SerializeField] private GameObject lockedMessage;
    // שומר את LockedMessage הקיים שמסביר שהדלת נעולה.
    [SerializeField] private float lockedMessageDuration = 2f;
    // קובע כמה שניות הודעת הדלת הנעולה תישאר על המסך.

    private Quaternion[] closedRotations;
    // שומר לכל כנף את הסיבוב המקומי שבו היא התחילה במצב סגור.
    private Quaternion[] openRotations;
    // שומר לכל כנף את הסיבוב המקומי שאליו היא צריכה להגיע.
    private bool shouldOpen;
    // זוכר אם המפתח אישר לדלת להתחיל להיפתח.

    public bool IsOpen
    // מחזיר אם כל כנפי הדלת כמעט הגיעו לסיבוב הפתוח שלהן.
    {
        // פתיחת גוף המאפיין IsOpen.
        get
        // מתחיל את הקוד שמחשב את התשובה בעת קריאת המאפיין.
        {
            // פתיחת גוף ה-get.
            if (!shouldOpen || doorPivots == null || openRotations == null)
            // בודק שהפתיחה אושרה ושמערכי הדלת קיימים.
            {
                // פתיחת תנאי הדלת שאינה מוכנה.
                return false;
                // מחזיר false כי עדיין אסור לנצח דרך הדלת.
            }
            // סיום תנאי הדלת שאינה מוכנה.

            for (int index = 0; index < doorPivots.Length; index++)
            // עובר על כל כנפי הדלת לפי האינדקס שלהן.
            {
                // פתיחת לולאת בדיקת הכנפיים.
                if (doorPivots[index] == null || Quaternion.Angle(doorPivots[index].localRotation, openRotations[index]) > 1f)
                // בודק אם כנף חסרה או עדיין רחוקה מהסיבוב הפתוח.
                {
                    // פתיחת תנאי כנף שאינה פתוחה.
                    return false;
                    // מחזיר false כי די בכנף אחת סגורה כדי שהדלת כולה לא תיחשב פתוחה.
                }
                // סיום תנאי כנף שאינה פתוחה.
            }
            // סיום לולאת בדיקת הכנפיים.

            return true;
            // מחזיר true לאחר שכל הכנפיים עברו את הבדיקה.
        }
        // סיום גוף ה-get.
    }
    // סיום גוף המאפיין IsOpen.

    private void Awake()
    // פועל פעם אחת כאשר דלת היציאה נטענת ומכין את המערכים והחיבורים.
    {
        // פתיחת המתודה Awake.
        if (playerInventory == null)
        // בודק אם מלאי השחקן לא חובר דרך ה-Inspector.
        {
            // פתיחת תנאי השלמת המלאי.
            playerInventory = FindFirstObjectByType<PlayerInventory>();
            // מחפש את מלאי השחקן היחיד בסצנה.
        }
        // סיום תנאי השלמת המלאי.

        if (doorPivots == null || doorPivots.Length == 0)
        // בודק אם לא הוגדרה אפילו כנף אחת.
        {
            // פתיחת תנאי המערך הריק.
            Debug.LogError("ExitDoorInteractable צריך לפחות Door Pivot אחד", this);
            // מסביר ב-Console שצריך למלא את המערך.
            enabled = false;
            // מכבה את הרכיב כדי למנוע שגיאות בכל פריים.
            return;
            // עוצר את פעולת ההכנה.
        }
        // סיום תנאי המערך הריק.

        if (openAngles == null || openAngles.Length != doorPivots.Length)
        // בודק שמספר הזוויות זהה למספר הכנפיים.
        {
            // פתיחת תנאי המערכים הלא תואמים.
            Debug.LogError("Door Pivots ו-Open Angles חייבים להיות באותו Size", this);
            // מסביר כיצד לתקן את שני המערכים ב-Inspector.
            enabled = false;
            // מכבה את הרכיב כי אי אפשר להתאים זווית לכל כנף.
            return;
            // עוצר את פעולת ההכנה.
        }
        // סיום תנאי המערכים הלא תואמים.

        closedRotations = new Quaternion[doorPivots.Length];
        // יוצר מערך סיבובים סגורים בגודל מספר הכנפיים.
        openRotations = new Quaternion[doorPivots.Length];
        // יוצר מערך סיבובים פתוחים באותו גודל.

        for (int index = 0; index < doorPivots.Length; index++)
        // עובר על כל כנף ומחשב את שני מצבי הסיבוב שלה.
        {
            // פתיחת לולאת הכנת הכנפיים.
            if (doorPivots[index] == null)
            // בודק אם תא מסוים במערך נשאר ריק.
            {
                // פתיחת תנאי הכנף החסרה.
                Debug.LogError("קיים תא ריק במערך Door Pivots", this);
                // מציג ב-Console שחסר חיבור באחד האינדקסים.
                enabled = false;
                // מכבה את הרכיב כדי למנוע שימוש ב-null.
                return;
                // עוצר את פעולת ההכנה.
            }
            // סיום תנאי הכנף החסרה.

            closedRotations[index] = doorPivots[index].localRotation;
            // שומר את הסיבוב הנוכחי של הכנף בתור מצב סגור.
            openRotations[index] = closedRotations[index] * Quaternion.Euler(0f, openAngles[index], 0f);
            // מחשב את מצב הפתיחה של אותה כנף סביב ציר Y המקומי.
        }
        // סיום לולאת הכנת הכנפיים.

        shouldOpen = false;
        // מתחיל כאשר דלת היציאה עדיין סגורה ונעולה.

        if (lockedMessage != null)
        // בודק אם הודעת הנעילה חוברה.
        {
            // פתיחת תנאי הודעת הנעילה.
            lockedMessage.SetActive(false);
            // מסתיר את ההודעה בתחילת המשחק.
        }
        // סיום תנאי הודעת הנעילה.
    }
    // סיום המתודה Awake.

    private void Update()
    // פועל בכל פריים ומניע את כל הכנפיים לאחר שהמפתח אישר פתיחה.
    {
        // פתיחת המתודה Update.
        if (!shouldOpen || openRotations == null)
        // בודק אם עדיין אין אישור פתיחה או שההכנה נכשלה.
        {
            // פתיחת תנאי הדלת הסגורה.
            return;
            // משאיר את הדלת במצב הנוכחי.
        }
        // סיום תנאי הדלת הסגורה.

        for (int index = 0; index < doorPivots.Length; index++)
        // עובר על כל כנפי דלת היציאה.
        {
            // פתיחת לולאת תנועת הכנפיים.
            doorPivots[index].localRotation = Quaternion.RotateTowards(doorPivots[index].localRotation, openRotations[index], openingSpeed * Time.deltaTime);
            // מסובב כל כנף בהדרגה ובקצב שאינו תלוי בפריימים.
        }
        // סיום לולאת תנועת הכנפיים.
    }
    // סיום המתודה Update.

    public void Interact()
    // נקראת כאשר השחקן לוחץ E ומסתכל על הקוליידר של מערכת דלת היציאה.
    {
        // פתיחת המתודה Interact.
        if (shouldOpen)
        // בודק אם הדלת כבר קיבלה אישור להיפתח.
        {
            // פתיחת תנאי הדלת שכבר נפתחת.
            return;
            // מונע הפעלה חוזרת ומשאיר את הדלת פתוחה.
        }
        // סיום תנאי הדלת שכבר נפתחת.

        if (playerInventory == null || !playerInventory.HasExitKey())
        // בודק אם אין מלאי או שהמפתח עדיין לא נאסף.
        {
            // פתיחת תנאי הדלת הנעולה.
            ShowLockedMessage();
            // מציג לשחקן הודעה שעליו למצוא את המפתח.
            return;
            // עוצר בלי לפתוח את הדלת.
        }
        // סיום תנאי הדלת הנעולה.

        shouldOpen = true;
        // מאשר ל-Update להתחיל לסובב את כל כנפי הדלת.

        if (lockedMessage != null)
        // בודק אם הודעת הנעילה קיימת.
        {
            // פתיחת תנאי הסתרת ההודעה.
            lockedMessage.SetActive(false);
            // מסתיר הודעה ישנה ברגע שהמפתח קיים.
        }
        // סיום תנאי הסתרת ההודעה.
    }
    // סיום המתודה Interact.

    private void ShowLockedMessage()
    // מציגה את הודעת הנעילה לזמן מוגבל.
    {
        // פתיחת המתודה ShowLockedMessage.
        if (lockedMessage == null)
        // בודק אם אין אובייקט UI שאפשר להציג.
        {
            // פתיחת תנאי ההודעה החסרה.
            Debug.Log("דלת היציאה נעולה - צריך למצוא את המפתח", this);
            // מציג הודעת גיבוי ב-Console.
            return;
            // יוצא כי אין חלונית שאפשר להפעיל.
        }
        // סיום תנאי ההודעה החסרה.

        CancelInvoke(nameof(HideLockedMessage));
        // מבטל טיימר קודם כדי שלחיצה נוספת תתחיל את משך ההצגה מחדש.
        lockedMessage.SetActive(true);
        // מציג את הודעת הדלת הנעולה.
        Invoke(nameof(HideLockedMessage), lockedMessageDuration);
        // מבקש מ-Unity להסתיר את ההודעה לאחר הזמן שהוגדר.
    }
    // סיום המתודה ShowLockedMessage.

    private void HideLockedMessage()
    // מופעלת אוטומטית לאחר תום משך הודעת הנעילה.
    {
        // פתיחת המתודה HideLockedMessage.
        if (lockedMessage != null)
        // בודק שהודעת ה-UI עדיין קיימת.
        {
            // פתיחת תנאי הסתרת ההודעה.
            lockedMessage.SetActive(false);
            // מסתיר את הודעת הדלת הנעולה.
        }
        // סיום תנאי הסתרת ההודעה.
    }
    // סיום המתודה HideLockedMessage.

    private void OnDisable()
    // פועל אם הדלת או הסקריפט נכבים בזמן שטיימר ההודעה פעיל.
    {
        // פתיחת המתודה OnDisable.
        CancelInvoke();
        // מבטל קריאות מתוזמנות כדי שלא יופעלו על רכיב כבוי.
    }
    // סיום המתודה OnDisable.
}
// סיום גוף המחלקה ExitDoorInteractable.
