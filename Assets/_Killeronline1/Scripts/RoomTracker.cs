using UnityEngine;

public class RoomTracker : MonoBehaviour
{
    // החדר הנוכחי של האובייקט.
    public RoomVolume CurrentRoom { get; private set; }

    // הפונקציה הזאת מופעלת כאשר האובייקט נכנס ל-Trigger.
    private void OnTriggerEnter(Collider other)
    {
        // מנסה למצוא RoomVolume על ה-Trigger שאליו נכנסנו.
        RoomVolume room = other.GetComponent<RoomVolume>();

        // אם לא מצאנו חדר, אין לנו מה לעשות.
        if (room == null)
        {
            return;
        }

        // שומרים שהחדר הנוכחי הוא החדר שאליו נכנסנו.
        CurrentRoom = room;
    }

    // הפונקציה הזאת מופעלת כאשר האובייקט יוצא מה-Trigger.
    private void OnTriggerExit(Collider other)
    {
        // מנסה למצוא את החדר שממנו יצאנו.
        RoomVolume room = other.GetComponent<RoomVolume>();

        // אם זה לא RoomVolume, אין לנו מה לעשות.
        if (room == null)
        {
            return;
        }

        // אם החדר שממנו יצאנו הוא באמת החדר הנוכחי,
        // אנחנו כבר לא נמצאים בו.
        if (CurrentRoom == room)
        {
            CurrentRoom = null;
        }
    }
}
