using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable // קומפוננטה שמקיימת את חוזה האינטרקטאבל
{
    public void Interact() // הפעולה שתתבצע כשמפעילים את החפץ
    {
        transform.Rotate(0f, 45f, 0f); // מסובב את הקובייה ב־45 מעלות סביב ציר וואי
        Debug.Log("Interacted with: " + gameObject.name); // מדפיס את שם החפץ שהופעל
    }
}