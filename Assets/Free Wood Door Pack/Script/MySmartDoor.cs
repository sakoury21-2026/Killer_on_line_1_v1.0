using UnityEngine;
using System.Collections;

public class MySmartDoor : MonoBehaviour
{
    [Header("Door Movement")]
    [SerializeField] private float openAngle = -90f; // זווית הפתיחה (אם נפתח הפוך, שנה ל-90)
    [SerializeField] private float openSpeed = 5f;    // מהירות פתיחת הדלת

    [Header("Target Setup")]
    [Tooltip("גרור לכאן את אובייקט השחקן שלך מההיררכיה!")]
    [SerializeField] private Transform playerTransform; // השחקן שלך

    [Header("Distance Settings")]
    [SerializeField] private float maxDistance = 2.5f; // המרחק המקסימלי במטרים לפתיחה

    private bool isOpen = false;
    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _currentCoroutine;

    void Start()
    {
        _closedRotation = transform.localRotation;
        _openRotation = transform.localRotation * Quaternion.Euler(0, openAngle, 0);

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    void Update()
    {
        // בדיקת מקלדת בסיסית: מדפיס לוג בכל פעם שאתה לוחץ על מקש E, בלי קשר למרחק
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("--- זוהתה לחיצה על מקש E במקלדת! ---");

            // אם השחקן לא מחובר, נציג אזהרה
            if (playerTransform == null)
            {
                Debug.LogError("שגיאה: לא גררת את אובייקט השחקן לתוך שדה ה-Player Transform באינספקטור של הדלת!");
                return;
            }

            // חישוב המרחק הנוכחי
            float currentDistance = Vector3.Distance(transform.position, playerTransform.position);
            Debug.Log("המרחק הנוכחי מהדלת הוא: " + currentDistance + " מטרים. המרחק המותר הוא: " + maxDistance);

            // בדיקה האם השחקן מספיק קרוב
            if (currentDistance <= maxDistance)
            {
                Debug.Log("השחקן קרוב מספיק! הדלת מתחילה להסתובב.");
                isOpen = !isOpen; // הפיכת המצב (פתח/סגור)

                if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
                _currentCoroutine = StartCoroutine(RotateDoor());
            }
            else
            {
                Debug.LogWarning("לחצת על E אבל אתה רחוק מדי מהדלת! תתקרב עוד קצת.");
            }
        }
    }

    private IEnumerator RotateDoor()
    {
        Quaternion targetRotation = isOpen ? _openRotation : _closedRotation;

        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.01f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }

        transform.localRotation = targetRotation;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}
