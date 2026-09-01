using UnityEngine; // נותן גישה ל-MonoBehaviour, ל-Vector3, ל-Debug ולחיפוש רכיבים של Unity.

public class NoiseListenerTest : MonoBehaviour // מגדיר מאזין ניסויי שמוכיח שאירועי NoiseSystem מגיעים למנויים.
{ // פתיחת גוף המחלקה NoiseListenerTest.
    [SerializeField] private NoiseSystem noiseSystem; // שומר חיבור למערכת הרעש שאליה המאזין יירשם.

    private void Awake() // פועל פעם אחת כאשר ה-GameObject נטען ומנסה להשלים חיבור חסר.
    { // פתיחת המתודה Awake.
        if (noiseSystem == null) // בודק אם מערכת הרעש לא חוברה דרך ה-Inspector.
        { // פתיחת תנאי החיפוש של NoiseSystem.
            noiseSystem = FindFirstObjectByType<NoiseSystem>(); // מחפש בסצנה את מערכת הרעש הראשונה כגיבוי לחיבור הידני.
        } // סיום תנאי החיפוש של NoiseSystem.

        if (noiseSystem == null) // בודק אם גם החיפוש האוטומטי לא מצא מערכת רעש.
        { // פתיחת תנאי השגיאה.
            Debug.LogError("לא נמצאה מערכת רעש עבור המאזין הניסויי", this); // מציג שגיאה ומקשר אותה למאזין שלא חובר.
        } // סיום תנאי השגיאה.
    } // סיום המתודה Awake.

    private void OnEnable() // פועל בכל פעם שהרכיב נהיה פעיל ומתאים להתחברות לאירועים.
    { // פתיחת המתודה OnEnable.
        if (noiseSystem == null) // בודק אם אין Publisher שאליו אפשר להירשם.
        { // פתיחת תנאי החיבור החסר.
            return; // יוצא כדי למנוע ניסיון הרשמה דרך ערך null.
        } // סיום תנאי החיבור החסר.

        noiseSystem.NoiseReported += HandleNoiseReported; // רושם את HandleNoiseReported כמאזין לאירוע הרעש.
    } // סיום המתודה OnEnable.

    private void OnDisable() // פועל בכל פעם שהרכיב מפסיק להיות פעיל ומתאים לניתוק מאירועים.
    { // פתיחת המתודה OnDisable.
        if (noiseSystem == null) // בודק אם אין Publisher שממנו צריך להתנתק.
        { // פתיחת תנאי החיבור החסר.
            return; // יוצא כי אין אירוע שממנו אפשר להסיר את המאזין.
        } // סיום תנאי החיבור החסר.

        noiseSystem.NoiseReported -= HandleNoiseReported; // מסיר את המאזין כדי למנוע תגובות כפולות לאחר כיבוי והפעלה.
    } // סיום המתודה OnDisable.

    private void HandleNoiseReported(Vector3 noisePosition, float noiseRadius) // מקבלת מהאירוע את מיקום הרעש ואת רדיוס השמיעה.
    { // פתיחת המתודה HandleNoiseReported.
        float distanceToNoise = Vector3.Distance(transform.position, noisePosition); // מחשב את המרחק בין המאזין הניסויי לבין מקור הרעש.

        if (distanceToNoise <= noiseRadius) // בודק אם המאזין נמצא בתוך רדיוס השמיעה.
        { // פתיחת התנאי שבו הרעש נשמע.
            Debug.Log("Noise heard | Distance: " + distanceToNoise + " | Radius: " + noiseRadius, this); // מדפיס שהרעש נשמע ואת נתוני הבדיקה.
        } // סיום התנאי שבו הרעש נשמע.
        else // מופעל כאשר המרחק גדול מרדיוס השמיעה.
        { // פתיחת התנאי שבו הרעש רחוק מדי.
            Debug.Log("Noise ignored | Distance: " + distanceToNoise + " | Radius: " + noiseRadius, this); // מדפיס שהאירוע הגיע אך המאזין החליט שהרעש רחוק מדי.
        } // סיום התנאי שבו הרעש רחוק מדי.
    } // סיום המתודה HandleNoiseReported.
} // סיום המחלקה NoiseListenerTest.
