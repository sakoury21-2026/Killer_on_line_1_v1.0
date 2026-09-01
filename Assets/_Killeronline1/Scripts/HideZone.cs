using UnityEngine; // מאפשר גישה לרכיבי המנוע הנדרשים בסקריפט.

[DisallowMultipleComponent] // מונע הוספה כפולה של רכיב אזור המסתור לאותו אובייקט.
[RequireComponent(typeof(BoxCollider))] // מבטיח שלאובייקט תהיה תיבת התנגשות שתזהה כניסה ויציאה.
public sealed class HideZone : MonoBehaviour // מגדיר אזור שמסתיר את השחקן בזמן שהוא נמצא בתוכו.
{ // פתיחת גוף המחלקה.
    private void Reset() // פועל כאשר מוסיפים את הסקריפט לאובייקט או מאפסים אותו בחלון המאפיינים.
    { // פתיחת פעולת האיפוס.
        BoxCollider hideZoneCollider = GetComponent<BoxCollider>(); // מוצא את תיבת ההתנגשות שנמצאת על אותו אובייקט.
        hideZoneCollider.isTrigger = true; // מסמן מיד את תיבת ההתנגשות כאזור זיהוי שאינו חוסם את השחקן.
    } // סיום פעולת האיפוס.

    private void Awake() // פועל פעם אחת כאשר אזור המסתור נטען בסצנה.
    { // פתיחת פעולת ההכנה.
        BoxCollider hideZoneCollider = GetComponent<BoxCollider>(); // מוצא את תיבת ההתנגשות שנמצאת על אותו אובייקט.
        hideZoneCollider.isTrigger = true; // הופך את תיבת ההתנגשות לאזור זיהוי שלא חוסם את תנועת השחקן.
    } // סיום פעולת ההכנה.

    private void OnTriggerEnter(Collider other) // פועל כאשר גוף אחר נכנס לתוך אזור המסתור.
    { // פתיחת פעולת הכניסה.
        PlayerStealthState stealth = other.GetComponentInParent<PlayerStealthState>(); // מחפש את מצב ההתגנבות על הגוף שנכנס או על אחד מהאובייקטים שמעליו.

        if (stealth != null) // בודק שהגוף שנכנס באמת שייך לשחקן שמחזיק מצב התגנבות.
        { // פתיחת תנאי מציאת השחקן.
            stealth.SetHidden(true); // מסמן את השחקן כמוסתר כל עוד הוא נמצא בתוך האזור.
        } // סיום תנאי מציאת השחקן.
    } // סיום פעולת הכניסה.

    private void OnTriggerExit(Collider other) // פועל כאשר גוף אחר יוצא מתוך אזור המסתור.
    { // פתיחת פעולת היציאה.
        PlayerStealthState stealth = other.GetComponentInParent<PlayerStealthState>(); // מחפש את מצב ההתגנבות על הגוף שיצא או על אחד מהאובייקטים שמעליו.

        if (stealth != null) // בודק שהגוף שיצא באמת שייך לשחקן שמחזיק מצב התגנבות.
        { // פתיחת תנאי מציאת השחקן.
            stealth.SetHidden(false); // מסמן את השחקן כגלוי לאחר שהוא יצא מאזור המסתור.
        } // סיום תנאי מציאת השחקן.
    } // סיום פעולת היציאה.
} // סיום גוף המחלקה.
