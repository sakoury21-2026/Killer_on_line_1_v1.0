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
        if (playerCamera == null)
        // בודק אם המצלמה לא חוברה דרך ה-Inspector
        {
            playerCamera = GetComponentInChildren<Camera>();
            // מחפש מצלמה בתוך ילדי השחקן
        }

        if (playerCamera == null)
        // בודק אם גם החיפוש האוטומטי נכשל
        {
            Debug.LogError("PlayerInteraction לא מצא מצלמה של השחקן", this);
            // מציג הוראת חיבור ברורה ב-Console
        }
    }

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
            return;
            // מונע התחלה של שתי אחיזות באותה לחיצה
        }

        if (playerCamera == null)
        // בודק אם אין מצלמה שממנה אפשר לשלוח קרן
        {
            return;
            // עוצר ומונע שגיאת NullReference
        }

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
            IInteractable directInteractable =
                hit.collider.GetComponent<IInteractable>();
            // מחפש אינטראקציה קצרה על האובייקט המדויק שבו הקרן פגעה

            if (directInteractable != null)
            // בודק אם נפגענו ישירות בחפץ כמו המפתח
            {
                if (directInteractable is Behaviour directBehaviour &&
                    !directBehaviour.isActiveAndEnabled)
                // בודק אם רכיב האינטראקציה קיים אבל כבוי
                {
                    return;
                    // עוצר כדי שלא יהיה אפשר להשתמש ברכיב כבוי
                }

                directInteractable.Interact();
                // מפעיל את המפתח או את החפץ המדויק שבו הקרן פגעה

                return;
                // מונע מהקוד להפעיל גם את המגירה שמעל המפתח בהיררכיה
            }

            IHoldInteractable holdInteractable =
                hit.collider.GetComponentInParent<IHoldInteractable>();
            // מחפש מגירה או דלת על האובייקט או על אחד ההורים שלו

            if (holdInteractable != null)
            // בודק אם נמצאה אינטראקציה שדורשת החזקת E
            {
                if (holdInteractable is Behaviour holdBehaviour &&
                    !holdBehaviour.isActiveAndEnabled)
                // בודק אם רכיב האחיזה קיים אבל כבוי
                {
                    return;
                    // עוצר כדי שלא יהיה אפשר להשתמש ברכיב כבוי
                }

                currentHoldInteractable = holdInteractable;
                // שומר את המגירה או הדלת שהשחקן התחיל להחזיק

                currentHoldInteractable.BeginInteract();
                // מתחיל את פעולת האחיזה

                return;
                // עוצר כדי שלא תופעל גם אינטראקציה אחרת
            }

            IInteractable parentInteractable =
                hit.collider.GetComponentInParent<IInteractable>();
            // מחפש אינטראקציה קצרה על אובייקט הורה

            if (parentInteractable != null)
            // בודק אם נמצאה אינטראקציה קצרה אצל אחד ההורים
            {
                if (parentInteractable is Behaviour parentBehaviour &&
                    !parentBehaviour.isActiveAndEnabled)
                // בודק אם רכיב האינטראקציה של ההורה כבוי
                {
                    return;
                    // עוצר כדי שלא יהיה אפשר להשתמש ברכיב כבוי
                }

                parentInteractable.Interact();
                // מפעיל את האינטראקציה שנמצאה אצל ההורה
            }
            else
            // מופעל אם הקרן פגעה בחפץ שאין עליו מערכת אינטראקציה
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
    // מופעל אוטומטית כאשר רכיב האינטראקציה של השחקן נכבה
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