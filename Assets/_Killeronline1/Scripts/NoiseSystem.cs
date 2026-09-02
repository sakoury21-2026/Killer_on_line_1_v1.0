using System;
// נותן גישה ל-Action, הטיפוס שבו נשתמש כדי להגדיר אירוע של C#.
using UnityEngine;
// נותן גישה ל-MonoBehaviour, ל-Vector3 ול-Debug של Unity.

[DisallowMultipleComponent]
// מונע שתי מערכות רעש שונות על אותו GameObject.
public class NoiseSystem : MonoBehaviour
// מגדיר Component מרכזי שמקבל דיווחי רעש ומפיץ אותם למאזינים.
{
    // פתיחת גוף המחלקה NoiseSystem.
    public event Action<Vector3, float> NoiseReported;
    // מגדיר אירוע שמעביר לכל מאזין את מיקום הרעש ואת רדיוס השמיעה שלו.

    [SerializeField] private bool logNoiseToConsole = true;
    // מציג מתג ב-Inspector שמכבה רק הודעות בדיקה ולא את אירועי המשחק.

    public void ReportNoise(Vector3 noisePosition, float noiseRadius)
    // מספק לכל מקור רעש מתודה אחת שבאמצעותה הוא מדווח מיקום ורדיוס.
    {
        // פתיחת המתודה ReportNoise.
        if (logNoiseToConsole)
        // בודק האם ביקשנו לראות הודעות רעש בקונסול.
        {
            // פתיחת התנאי שמטפל בהודעות הבדיקה.
            Debug.Log("Noise reported | Position: " + noisePosition + " | Radius: " + noiseRadius, this);
            // מדפיס מידע שימושי ומקשר את ההודעה לנוייז סיסטם שדיווח.
        }
        // סיום התנאי שמטפל בהודעות הבדיקה.

        NoiseReported?.Invoke(noisePosition, noiseRadius);
        // מפעיל את האירוע גם כשהלוג כבוי; סימן השאלה מונע שגיאה אם עדיין אין מאזינים.
    }
    // סיום המתודה ReportNoise.
}
// סיום המחלקה NoiseSystem.
