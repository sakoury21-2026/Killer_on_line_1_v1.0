
using UnityEngine;
using System.Collections;

public class door1 : MonoBehaviour
{
    [Header("Door Movement")]
    [SerializeField] private float openAngle = -90f; // זווית הפתיחה (אם נפתח הפוך, שנה ל-90)
    [SerializeField] private float openSpeed = 5f;    // מהירות פתיחת הדלת

    [Header("Distance Settings")]
    [SerializeField] private float maxDistance = 2.5f; // המרחק המקסימלי במטרים שבו מותר לפתוח את הדלת

    private bool isOpen = false;
    private Transform _playerTransform;
    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _currentCoroutine;

    void Start()
    {
        // שמירת הזוויות ההתחלתיות של הדלת ביחס למיקום הנוכחי שלה
        _closedRotation = transform.localRotation;
        _openRotation = transform.localRotation * Quaternion.Euler(0, openAngle, 0);

        // הקוד מוצא אוטומטית את השחקן בסצנה לפי ה-Tag שלו
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("[door1] שגיאה: לא נמצא שחקן עם ה-Tag בשם 'Player'! אנא הגדר את ה-Tag של השחקן ב-Inspector.");
        }
    }

    void Update()
    {
        // הגנה: אם השחקן לא נמצא, הקוד לא ימשיך
        if (_playerTransform == null) return;

        // חישוב מתמטי נקי של המרחק בין הדלת לשחקן
        float currentDistance = Vector3.Distance(transform.position, _playerTransform.position);

        // אם השחקן בתוך הטווח המותר ולחץ על E
        if (currentDistance <= maxDistance && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen; // הפיכת המצב (פתח/סגור)

            if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(RotateDoor());
        }
    }

    private IEnumerator RotateDoor()
    {
        Quaternion targetRotation = isOpen ? _openRotation : _closedRotation;

        // סיבוב חלק ומיוצב שלא תלוי בקצב הפריימים של המשחק
        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.01f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }

        transform.localRotation = targetRotation;
    }

    // ציור עיגול צהוב בחלון ה-Scene כדי שתראה בעיניים את טווח הפתיחה המדויק
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}

