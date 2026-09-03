using UnityEngine;
// מאפשר להשתמש בכלים ובמחלקות של Unity
using UnityEngine.InputSystem;
// מאפשר להשתמש בפעולות ממערכת הקלט החדשה

[DisallowMultipleComponent]
// מונע שתי מערכות אינטראקציה ששולחות שתי קרניים מאותו שחקן
public class PlayerInteraction : MonoBehaviour
// מנהל את האינטראקציות שהשחקן מבצע
{
    [SerializeField] private Camera playerCamera;
    // המצלמה שממנה נשלחת הקרן לבדיקת חפצים
    [SerializeField] private float interactionDistance = 2f;
    // המרחק המרבי שבו השחקן יכול לבצע אינטראקציה

    private IHoldInteractable currentHoldInteractable;
    // שומר את החפץ שהשחקן מחזיק כרגע

    private void Awake()
    // מופעלת פעם אחת ומוודאת שקיימת מצלמה לשליחת הקרן
    {
        // פתיחת פעולת ההכנה
        if (playerCamera == null)
        // בודק אם המצלמה לא חוברה דרך ה-Inspector
        {
            // פתיחת תנאי חיפוש המצלמה
            playerCamera = GetComponentInChildren<Camera>();
            // מחפש מצלמה בתוך ילדי השחקן
        }
        // סיום תנאי חיפוש המצלמה

        if (playerCamera == null)
        // בודק אם גם החיפוש האוטומטי נכשל
        {
            // פתיחת תנאי השגיאה
            Debug.LogError("PlayerInteraction לא מצא מצלמה של השחקן", this);
            // מציג הוראת חיבור ברורה ב-Console
        }
        // סיום תנאי השגיאה
    }
    // סיום פעולת ההכנה

    public void OnInteract(InputAction.CallbackContext context)
    // מופעלת מאירוע האינטראקציה של הכפתור E
    {
        if (context.canceled)
        // בודק אם השחקן שחרר את E
        {
            ReleaseCurrentHold();
            // משחרר את החפץ שהשחקן מחזיק
            return;
            // מסיים את הטיפול באירוע השחרור
        }

        if (!context.performed)
        // בודק אם הלחיצה עדיין לא הגיעה לשלב הביצוע
        {
            return;
            // עוצר כי זו אינה הלחיצה הרצויה
        }

        if (currentHoldInteractable != null)
        // בודק אם השחקן כבר מחזיק חפץ אחר
        {
            // פתיחת תנאי האחיזה הקיימת
            return;
            // מונע התחלה של שתי אחיזות באותה לחיצה
        }
        // סיום תנאי האחיזה הקיימת

        if (playerCamera == null)
        // בודק אם אין מצלמה שממנה אפשר לשלוח קרן
        {
            // פתיחת תנאי המצלמה החסרה
            return;
            // עוצר ומונע שגיאת NullReference
        }
        // סיום תנאי המצלמה החסרה

        if (Physics.Raycast(
            playerCamera.transform.position,
            // מגדיר שמיקום המצלמה הוא נקודת היציאה של הקרן
            playerCamera.transform.forward,
            // שולח את הקרן קדימה לפי כיוון המצלמה
            out RaycastHit hit,
            // שומר מידע על האובייקט שבו הקרן פגעה
            interactionDistance))
        // מגביל את הקרן למרחק האינטראקציה
        {
            IHoldInteractable holdInteractable = hit.collider.GetComponentInParent<IHoldInteractable>();
            // מחפש אינטראקציית אחיזה גם על הקוליידר וגם על אובייקט אב שלו

            if (holdInteractable != null)
            // בודק אם נמצאה אינטראקציה באמצעות אחיזה
            {
                if (holdInteractable is Behaviour holdBehaviour && !holdBehaviour.isActiveAndEnabled)
                // בודק אם רכיב האחיזה קיים אבל כבוי
                {
                    return;
                    // עוצר כדי שלא יהיה אפשר להשתמש ברכיב כבוי
                }

                currentHoldInteractable = holdInteractable;
                // שומר את החפץ שהשחקן מתחיל להחזיק
                currentHoldInteractable.BeginInteract();
                // מתחיל את האחיזה בחפץ
                return;
                // עוצר כדי שלא להפעיל גם אינטראקציה קצרה
            }

            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            // מחפש אינטראקציה קצרה גם על הקוליידר וגם על אובייקט אב שלו

            if (interactable != null)
            // בודק אם נמצאה אינטראקציה קצרה
            {
                if (interactable is Behaviour interactableBehaviour && !interactableBehaviour.isActiveAndEnabled)
                // בודק אם רכיב האינטראקציה קיים אבל כבוי
                {
                    return;
                    // עוצר כדי שלא יהיה אפשר להשתמש ברכיב כבוי
                }

                interactable.Interact();
                // מפעיל את האינטראקציה כאשר הרכיב קיים ופעיל
            }
            else
            // מופעל אם הקרן פגעה בחפץ שאין עליו אינטראקציה
            {
                Debug.Log("Hit object is not interactable");
                // מציג שהחפץ שנפגע אינו אינטראקטיבי
            }
        }
        else
        // מופעל אם הקרן לא פגעה בשום חפץ בטווח
        {
            Debug.Log("Raycast hit nothing");
            // מציג שהקרן לא פגעה בחפץ
        }
    }

    private void OnDisable()
    // מופעלת אוטומטית כאשר רכיב האינטראקציה של השחקן נכבה
    {
        ReleaseCurrentHold();
        // משחרר חפץ מוחזק כדי שהשחקן לא יישאר נעול
    }

    private void ReleaseCurrentHold()
    // מסיימת את האחיזה בחפץ שהשחקן מחזיק כרגע
    {
        if (currentHoldInteractable == null)
        // בודק אם אין כרגע חפץ מוחזק
        {
            return;
            // עוצר כי אין חפץ שצריך לשחרר
        }

        currentHoldInteractable.EndInteract();
        // מסיים את האחיזה ומחזיר לשחקן את השליטה
        currentHoldInteractable = null;
        // מוחק את החפץ מזיכרון האחיזה
    }
}
