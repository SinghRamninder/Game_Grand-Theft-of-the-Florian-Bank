using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    public void RestartScene()
    {
        SceneManager.LoadScene("Prototype");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    public void ReturnToMain()
    {
        SceneManager.LoadScene("Start");
    }

    public void quitGame()
    {
        Application.Quit();
    }
}
