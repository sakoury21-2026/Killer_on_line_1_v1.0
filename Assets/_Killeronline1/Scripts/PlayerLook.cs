using UnityEngine;
// נותן גישה ל-MonoBehaviour, ל-Transform, ל-Vector2, ל-Quaternion ול-Cursor של Unity.
using UnityEngine.InputSystem;
// נותן גישה ל-InputAction.CallbackContext של מערכת הקלט החדשה.

[DisallowMultipleComponent]
// מונע שני רכיבי מבט שיסובבו את המצלמה באותו זמן.
public class PlayerLook : MonoBehaviour
// מגדיר Component שאחראי לסיבוב גוף השחקן והמצלמה לפי תנועת העכבר.
{
    // פתיחת גוף המחלקה PlayerLook.
    [SerializeField] private Transform cameraTransform;
    // שומר חיבור ל-Transform של המצלמה שאותו נסובב למעלה ולמטה.
    [SerializeField] private float mouseSensitivity = 0.1f;
    // קובע כמה חזק תנועת העכבר משפיעה על סיבוב המבט.

    private Vector2 lookInput;
    // שומר את קלט המבט האחרון שהתקבל ממערכת הקלט.
    private float verticalRotation;
    // שומר את זווית המבט האנכית כדי להגביל אותה ולא להתהפך.
    private bool canLook = true;
    // שומר האם מותר כרגע להזיז את המצלמה; מתחיל ב-true כדי שהמבט יעבוד בתחילת המשחק.

    private void Start()
    // פועל בתחילת המשחק לאחר Awake של כל הרכיבים בסצנה.
    {
        // פתיחת המתודה Start.
        if (cameraTransform == null)
        // בודק אם המצלמה לא חוברה דרך ה-Inspector.
        {
            // פתיחת תנאי חיפוש המצלמה.
            Camera playerCamera = GetComponentInChildren<Camera>();
            // מחפש מצלמה באחד מילדי השחקן.
            cameraTransform = playerCamera != null ? playerCamera.transform : null;
            // שומר את ה-Transform אם נמצאה מצלמה.
        }
        // סיום תנאי חיפוש המצלמה.

        if (cameraTransform == null)
        // בודק אם גם החיפוש האוטומטי לא מצא מצלמה.
        {
            // פתיחת תנאי השגיאה.
            Debug.LogError("PlayerLook לא מצא מצלמה של השחקן", this);
            // מציג ב-Console הוראת חיבור ברורה.
            enabled = false;
            // מכבה את הרכיב כדי למנוע שגיאה בכל פריים.
            return;
            // עוצר את פעולת ההכנה.
        }
        // סיום תנאי השגיאה.

        Cursor.lockState = CursorLockMode.Locked;
        // נועל את הסמן למרכז חלון המשחק כדי שהעכבר ישלוט במצלמה.
        Cursor.visible = false;
        // מסתיר את הסמן בזמן המשחק.
    }
    // סיום המתודה Start.

    private void Update()
    // פועל בכל פריים ומחיל את קלט המבט על השחקן ועל המצלמה.
    {
        // פתיחת המתודה Update.
        if (!canLook)
        // בודק אם רכיב אחר, למשל מגירה או דלת, נעל את המבט.
        {
            // פתיחת תנאי נעילת המבט.
            return;
            // עוצר לפני כל סיבוב כדי שהעכבר יהיה פנוי לשליטה באובייקט המוחזק.
        }
        // סיום תנאי נעילת המבט.

        float mouseX = lookInput.x * mouseSensitivity;
        // ממיר את קלט X של העכבר לסיבוב אופקי.
        float mouseY = lookInput.y * mouseSensitivity;
        // ממיר את קלט Y של העכבר לסיבוב אנכי.
        transform.Rotate(Vector3.up * mouseX);
        // מסובב את גוף השחקן ימינה ושמאלה סביב ציר Y.
        verticalRotation -= mouseY;
        // הופך את כיוון Y ושומר את זווית המבט החדשה למעלה או למטה.
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);
        // מגביל את המבט כדי שהמצלמה לא תתהפך מעבר לראש או לרגליים.
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        // מחיל את הסיבוב האנכי על המצלמה ביחס לשחקן.
    }
    // סיום המתודה Update.

    public void OnLook(InputAction.CallbackContext context)
    // נקראת על ידי PlayerInput כאשר פעולת Look מקבלת ערך חדש.
    {
        // פתיחת המתודה OnLook.
        if (!canLook)
        // בודק אם המבט נעול בזמן שמגירה או דלת משתמשות בעכבר.
        {
            // פתיחת תנאי נעילת הקלט.
            lookInput = Vector2.zero;
            // מוחק קלט שעלול להצטבר בזמן שהמבט נעול.
            return;
            // עוצר כדי שלא תישמר קפיצת מצלמה לרגע שבו האחיזה תסתיים.
        }
        // סיום תנאי נעילת הקלט.

        lookInput = context.ReadValue<Vector2>();
        // קורא ושומר את תנועת העכבר כערך בעל ציר X וציר Y.
    }
    // סיום המתודה OnLook.

    public void SetLookEnabled(bool enabled)
    // מאפשר למגירה או לדלת להפעיל או לכבות את השליטה במבט.
    {
        // פתיחת המתודה SetLookEnabled.
        canLook = enabled;
        // שומר את מצב ההרשאה החדש של המבט.
        lookInput = Vector2.zero;
        // מוחק קלט ישן כדי למנוע קפיצה כאשר המבט ננעל או משתחרר.
    }
    // סיום המתודה SetLookEnabled.
}
// סיום המחלקה PlayerLook
