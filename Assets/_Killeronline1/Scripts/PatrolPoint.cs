
using UnityEngine;
// נותן גישה ל-MonoBehaviour ול-Transform של נקודת המסלול.

public sealed class PatrolPoint : MonoBehaviour
// מייצג נקודה אחת בלבד במסלול הסיור של Lauren.
{
    // פתיחת גוף המחלקה PatrolPoint.
    [SerializeField] private string pointName;
    // שומר שם ידידותי לנקודה לצורכי סדר ובדיקה.

    public string PointName => string.IsNullOrWhiteSpace(pointName) ? gameObject.name : pointName;
    // מחזיר את השם שהוגדר או את שם האובייקט אם השדה ריק.

    private void Reset()
    // פועל כאשר מוסיפים את הסקריפט או מאפסים אותו ב-Inspector.
    {
        // פתיחת המתודה Reset.
        pointName = gameObject.name;
        // נותן לנקודה שם התחלתי זהה לשם שלה בהיררכיה.
    }
    // סיום המתודה Reset.
}
// סיום גוף המחלקה PatrolPoint.
