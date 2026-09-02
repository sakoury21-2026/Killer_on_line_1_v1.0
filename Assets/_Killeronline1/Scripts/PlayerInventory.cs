using System;
// נותן גישה ל-Action שמאפשר ל-UI להאזין לשינוי במפתח.
using UnityEngine;
// נותן גישה ל-MonoBehaviour ול-Debug של Unity.

[DisallowMultipleComponent]
// מונע שני מלאים שונים על אותו שחקן.
public sealed class PlayerInventory : MonoBehaviour
// שומר את החפצים החשובים שהשחקן אסף במהלך השלב.
{
    // פתיחת גוף המחלקה PlayerInventory.
    private bool hasExitKey;
    // שומר אם מפתח היציאה כבר נאסף.

    public event Action<bool> ExitKeyChanged;
    // מודיע ל-UI שהמצב השתנה ושולח true או false במקום לבדוק בכל פריים.

    public void CollectExitKey()
    // מופעלת כאשר השחקן מבצע אינטראקציה עם מפתח היציאה.
    {
        // פתיחת המתודה CollectExitKey.
        if (hasExitKey)
        // בודק אם המפתח כבר נמצא במלאי.
        {
            // פתיחת תנאי מניעת איסוף כפול.
            return;
            // מונע אירוע והודעת בדיקה כפולים.
        }
        // סיום תנאי מניעת איסוף כפול.

        hasExitKey = true;
        // שומר שמפתח היציאה נמצא כעת אצל השחקן.
        Debug.Log("Exit key collected", this);
        // מציג הודעת בדיקה ומקשר אותה לשחקן.
        ExitKeyChanged?.Invoke(hasExitKey);
        // שולח לכל המאזינים את מצב המפתח החדש אם קיימים מאזינים.
    }
    // סיום המתודה CollectExitKey.

    public bool HasExitKey()
    // מאפשר לדלת היציאה ול-UI לשאול אם המפתח קיים.
    {
        // פתיחת המתודה HasExitKey.
        return hasExitKey;
        // מחזיר true אם המפתח נאסף ו-false אם עדיין לא.
    }
    // סיום המתודה HasExitKey.
}
// סיום גוף המחלקה PlayerInventory.
