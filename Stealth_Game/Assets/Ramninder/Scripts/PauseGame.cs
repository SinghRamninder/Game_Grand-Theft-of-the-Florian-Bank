using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private AudioManager audioManager;

    private bool isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isPaused)
        {
            pauseMenu.SetActive(true);
            isPaused = true;
            audioManager.PauseMusic();
            audioManager.PauseSFX();
            Time.timeScale = 0f;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isPaused)
        {
            pauseMenu.SetActive(false);
            audioManager.ResumeMusic();
            audioManager.ResumeSFX();
            isPaused = false;
            Time.timeScale = 1f;
        }
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        audioManager.ResumeMusic();
        audioManager.ResumeSFX();
        isPaused = false;
        Time.timeScale = 1f;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Start");
    }

    public void Exit()
    {
        Application.Quit();
        Debug.Log("Exit MainMenu");
    }
}
