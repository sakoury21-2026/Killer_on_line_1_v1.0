using UnityEngine;

public class NoiseListenerTest : MonoBehaviour
{
    [SerializeField] private NoiseSystem noiseSystem; // מערכת הרעש שאליה המאזין יתחבר
    private void Awake() // פועל פעם אחת כאשר המאזין נוצר
    {
        if (noiseSystem == null) // בודק אם עדיין אין חיבור למערכת הרעש
        {
            noiseSystem = FindFirstObjectByType<NoiseSystem>(); // מחפש את מערכת הרעש בסצנה

            if (noiseSystem == null) // בודק אם החיפוש נכשל
            {
                Debug.LogError("לא נמצאה מערכת רעש עבור המאזין", this); // מציג שגיאה ברורה פעם אחת
            }
        }
    }
    private void OnEnable() // מופעל כשהקומפוננטה נהיית פעילה
    {
        if (noiseSystem == null) // אם מערכת הרעש עדיין לא חוברה
        {
            return; // לא מנסים להתחבר לערך חסר
        }

        noiseSystem.NoiseReported += HandleNoiseReported; // מתחיל להאזין לאירוע הרעש
    }
    private void OnDisable() // מופעל כשהקומפוננטה מפסיקה להיות פעילה
    {
        if (noiseSystem == null) // אם אין מערכת רעש מחוברת
        {
            return; // אין ממה להתנתק
        }

        noiseSystem.NoiseReported -= HandleNoiseReported; // מפסיק להאזין לאירוע
    }
    private void HandleNoiseReported(Vector3 noisePosition, float noiseRadius) // מקבל את פרטי הרעש
    {
        float distanceToNoise = Vector3.Distance(transform.position, noisePosition); // מחשב את המרחק בין המאזין לרעש
        if (distanceToNoise <= noiseRadius) // אם המאזין נמצא בתוך טווח הרעש
        {
            Debug.Log("Noise heard | Distance: " + distanceToNoise + " | Radius: " + noiseRadius); // הרעש נשמע
        }
        else // אם המרחק גדול מרדיוס הרעש
        {
            Debug.Log("Noise ignored | Distance: " + distanceToNoise + " | Radius: " + noiseRadius); // הרעש רחוק מדי
        }
    }
}
