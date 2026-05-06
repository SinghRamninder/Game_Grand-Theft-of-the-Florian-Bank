using UnityEngine;

public class PauseMenuButtons : MonoBehaviour
{
    [HideInInspector] public PauseGame pauseGame;

    private void Start()
    {
        if (pauseGame == null)
        {
            pauseGame = GameObject.FindFirstObjectByType<PauseGame>();
        }
    }

    private void OnEnable()
    {
        if (pauseGame == null)
        {
            pauseGame = GameObject.FindFirstObjectByType<PauseGame>();
        }
    }

    public void Resume()
    {
        pauseGame.Resume();
    }

    public void MainMenu()
    {
        pauseGame.MainMenu();
    }

    public void Exit()
    {
        pauseGame.Exit();
    }
}
