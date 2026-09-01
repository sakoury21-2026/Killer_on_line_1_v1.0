using UnityEngine;

public class LaurenVision : MonoBehaviour
{
    // המטרה של הראייה היא השחקן.
    [SerializeField] private Transform player;

    // מערכת ה-Stealth של השחקן.
    [SerializeField] private PlayerStealthState playerStealthState;

    // המידע של החדר שבו נמצאת Lauren.
    [SerializeField] private RoomTracker laurenRoomTracker;

    // המידע של החדר שבו נמצא Michael.
    [SerializeField] private RoomTracker playerRoomTracker;

    // המרחק המקסימלי שבו Lauren יכולה לראות.
    [SerializeField] private float visionRange = 12f;

    // השכבות שה-Ray לא יכול לעבור דרכן.
    [SerializeField] private LayerMask visionBlockers;

    // הפונקציה מחזירה true אם Lauren באמת רואה את Michael.
    public bool CanSeePlayer()
    {
        // אם אין לנו Player, אי אפשר לראות אותו.
        if (player == null)
        {
            return false;
        }

        // אם אין לנו PlayerStealthState, אנחנו לא יכולים
        // לבדוק האם Michael מתחבא.
        if (playerStealthState == null)
        {
            return false;
        }

        // אם Michael מוסתר, Lauren לא רואה אותו.
        if (playerStealthState.IsHidden)
        {
            return false;
        }

        // אם אין מידע על החדרים, לא ננסה לנחש.
        if (laurenRoomTracker == null || playerRoomTracker == null)
        {
            return false;
        }

        // אם Lauren ו-Michael נמצאים בחדרים שונים,
        // Lauren לא יכולה לראות אותו.
        if (laurenRoomTracker.CurrentRoom != playerRoomTracker.CurrentRoom)
        {
            return false;
        }

        // מחשבים את המרחק בין Lauren לבין Michael.
        float distance = Vector3.Distance(transform.position, player.position);

        // אם Michael רחוק מדי, Lauren לא רואה אותו.
        if (distance > visionRange)
        {
            return false;
        }

        // מחשבים את הכיוון מ-Lauren אל Michael.
        Vector3 direction = player.position - transform.position;

        // מחשבים כמה רחוק אנחנו צריכים לירות את הקרן.
        float rayDistance = direction.magnitude;

        // מנרמלים את הכיוון כדי לקבל וקטור באורך 1.
        direction.Normalize();

        // יורים Raycast מ-Lauren לכיוון Michael.
        if (Physics.Raycast(
            transform.position,
            direction,
            out RaycastHit hit,
            rayDistance,
            visionBlockers))
        {
            // אם הקרן פגעה במשהו שחוסם ראייה,
            // Lauren לא רואה את Michael.
            return false;
        }

        // הגענו לכאן רק אם כל הבדיקות עברו.
        return true;
    }
}
