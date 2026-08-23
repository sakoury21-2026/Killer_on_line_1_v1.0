using UnityEngine; // נותן גישה למחלקות ולכלים של Unity
using UnityEngine.InputSystem; // נותן גישה לעכבר דרך מערכת הקלט החדשה

public class DrawerInteractable : MonoBehaviour, IHoldInteractable
{
    [SerializeField] private float openDistance = 0.35f; // המרחק המרבי שהמגירה תיפתח
    [SerializeField] private bool opensInPositiveZ = true; // האם המגירה נפתחת לכיוון החיובי של ציר Z
    [SerializeField] private PlayerLook playerLook; //  חיבור לסקריפט ששולט במבט של השחקן והוא מסוג פלייר לוק כדי שנוכל להכניס אליו רק רכיב שמכיל את הסקריפט פלייר לוק
    [SerializeField] private PlayerMovement playerMovement; // חיבור לתנועת השחקן
    [SerializeField] private float dragSensitivity = 0.001f; // ממיר את תנועת העכבר לתנועת מגירה
    [SerializeField] private float sidewaysNoiseThreshold = 4f; // כמות הסטייה בפתיחת המגירה שמפעילה רעש
    [SerializeField] private NoiseSystem noiseSystem; // חיבור למערכת שמדווחת על רעשים
    [SerializeField] private float drawerNoiseRadius = 5f; // המרחק שממנו ניתן לשמוע את רעש המגירה

    private float closedZ; // מיקום המגירה כשהיא סגורה
    private float openZ; // מיקום המגירה כשהיא פתוחה לגמרי
    private float openingDirection; // שומר 1 לפתיחה בזי חיובי או מינוס 1 לפתיחה בזי שלילי
    private bool isHeld; // האם השחקן אוחז כרגע במגירה
    private bool madeNoiseThisHold; // האם המגירה כבר הרעישה באחיזה הנוכחית
    private void Update() // בודקת את תנועת העכבר בכל פריים
    {
        if (!isHeld) // אם השחקן אינו אוחז במגירה
        {
            return; // עוצר את תזוזת המגירה
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue(); // קורא כמה העכבר זז בפריים הנוכחי
        if (!madeNoiseThisHold && Mathf.Abs(mouseDelta.x) > sidewaysNoiseThreshold) // אם הייתה סטייה חזקה הצידה
        {
            madeNoiseThisHold = true; // זוכר שכבר נוצר ניסיון לדווח על רעש

            if (noiseSystem != null) // בודק שקיימת מערכת רעש
            {
                noiseSystem.ReportNoise(transform.position, drawerNoiseRadius); // מדווח על רעש המגירה
            }
        }
        float drawerMovement = -mouseDelta.y * dragSensitivity * openingDirection; // מזיז את המגירה לפי תנועת העכבר וכיוון הפתיחה
        Vector3 drawerPosition = transform.localPosition; // שומר את מיקום המגירה ביחס לשולחן
        float minimumZ = Mathf.Min(closedZ, openZ); // מוצא את ערך Z הנמוך יותר
        float maximumZ = Mathf.Max(closedZ, openZ); // מוצא את ערך Z הגבוה יותר
        drawerPosition.z = Mathf.Clamp(drawerPosition.z + drawerMovement, minimumZ, maximumZ); // מגביל את המגירה בין סגור לפתוח
        transform.localPosition = drawerPosition; // מחיל את המיקום החדש על המגירה
    }
    public void BeginInteract() // מתחיל אחיזה כשלוחצים על E
    {
        if (isHeld) // אם המגירה כבר מוחזקת
        {
            return; // לא מתחיל אחיזה נוספת
        }
        if (playerLook == null || playerMovement == null) // בודק אם אחד מרכיבי השחקן חסר
        {
            return; // מונע אחיזה שלא ניתן לנעול בה את השחקן
        }

        isHeld = true; // מסמן שהמגירה מוחזקת
        madeNoiseThisHold = false; // מאפס את זיהוי הרעש לאחיזה החדשה
        playerLook.SetLookEnabled(false); // נועל את תנועת המצלמה
        playerMovement.SetMovementControlsEnabled(false); // נועל תנועה, ריצה וכפיפה
        Debug.Log("Drawer held: True"); // הודעת בדיקה בקונסול
    }
    public void EndInteract() // מסיים אחיזה כשמשחררים את E
    {
        if (!isHeld) // בודק אם המגירה כבר משוחררת
        {
            return; // אין אחיזה שצריך לסיים
        }

        isHeld = false; // מסמן שהמגירה כבר לא מוחזקת

        if (playerLook != null) // בודק שרכיב המבט עדיין קיים
        {
            playerLook.SetLookEnabled(true); // מחזיר את תנועת המצלמה
        }

        if (playerMovement != null) // בודק שרכיב התנועה עדיין קיים
        {
            playerMovement.SetMovementControlsEnabled(true); // מחזיר את שליטת השחקן
        }

        Debug.Log("Drawer held: False"); // מציג שהאחיזה הסתיימה
    }

    private void OnDisable() // מופעלת כשהרכיב או האובייקט נכבים
    {
        EndInteract(); // מסיימת את האחיזה במגירה ומחזירה לשחקן את השליטה
    }
    private void Awake() // פועלת פעם אחת כשהאובייקט מאותחל
    { // תחילת פעולת ההכנה
        if (playerLook == null) // בודק אם עדיין אין חיבור לרכיב המבט
        {
            playerLook = FindFirstObjectByType<PlayerLook>(); // מחפש את רכיב המבט

            if (playerLook == null) // בודק אם החיפוש נכשל
            {
                Debug.LogError("לא נמצא רכיב מבט של השחקן בסצנה", this); // מציג שגיאה פעם אחת
            }
        }

        if (playerMovement == null) // בודק אם עדיין אין חיבור לרכיב התנועה
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>(); // מחפש את רכיב התנועה

            if (playerMovement == null) // בודק אם החיפוש נכשל
            {
                Debug.LogError("לא נמצא רכיב תנועה של השחקן בסצנה", this); // מציג שגיאה פעם אחת
            }
        }

        if (noiseSystem == null) // בודק אם עדיין אין חיבור למערכת הרעש
        {
            noiseSystem = FindFirstObjectByType<NoiseSystem>(); // מחפש את מערכת הרעש

            if (noiseSystem == null) // בודק אם החיפוש נכשל
            {
                Debug.LogError("לא נמצאה מערכת רעש בסצנה", this); // מציג שגיאה פעם אחת
            }
        }

        closedZ = transform.localPosition.z; // שומר את מיקום הסגירה הנוכחי
        openingDirection = opensInPositiveZ ? 1f : -1f; // קובע את כיוון פתיחת המגירה
        openZ = closedZ + openDistance * openingDirection; // מחשב את מיקום הפתיחה המרבי
    } // סוף פעולת ההכנה
}