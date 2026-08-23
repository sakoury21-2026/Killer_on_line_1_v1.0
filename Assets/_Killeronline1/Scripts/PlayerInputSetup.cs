using UnityEngine; // מאפשר שימוש בכלים הבסיסיים של המנוע
using UnityEngine.InputSystem; // מאפשר שימוש בכלים של מערכת הקלט

[RequireComponent(typeof(PlayerInput))] // מחייב רכיב קלט על אותו עצם
public class PlayerInputSetup : MonoBehaviour // מגדיר מחלקה שאפשר לחבר לעצם במשחק
{ // תחילת המחלקה
    private PlayerInput playerInput; // שומר הפניה לרכיב הקלט של השחקן

    private void Awake() // פעולה שמופעלת בעת יצירת העצם
    { // תחילת פעולת ההכנה
        playerInput = GetComponent<PlayerInput>(); // מוצא ושומר את רכיב הקלט שעל אותו עצם
    } // סוף פעולת ההכנה

    private void Start() // פעולה שמופעלת בתחילת המשחק לאחר ההכנה
    { // תחילת פעולת ההתחלה
        InputSystem.actions.Disable(); // מכבה את כל מפות הפעולות הכלליות
        playerInput.currentActionMap?.Enable(); // מפעיל את המפה הנוכחית רק אם היא קיימת
    } // סוף פעולת ההתחלה
} // סוף המחלקה