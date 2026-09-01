using UnityEngine; // נותן גישה ל-MonoBehaviour, ל-Transform ול-Debug של Unity.

public class TestInteractable : MonoBehaviour, IInteractable // מגדיר אובייקט בדיקה שמקיים את חוזה האינטראקציה הקצרה.
{ // פתיחת המחלקה TestInteractable.
    public void Interact() // נקראת על ידי PlayerInteraction כאשר השחקן לוחץ על E מול האובייקט.
    { // פתיחת Interact.
        transform.Rotate(0f, 45f, 0f); // מסובב את ה-GameObject בארבעים וחמש מעלות סביב ציר Y.
        Debug.Log("Interacted with: " + gameObject.name, this); // מדפיס איזה אובייקט קיבל את האינטראקציה ומקשר את ההודעה אליו.
    } // סיום Interact.
} // סיום המחלקה TestInteractable.
