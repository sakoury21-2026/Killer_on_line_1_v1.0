using UnityEngine;
// נותן גישה ל-MonoBehaviour, ל-Transform, ל-Quaternion, ל-Mathf ול-Time של Unity.
using UnityEngine.InputSystem;
// נותן גישה לעכבר דרך מערכת הקלט החדשה של Unity.

[DisallowMultipleComponent]
// מונע שני רכיבי דלת שמתחרים על אותו ציר ומתקן את סכנת הכפילות שנמצאה ב-NewsRoom.
public class RoomDoorInteractable : MonoBehaviour, IHoldInteractable
// מגדיר דלת חדר רגילה שנפתחת באמצעות החזקת E וגרירת עכבר.
{
    // פתיחת גוף המחלקה RoomDoorInteractable.
    [Header("Door Movement")]
    // יוצר כותרת ב-Inspector עבור הגדרות תנועת הדלת.
    [SerializeField] private Transform doorPivot;
    // קובע איזה Transform מסתובב סביב ציר הדלת.
    [SerializeField] private Transform doorSideReference;
    // קובע כיוון קבוע של הפתח; החץ הכחול שלו צריך להצביע לכיוון הלובי.
    [SerializeField] private float openAngle = 90f;
    // קובע את זווית הפתיחה; ערך שלילי פותח את הדלת לצד ההפוך.
    [SerializeField] private float dragSensitivity = 0.0025f;
    // ממיר את תנועת העכבר לכמות פתיחה בין אפס לאחד.

    [Header("AI Opening")]
    // יוצר כותרת ב-Inspector עבור פתיחה אוטומטית של הדלת על ידי Lauren.
    [SerializeField, Min(0.1f)] private float aiOpeningSpeed = 1.5f;
    // קובע כמה מהר Lauren פותחת את הדלת; ערך 1.5 פותח אותה בערך בשני שלישי שנייה.

    [Header("Player References")]
    // יוצר כותרת ב-Inspector עבור החיבורים לרכיבי השחקן.
    [SerializeField] private PlayerLook playerLook;
    // שומר חיבור לרכיב המבט כדי לנעול את המצלמה בזמן האחיזה.
    [SerializeField] private PlayerMovement playerMovement;
    // שומר חיבור לרכיב התנועה כדי לנעול הליכה, ריצה וכריעה בזמן האחיזה.

    [Header("Noise")]
    // יוצר כותרת ב-Inspector עבור הגדרות הרעש של הדלת.
    [SerializeField] private NoiseSystem noiseSystem;
    // שומר חיבור למערכת שמפרסמת אירועי רעש.
    [SerializeField] private float doorNoiseRadius = 5f;
    // קובע שרעש הדלת מדווח ברדיוס של חמישה מטרים.
    [SerializeField] private float sidewaysNoiseThreshold = 4f;
    // קובע כמה פיקסלים הצידה בפריים נחשבים למשיכה עקומה.
    [SerializeField] private float fastPullThreshold = 900f;
    // קובע איזו מהירות אנכית בפיקסלים לשנייה נחשבת למשיכה מהירה.
    [SerializeField] private float noiseCooldown = 0.35f;
    // קובע כמה שניות חייבות לעבור בין שני דיווחי רעש של אותה דלת.

    private Quaternion closedRotation;
    // שומר את הסיבוב המקומי של הדלת במצב הסגור.
    private Quaternion openRotation;
    // שומר את הסיבוב המקומי של הדלת במצב הפתוח.
    private Vector3 doorSideForward;
    // שומר בתחילת המשחק כיוון עולמי קבוע שמצביע אל צד הלובי ואינו מסתובב עם הדלת.
    private float openAmount;
    // שומר מספר בין אפס לאחד שמתאר כמה הדלת פתוחה כרגע.
    private float mouseOpeningDirection = -1f;
    // שומר 1 כאשר עכבר למעלה פותח או מינוס 1 כאשר עכבר למטה פותח.
    private float nextAllowedNoiseTime;
    // שומר את הזמן המוקדם הבא שבו מותר לדלת לדווח רעש.
    private bool isHeld;
    // שומר האם השחקן מחזיק כרגע את הדלת באמצעות E.
    private bool isOpeningForAI;
    // שומר האם Lauren ביקשה מהדלת להיפתח באופן אוטומטי.

    public Vector3 DoorPosition => doorPivot != null ? doorPivot.position : transform.position;
    // מאפשר ל-Lauren למדוד מרחק אל ציר הדלת בלי לקבל גישה לשינוי ה-Transform.

    private void Awake()
    // פועל פעם אחת כאשר ה-GameObject נטען ומכין את חיבורי הדלת ואת זוויותיה.
    {
        // פתיחת המתודה Awake.
        if (doorPivot == null)
        // בודק אם שכחנו לחבר Transform בשדה Door Pivot.
        {
            // פתיחת תנאי ברירת המחדל של הציר.
            doorPivot = transform;
            // משתמש ב-Transform שעליו נמצא הסקריפט כציר הדלת.
        }
        // סיום תנאי ברירת המחדל של הציר.

        if (doorSideReference == null)
        // בודק אם לא חיברנו Transform קבוע שמגדיר את צד הלובי.
        {
            // פתיחת תנאי ברירת המחדל של כיוון הצד.
            doorSideReference = doorPivot;
            // משתמש בכיוון הסגור של ציר הדלת כגיבוי אם אין Reference נפרד.
        }
        // סיום תנאי ברירת המחדל של כיוון הצד.

        if (playerLook == null)
        // בודק אם שכחנו לחבר את PlayerLook דרך ה-Inspector.
        {
            // פתיחת תנאי החיפוש של PlayerLook.
            playerLook = FindFirstObjectByType<PlayerLook>();
            // מחפש בסצנה את רכיב המבט הראשון כגיבוי לחיבור הידני.
        }
        // סיום תנאי החיפוש של PlayerLook.

        if (playerLook == null)
        // בודק אם גם החיפוש האוטומטי לא מצא PlayerLook.
        {
            // פתיחת תנאי השגיאה של PlayerLook.
            Debug.LogError("לא נמצא PlayerLook עבור דלת החדר", this);
            // מציג שגיאה ומקשר אותה לדלת הבעייתית.
        }
        // סיום תנאי השגיאה של PlayerLook.

        if (playerMovement == null)
        // בודק אם שכחנו לחבר את PlayerMovement דרך ה-Inspector.
        {
            // פתיחת תנאי החיפוש של PlayerMovement.
            playerMovement = FindFirstObjectByType<PlayerMovement>();
            // מחפש בסצנה את רכיב התנועה הראשון כגיבוי לחיבור הידני.
        }
        // סיום תנאי החיפוש של PlayerMovement.

        if (playerMovement == null)
        // בודק אם גם החיפוש האוטומטי לא מצא PlayerMovement.
        {
            // פתיחת תנאי השגיאה של PlayerMovement.
            Debug.LogError("לא נמצא PlayerMovement עבור דלת החדר", this);
            // מציג שגיאה ומקשר אותה לדלת הבעייתית.
        }
        // סיום תנאי השגיאה של PlayerMovement.

        if (noiseSystem == null)
        // בודק אם שכחנו לחבר את NoiseSystem דרך ה-Inspector.
        {
            // פתיחת תנאי החיפוש של NoiseSystem.
            noiseSystem = FindFirstObjectByType<NoiseSystem>();
            // מחפש בסצנה את מערכת הרעש הראשונה כגיבוי לחיבור הידני.
        }
        // סיום תנאי החיפוש של NoiseSystem.

        if (noiseSystem == null)
        // בודק אם גם החיפוש האוטומטי לא מצא NoiseSystem.
        {
            // פתיחת תנאי השגיאה של NoiseSystem.
            Debug.LogError("לא נמצא NoiseSystem עבור דלת החדר", this);
            // מציג שגיאה כי הדלת לא תוכל לדווח רעש.
        }
        // סיום תנאי השגיאה של NoiseSystem.

        closedRotation = doorPivot.localRotation;
        // שומר את הסיבוב שבו הדלת הונחה בסצנה בתור מצב סגור.
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
        // מחשב את מצב הפתיחה באמצעות סיבוב סביב ציר וואי המקומי.
        doorSideForward = Vector3.ProjectOnPlane(doorSideReference.forward, Vector3.up).normalized;
        // שומר כיוון אופקי קבוע של החץ הכחול כדי לזהות מאיזה צד השחקן ניגש.

        if (doorSideForward.sqrMagnitude < 0.0001f)
        // בודק אם החץ הכחול של ה-Reference מכוון בטעות כמעט ישר למעלה או למטה.
        {
            // פתיחת תנאי הכיוון הלא תקין.
            Debug.LogError("Door Side Reference חייב להצביע אופקית לכיוון הלובי", this);
            // מסביר ב-Console כיצד לתקן את כיוון ה-Reference.
            doorSideForward = Vector3.forward;
            // מציב כיוון בטוח זמני כדי שהחישוב לא יעבוד עם וקטור אפס.
        }
        // סיום תנאי הכיוון הלא תקין.

        openAmount = 0f;
        // מתחיל במצב סגור שתואם ל-closedRotation.
    }
    // סיום המתודה Awake.

    private void Update()
    // פועל בכל פריים ומסובב את הדלת בזמן אחיזת השחקן או בזמן בקשת פתיחה של Lauren.
    {
        // פתיחת המתודה Update.
        if (isHeld)
        // בודק אם השחקן מחזיק כרגע את הדלת וצריך לקבל עדיפות על ה-AI.
        {
            // פתיחת תנאי שליטת השחקן.
            UpdatePlayerOpening();
            // קורא את העכבר ומעדכן את הפתיחה הידנית בדיוק כמו לפני הוספת ה-AI.
            return;
            // מונע מהפתיחה האוטומטית להתחרות בשחקן באותו פריים.
        }
        // סיום תנאי שליטת השחקן.

        if (!isOpeningForAI)
        // בודק אם Lauren לא ביקשה לפתוח את הדלת.
        {
            // פתיחת תנאי אין בקשת AI.
            return;
            // עוצר כי אין גורם שמנסה להזיז את הדלת.
        }
        // סיום תנאי אין בקשת AI.

        openAmount = Mathf.MoveTowards(openAmount, 1f, aiOpeningSpeed * Time.deltaTime);
        // מתקדם בצורה חלקה מהמצב הנוכחי אל פתיחה מלאה בקצב שאינו תלוי במספר הפריימים.
        ApplyDoorRotation();
        // מחיל על ציר הדלת את הסיבוב שמתאים לכמות הפתיחה החדשה.

        if (openAmount >= 1f)
        // בודק אם הדלת כבר פתוחה לחלוטין.
        {
            // פתיחת תנאי סיום פתיחת ה-AI.
            isOpeningForAI = false;
            // מפסיק לעדכן את הדלת עד שתתקבל בקשה חדשה.
        }
        // סיום תנאי סיום פתיחת ה-AI.
    }
    // סיום המתודה Update.

    private void UpdatePlayerOpening()
    // מטפלת רק בפתיחה ובסגירה הידנית באמצעות גרירת העכבר.
    {
        // פתיחת המתודה UpdatePlayerOpening.
        if (Mouse.current == null)
        // בודק אם קיים עכבר פעיל במערכת הקלט.
        {
            // פתיחת תנאי בדיקת העכבר.
            return;
            // מונע NullReferenceException אם המשחק מופעל ללא עכבר.
        }
        // סיום תנאי בדיקת העכבר.

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        // קורא את תנועת העכבר בציר X ובציר Y מאז הפריים הקודם.
        TryReportRoughMovement(mouseDelta);
        // בודק אם המשיכה הייתה מהירה או עקומה ומדווח רעש במקרה הצורך.

        float openAmountChange = mouseDelta.y * dragSensitivity * mouseOpeningDirection;
        // פותח למעלה מצד הלובי ולמטה מצד החדר לפי הכיוון שנשמר בתחילת האחיזה.
        openAmount = Mathf.Clamp01(openAmount + openAmountChange);
        // מוסיף את השינוי ומגביל את מצב הדלת לטווח שבין סגור לפתוח.
        ApplyDoorRotation();
        // מחיל על ציר הדלת את הסיבוב שמתאים לכמות הפתיחה החדשה.
    }
    // סיום המתודה UpdatePlayerOpening.

    private void ApplyDoorRotation()
    // מרכזת את החלת הסיבוב כדי שהשחקן ו-Lauren ישתמשו באותו חישוב בדיוק.
    {
        // פתיחת המתודה ApplyDoorRotation.
        doorPivot.localRotation = Quaternion.Slerp(closedRotation, openRotation, openAmount);
        // מחשב ומחיל את הסיבוב שבין מצב סגור למצב פתוח.
    }
    // סיום המתודה ApplyDoorRotation.

    public void OpenForAI()
    // נקראת על ידי LaurenAI כאשר Lauren מתקרבת לדלת בזמן תנועה במסלול.
    {
        // פתיחת המתודה OpenForAI.
        if (isHeld || openAmount >= 1f)
        // בודק אם השחקן שולט בדלת כרגע או שהדלת כבר פתוחה לחלוטין.
        {
            // פתיחת תנאי מניעת בקשה מיותרת.
            return;
            // משאיר את השליטה לשחקן או נמנע מעדכון נוסף של דלת פתוחה.
        }
        // סיום תנאי מניעת בקשה מיותרת.

        isOpeningForAI = true;
        // מסמן ל-Update להתחיל לפתוח את הדלת בצורה חלקה.
    }
    // סיום המתודה OpenForAI.

    private void TryReportRoughMovement(Vector2 mouseDelta)
    // מקבלת את תנועת העכבר ובודקת אם היא מהירה מדי או נוטה הצידה.
    {
        // פתיחת המתודה TryReportRoughMovement.
        float safeDeltaTime = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
        // מונע חלוקה באפס ומשתמש בזמן שאינו מושפע מהאטה או עצירה.
        float pullSpeed = Mathf.Abs(mouseDelta.y) / safeDeltaTime;
        // ממיר את תנועת העכבר האנכית למהירות חיובית בפיקסלים לשנייה.
        bool pulledTooFast = pullSpeed >= fastPullThreshold;
        // שומר true אם מהירות המשיכה עברה את הסף המותר.
        bool pulledSideways = Mathf.Abs(mouseDelta.x) >= sidewaysNoiseThreshold;
        // שומר true אם הייתה סטייה גדולה הצידה ולכן המשיכה עקומה.

        if (!pulledTooFast && !pulledSideways)
        // בודק אם המשיכה הייתה גם איטית וגם ישרה.
        {
            // פתיחת תנאי התנועה השקטה.
            return;
            // יוצא בלי רעש כי השחקן הזיז את הדלת בעדינות.
        }
        // סיום תנאי התנועה השקטה.

        if (Time.unscaledTime < nextAllowedNoiseTime)
        // בודק אם ה-Cooldown מהדיווח הקודם עדיין פעיל.
        {
            // פתיחת תנאי ה-Cooldown.
            return;
            // מונע יצירת Event בכל פריים בזמן משיכה רועשת.
        }
        // סיום תנאי ה-Cooldown.

        nextAllowedNoiseTime = Time.unscaledTime + noiseCooldown;
        // קובע מתי יהיה מותר לדווח שוב על רעש.
        noiseSystem?.ReportNoise(doorPivot.position, doorNoiseRadius);
        // מדווח את המיקום העולמי של ציר הדלת ורדיוס של חמישה מטרים אם המערכת קיימת.
    }
    // סיום המתודה TryReportRoughMovement.

    public void BeginInteract()
    // נקראת על ידי PlayerInteraction כאשר פעולת Interact מגיעה לשלב performed.
    {
        // פתיחת המתודה BeginInteract.
        if (isHeld)
        // בודק אם הדלת כבר מוחזקת.
        {
            // פתיחת תנאי מניעת האחיזה הכפולה.
            return;
            // מונע התחלה כפולה ונעילה חוזרת של השליטה.
        }
        // סיום תנאי מניעת האחיזה הכפולה.

        if (playerLook == null || playerMovement == null)
        // בודק אם חסר אחד מרכיבי השליטה שחייבים להינעל.
        {
            // פתיחת תנאי החיבורים החסרים.
            return;
            // מונע התחלת אינטראקציה חלקית שעלולה להשאיר את השחקן במצב לא תקין.
        }
        // סיום תנאי החיבורים החסרים.

        DetermineMouseOpeningDirection();
        // בודק פעם אחת באיזה צד השחקן עומד וקובע איזו תנועת עכבר תפתח את הדלת.
        isOpeningForAI = false;
        // עוצר בקשת פתיחה אוטומטית קיימת כדי לתת לשחקן שליטה מלאה בדלת.
        isHeld = true;
        // מסמן שהדלת מוחזקת ולכן Update רשאי לקרוא את תנועת העכבר.
        playerLook.SetLookEnabled(false);
        // נועל את סיבוב המצלמה כדי שהעכבר ישלוט בדלת.
        playerMovement.SetMovementControlsEnabled(false);
        // נועל את תנועת השחקן בזמן השימוש בדלת.
    }
    // סיום המתודה BeginInteract.

    private void DetermineMouseOpeningDirection()
    // מחשבת אם השחקן נמצא בצד הלובי או בצד החדר בתחילת האחיזה.
    {
        // פתיחת המתודה DetermineMouseOpeningDirection.
        Vector3 playerOffset = playerMovement.transform.position - doorPivot.position;
        // יוצר וקטור מהדלת אל מיקום השחקן.
        Vector3 horizontalPlayerOffset = Vector3.ProjectOnPlane(playerOffset, Vector3.up);
        // מסיר את הפרש הגובה כדי שהבדיקה תתחשב רק בצד האופקי של הדלת.
        float playerSide = Vector3.Dot(doorSideForward, horizontalPlayerOffset);
        // מחזיר ערך חיובי בצד שאליו מצביע החץ הכחול ושלילי בצד הנגדי.
        mouseOpeningDirection = playerSide >= 0f ? 1f : -1f;
        // קובע עכבר למעלה בצד הלובי ועכבר למטה בצד החדר.
    }
    // סיום המתודה DetermineMouseOpeningDirection.

    public void EndInteract()
    // נקראת על ידי PlayerInteraction כאשר פעולת Interact מגיעה לשלב canceled.
    {
        // פתיחת המתודה EndInteract.
        if (!isHeld)
        // בודק אם אין אחיזה פעילה שצריך לסיים.
        {
            // פתיחת תנאי האחיזה הלא פעילה.
            return;
            // יוצא בלי פעולה כדי לא לשנות שליטה שלא ננעלה על ידי הדלת הזאת.
        }
        // סיום תנאי האחיזה הלא פעילה.

        isHeld = false;
        // מסמן שהדלת כבר אינה מוחזקת ולכן Update מפסיק לסובב אותה.

        if (playerLook != null)
        // בודק שרכיב המבט עדיין קיים.
        {
            // פתיחת תנאי החזרת המבט.
            playerLook.SetLookEnabled(true);
            // מחזיר לשחקן את השליטה במצלמה.
        }
        // סיום תנאי החזרת המבט.

        if (playerMovement != null)
        // בודק שרכיב התנועה עדיין קיים.
        {
            // פתיחת תנאי החזרת התנועה.
            playerMovement.SetMovementControlsEnabled(true);
            // מחזיר לשחקן את פקודות התנועה.
        }
        // סיום תנאי החזרת התנועה.
    }
    // סיום המתודה EndInteract.

    private void OnDisable()
    // פועלת אם הדלת או הסקריפט נכבים בזמן שהשחקן עדיין מחזיק E.
    {
        // פתיחת המתודה OnDisable.
        EndInteract();
        // מסיימת את האחיזה ומבטיחה שהשליטה בשחקן לא תישאר נעולה.
        isOpeningForAI = false;
        // מנקה בקשת AI ישנה כדי שהדלת לא תמשיך להיפתח לאחר הפעלה מחדש.
    }
    // סיום המתודה OnDisable.
}
// סיום המחלקה RoomDoorInteractable
