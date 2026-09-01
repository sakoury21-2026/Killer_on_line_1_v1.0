using UnityEngine; // נותן גישה ל-MonoBehaviour, ל-Vector2, ל-Vector3, ל-Mathf ול-Time של Unity.
using UnityEngine.InputSystem; // נותן גישה לעכבר דרך מערכת הקלט החדשה של Unity.

public class DrawerInteractable : MonoBehaviour, IHoldInteractable // מגדיר מגירה שמתחילה לפעול בלחיצה על E ומפסיקה בשחרור E.
{ // פתיחת גוף המחלקה DrawerInteractable.
    [Header("Drawer Movement")] // יוצר כותרת ב-Inspector עבור ערכי התנועה של המגירה.
    [SerializeField] private float openDistance = 0.35f; // קובע כמה מטרים המגירה יכולה לנוע מהמיקום הסגור למיקום הפתוח.
    [SerializeField] private bool opensInPositiveZ = true; // קובע האם כיוון הפתיחה הוא Z חיובי או Z שלילי ביחס ל-Parent של המגירה.
    [SerializeField] private float dragSensitivity = 0.001f; // ממיר את תנועת העכבר למרחק תנועה של המגירה.

    [Header("Player References")] // יוצר כותרת ב-Inspector עבור החיבורים לרכיבי השחקן.
    [SerializeField] private PlayerLook playerLook; // שומר חיבור לרכיב שמסובב את המצלמה כדי שנוכל לנעול אותו בזמן האחיזה.
    [SerializeField] private PlayerMovement playerMovement; // שומר חיבור לרכיב שמזיז את השחקן כדי שנוכל לנעול אותו בזמן האחיזה.

    [Header("Noise")] // יוצר כותרת ב-Inspector עבור הגדרות הרעש של המגירה.
    [SerializeField] private NoiseSystem noiseSystem; // שומר חיבור למערכת שמפרסמת את אירועי הרעש.
    [SerializeField] private float drawerNoiseRadius = 5f; // קובע שהרעש של המגירה נשמע ברדיוס התחלתי של חמישה מטרים.
    [SerializeField] private float sidewaysNoiseThreshold = 4f; // קובע כמה פיקסלים הצידה בפריים נחשבים למשיכה עקומה.
    [SerializeField] private float fastPullThreshold = 900f; // קובע איזו מהירות אנכית בפיקסלים לשנייה נחשבת למשיכה מהירה.
    [SerializeField] private float noiseCooldown = 0.35f; // קובע כמה שניות חייבות לעבור לפני שמותר לדווח שוב על רעש מאותה מגירה.

    private float closedZ; // שומר את מיקום Z המקומי של המגירה כשהיא סגורה.
    private float openZ; // שומר את מיקום Z המקומי של המגירה כשהיא פתוחה עד הסוף.
    private float openingDirection; // שומר 1 לפתיחה ב-Z חיובי או מינוס 1 לפתיחה ב-Z שלילי.
    private float nextAllowedNoiseTime; // שומר את הזמן המוקדם הבא שבו מותר לפרסם אירוע רעש.
    private bool isHeld; // שומר האם השחקן מחזיק כרגע את המגירה באמצעות E.

    private void Awake() // פועל פעם אחת כאשר ה-GameObject נטען ומכין את כל הנתונים הדרושים למגירה.
    { // פתיחת המתודה Awake.
        if (playerLook == null) // בודק אם שכחנו לחבר את PlayerLook דרך ה-Inspector.
        { // פתיחת תנאי החיפוש של PlayerLook.
            playerLook = FindFirstObjectByType<PlayerLook>(); // מחפש בסצנה את רכיב המבט הראשון כגיבוי לחיבור הידני.
        } // סיום תנאי החיפוש של PlayerLook.

        if (playerLook == null) // בודק אם גם החיפוש האוטומטי לא מצא PlayerLook.
        { // פתיחת תנאי השגיאה של PlayerLook.
            Debug.LogError("לא נמצא PlayerLook בסצנה", this); // מציג שגיאה ומקשר אותה למגירה הבעייתית.
        } // סיום תנאי השגיאה של PlayerLook.

        if (playerMovement == null) // בודק אם שכחנו לחבר את PlayerMovement דרך ה-Inspector.
        { // פתיחת תנאי החיפוש של PlayerMovement.
            playerMovement = FindFirstObjectByType<PlayerMovement>(); // מחפש בסצנה את רכיב התנועה הראשון כגיבוי לחיבור הידני.
        } // סיום תנאי החיפוש של PlayerMovement.

        if (playerMovement == null) // בודק אם גם החיפוש האוטומטי לא מצא PlayerMovement.
        { // פתיחת תנאי השגיאה של PlayerMovement.
            Debug.LogError("לא נמצא PlayerMovement בסצנה", this); // מציג שגיאה ומקשר אותה למגירה הבעייתית.
        } // סיום תנאי השגיאה של PlayerMovement.

        if (noiseSystem == null) // בודק אם שכחנו לחבר את NoiseSystem דרך ה-Inspector.
        { // פתיחת תנאי החיפוש של NoiseSystem.
            noiseSystem = FindFirstObjectByType<NoiseSystem>(); // מחפש בסצנה את מערכת הרעש הראשונה כגיבוי לחיבור הידני.
        } // סיום תנאי החיפוש של NoiseSystem.

        if (noiseSystem == null) // בודק אם גם החיפוש האוטומטי לא מצא NoiseSystem.
        { // פתיחת תנאי השגיאה של NoiseSystem.
            Debug.LogError("לא נמצא NoiseSystem בסצנה", this); // מציג שגיאה כי המגירה לא תוכל לדווח רעש.
        } // סיום תנאי השגיאה של NoiseSystem.

        closedZ = transform.localPosition.z; // שומר את מיקום הסגירה מתוך המיקום שבו המגירה הונחה ב-Prefab או בסצנה.
        openingDirection = opensInPositiveZ ? 1f : -1f; // מתרגם את בחירת הכיוון ב-Inspector למספר שבו נוכל להשתמש בחישוב.
        openZ = closedZ + openDistance * openingDirection; // מחשב את גבול הפתיחה המרבי של המגירה.
    } // סיום המתודה Awake.

    private void Update() // פועל בכל פריים ומזיז את המגירה רק בזמן שהשחקן מחזיק אותה.
    { // פתיחת המתודה Update.
        if (!isHeld) // בודק אם אין כרגע אחיזה פעילה במגירה.
        { // פתיחת תנאי האחיזה.
            return; // עוצר את Update של המגירה כי אסור להזיז אותה ללא החזקת E.
        } // סיום תנאי האחיזה.

        if (Mouse.current == null) // בודק אם קיים עכבר פעיל במערכת הקלט.
        { // פתיחת תנאי בדיקת העכבר.
            return; // מונע NullReferenceException אם המשחק מופעל ללא עכבר.
        } // סיום תנאי בדיקת העכבר.

        Vector2 mouseDelta = Mouse.current.delta.ReadValue(); // קורא כמה העכבר זז מאז הפריים הקודם בציר X ובציר Y.
        TryReportRoughMovement(mouseDelta); // בודק אם המשיכה הייתה מהירה או עקומה ומדווח רעש במקרה הצורך.

        float drawerMovement = -mouseDelta.y * dragSensitivity * openingDirection; // הופך תנועת עכבר אנכית לשינוי במיקום המגירה בכיוון הפתיחה הנבחר.
        Vector3 drawerPosition = transform.localPosition; // יוצר עותק של המיקום המקומי הנוכחי כדי לשנות רק את ציר Z.
        float minimumZ = Mathf.Min(closedZ, openZ); // מוצא את גבול Z הנמוך בין המצב הסגור למצב הפתוח.
        float maximumZ = Mathf.Max(closedZ, openZ); // מוצא את גבול Z הגבוה בין המצב הסגור למצב הפתוח.
        drawerPosition.z = Mathf.Clamp(drawerPosition.z + drawerMovement, minimumZ, maximumZ); // מוסיף את תנועת העכבר ומונע מהמגירה לעבור את גבולותיה.
        transform.localPosition = drawerPosition; // מחיל בפועל את המיקום החדש על המגירה.
    } // סיום המתודה Update.

    private void TryReportRoughMovement(Vector2 mouseDelta) // מקבלת את תנועת העכבר ובודקת אם היא מהירה מדי או נוטה הצידה.
    { // פתיחת המתודה TryReportRoughMovement.
        float safeDeltaTime = Mathf.Max(Time.unscaledDeltaTime, 0.0001f); // מונע חלוקה באפס ומשתמש בזמן שאינו מושפע מהאטה או עצירה של המשחק.
        float pullSpeed = Mathf.Abs(mouseDelta.y) / safeDeltaTime; // ממיר את תנועת העכבר האנכית בפיקסלים למהירות חיובית בפיקסלים לשנייה.
        bool pulledTooFast = pullSpeed >= fastPullThreshold; // שומר true אם מהירות המשיכה עברה את הסף שקבענו.
        bool pulledSideways = Mathf.Abs(mouseDelta.x) >= sidewaysNoiseThreshold; // שומר true אם היד זזה הצידה יותר מהסף ולכן המשיכה נחשבת עקומה.

        if (!pulledTooFast && !pulledSideways) // בודק אם המשיכה הייתה גם איטית וגם ישרה.
        { // פתיחת תנאי המשיכה השקטה.
            return; // יוצא בלי רעש כי השחקן השתמש במגירה בעדינות.
        } // סיום תנאי המשיכה השקטה.

        if (Time.unscaledTime < nextAllowedNoiseTime) // בודק אם ה-Cooldown מהדיווח הקודם עדיין פעיל.
        { // פתיחת תנאי ה-Cooldown.
            return; // מונע יצירת Event בכל פריים בזמן משיכה רועשת.
        } // סיום תנאי ה-Cooldown.

        nextAllowedNoiseTime = Time.unscaledTime + noiseCooldown; // קובע מתי יהיה מותר לפרסם את אירוע הרעש הבא.
        noiseSystem?.ReportNoise(transform.position, drawerNoiseRadius); // מדווח את מיקום המגירה ורדיוס הרעש אם מערכת הרעש קיימת.
    } // סיום המתודה TryReportRoughMovement.

    public void BeginInteract() // נקראת על ידי PlayerInteraction כאשר פעולת Interact מגיעה לשלב performed.
    { // פתיחת המתודה BeginInteract.
        if (isHeld) // בודק אם המגירה כבר מוחזקת.
        { // פתיחת תנאי מניעת האחיזה הכפולה.
            return; // מונע התחלה כפולה ונעילה חוזרת של השליטה.
        } // סיום תנאי מניעת האחיזה הכפולה.

        if (playerLook == null || playerMovement == null) // בודק אם חסר אחד מרכיבי השליטה שחייבים להינעל.
        { // פתיחת תנאי החיבורים החסרים.
            return; // מונע התחלת אינטראקציה חלקית שעלולה להשאיר את השחקן במצב לא תקין.
        } // סיום תנאי החיבורים החסרים.

        isHeld = true; // מסמן שהמגירה מוחזקת ולכן Update רשאי לקרוא את תנועת העכבר.
        playerLook.SetLookEnabled(false); // נועל את סיבוב המצלמה כדי שהעכבר ישלוט במגירה.
        playerMovement.SetMovementControlsEnabled(false); // נועל הליכה, ריצה וכריעה בזמן השימוש במגירה.
    } // סיום המתודה BeginInteract.

    public void EndInteract() // נקראת על ידי PlayerInteraction כאשר פעולת Interact מגיעה לשלב canceled.
    { // פתיחת המתודה EndInteract.
        if (!isHeld) // בודק אם אין אחיזה פעילה שצריך לסיים.
        { // פתיחת תנאי האחיזה הלא פעילה.
            return; // יוצא בלי פעולה כדי לא לשנות שליטה שלא ננעלה על ידי המגירה הזאת.
        } // סיום תנאי האחיזה הלא פעילה.

        isHeld = false; // מסמן שהמגירה כבר אינה מוחזקת ולכן Update מפסיק להזיז אותה.

        if (playerLook != null) // בודק שרכיב המבט עדיין קיים.
        { // פתיחת תנאי החזרת המבט.
            playerLook.SetLookEnabled(true); // מחזיר לשחקן את השליטה במצלמה.
        } // סיום תנאי החזרת המבט.

        if (playerMovement != null) // בודק שרכיב התנועה עדיין קיים.
        { // פתיחת תנאי החזרת התנועה.
            playerMovement.SetMovementControlsEnabled(true); // מחזיר לשחקן את פקודות ההליכה, הריצה והכריעה.
        } // סיום תנאי החזרת התנועה.
    } // סיום המתודה EndInteract.

    private void OnDisable() // פועלת אם המגירה או הסקריפט נכבים בזמן שהשחקן עדיין מחזיק E.
    { // פתיחת המתודה OnDisable.
        EndInteract(); // מסיימת את האחיזה ומבטיחה שהשליטה בשחקן לא תישאר נעולה.
    } // סיום המתודה OnDisable.
} // סיום המחלקה DrawerInteractable.
