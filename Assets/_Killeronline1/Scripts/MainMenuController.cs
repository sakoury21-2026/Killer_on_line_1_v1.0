using UnityEngine;
using UnityEngine.SceneManagement;

// נותן גישה ל-MonoBehaviour, ל-GameObject, ל-Application ול-Time של Unity.
// נותן גישה לטעינת סצנת המשחק לפי שמה.

namespace KillerOnline
{
    [DisallowMultipleComponent]
    // מונע שני בקרים שונים על אותו תפריט ראשי.
    public sealed class MainMenuController : MonoBehaviour
    // מחבר את כפתורי התפריט הראשי לפעולות של המשחק.
    {
        // פתיחת גוף המחלקה MainMenuController.
        [SerializeField] private string gameplayScene = "Showcase_Graybox";
        // שומר את שם סצנת המשחק הראשית שנטענת בלחיצה על Play.
        [SerializeField] private GameObject InstructionsPanel;
        // שומר את חלונית ההוראות שאפשר לפתוח ולסגור.

        private void Awake()
        // פועל כאשר סצנת MAIN_MENU נטענת.
        {
            // פתיחת המתודה Awake.
            Time.timeScale = 1f;
            // מבטיח שהתפריט פועל בזמן רגיל גם אם חזרנו ממשחק שהיה בהשהיה.
            Cursor.lockState = CursorLockMode.None;
            // משחרר את סמן העכבר כדי שאפשר יהיה ללחץ על הכפתורים.
            Cursor.visible = true;
            // מציג את סמן העכבר בתפריט.

            if (InstructionsPanel != null)
            // בודק אם חלונית ההוראות חוברה.
            {
                // פתיחת תנאי חלונית ההוראות.
                InstructionsPanel.SetActive(false);
                // מסתיר את ההוראות בתחילת התפריט.
            }
            // סיום תנאי חלונית ההוראות.
        }
        // סיום המתודה Awake.

        public void PlayGame()
        // מחוברת לאירוע On Click של כפתור Play.
        {
            // פתיחת המתודה PlayGame.
            SceneManager.LoadScene(gameplayScene);
            // טוען את הסצנה לפי השם שהוגדר ב-Inspector.
        }
        // סיום המתודה PlayGame.

        public void OpenInstructions()
        // מחוברת לכפתור Instructions.
        {
            // פתיחת המתודה OpenInstructions.
            if (InstructionsPanel != null)
            // בודק אם החלונית קיימת.
            {
                // פתיחת תנאי פתיחת ההוראות.
                InstructionsPanel.SetActive(true);
                // מציג את חלונית ההוראות.
            }
            // סיום תנאי פתיחת ההוראות.
        }
        // סיום המתודה OpenInstructions.

        public void CloseInstructions()
        // מחוברת לכפתור Back שבתוך חלונית ההוראות.
        {
            // פתיחת המתודה CloseInstructions.
            if (InstructionsPanel != null)
            // בודק אם החלונית קיימת.
            {
                // פתיחת תנאי סגירת ההוראות.
                InstructionsPanel.SetActive(false);
                // מסתיר את חלונית ההוראות וחוזר לכפתורי התפריט.
            }
            // סיום תנאי סגירת ההוראות.
        }
        // סיום המתודה CloseInstructions.

        public void QuitGame()
        // מחוברת לאירוע On Click של כפתור Quit.
        {
            // פתיחת המתודה QuitGame.
    #if UNITY_EDITOR
            // מתחיל קוד שיופעל רק בתוך Unity Editor.
            UnityEditor.EditorApplication.isPlaying = false;
            // עוצר את Play Mode כדי לבדוק את הכפתור בתוך ה-Editor.
    #else
    // מתחיל קוד שיופעל ב-Build אמיתי.
            Application.Quit();
    // סוגר את המשחק בגרסת Windows.
    #endif
            // מסיים את הבחירה בין Editor לבין Build.
        }
        // סיום המתודה QuitGame.
    }
    // סיום גוף המחלקה MainMenuController.
}

