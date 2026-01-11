using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string sceneToLoad;
    public GameObject creditsObject;
    public GameObject buttonsObject;
    private bool areCreditsDisplayed = false;

    public void Play()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void ToggleCredits()
    {
        if(areCreditsDisplayed)
        {
            areCreditsDisplayed = false;
            creditsObject.SetActive(false);
            buttonsObject.SetActive(true);
        }
        else
        {
            areCreditsDisplayed = true;
            creditsObject.SetActive(true);
            buttonsObject.SetActive(false);
        }
    }


    public void ToggleCreditsShort()
    {
        areCreditsDisplayed = !areCreditsDisplayed;
        creditsObject.SetActive(areCreditsDisplayed);
        buttonsObject.SetActive(!areCreditsDisplayed);
    }

    public void Exit()
    {
        Application.Quit();
        Debug.Log("Exit MainMenu");
    }
}
