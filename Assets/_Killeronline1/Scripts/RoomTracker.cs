using System.Collections.Generic;
// נותן גישה ל-List ששומר את החדרים החופפים כרגע.
using UnityEngine;
// נותן גישה ל-MonoBehaviour, ל-Collider ולכלים של Unity.

[DisallowMultipleComponent]
// מונע שני זיכרונות חדר שונים על אותו אובייקט.
public sealed class RoomTracker : MonoBehaviour
// זוכר באיזה חדר נמצא האובייקט שעליו הוא מותקן.
{
    // פתיחת גוף המחלקה RoomTracker.
    private readonly List<RoomVolume> roomsInside = new List<RoomVolume>();
    // שומר את כל החדרים שהאובייקט נמצא בתוכם כדי לטפל נכון בחפיפה ליד דלת.
    [SerializeField] private RoomVolume currentRoom;
    // מציג בזמן Play Mode את החדר הנוכחי כדי שהצוות יוכל לבדוק את המערכת ב-Inspector.

    public RoomVolume CurrentRoom => currentRoom;
    // מאפשר למערכות אחרות לקרוא את החדר הנוכחי אך לא לשנות אותו מבחוץ.

    private void OnTriggerEnter(Collider other)
    // פועל כאשר האובייקט נכנס ל-Trigger כלשהו.
    {
        // פתיחת המתודה OnTriggerEnter.
        RoomVolume room = other.GetComponentInParent<RoomVolume>();
        // מחפש RoomVolume על ה-Trigger או על אובייקט האב שלו.

        if (room == null)
        // בודק אם ה-Trigger שנכנסנו אליו בכלל מייצג חדר.
        {
            // פתיחת תנאי ה-Trigger שאינו חדר.
            return;
            // יוצא בלי לשנות את החדר כי זה יכול להיות Trigger של מחבוא או יציאה.
        }
        // סיום תנאי ה-Trigger שאינו חדר.

        if (!roomsInside.Contains(room))
        // בודק אם החדר עדיין לא נמצא ברשימת החפיפות.
        {
            // פתיחת תנאי הוספת החדר.
            roomsInside.Add(room);
            // מוסיף את החדר לרשימה פעם אחת בלבד.
        }
        // סיום תנאי הוספת החדר.

        currentRoom = room;
        // קובע שהחדר שאליו נכנסנו לאחרונה הוא החדר הנוכחי.
    }
    // סיום המתודה OnTriggerEnter.

    private void OnTriggerExit(Collider other)
    // פועל כאשר האובייקט יוצא מ-Trigger כלשהו.
    {
        // פתיחת המתודה OnTriggerExit.
        RoomVolume room = other.GetComponentInParent<RoomVolume>();
        // מחפש RoomVolume על ה-Trigger שממנו יצאנו או על אובייקט האב שלו.

        if (room == null)
        // בודק אם ה-Trigger שממנו יצאנו אינו חדר.
        {
            // פתיחת תנאי ה-Trigger שאינו חדר.
            return;
            // יוצא בלי לשנות דבר כי ה-Trigger אינו חלק ממערכת החדרים.
        }
        // סיום תנאי ה-Trigger שאינו חדר.

        roomsInside.Remove(room);
        // מסיר את החדר שממנו יצאנו מרשימת החפיפות.

        if (currentRoom == room)
        // בודק אם יצאנו מהחדר שנחשב כרגע לחדר הראשי שלנו.
        {
            // פתיחת תנאי החלפת החדר.
            currentRoom = roomsInside.Count > 0 ? roomsInside[roomsInside.Count - 1] : null;
            // חוזר לחדר החופף האחרון או ל-null אם איננו בתוך שום חדר.
        }
        // סיום תנאי החלפת החדר.
    }
    // סיום המתודה OnTriggerExit.

    private void OnDisable()
    // פועל כאשר האובייקט או הסקריפט נכבים.
    {
        // פתיחת המתודה OnDisable.
        roomsInside.Clear();
        // מוחק מידע ישן כדי שלא יישאר חדר מזויף לאחר הפעלה מחדש.
        currentRoom = null;
        // מסמן שאין חדר נוכחי בזמן שהרכיב כבוי.
    }
    // סיום המתודה OnDisable.
}
// סיום גוף המחלקה RoomTracker.
