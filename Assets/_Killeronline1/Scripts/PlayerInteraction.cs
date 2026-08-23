using UnityEngine; // מאפשר להשתמש בכלים ובמחלקות של Unity
using UnityEngine.InputSystem; // מאפשר להשתמש בפעולות ממערכת הקלט החדשה

public class PlayerInteraction : MonoBehaviour // מנהל את האינטראקציות שהשחקן מבצע
{
    [SerializeField] private Camera playerCamera; // המצלמה שממנה נשלחת הקרן לבדיקת חפצים
    [SerializeField] private float interactionDistance = 2f; // המרחק המרבי שבו השחקן יכול לבצע אינטראקציה

    private IHoldInteractable currentHoldInteractable; // שומר את החפץ שהשחקן מחזיק כרגע

    public void OnInteract(InputAction.CallbackContext context) // מופעלת מאירוע האינטראקציה של הכפתור E
    {
        if (context.canceled) // בודק אם השחקן שחרר את E
        {
            ReleaseCurrentHold(); // משחרר את החפץ שהשחקן מחזיק
            return; // מסיים את הטיפול באירוע השחרור
        }

        if (!context.performed) // בודק אם הלחיצה עדיין לא הגיעה לשלב הביצוע
        {
            return; // עוצר כי זו אינה הלחיצה הרצויה
        }

        if (Physics.Raycast(
            playerCamera.transform.position, // מגדיר שמיקום המצלמה הוא נקודת היציאה של הקרן
            playerCamera.transform.forward, // שולח את הקרן קדימה לפי כיוון המצלמה
            out RaycastHit hit, // שומר מידע על האובייקט שבו הקרן פגעה
            interactionDistance)) // מגביל את הקרן למרחק האינטראקציה
        {
            if (hit.collider.TryGetComponent<IHoldInteractable>(out IHoldInteractable holdInteractable)) // מחפש רכיב של אינטראקציה באמצעות אחיזה
            {
                if (holdInteractable is Behaviour holdBehaviour && !holdBehaviour.isActiveAndEnabled) // בודק אם רכיב האחיזה קיים אבל כבוי
                {
                    return; // עוצר כדי שלא יהיה אפשר להשתמש ברכיב כבוי
                }

                currentHoldInteractable = holdInteractable; // שומר את החפץ שהשחקן מתחיל להחזיק
                currentHoldInteractable.BeginInteract(); // מתחיל את האחיזה בחפץ
                return; // עוצר כדי שלא להפעיל גם אינטראקציה קצרה
            }

            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable)) // מחפש רכיב של אינטראקציה קצרה
            {
                if (interactable is Behaviour interactableBehaviour && !interactableBehaviour.isActiveAndEnabled) // בודק אם רכיב האינטראקציה קיים אבל כבוי
                {
                    return; // עוצר כדי שלא יהיה אפשר להשתמש ברכיב כבוי
                }

                interactable.Interact(); // מפעיל את האינטראקציה כאשר הרכיב קיים ופעיל
            }
            else // מופעל אם הקרן פגעה בחפץ שאין עליו אינטראקציה
            {
                Debug.Log("Hit object is not interactable"); // מציג שהחפץ שנפגע אינו אינטראקטיבי
            }
        }
        else // מופעל אם הקרן לא פגעה בשום חפץ בטווח
        {
            Debug.Log("Raycast hit nothing"); // מציג שהקרן לא פגעה בחפץ
        }
    }

    private void OnDisable() // מופעלת אוטומטית כאשר רכיב האינטראקציה של השחקן נכבה
    {
        ReleaseCurrentHold(); // משחרר חפץ מוחזק כדי שהשחקן לא יישאר נעול
    }

    private void ReleaseCurrentHold() // מסיימת את האחיזה בחפץ שהשחקן מחזיק כרגע
    {
        if (currentHoldInteractable == null) // בודק אם אין כרגע חפץ מוחזק
        {
            return; // עוצר כי אין חפץ שצריך לשחרר
        }

        currentHoldInteractable.EndInteract(); // מסיים את האחיזה ומחזיר לשחקן את השליטה
        currentHoldInteractable = null; // מוחק את החפץ מזיכרון האחיזה
    }
}