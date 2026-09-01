using UnityEngine;
// נותן גישה ל-MonoBehaviour, ל-GameObject, ל-Cursor ול-Time של Unity.
using UnityEngine.InputSystem;
// נותן גישה ל-PlayerInput כדי לעצור את שליטת השחקן בסיום המשחק.

[DisallowMultipleComponent]
// מונע שני מנהלי סיום שונים על אותו GameObject.
public sealed class GameFlow : MonoBehaviour
// מנהל את מצבי הניצחון וההפסד בזמן ההצגה בתוך Unity.
{
    // פתיחת גוף המחלקה GameFlow.
    [Header("Gameplay Reference")]
    // יוצר כותרת ב-Inspector עבור חיבור השחקן.
    [SerializeField] private PlayerInput playerInput;
    // שומר את רכיב הקלט כדי לעצור תנועה ומבט לאחר סיום המשחק.

    [Header("End Panels")]
    // יוצר כותרת ב-Inspector עבור מסכי הסיום הקיימים ב-GameUI.
    [SerializeField] private GameObject winPanel;
    // שומר את WinPanel שיופיע כאשר השחקן יוצא עם המפתח.
    [SerializeField] private GameObject losePanel;
    // שומר את LosePanel שיופיע כאשר Lauren תופסת את השחקן.

    public bool IsFinished { get; private set; }
    // מאפשר למערכות אחרות לקרוא אם המשחק הסתיים בלי לשנות את המצב מבחוץ.

    private void Awake()
    // פועל פעם אחת כאשר סצנת המשחק נטענת ומכין את מצב ההצגה.
    {
        // פתיחת המתודה Awake.
        Time.timeScale = 1f;
        // מבטיח שהזמן רץ גם אם עצרנו את ההצגה הקודמת במסך סיום.
        IsFinished = false;
        // מסמן שהמשחק עדיין פעיל.

        if (playerInput == null)
        // בודק אם קלט השחקן לא חובר דרך ה-Inspector.
        {
            // פתיחת תנאי השלמת קלט השחקן.
            playerInput = FindFirstObjectByType<PlayerInput>();
            // מחפש את PlayerInput היחיד בסצנה כגיבוי לחיבור ידני.
        }
        // סיום תנאי השלמת קלט השחקן.

        if (winPanel != null)
        // בודק אם חלונית הניצחון חוברה.
        {
            // פתיחת תנאי חלונית הניצחון.
            winPanel.SetActive(false);
            // מסתיר את חלונית הניצחון בתחילת המשחק.
        }
        // סיום תנאי חלונית הניצחון.

        if (losePanel != null)
        // בודק אם חלונית ההפסד חוברה.
        {
            // פתיחת תנאי חלונית ההפסד.
            losePanel.SetActive(false);
            // מסתיר את חלונית ההפסד בתחילת המשחק.
        }
        // סיום תנאי חלונית ההפסד.

        if (playerInput == null || winPanel == null || losePanel == null)
        // בודק אם אחד מחיבורי החובה עדיין חסר.
        {
            // פתיחת תנאי החיבור החסר.
            Debug.LogError("GameFlow צריך PlayerInput, WinPanel ו-LosePanel מחוברים", this);
            // מציג ב-Console רשימת חיבורים ברורה.
        }
        // סיום תנאי החיבור החסר.
    }
    // סיום המתודה Awake.

    public void ShowWin()
    // נקראת כאשר השחקן עבר את אזור היציאה עם המפתח ודלת פתוחה.
    {
        // פתיחת המתודה ShowWin.
        FinishGame(winPanel);
        // מסיימת את המשחק ומציגה את חלונית הניצחון.
    }
    // סיום המתודה ShowWin.

    public void ShowLose()
    // נקראת כאשר אנימציית התפיסה של Lauren הסתיימה.
    {
        // פתיחת המתודה ShowLose.
        FinishGame(losePanel);
        // מסיימת את המשחק ומציגה את חלונית ההפסד.
    }
    // סיום המתודה ShowLose.

    public void PlayerCaught()
    // שומרת תאימות לקוד ישן שעלול לקרוא בשם PlayerCaught.
    {
        // פתיחת המתודה PlayerCaught.
        ShowLose();
        // מעבירה את הבקשה למתודת ההפסד שהיא מקור האמת היחיד.
    }
    // סיום המתודה PlayerCaught.

    private void FinishGame(GameObject panelToShow)
    // מרכזת במקום אחד את הפעולות המשותפות לניצחון ולהפסד.
    {
        // פתיחת המתודה FinishGame.
        if (IsFinished)
        // בודק אם אירוע קודם כבר סיים את המשחק.
        {
            // פתיחת תנאי מניעת סיום כפול.
            return;
            // מונע הצגת מסכים ושינוי מצב פעמיים.
        }
        // סיום תנאי מניעת סיום כפול.

        IsFinished = true;
        // מסמן לכל המערכות שהמשחק הסתיים.
        playerInput?.DeactivateInput();
        // עוצר את קלט השחקן אם הרכיב חובר.

        if (panelToShow != null)
        // בודק אם קיימת חלונית שאפשר להציג.
        {
            // פתיחת תנאי הצגת החלונית.
            panelToShow.SetActive(true);
            // מציג את מסך הניצחון או ההפסד שנשלח לפונקציה.
        }
        // סיום תנאי הצגת החלונית.

        Cursor.lockState = CursorLockMode.None;
        // משחרר את העכבר בסיום ההצגה.
        Cursor.visible = true;
        // מציג את סמן העכבר.
        Time.timeScale = 0f;
        // עוצר את עולם המשחק לאחר שהתקבלה תוצאת ניצחון או הפסד.
    }
    // סיום המתודה FinishGame.

    private void OnDestroy()
    // פועל כאשר יוצאים מהסצנה או מפסיקים Play Mode.
    {
        // פתיחת המתודה OnDestroy.
        Time.timeScale = 1f;
        // מחזיר את הזמן למצב רגיל כדי שההפעלה הבאה לא תתחיל קפואה.
    }
    // סיום המתודה OnDestroy.
}
// סיום גוף המחלקה GameFlow.
