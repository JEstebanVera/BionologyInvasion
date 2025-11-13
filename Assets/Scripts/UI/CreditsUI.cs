using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsUI : MonoBehaviour
{
    private void Start()
    {

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void MenuGame()
    {
        FreeCursor();
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        FreeCursor();
        Debug.Log("Cerrando el juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void FreeCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
