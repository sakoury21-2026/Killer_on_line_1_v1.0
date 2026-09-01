using TMPro;
// נותן גישה ל-TMP_Text שמציג את מטרת השלב על המסך.
using UnityEngine;
// נותן גישה ל-MonoBehaviour, ל-GameObject ול-SerializeField של Unity.

[DisallowMultipleComponent]
// מונע שני בקרים שונים שמנסים לעדכן את אותו HUD.
public sealed class GameplayHUD : MonoBehaviour
// מציג את מטרת המשחק ואת סמל מפתח היציאה.
{
    // פתיחת גוף המחלקה GameplayHUD.
    [Header("Data Source")]
    // יוצר כותרת ב-Inspector עבור מקור המידע של ה-HUD.
    [SerializeField] private PlayerInventory playerInventory;
    // שומר חיבור למלאי שהוא מקור האמת של מצב המפתח.

    [Header("UI References")]
    // יוצר כותרת ב-Inspector עבור האובייקטים שמוצגים על המסך.
    [SerializeField] private TMP_Text objectiveText;
    // שומר את רכיב הטקסט שמציג את המטרה הנוכחית.
    [SerializeField] private GameObject exitKeyIcon;
    // שומר את סמל המפתח שיופיע רק לאחר האיסוף.

    [Header("Objective Texts")]
    // יוצר כותרת ב-Inspector עבור מערך משפטי המטרה.
    [SerializeField] private string[] objectives = { "מצאו את מפתח היציאה", "הגיעו אל דלת היציאה" };
    // שומר שתי מטרות בתאים אפס ואחד של מערך מחרוזות.

    private void Awake()
    // פועל פעם אחת ומשלים חיבור למלאי אם הוא לא הוגדר ידנית.
    {
        // פתיחת המתודה Awake.
        if (playerInventory == null)
        // בודק אם מלאי השחקן לא חובר ב-Inspector.
        {
            // פתיחת תנאי השלמת המלאי.
            playerInventory = FindFirstObjectByType<PlayerInventory>();
            // מחפש את מלאי השחקן היחיד בסצנה.
        }
        // סיום תנאי השלמת המלאי.
    }
    // סיום המתודה Awake.

    private void OnEnable()
    // פועל בכל פעם שה-HUD מופעל ומתחיל להאזין לשינוי במפתח.
    {
        // פתיחת המתודה OnEnable.
        if (playerInventory != null)
        // בודק שקיים מלאי שאפשר להאזין לו.
        {
            // פתיחת תנאי ההרשמה.
            playerInventory.ExitKeyChanged += RefreshKeyState;
            // מחבר את פונקציית רענון ה-UI לאירוע שינוי המפתח.
            RefreshKeyState(playerInventory.HasExitKey());
            // מציג מיד את המצב הנוכחי גם לפני שהתרחש אירוע חדש.
        }
        // סיום תנאי ההרשמה.
    }
    // סיום המתודה OnEnable.

    private void OnDisable()
    // פועל כאשר ה-HUD נכבה או כשהסצנה מתחלפת.
    {
        // פתיחת המתודה OnDisable.
        if (playerInventory != null)
        // בודק שהמלאי עדיין קיים.
        {
            // פתיחת תנאי ביטול ההרשמה.
            playerInventory.ExitKeyChanged -= RefreshKeyState;
            // מנתק את המאזין כדי למנוע הפעלה כפולה ודליפת אירועים.
        }
        // סיום תנאי ביטול ההרשמה.
    }
    // סיום המתודה OnDisable.

    private void RefreshKeyState(bool hasExitKey)
    // מקבלת את מצב המפתח ומעדכנת את כל חלקי ה-HUD שתלויים בו.
    {
        // פתיחת המתודה RefreshKeyState.
        if (exitKeyIcon != null)
        // בודק אם סמל המפתח חובר.
        {
            // פתיחת תנאי עדכון הסמל.
            exitKeyIcon.SetActive(hasExitKey);
            // מציג את הסמל כאשר יש מפתח ומסתיר אותו כאשר אין מפתח.
        }
        // סיום תנאי עדכון הסמל.

        int objectiveIndex = hasExitKey ? 1 : 0;
        // בוחר תא מספר אחד אחרי האיסוף ותא מספר אפס לפני האיסוף.

        if (objectiveText != null && objectives != null && objectiveIndex < objectives.Length)
        // בודק שהטקסט קיים ושהאינדקס נמצא בתוך גבולות המערך.
        {
            // פתיחת תנאי עדכון המטרה.
            objectiveText.text = "מטרה: " + objectives[objectiveIndex];
            // משרשר את הכותרת הקבועה עם המטרה שנבחרה מתוך המערך.
        }
        // סיום תנאי עדכון המטרה.
    }
    // סיום המתודה RefreshKeyState.
}
// סיום גוף המחלקה GameplayHUD.
