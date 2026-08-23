using UnityEngine; // שימוש במערכות הבסיסיות של Unity
using UnityEngine.InputSystem; // שימוש במערכת הקלט החדשה של Unity

public class PlayerMovement : MonoBehaviour // מחלקה שאחראית על תנועת השחקן
{ // פתיחת המחלקה
    private CharacterController characterController; // הפניה לרכיב שמזיז את השחקן ומטפל בהתנגשויות
    private Vector2 moveInput; // שומר את קלט התנועה בשני צירים: X לצדדים ו-Y קדימה ואחורה
    [SerializeField] private float movementSpeed = 4f; // מהירות תנועת השחקן במצב רגיל
    [SerializeField] private float sprintSpeed = 6f; // מהירות תנועת השחקן בריצה
    [SerializeField] private float crouchSpeed = 2f; // מהירות תנועת השחקן בשפיפה
    [SerializeField] private float standingHeight = 1.8f; // גובה הקפסולה בעמידה
    [SerializeField] private float crouchHeight = 1f; // גובה הקפסולה בשפיפה
    [SerializeField] private float standingCameraHeight = 1.6f; // גובה המצלמה בעמידה
    [SerializeField] private float crouchingCameraHeight = 0.8f; // גובה המצלמה בשפיפה
    [SerializeField] private bool startCrouched = true; // האם השחקן מתחיל את המשחק במצב שפיפה
    [SerializeField] private LayerMask obstructionMask; // השכבות שנחשבות למכשול מעל ראש השחקן
    [SerializeField] private float gravity = -9.81f; // עוצמת כוח המשיכה כלפי מטה
    [SerializeField] private Transform playerCamera; // הפניה למצלמה הראשית שנחבר דרך האינספקטור
    [SerializeField] private NoiseSystem noiseSystem; // מערכת הרעש שאליה השחקן מדווח
    [SerializeField] private float sprintNoiseRadius = 8f; // המרחק שממנו ניתן לשמוע ספרינט
    [SerializeField] private float sprintNoiseInterval = 0.5f; // הזמן בין רעש צעד אחד לבא
    private float sprintNoiseTimer; // סופר את הזמן עד דיווח הרעש הבא מהספרינט
    private float verticalVelocity; // שומר את מהירות התנועה האנכית והנפילה
    private bool isCrouching; // זוכר אם השחקן נמצא כרגע בשפיפה
    private bool isSprinting; // זוכר אם השחקן נמצא כרגע בריצה
    private bool movementControlsEnabled = true; // האם פקודות התנועה פעילות

    private void Awake() // פועל פעם אחת כאשר האובייקט נטען
    { // תחילת פעולת ההכנה
        characterController = GetComponent<CharacterController>(); // מוצא ושומר את רכיב התנועה וההתנגשויות שעל השחקן

        if (noiseSystem == null) // בודק אם עדיין אין חיבור למערכת הרעש
        { // תחילת התנאי
            noiseSystem = FindFirstObjectByType<NoiseSystem>(); // מחפש בסצנה את מערכת הרעש ושומר אותה
        } // סוף התנאי

        if (noiseSystem == null) // בודק אם מערכת הרעש עדיין לא נמצאה
        { // תחילת התנאי
            Debug.LogError("לא נמצאה מערכת רעש בסצנה", this); // מציג שגיאה ברורה ומקשר אותה לרכיב הנוכחי
        } // סוף התנאי

        isCrouching = startCrouched; // קובע את מצב ההתכופפות ההתחלתי
        ApplyCrouchState(); // מעדכן את גובה הקפסולה והמצלמה
    } // סוף פעולת ההכנה

    private void Update() // פועל בכל פריים ומחשב את תנועת השחקן
    { // פתיחת Update
        Vector3 movement = transform.right * moveInput.x + transform.forward * moveInput.y; // מחבר תנועה לצדדים עם תנועה קדימה ואחורה
        movement = Vector3.ClampMagnitude(movement, 1f); // מגביל את עוצמת הכיוון כדי שאלכסון לא יהיה מהיר יותר
        float currentSpeed = movementSpeed; // קובע כברירת מחדל את מהירות ההליכה
        if (isSprinting) // בודק אם מצב הריצה פעיל
        { // פתיחת תנאי הריצה
            currentSpeed = sprintSpeed; // מחליף את המהירות למהירות ריצה
        } // סיום תנאי הריצה
        if (isCrouching) // בודק אם מצב השפיפה פעיל
        { // פתיחת תנאי השפיפה
            currentSpeed = crouchSpeed; // מחליף את המהירות למהירות שפיפה
        } // סיום תנאי השפיפה
        if (characterController.isGrounded && verticalVelocity < 0f) // בודק שהשחקן על הקרקע ונע כלפי מטה
        { // פתיחת תנאי הקרקע
            verticalVelocity = -2f; // מפעיל כוח קטן מטה כדי להצמיד את השחקן לרצפה
        } // סיום תנאי הקרקע
        verticalVelocity += gravity * Time.deltaTime; // מעדכן את מהירות הנפילה לפי הזמן שעבר
        movement *= currentSpeed; // מכפיל את כיוון התנועה במהירות שנבחרה
        movement.y = verticalVelocity; // מוסיף לתנועה את הנפילה בציר Y

        characterController.Move(movement * Time.deltaTime); // מזיז את השחקן לפי התנועה ובקצב שאינו תלוי בפריימים
        HandleSprintNoise(); // בודק אם צריך לדווח על רעש ספרינט
    } // סיום Update
    private void HandleSprintNoise() // מנהלת את דיווחי הרעש בזמן ספרינט
    {
        if (noiseSystem == null) // בודק אם אין מערכת שאפשר לדווח אליה
        { // תחילת התנאי
            return; // עוצר את הפעולה ומונע ניסיון להשתמש בחיבור חסר
        } // סוף התנאי

        if (!isSprinting || moveInput.sqrMagnitude < 0.01f || !characterController.isGrounded) // אם השחקן לא באמת רץ
        {
            sprintNoiseTimer = 0f; // מאפס את זמן ההמתנה

            return; // לא מדווח על רעש
        }
        sprintNoiseTimer -= Time.deltaTime; // מוריד מהטיימר את הזמן שעבר בפריים
        if (sprintNoiseTimer > 0f) // אם עדיין לא הגיע הזמן לרעש הבא
        {
            return; // ממשיך להמתין
        }
        noiseSystem.ReportNoise(transform.position, sprintNoiseRadius); // מדווח על רעש במיקום השחקן
        sprintNoiseTimer = sprintNoiseInterval; // מתחיל את ההמתנה עד לרעש הבא
    }
    public void OnMove(InputAction.CallbackContext context) // מקבל את קלט התנועה ממערכת הקלט
    { // פתיחת OnMove
        if (!movementControlsEnabled) // אם פקודות התנועה נעולות
        {
            moveInput = Vector2.zero; // מוודא שאין תנועה שנשארה

            return; // מפסיק את און מוב
        }
        moveInput = context.ReadValue<Vector2>(); // קורא ושומר את כיוון התנועה כ-Vector2
    } // סיום OnMove

    public void OnSprint(InputAction.CallbackContext context) // מקבל את הלחיצה והשחרור של כפתור הריצה
    { // פתיחת OnSprint
        if (!movementControlsEnabled) // אם פקודות התנועה נעולות
        {
            return; // לא מאפשר להתחיל לרוץ
        }
        isSprinting = context.ReadValueAsButton(); // שומר true בלחיצה על Shift ו-false בשחרור

        if (isSprinting && isCrouching) // בודק אם השחקן מנסה לרוץ בזמן שפיפה
        { // פתיחת תנאי ריצה מתוך שפיפה
            if (!CanStandUp()) // בודק אם אין מספיק מקום לעמוד
            { // פתיחת תנאי המקום החסום
                isSprinting = false; // מבטל את מצב הריצה
                return; // עוצר את הפונקציה כדי שהשחקן לא יעמוד
            } // סיום תנאי המקום החסום

            isCrouching = false; // מוציא את השחקן ממצב שפיפה
            ApplyCrouchState(); // מחזיר את הקפסולה והמצלמה למצב עמידה
        } // סיום תנאי ריצה מתוך שפיפה
    } // סיום OnSprint

    public void OnCrouch(InputAction.CallbackContext context) // מקבל את הלחיצה על כפתור השפיפה
    { // פתיחת OnCrouch
        if (!movementControlsEnabled) // אם פקודות התנועה נעולות
        {
            return; // לא מאפשר לשנות מצב כפיפה
        }
        if (!context.performed) // בודק אם האירוע אינו שלב הלחיצה שבוצעה
        { // פתיחת בדיקת שלב הלחיצה
            return; // מתעלם משלבים אחרים כמו שחרור הכפתור
        } // סיום בדיקת שלב הלחיצה
        if (isCrouching && !CanStandUp()) // בודק אם השחקן שפוף ואין מקום לעמוד
        { // פתיחת בדיקת המקום מעל הראש
            return; // משאיר את השחקן שפוף ועוצר את הפונקציה
        } // סיום בדיקת המקום מעל הראש
        isCrouching = !isCrouching; // הופך את מצב השפיפה בלחיצה: true ל-false או להפך
        ApplyCrouchState(); // מיישם את מצב השפיפה החדש על הקפסולה והמצלמה
    } // סיום OnCrouch

    private void ApplyCrouchState() // מיישם בפועל את מצב העמידה או השפיפה
    { // פתיחת ApplyCrouchState
        float targetHeight = isCrouching ? crouchHeight : standingHeight; // בוחר גובה שפיפה או עמידה לפי המצב
        characterController.height = targetHeight; // משנה את גובה הקפסולה
        characterController.center = new Vector3(0f, targetHeight / 2f, 0f); // שומר את תחתית הקפסולה על הרצפה
        Vector3 cameraPosition = playerCamera.localPosition; // שומר עותק של מיקום המצלמה ביחס ל-Player
        cameraPosition.y = isCrouching ? crouchingCameraHeight : standingCameraHeight; // בוחר את גובה המצלמה לפי המצב
        playerCamera.localPosition = cameraPosition; // מחיל את מיקום המצלמה המעודכן
    } // סיום ApplyCrouchState

    private bool CanStandUp() // בודק ומחזיר האם יש מספיק מקום לעמוד
    { // פתיחת CanStandUp
        float checkRadius = characterController.radius * 0.95f; // יוצר רדיוס בדיקה מעט קטן מרדיוס הקפסולה
        Vector3 capsuleBottom = transform.position + Vector3.up * (crouchHeight + checkRadius); // מגדיר את תחתית אזור הבדיקה מעל גובה השפיפה
        Vector3 capsuleTop = transform.position + Vector3.up * (standingHeight - checkRadius); // מגדיר את החלק העליון של אזור הבדיקה בגובה העמידה
        bool isBlocked = Physics.CheckCapsule(capsuleBottom, capsuleTop, checkRadius, obstructionMask, QueryTriggerInteraction.Ignore); // בודק אם Collider חוסם את חלל העמידה
        return !isBlocked; // מחזיר true כשהמקום פנוי ו-false כשהוא חסום
    } // סיום CanStandUp
    public void SetMovementControlsEnabled(bool enabled) // מפעילה או נועלת את פקודות התנועה
    {
        movementControlsEnabled = enabled; // שומרת את מצב פקודות התנועה

        if (!enabled) // אם נעלנו את התנועה
        {
            moveInput = Vector2.zero; // עוצר מיד את התנועה האופקית

            isSprinting = false; // מבטל ריצה מהירה
        }
    }
} // סיום המחלקה PlayerMovement