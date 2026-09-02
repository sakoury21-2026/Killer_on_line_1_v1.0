using UnityEngine;
// נותן גישה ל-MonoBehaviour, ל-BoxCollider ול-Collider של Unity.

[DisallowMultipleComponent]
// מונע שני אזורי סיום על אותו אובייקט.
[RequireComponent(typeof(BoxCollider))]
// מבטיח שקיים BoxCollider שיזהה את מעבר השחקן.
public sealed class ExitTrigger : MonoBehaviour
// מסיים את השלב רק כאשר השחקן עבר בדלת פתוחה עם המפתח.
{
    // פתיחת גוף המחלקה ExitTrigger.
    [SerializeField] private ExitDoorInteractable exitDoor;
    // שומר חיבור לדלת כדי לבדוק שהיא באמת פתוחה.
    [SerializeField] private GameFlow gameFlow;
    // שומר חיבור למנהל המשחק כדי להציג ניצחון.

    private void Reset()
    // פועל כאשר מוסיפים את הסקריפט או מאפסים אותו ב-Inspector.
    {
        // פתיחת המתודה Reset.
        GetComponent<BoxCollider>().isTrigger = true;
        // הופך את הקוליידר לאזור זיהוי שאינו חוסם את השחקן.
    }
    // סיום המתודה Reset.

    private void Awake()
    // פועל פעם אחת כאשר אזור היציאה נטען.
    {
        // פתיחת המתודה Awake.
        GetComponent<BoxCollider>().isTrigger = true;
        // מוודא שהקוליידר נשאר Trigger גם בזמן המשחק.

        if (gameFlow == null)
        // בודק אם GameFlow לא חובר דרך ה-Inspector.
        {
            // פתיחת תנאי השלמת מנהל המשחק.
            gameFlow = FindFirstObjectByType<GameFlow>();
            // מחפש את מנהל המשחק היחיד בסצנה.
        }
        // סיום תנאי השלמת מנהל המשחק.
    }
    // סיום המתודה Awake.

    private void OnTriggerEnter(Collider other)
    // פועל כאשר Collider כלשהו נכנס לאזור שמאחורי דלת היציאה.
    {
        // פתיחת המתודה OnTriggerEnter.
        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
        // מחפש מלאי על הגוף שנכנס או על אובייקט האב שלו.

        if (inventory == null)
        // בודק אם הגוף שנכנס אינו השחקן.
        {
            // פתיחת תנאי גוף שאינו שחקן.
            return;
            // מתעלם מ-Lauren ומחפצים אחרים.
        }
        // סיום תנאי גוף שאינו שחקן.

        if (!inventory.HasExitKey())
        // בודק אם לשחקן אין את מפתח היציאה.
        {
            // פתיחת תנאי המפתח החסר.
            return;
            // מונע ניצחון ללא איסוף המפתח.
        }
        // סיום תנאי המפתח החסר.

        if (exitDoor == null || !exitDoor.IsOpen)
        // בודק אם דלת היציאה חסרה או עדיין לא סיימה להיפתח.
        {
            // פתיחת תנאי הדלת הסגורה.
            return;
            // מונע ניצחון דרך Trigger בלי לפתוח את הדלת.
        }
        // סיום תנאי הדלת הסגורה.

        gameFlow?.ShowWin();
        // מציג מסך ניצחון אם GameFlow קיים ומבטיח שהסיום יופעל פעם אחת בלבד.
    }
    // סיום המתודה OnTriggerEnter.
}
// סיום גוף המחלקה ExitTrigger.
