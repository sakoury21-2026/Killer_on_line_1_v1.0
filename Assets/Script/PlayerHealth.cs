using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public bool isDead = false;

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Game Over - המפלצת תפסה אותך");

        // השבתת תנועה (התאם לשם הסקריפט שלך)
        // GetComponent<PlayerMovement>().enabled = false;

        // טעינה מחדש של הסצנה אחרי שנייה (למסך מוות קטן)
        Invoke(nameof(RestartLevel), 1.5f);
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
