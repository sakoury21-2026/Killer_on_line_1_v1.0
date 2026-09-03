using UnityEngine;
// נותן גישה ל-MonoBehaviour, ל-BoxCollider ולכלים הבסיסיים של Unity.

[DisallowMultipleComponent]
// מונע הוספה של יותר מרכיב RoomVolume אחד לאותו אובייקט.
[RequireComponent(typeof(BoxCollider))]
// מבטיח שלחדר יהיה BoxCollider שיכול לשמש כאזור זיהוי.
public sealed class RoomVolume : MonoBehaviour
// מייצג קופסה שקופה שמגדירה חדר אחד במשחק.
{
    // פתיחת גוף המחלקה RoomVolume.
    [SerializeField] private string roomName;
    // שומר שם קריא לחדר כדי לזהות אותו ב-Inspector וב-Console.

    public string RoomName => string.IsNullOrWhiteSpace(roomName) ? gameObject.name : roomName;
    // מחזיר את השם שהוגדר או את שם האובייקט אם השדה ריק.

    private void Reset()
    // פועל כשמוסיפים את הסקריפט לאובייקט או לוחצים Reset ב-Inspector.
    {
        // פתיחת המתודה Reset.
        BoxCollider roomCollider = GetComponent<BoxCollider>();
        // מוצא את ה-BoxCollider שנמצא על אותו אובייקט.
        roomCollider.isTrigger = true;
        // הופך את הקוליידר לחיישן שלא חוסם את השחקן או את Lauren.
        roomName = gameObject.name;
        // נותן לחדר שם התחלתי זהה לשם האובייקט בהיררכיה.
    }
    // סיום המתודה Reset.

    private void Awake()
    // פועל פעם אחת כאשר החדר נטען בסצנה.
    {
        // פתיחת המתודה Awake.
        GetComponent<BoxCollider>().isTrigger = true;
        // מוודא שגם בזמן משחק הקוליידר נשאר Trigger.
    }
    // סיום המתודה Awake.
}
// סיום גוף המחלקה RoomVolume.
