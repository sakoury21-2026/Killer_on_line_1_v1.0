
public interface IHoldInteractable
// חוזה לחפץ שמגיב ללחיצה ולשחרור
{
    void BeginInteract();
    // מתחיל את האחיזה כשהכפתור נלחץ
    void EndInteract();
    // מסיים את האחיזה כשהכפתור משתחרר
}
