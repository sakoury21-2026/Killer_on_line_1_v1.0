using System.Collections;
using UnityEngine;
using UnityEngine.AI;
 [RequireComponent(typeof(NavMeshAgent))]
public class PatrolPoint : MonoBehaviour
{
    // פתיחת גוף המחלקה PatrolPoint.
    [SerializeField] private string pointName;
    // שומר שם ידידותי לנקודה לצורכי סדר ובדיקה.

    public string PointName => string.IsNullOrWhiteSpace(pointName) ? gameObject.name : pointName;
    // מחזיר את השם שהוגדר או את שם האובייקט אם השדה ריק.

    private void Reset()
    // פועל כאשר מוסיפים את הסקריפט או מאפסים אותו ב-Inspector.
    {
        // פתיחת המתודה Reset.
        pointName = gameObject.name;
        // נותן לנקודה שם התחלתי זהה לשם שלה בהיררכיה.
    }
    // סיום המתודה Reset.

}
