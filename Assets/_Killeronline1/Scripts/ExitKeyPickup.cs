using UnityEngine;
// מאפשר שימוש בכלים הבסיסיים של המנוע

[DisallowMultipleComponent]
// מונע הוספה כפולה של מערכת איסוף לאותו מפתח
public class ExitKeyPickup : MonoBehaviour, IInteractable
// מגדיר מפתח שאפשר לאסוף באמצעות אינטראקציה
{
    // תחילת המחלקה
    [SerializeField] private PlayerInventory playerInventory;
    // שומר הפניה למלאי של השחקן

    private void Awake()
    // פועלת פעם אחת כאשר המפתח נוצר
    {
        // תחילת פעולת ההכנה
        if (playerInventory == null)
        // בודק אם עדיין אין חיבור למלאי השחקן
        {
            // תחילת התנאי
            playerInventory = FindFirstObjectByType<PlayerInventory>();
            // מחפש בסצנה את מלאי השחקן ושומר אותו

            if (playerInventory == null)
            // בודק אם החיפוש לא מצא מלאי
            {
                // תחילת התנאי הפנימי
                Debug.LogError("לא נמצא מלאי שחקן בסצנה", this);
                // מציג שגיאה ברורה פעם אחת
            }
            // סוף התנאי הפנימי
        }
        // סוף התנאי
    }
    // סוף פעולת ההכנה

    public void Interact()
    // מופעלת כאשר השחקן מבצע אינטראקציה עם המפתח
    {
        // תחילת פעולת האיסוף
        if (playerInventory == null)
        // בודק אם אין מלאי שאפשר להכניס אליו את המפתח
        {
            // תחילת התנאי
            return;
            // עוצר את האיסוף ומונע שגיאה
        }
        // סוף התנאי

        playerInventory.CollectExitKey();
        // מוסיף את מפתח היציאה למלאי השחקן
        gameObject.SetActive(false);
        // מסתיר את המפתח לאחר האיסוף
    }
    // סוף פעולת האיסוף
}
// סוף המחלקה
