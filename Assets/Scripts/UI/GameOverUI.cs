using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RetryGame()
    {
        FreeCursor();
        SceneManager.LoadScene("Game");
    }

    public void ExitToMainMenu()
    {
        FreeCursor();
        SceneManager.LoadScene("MainMenu");
    }

    private void FreeCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
