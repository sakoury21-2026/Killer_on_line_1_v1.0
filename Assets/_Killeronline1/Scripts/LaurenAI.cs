using UnityEngine;
// נותן גישה ל-MonoBehaviour, ל-Transform, ל-Animator, ל-Vector3 ול-Time של Unity.
using UnityEngine.AI;
// נותן גישה ל-NavMeshAgent, ל-NavMesh ולבדיקת נקודות הליכה חוקיות.

public enum LaurenState
// מגדיר את כל המצבים האפשריים של Lauren כמילים ברורות במקום הרבה משתני bool סותרים.
{
    // פתיחת גוף ה-enum LaurenState.
    Patrol,
    // Lauren הולכת בין נקודות המסלול.
    Investigate,
    // Lauren הולכת אל המקום שבו שמעה רעש או ראתה את השחקן לאחרונה.
    Search,
    // Lauren עומדת ומחפשת לזמן קצר לאחר שהגיעה למקום החשוד.
    Chase,
    // Lauren רודפת אחרי השחקן כל עוד היא באמת רואה אותו.
    ReturnToPatrol,
    // Lauren חוזרת למסלול לאחר שסיימה לחפש.
    Caught
    // Lauren עצרה את התנועה ומפעילה את אנימציית התפיסה.
}
// סיום גוף ה-enum LaurenState.

[DisallowMultipleComponent]
// מונע שני מוחות שונים של Lauren על אותו אובייקט.
[RequireComponent(typeof(NavMeshAgent), typeof(LaurenVision), typeof(RoomTracker))]
// מבטיח שכל הרכיבים החיוניים לתנועה, לראייה ולחדרים קיימים.
[RequireComponent(typeof(Animator))]
// מבטיח שה-Animator נמצא על אותו GameObject כדי ש-Animation Event יוכל למצוא את LaurenAI.
public sealed class LaurenAI : MonoBehaviour
// משמש כמוח היחיד שמחליט מה Lauren עושה בכל רגע.
{
    // פתיחת גוף המחלקה LaurenAI.
    [Header("Required References")]
    // יוצר כותרת ב-Inspector עבור החיבורים שחייבים להיות קיימים.
    [SerializeField] private NavMeshAgent agent;
    // מזיז את Lauren על ה-NavMesh ואחראי למסלול הפיזי שלה.
    [SerializeField] private LaurenVision vision;
    // עונה למוח אם Lauren רואה את השחקן כרגע.
    [SerializeField] private RoomTracker roomTracker;
    // זוכר באיזה חדר נמצאת Lauren כדי לבצע עצירה בכל חדר חדש.
    [SerializeField] private Transform player;
    // שומר את מיקום השחקן עבור מרדף ומדידת מרחק תפיסה.
    [SerializeField] private NoiseSystem noiseSystem;
    // מפרסם ל-Lauren רעשים של ספרינט, מגירות ודלתות.
    [SerializeField] private GameFlow gameFlow;
    // מציג מסך הפסד לאחר שהסתיימה תפיסת השחקן.
    [SerializeField] private Animator animator;
    // מציג Idle, הליכה, ריצה ואנימציית תפיסה לפי מצב ה-AI.

    [Header("Patrol Route")]
    // יוצר כותרת ב-Inspector עבור מערך נקודות הסיור.
    [SerializeField] private PatrolPoint[] patrolPoints;
    // שומר מערך מסודר של נקודות ש-Lauren תבקר בהן לפי הסדר.
    [SerializeField] private float roomIdleDuration = 4.5f;
    // קובע כמה שניות Lauren תעמוד בפעם הראשונה שהיא נכנסת לחדר חדש.

    [Header("Movement")]
    // יוצר כותרת ב-Inspector עבור מהירויות ומרחקי עצירה.
    [SerializeField] private float patrolSpeed = 2f;
    // קובע את מהירות ההליכה בזמן סיור, חקירה וחזרה.
    [SerializeField] private float chaseSpeed = 4.5f;
    // קובע את מהירות הריצה בזמן מרדף.
    [SerializeField] private float arrivalDistance = 0.45f;
    // קובע מתי יעד נחשב כיעד שהגענו אליו.
    [SerializeField] private float navMeshSampleRadius = 2f;
    // קובע כמה רחוק מותר לחפש נקודת NavMesh תקינה סביב רעש או שחקן.

    [Header("Search And Chase")]
    // יוצר כותרת ב-Inspector עבור זמני חיפוש ותפיסה.
    [SerializeField] private float searchDuration = 4f;
    // קובע כמה זמן Lauren מחפשת לאחר שהגיעה למקום החשוד.
    [SerializeField] private float searchTurnSpeed = 70f;
    // קובע כמה מהר Lauren מסתובבת בזמן החיפוש.
    [SerializeField] private float chaseRefreshInterval = 0.2f;
    // קובע באיזו תדירות יעד המרדף מתעדכן כדי לא לחשב מסלול בכל פריים.
    [SerializeField] private float catchDistance = 1.2f;
    // קובע את המרחק שבו Lauren מתחילה את תפיסת השחקן.
    [SerializeField] private float catchFallbackDuration = 2.5f;
    // קובע מתי להציג הפסד אם אירוע האנימציה לא חובר בטעות.

    [Header("Animator Parameters")]
    // יוצר כותרת ב-Inspector עבור שמות הפרמטרים ב-Animator Controller.
    [SerializeField] private string movingParameter = "IsMoving";
    // שומר את שם פרמטר ה-bool שמפעיל הליכה או Idle.
    [SerializeField] private string runningParameter = "IsRunning";
    // שומר את שם פרמטר ה-bool שמבדיל בין הליכה לריצה.
    [SerializeField] private string killingTrigger = "Killing";
    // שומר את שם ה-Trigger שמפעיל את אנימציית התפיסה.

    [SerializeField] private LaurenState currentState;
    // שומר ומציג ב-Inspector את המצב הפעיל שהוא מקור האמת היחיד להתנהגות Lauren.
    private bool stateWasInitialized;
    // זוכר אם כבר נכנסנו למצב ראשון כדי לא לדלג על פעולות ההתחלה שלו.
    private int patrolIndex;
    // שומר את האינדקס של נקודת הסיור הבאה בתוך המערך.
    private float roomIdleTimer;
    // סופר לאחור את זמן העמידה בחדר חדש.
    private float searchTimer;
    // סופר לאחור את זמן החיפוש במקום החשוד.
    private float chaseRefreshTimer;
    // סופר לאחור עד עדכון יעד המרדף הבא.
    private float catchTimer;
    // סופר לאחור עד הצגת הפסד במקרה שאירוע האנימציה לא הופעל.
    private Vector3 investigationPosition;
    // שומר את מיקום הרעש או את המקום האחרון שבו השחקן נראה.
    private Vector3 lastKnownPlayerPosition;
    // שומר את מיקום השחקן האחרון שנראה לפני שאבד קשר העין.
    private RoomVolume lastVisitedRoom;
    // שומר את החדר האחרון שבו כבר בוצעה עצירת ה-Idle.
    private bool catchFinished;
    // מונע הצגת מסך הפסד יותר מפעם אחת.

    public LaurenState CurrentState => currentState;
    // מאפשר למסך בדיקה לקרוא את מצב Lauren בלי לשנות אותו.

    private void Awake()
    // פועל פעם אחת כאשר Lauren נטענת ומשלים חיבורים מקומיים וחיבורי סצנה.
    {
        // פתיחת המתודה Awake.
        if (agent == null)
        // בודק אם ה-NavMeshAgent לא חובר דרך ה-Inspector.
        {
            // פתיחת תנאי השלמת ה-Agent.
            agent = GetComponent<NavMeshAgent>();
            // מוצא את ה-NavMeshAgent על אותו אובייקט.
        }
        // סיום תנאי השלמת ה-Agent.

        if (vision == null)
        // בודק אם LaurenVision לא חובר דרך ה-Inspector.
        {
            // פתיחת תנאי השלמת הראייה.
            vision = GetComponent<LaurenVision>();
            // מוצא את LaurenVision על אותו אובייקט.
        }
        // סיום תנאי השלמת הראייה.

        if (roomTracker == null)
        // בודק אם RoomTracker לא חובר דרך ה-Inspector.
        {
            // פתיחת תנאי השלמת החדרים.
            roomTracker = GetComponent<RoomTracker>();
            // מוצא את RoomTracker על אותו אובייקט.
        }
        // סיום תנאי השלמת החדרים.

        if (animator == null)
        // בודק אם Animator לא חובר דרך ה-Inspector.
        {
            // פתיחת תנאי השלמת האנימטור.
            animator = GetComponent<Animator>();
            // מחפש Animator על אותו GameObject כדי שאירוע האנימציה יוכל לקרוא ל-FinishCatchAnimation.
        }
        // סיום תנאי השלמת האנימטור.

        if (noiseSystem == null)
        // בודק אם NoiseSystem לא חובר דרך ה-Inspector.
        {
            // פתיחת תנאי השלמת מערכת הרעש.
            noiseSystem = FindFirstObjectByType<NoiseSystem>();
            // מחפש את מערכת הרעש היחידה בסצנה כגיבוי לחיבור ידני.
        }
        // סיום תנאי השלמת מערכת הרעש.

        if (gameFlow == null)
        // בודק אם GameFlow לא חובר דרך ה-Inspector.
        {
            // פתיחת תנאי השלמת זרימת המשחק.
            gameFlow = FindFirstObjectByType<GameFlow>();
            // מחפש את מנהל הניצחון וההפסד היחיד בסצנה.
        }
        // סיום תנאי השלמת זרימת המשחק.

        if (agent != null)
        // בודק שה-Agent קיים לפני שמשנים הגדרה שלו.
        {
            // פתיחת תנאי הגדרת ה-Agent.
            agent.autoBraking = true;
            // מאפשר ל-Lauren להאט כשהיא מתקרבת ליעד במקום לעבור אותו.
        }
        // סיום תנאי הגדרת ה-Agent.
    }
    // סיום המתודה Awake.

    private void OnEnable()
    // פועל בכל פעם שהרכיב מופעל ומתחיל להאזין לרעשים.
    {
        // פתיחת המתודה OnEnable.
        if (noiseSystem != null)
        // בודק שקיימת מערכת רעש שאפשר להירשם אליה.
        {
            // פתיחת תנאי ההרשמה.
            noiseSystem.NoiseReported += HandleNoiseReported;
            // מחבר את פונקציית השמיעה לאירוע הרעש של טל.
        }
        // סיום תנאי ההרשמה.
    }
    // סיום המתודה OnEnable.

    private void OnDisable()
    // פועל בכל פעם שהרכיב נכבה ומנקה חיבורים זמניים.
    {
        // פתיחת המתודה OnDisable.
        if (noiseSystem != null)
        // בודק שמערכת הרעש עדיין קיימת.
        {
            // פתיחת תנאי ביטול ההרשמה.
            noiseSystem.NoiseReported -= HandleNoiseReported;
            // מנתק את פונקציית השמיעה כדי למנוע הפעלות כפולות ודליפת אירועים.
        }
        // סיום תנאי ביטול ההרשמה.

        if (agent != null && agent.isOnNavMesh)
        // בודק שה-Agent קיים ונמצא על NavMesh תקין.
        {
            // פתיחת תנאי איפוס המסלול.
            agent.ResetPath();
            // עוצר מסלול פעיל כאשר Lauren נכבית.
        }
        // סיום תנאי איפוס המסלול.
    }
    // סיום המתודה OnDisable.

    private void Start()
    // פועל בתחילת המשחק לאחר שכל רכיבי הסצנה סיימו Awake.
    {
        // פתיחת המתודה Start.
        if (!HasRequiredRuntimeSetup())
        // בודק שכל החיבורים וה-NavMesh הדרושים להפעלה קיימים.
        {
            // פתיחת תנאי ההגדרה הלא תקינה.
            enabled = false;
            // מכבה את המוח כדי למנוע רצף שגיאות בכל פריים.
            return;
            // עוצר את פעולת ההתחלה.
        }
        // סיום תנאי ההגדרה הלא תקינה.

        ChangeState(LaurenState.Patrol, true);
        // מכניס את Lauren למצב סיור ומכריח את פעולות האתחול של המצב הראשון.
    }
    // סיום המתודה Start.

    private void Update()
    // פועל בכל פריים ומריץ רק את ההיגיון של המצב הנוכחי.
    {
        // פתיחת המתודה Update.
        if (gameFlow != null && gameFlow.IsFinished)
        // בודק אם המשחק כבר הסתיים בניצחון או בהפסד.
        {
            // פתיחת תנאי סיום המשחק.
            StopAgent();
            // עוצר את Lauren כדי שלא תמשיך ללכת מאחורי מסך הסיום.
            UpdateAnimatorParameters();
            // מעדכן את האנימטור למצב שאינו זז.
            return;
            // עוצר את שאר החלטות ה-AI.
        }
        // סיום תנאי סיום המשחק.

        if (currentState == LaurenState.Caught)
        // בודק אם Lauren כבר התחילה את תפיסת השחקן.
        {
            // פתיחת תנאי התפיסה.
            UpdateCaught();
            // ממשיך רק את טיימר סיום התפיסה.
            UpdateAnimatorParameters();
            // משאיר את פרמטרי התנועה כבויים בזמן האנימציה.
            return;
            // מונע מעבר למצב אחר באמצע התפיסה.
        }
        // סיום תנאי התפיסה.

        bool seesPlayer = vision.CanSeePlayer();
        // שואל פעם אחת בפריים את מערכת הראייה אם השחקן נראה.

        if (seesPlayer)
        // בודק אם כל חוקי הראייה עברו בהצלחה.
        {
            // פתיחת תנאי ראיית השחקן.
            lastKnownPlayerPosition = player.position;
            // שומר את המיקום האחרון שבו השחקן נראה בבירור.

            if (Vector3.Distance(transform.position, player.position) <= catchDistance)
            // בודק אם השחקן קרוב מספיק לתפיסה.
            {
                // פתיחת תנאי מרחק התפיסה.
                BeginCatch();
                // מתחיל את מצב התפיסה ואת אנימציית Killing.
                UpdateAnimatorParameters();
                // מכבה את פרמטרי התנועה מיד בפריים התפיסה.
                return;
                // עוצר החלטות נוספות באותו פריים.
            }
            // סיום תנאי מרחק התפיסה.

            ChangeState(LaurenState.Chase);
            // מעביר את Lauren למרדף כל עוד היא רואה את השחקן והוא עדיין רחוק.
        }
        // סיום תנאי ראיית השחקן.
        else if (currentState == LaurenState.Chase)
        // בודק אם קשר העין אבד בדיוק בזמן מרדף.
        {
            // פתיחת תנאי אובדן קשר העין.
            investigationPosition = lastKnownPlayerPosition;
            // קובע שהיעד הבא הוא המקום האחרון שבו השחקן נראה.
            ChangeState(LaurenState.Investigate);
            // עובר לחקירה במקום להמשיך לרדוף דרך קירות או בין חדרים.
        }
        // סיום תנאי אובדן קשר העין.

        switch (currentState)
        // בוחר להריץ רק את קוד ההתנהגות של המצב הנוכחי.
        {
            // פתיחת גוף ה-switch.
            case LaurenState.Patrol:
                // מופעל כאשר Lauren נמצאת בסיור רגיל.
                UpdatePatrol();
                // מטפל בהליכה בין נקודות ובעצירה בחדר חדש.
                break;
            // מסיים את המקרה Patrol.
            case LaurenState.Investigate:
                // מופעל כאשר Lauren בדרך למיקום חשוד.
                UpdateInvestigate();
                // בודק אם היא הגיעה למקום הרעש או המיקום האחרון.
                break;
            // מסיים את המקרה Investigate.
            case LaurenState.Search:
                // מופעל כאשר Lauren מחפשת במקום החשוד.
                UpdateSearch();
                // מסובב אותה ומנהל את טיימר החיפוש.
                break;
            // מסיים את המקרה Search.
            case LaurenState.Chase:
                // מופעל כאשר Lauren רואה את השחקן ורודפת אחריו.
                UpdateChase();
                // מעדכן את יעד המרדף בקצב מוגבל.
                break;
            // מסיים את המקרה Chase.
            case LaurenState.ReturnToPatrol:
                // מופעל כאשר Lauren חוזרת למסלול הרגיל.
                UpdateReturnToPatrol();
                // בודק מתי היא הגיעה לנקודת החזרה.
                break;
                // מסיים את המקרה ReturnToPatrol.
        }
        // סיום גוף ה-switch.

        UpdateAnimatorParameters();
        // מעדכן Idle, הליכה וריצה לפי המהירות והמצב הנוכחיים.
    }
    // סיום המתודה Update.

    private bool HasRequiredRuntimeSetup()
    // בודק פעם אחת אם כל החלקים החיוניים מחוברים ותקינים.
    {
        // פתיחת המתודה HasRequiredRuntimeSetup.
        bool isValid = true;
        // מתחיל בהנחה שההגדרה תקינה ומשנה אותה כאשר מתגלה בעיה.

        if (agent == null || !agent.isOnNavMesh)
        // בודק אם חסר Agent או שהוא לא עומד על NavMesh אפוי.
        {
            // פתיחת תנאי ה-Agent הלא תקין.
            Debug.LogError("LaurenAI צריך NavMeshAgent שעומד על NavMesh אפוי", this);
            // מסביר את התיקון הנדרש ומקשר ל-Lauren.
            isValid = false;
            // מסמן שאסור להפעיל את המוח במצב הנוכחי.
        }
        // סיום תנאי ה-Agent הלא תקין.

        if (vision == null || player == null)
        // בודק אם חסרה מערכת הראייה או הפניה לשחקן.
        {
            // פתיחת תנאי חיבורי הראייה החסרים.
            Debug.LogError("LaurenAI צריך LaurenVision וחיבור ל-Player", this);
            // מציג הודעת חיבור ברורה ב-Console.
            isValid = false;
            // מסמן שההגדרה אינה מוכנה.
        }
        // סיום תנאי חיבורי הראייה החסרים.

        if (patrolPoints == null || patrolPoints.Length == 0)
        // בודק אם מערך נקודות הסיור ריק.
        {
            // פתיחת תנאי המסלול הריק.
            Debug.LogError("LaurenAI צריך לפחות PatrolPoint אחד במערך", this);
            // מסביר שצריך לגרור נקודות למערך ב-Inspector.
            isValid = false;
            // מסמן שאין מסלול שאפשר להתחיל בו.
        }
        // סיום תנאי המסלול הריק.

        if (roomTracker == null || noiseSystem == null || gameFlow == null || animator == null)
        // בודק את חיבורי החדרים, השמיעה, סיום המשחק והאנימציה.
        {
            // פתיחת תנאי החיבורים החסרים.
            Debug.LogError("LaurenAI צריך RoomTracker, NoiseSystem, GameFlow ו-Animator מחוברים", this);
            // מסביר אילו מערכות חסרות ל-Lauren.
            isValid = false;
            // מסמן שאסור להתחיל את ה-AI לפני השלמת החיבורים.
        }
        // סיום תנאי החיבורים החסרים.

        return isValid;
        // מחזיר את תוצאת כל בדיקות ההגדרה.
    }
    // סיום המתודה HasRequiredRuntimeSetup.

    private void ChangeState(LaurenState newState, bool forceRestart = false)
    // מרכז את כל המעברים בין המצבים במקום אחד ברור.
    {
        // פתיחת המתודה ChangeState.
        if (stateWasInitialized && currentState == newState && !forceRestart)
        // בודק אם כבר נמצאים באותו מצב ואין צורך לאתחל אותו שוב.
        {
            // פתיחת תנאי מניעת אתחול כפול.
            return;
            // יוצא כדי לא לאפס טיימרים וטריגרים בכל פריים.
        }
        // סיום תנאי מניעת אתחול כפול.

        currentState = newState;
        // שומר את המצב החדש כמקור האמת של ה-AI.
        stateWasInitialized = true;
        // מסמן שלפחות מצב אחד כבר אותחל.

        switch (currentState)
        // בוחר את פעולת הכניסה שמתאימה למצב החדש.
        {
            // פתיחת גוף ה-switch של הכניסה למצב.
            case LaurenState.Patrol:
                // מופעל בכניסה לסיור.
                agent.speed = patrolSpeed;
                // קובע מהירות הליכה רגילה.
                agent.isStopped = false;
                // מאפשר ל-Agent לנוע שוב.
                roomIdleTimer = 0f;
                // מאפס טיימר ישן של עצירה בחדר.
                if (!agent.hasPath)
                // בודק אם עדיין אין מסלול פעיל כאשר נכנסים למצב הסיור.
                {
                    // פתיחת תנאי בחירת היעד הראשון.
                    GoToNextPatrolPoint();
                    // שולח את Lauren לנקודה הראשונה הזמינה במסלול.
                }
                // סיום תנאי בחירת היעד הראשון.
                break;
            // מסיים את פעולות הכניסה ל-Patrol.
            case LaurenState.Investigate:
                // מופעל בכניסה לחקירת מיקום חשוד.
                agent.speed = patrolSpeed;
                // משתמש במהירות הליכה כדי שהחקירה לא תהיה מרדף מלא.
                agent.isStopped = false;
                // מאפשר תנועה אל המיקום החשוד.
                if (!TrySetDestination(investigationPosition))
                // בודק אם לא נמצאה נקודת NavMesh חוקית ליד היעד החשוד.
                {
                    // פתיחת תנאי יעד החקירה הלא חוקי.
                    ChangeState(LaurenState.Search, true);
                    // עובר לחיפוש במקום להישאר תקוע בלי מסלול.
                }
                // סיום תנאי יעד החקירה הלא חוקי.
                break;
            // מסיים את פעולות הכניסה ל-Investigate.
            case LaurenState.Search:
                // מופעל בכניסה לחיפוש.
                StopAgent();
                // עוצר את Lauren במקום החשוד.
                searchTimer = searchDuration;
                // מתחיל ספירה לאחור של זמן החיפוש.
                break;
            // מסיים את פעולות הכניסה ל-Search.
            case LaurenState.Chase:
                // מופעל בכניסה למרדף.
                agent.speed = chaseSpeed;
                // מחליף למהירות הריצה.
                agent.isStopped = false;
                // מאפשר ל-Agent לרוץ.
                chaseRefreshTimer = 0f;
                // מכריח עדכון יעד מיידי בפריים הבא.
                break;
            // מסיים את פעולות הכניסה ל-Chase.
            case LaurenState.ReturnToPatrol:
                // מופעל כאשר מסתיים החיפוש.
                agent.speed = patrolSpeed;
                // חוזר למהירות הליכה.
                agent.isStopped = false;
                // מאפשר תנועה למסלול.
                GoToNextPatrolPoint();
                // בוחר את נקודת הסיור הבאה כיעד החזרה.
                break;
            // מסיים את פעולות הכניסה ל-ReturnToPatrol.
            case LaurenState.Caught:
                // מופעל כאשר Lauren תפסה את השחקן.
                StopAgent();
                // עוצר כל מסלול ותנועה של Lauren.
                catchTimer = catchFallbackDuration;
                // מתחיל טיימר גיבוי למקרה שאירוע האנימציה לא חובר.
                if (animator != null)
                // בודק אם חובר Animator שאפשר להפעיל.
                {
                    // פתיחת תנאי הפעלת אנימציית התפיסה.
                    animator.SetTrigger(killingTrigger);
                    // מפעיל פעם אחת את אנימציית Killing.
                }
                // סיום תנאי הפעלת אנימציית התפיסה.
                break;
                // מסיים את פעולות הכניסה ל-Caught.
        }
        // סיום גוף ה-switch של הכניסה למצב.
    }
    // סיום המתודה ChangeState.

    private void UpdatePatrol()
    // מנהל את ההליכה השקטה בין נקודות המסלול.
    {
        // פתיחת המתודה UpdatePatrol.
        if (TryStartRoomIdle())
        // בודק אם Lauren נכנסה עכשיו לחדר חדש.
        {
            // פתיחת תנאי תחילת העצירה בחדר.
            return;
            // ממתין לפריים הבא לאחר שהטיימר התחיל.
        }
        // סיום תנאי תחילת העצירה בחדר.

        if (roomIdleTimer > 0f)
        // בודק אם Lauren עדיין צריכה לעמוד בחדר החדש.
        {
            // פתיחת תנאי טיימר ה-Idle.
            roomIdleTimer -= Time.deltaTime;
            // מפחית מהטיימר את הזמן שעבר בפריים הנוכחי.

            if (roomIdleTimer <= 0f)
            // בודק אם זמן העמידה הסתיים.
            {
                // פתיחת תנאי סיום ה-Idle.
                agent.isStopped = false;
                // מאפשר ל-Agent לנוע שוב.
                GoToNextPatrolPoint();
                // ממשיך לנקודה הבאה במסלול.
            }
            // סיום תנאי סיום ה-Idle.

            return;
            // מונע בחירת יעד נוסף בזמן העמידה.
        }
        // סיום תנאי טיימר ה-Idle.

        if (!agent.hasPath || ReachedDestination())
        // בודק אם אין מסלול פעיל או שהגענו ליעד הנוכחי.
        {
            // פתיחת תנאי מעבר לנקודה הבאה.
            GoToNextPatrolPoint();
            // בוחר את הנקודה הבאה במערך המעגלי.
        }
        // סיום תנאי מעבר לנקודה הבאה.
    }
    // סיום המתודה UpdatePatrol.

    private bool TryStartRoomIdle()
    // מתחיל עצירה רק בפעם הראשונה ש-Lauren נכנסת לחדר אחר.
    {
        // פתיחת המתודה TryStartRoomIdle.
        RoomVolume currentRoom = roomTracker != null ? roomTracker.CurrentRoom : null;
        // קורא את החדר הנוכחי או null אם RoomTracker חסר.

        if (currentRoom == null || currentRoom == lastVisitedRoom)
        // בודק אם החדר לא ידוע או שכבר עצרנו בו קודם.
        {
            // פתיחת תנאי אין חדר חדש.
            return false;
            // מחזיר false כי אין צורך להתחיל Idle חדש.
        }
        // סיום תנאי אין חדר חדש.

        lastVisitedRoom = currentRoom;
        // זוכר שבחדר הזה כבר בוצעה עצירה.
        roomIdleTimer = roomIdleDuration;
        // מתחיל את זמן העמידה שהוגדר ב-Inspector.
        StopAgent();
        // עוצר את המסלול בזמן ה-Idle.
        return true;
        // מחזיר true כדי ש-UpdatePatrol לא יבצע פעולה נוספת באותו פריים.
    }
    // סיום המתודה TryStartRoomIdle.

    private void GoToNextPatrolPoint()
    // בוחר יעד חוקי מתוך מערך נקודות הסיור ומתקדם באינדקס.
    {
        // פתיחת המתודה GoToNextPatrolPoint.
        if (patrolPoints == null || patrolPoints.Length == 0)
        // בודק שהמערך קיים ומכיל לפחות נקודה אחת.
        {
            // פתיחת תנאי המערך הריק.
            return;
            // יוצא כי אין יעד שאפשר לבחור.
        }
        // סיום תנאי המערך הריק.

        for (int attempts = 0; attempts < patrolPoints.Length; attempts++)
        // מנסה לכל היותר פעם אחת כל תא במערך כדי לדלג על חיבור חסר.
        {
            // פתיחת לולאת חיפוש נקודה חוקית.
            PatrolPoint point = patrolPoints[patrolIndex];
            // קורא את נקודת הסיור שנמצאת באינדקס הנוכחי.
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            // מתקדם לנקודה הבאה וחוזר לאפס בסוף המערך.

            if (point != null && TrySetDestination(point.transform.position))
            // בודק שהנקודה חוברה ושנמצא לידה NavMesh תקין.
            {
                // פתיחת תנאי היעד החוקי.
                return;
                // יוצא לאחר שהוגדר יעד אחד תקין.
            }
            // סיום תנאי היעד החוקי.
        }
        // סיום לולאת חיפוש נקודה חוקית.

        Debug.LogWarning("LaurenAI לא מצא PatrolPoint חוקי על ה-NavMesh", this);
        // מציג אזהרה אם אף נקודה במערך אינה ניתנת להגעה.
    }
    // סיום המתודה GoToNextPatrolPoint.

    private void UpdateInvestigate()
    // בודק אם Lauren הגיעה אל מקום הרעש או אל המיקום האחרון של השחקן.
    {
        // פתיחת המתודה UpdateInvestigate.
        if (ReachedDestination())
        // בודק אם המסלול הסתיים בתוך מרחק ההגעה.
        {
            // פתיחת תנאי ההגעה למקום החשוד.
            ChangeState(LaurenState.Search);
            // עובר לחיפוש קצר במקום החשוד.
        }
        // סיום תנאי ההגעה למקום החשוד.
    }
    // סיום המתודה UpdateInvestigate.

    private void UpdateSearch()
    // מנהל את סיבוב החיפוש ואת סיום זמן החיפוש.
    {
        // פתיחת המתודה UpdateSearch.
        transform.Rotate(Vector3.up, searchTurnSpeed * Time.deltaTime);
        // מסובב את Lauren בעדינות כדי להמחיש שהיא בודקת את הסביבה.
        searchTimer -= Time.deltaTime;
        // מפחית מהטיימר את הזמן שעבר בפריים.

        if (searchTimer <= 0f)
        // בודק אם זמן החיפוש הסתיים.
        {
            // פתיחת תנאי סיום החיפוש.
            ChangeState(LaurenState.ReturnToPatrol);
            // מחזיר את Lauren למסלול במקום להשאיר אותה במקום.
        }
        // סיום תנאי סיום החיפוש.
    }
    // סיום המתודה UpdateSearch.

    private void UpdateChase()
    // מעדכן את יעד המרדף בקצב קבוע ולא בכל פריים.
    {
        // פתיחת המתודה UpdateChase.
        chaseRefreshTimer -= Time.deltaTime;
        // מפחית מהטיימר את הזמן שעבר בפריים.

        if (chaseRefreshTimer > 0f)
        // בודק אם עדיין לא הגיע זמן עדכון היעד הבא.
        {
            // פתיחת תנאי ההמתנה לעדכון.
            return;
            // משאיר את היעד הקודם וחוסך חישוב מסלול מיותר.
        }
        // סיום תנאי ההמתנה לעדכון.

        TrySetDestination(player.position);
        // מעדכן את יעד ה-Agent לנקודת NavMesh חוקית ליד מיקום השחקן.
        chaseRefreshTimer = chaseRefreshInterval;
        // מתחיל את ההמתנה עד עדכון המרדף הבא.
    }
    // סיום המתודה UpdateChase.

    private void UpdateReturnToPatrol()
    // בודק אם Lauren הגיעה בחזרה אל נקודת המסלול שנבחרה.
    {
        // פתיחת המתודה UpdateReturnToPatrol.
        if (!agent.hasPath || ReachedDestination())
        // בודק אם מסלול החזרה הסתיים או לא נוצר.
        {
            // פתיחת תנאי סיום החזרה.
            ChangeState(LaurenState.Patrol);
            // מחזיר את המוח למצב הסיור הרגיל.
        }
        // סיום תנאי סיום החזרה.
    }
    // סיום המתודה UpdateReturnToPatrol.

    private void HandleNoiseReported(Vector3 position, float radius)
    // מקבלת מ-NoiseSystem את מיקום הרעש ואת הרדיוס שבו אפשר לשמוע אותו.
    {
        // פתיחת המתודה HandleNoiseReported.
        if (!enabled || agent == null || !agent.isOnNavMesh)
        // בודק שהמוח פעיל וה-Agent מוכן לתנועה.
        {
            // פתיחת תנאי המערכת הלא מוכנה.
            return;
            // מתעלם מהרעש כדי למנוע שגיאת מסלול.
        }
        // סיום תנאי המערכת הלא מוכנה.

        if (currentState == LaurenState.Chase || currentState == LaurenState.Caught)
        // בודק אם Lauren כבר רודפת או מבצעת תפיסה.
        {
            // פתיחת תנאי מצב בעדיפות גבוהה.
            return;
            // מתעלם מרעש כי ראיית שחקן ותפיסה חשובות יותר.
        }
        // סיום תנאי מצב בעדיפות גבוהה.

        if (Vector3.Distance(transform.position, position) > radius)
        // בודק אם Lauren נמצאת מחוץ לרדיוס הרעש.
        {
            // פתיחת תנאי הרעש הרחוק.
            return;
            // מתעלם מרעש שהיא אינה אמורה לשמוע.
        }
        // סיום תנאי הרעש הרחוק.

        investigationPosition = position;
        // שומר את מיקום הרעש בתור היעד החשוד החדש.
        ChangeState(LaurenState.Investigate, true);
        // מתחיל חקירה חדשה גם אם Lauren כבר חקרה רעש קודם.
    }
    // סיום המתודה HandleNoiseReported.

    private bool TrySetDestination(Vector3 requestedPosition)
    // מנסה להמיר מיקום עולמי לנקודה חוקית על ה-NavMesh ולהגדיר אותה כיעד.
    {
        // פתיחת המתודה TrySetDestination.
        if (agent == null || !agent.isOnNavMesh)
        // בודק שה-Agent קיים ועומד על NavMesh.
        {
            // פתיחת תנאי ה-Agent הלא מוכן.
            return false;
            // מחזיר false כי אי אפשר לבנות מסלול.
        }
        // סיום תנאי ה-Agent הלא מוכן.

        if (!NavMesh.SamplePosition(requestedPosition, out NavMeshHit navHit, navMeshSampleRadius, NavMesh.AllAreas))
        // מחפש נקודת הליכה חוקית סביב המיקום המבוקש.
        {
            // פתיחת תנאי כישלון הדגימה.
            return false;
            // מחזיר false כי לא נמצאה נקודת NavMesh קרובה.
        }
        // סיום תנאי כישלון הדגימה.

        agent.isStopped = false;
        // מאפשר ל-Agent לנוע אם הוא נעצר במצב קודם.
        return agent.SetDestination(navHit.position);
        // מגדיר את היעד החוקי ומחזיר אם Unity הצליחה להתחיל חישוב מסלול.
    }
    // סיום המתודה TrySetDestination.

    private bool ReachedDestination()
    // מחזיר true רק כאשר ה-Agent סיים לחשב ונמצא קרוב מספיק ליעד.
    {
        // פתיחת המתודה ReachedDestination.
        return !agent.pathPending && agent.remainingDistance <= Mathf.Max(arrivalDistance, agent.stoppingDistance);
        // משווה למרחק הבטוח הגדול מבין שתי הגדרות העצירה.
    }
    // סיום המתודה ReachedDestination.

    private void BeginCatch()
    // מתחיל את התפיסה פעם אחת בלבד.
    {
        // פתיחת המתודה BeginCatch.
        if (currentState == LaurenState.Caught)
        // בודק אם התפיסה כבר התחילה בפריים קודם.
        {
            // פתיחת תנאי מניעת תפיסה כפולה.
            return;
            // מונע הפעלה חוזרת של Trigger האנימציה.
        }
        // סיום תנאי מניעת תפיסה כפולה.

        catchFinished = false;
        // מאפס את דגל הסיום עבור התפיסה החדשה.
        ChangeState(LaurenState.Caught);
        // עוצר את Lauren ומפעיל את אנימציית Killing.
    }
    // סיום המתודה BeginCatch.

    private void UpdateCaught()
    // מנהל טיימר גיבוי בזמן שאנימציית התפיסה מתנגנת.
    {
        // פתיחת המתודה UpdateCaught.
        catchTimer -= Time.deltaTime;
        // מפחית את הזמן שעבר כל עוד המשחק עדיין לא נעצר.

        if (catchTimer <= 0f)
        // בודק אם אירוע האנימציה לא סיים את התפיסה בזמן שהוגדר.
        {
            // פתיחת תנאי טיימר הגיבוי.
            FinishCatchAnimation();
            // מסיים את התפיסה כדי שהמשחק לא ייתקע בגלל חיבור Animation Event חסר.
        }
        // סיום תנאי טיימר הגיבוי.
    }
    // סיום המתודה UpdateCaught.

    public void FinishCatchAnimation()
    // מיועדת גם ל-Animation Event בפריים האחרון של קליפ Killing.
    {
        // פתיחת המתודה FinishCatchAnimation.
        if (catchFinished)
        // בודק אם אירוע האנימציה או טיימר הגיבוי כבר סיימו את התפיסה.
        {
            // פתיחת תנאי מניעת סיום כפול.
            return;
            // מונע הצגת מסך הפסד פעמיים.
        }
        // סיום תנאי מניעת סיום כפול.

        catchFinished = true;
        // מסמן שהתפיסה הסתיימה באופן סופי.

        if (gameFlow != null)
        // בודק שמנהל המשחק חובר.
        {
            // פתיחת תנאי הצגת ההפסד.
            gameFlow.ShowLose();
            // מציג מסך הפסד, עוצר זמן ומשחרר את סמן העכבר.
        }
        // סיום תנאי הצגת ההפסד.
        else
        // מופעל אם GameFlow לא חובר בטעות.
        {
            // פתיחת תנאי השגיאה.
            Debug.LogError("LaurenAI לא יכול להציג הפסד כי GameFlow חסר", this);
            // מציג הוראת תיקון ברורה ב-Console.
        }
        // סיום תנאי השגיאה.
    }
    // סיום המתודה FinishCatchAnimation.

    private void StopAgent()
    // מרכז את עצירת ה-NavMeshAgent במקום אחד בטוח.
    {
        // פתיחת המתודה StopAgent.
        if (agent == null || !agent.isOnNavMesh)
        // בודק שה-Agent קיים ועומד על NavMesh.
        {
            // פתיחת תנאי ה-Agent הלא מוכן.
            return;
            // יוצא בלי לקרוא לפעולה שאינה חוקית.
        }
        // סיום תנאי ה-Agent הלא מוכן.

        agent.isStopped = true;
        // מורה ל-Agent להפסיק לנוע.
        agent.ResetPath();
        // מוחק את המסלול הישן כדי שלא ימשיך לאחר שחרור העצירה.
    }
    // סיום המתודה StopAgent.

    private void UpdateAnimatorParameters()
    // מתרגם את מצב התנועה של ה-Agent לפרמטרים פשוטים ב-Animator.
    {
        // פתיחת המתודה UpdateAnimatorParameters.
        if (animator == null || agent == null)
        // בודק שקיימים גם Animator וגם Agent.
        {
            // פתיחת תנאי חיבורי האנימציה החסרים.
            return;
            // יוצא כי אין רכיב שאפשר לעדכן.
        }
        // סיום תנאי חיבורי האנימציה החסרים.

        bool isMoving = currentState != LaurenState.Caught && !agent.isStopped && agent.velocity.sqrMagnitude > 0.01f;
        // קובע אם Lauren באמת נעה ולא נמצאת בתפיסה.
        bool isRunning = isMoving && currentState == LaurenState.Chase;
        // קובע שריצה פעילה רק בזמן תנועה במצב Chase.
        animator.SetBool(movingParameter, isMoving);
        // מעדכן את פרמטר ההליכה או ה-Idle.
        animator.SetBool(runningParameter, isRunning);
        // מעדכן את פרמטר הריצה.
    }
    // סיום המתודה UpdateAnimatorParameters.
}
// סיום גוף המחלקה LaurenAI.
