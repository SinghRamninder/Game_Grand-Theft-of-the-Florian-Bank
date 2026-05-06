using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButtonOption : MonoBehaviour
{
    public void Restart()
    {
        if (CheckPoint.instance != null)
        {
            CheckPoint.instance.Restart();
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }
}
