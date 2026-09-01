using UnityEngine;
using UnityEngine.InputSystem;

public sealed class GameFlow : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    private bool isFinished;

    private void Awake()
    {
        Time.timeScale = 1f;

        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    public void ShowWin()
    {
        Finish(winPanel);
    }

    public void ShowLose()
    {
        Finish(losePanel);
    }

    private void Finish(GameObject panel)
    {
        if (isFinished)
        {
            return;
        }

        isFinished = true;

        playerInput.DeactivateInput();

        panel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }
}