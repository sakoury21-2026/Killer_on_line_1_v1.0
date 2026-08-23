using UnityEngine; // נותן גישה למערכת של יוניטי

public class PlayerInventory : MonoBehaviour // שומר את החפצים שהשחקן אסף
{
    private bool hasExitKey; // האם השחקן מחזיק במפתח היציאה
    public void CollectExitKey() // מופעלת כשהשחקן אוסף את מפתח היציאה
    {
        hasExitKey = true; // זוכר שהמפתח נמצא אצל השחקן

        Debug.Log("Exit key collected"); // הודעת בדיקה זמנית
    }
        public bool HasExitKey() // מחזירה תשובה האם השחקן מחזיק במפתח
    {
        return hasExitKey; // מחזירה אמת או שקר
    }
}