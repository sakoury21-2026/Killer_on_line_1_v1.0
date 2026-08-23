using System; // נותן גישה לאקשן ולאירועים של סי שארפ
using UnityEngine; // נותן גישה לכלים של יוניטי

public class NoiseSystem : MonoBehaviour // קומפוננטה שמנהלת דיווחי רעש
{
    public event Action<Vector3, float> NoiseReported; // אירוע שמעביר מיקום ורדיוס של רעש
    public void ReportNoise(Vector3 noisePosition, float noiseRadius) // מקבלת את מיקום הרעש וטווח השמיעה
    {
        Debug.Log("Noise reported | Position: " + noisePosition + " | Radius: " + noiseRadius); // בדיקת דיווח זמנית
        NoiseReported?.Invoke(noisePosition, noiseRadius); // שולח את פרטי הרעש לכל המאזינים
    }
}
