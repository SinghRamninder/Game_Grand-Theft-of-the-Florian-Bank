using System.Collections;
using UnityEngine;

public class Cutscene : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerController;
    [SerializeField] private CameraMovement cameraControl;
    [SerializeField] private Animator cameraAnimation;
    [SerializeField] private GameObject stealCash;


    private void Awake()
    {
        playerController.enabled = false;
        cameraControl.enabled = false;
        stealCash.SetActive(false);
    }

    void Start()
    {
        StartCoroutine(CutsceneStart());
    }


    IEnumerator CutsceneStart()
    {
        yield return new WaitForSeconds(1f);

        cameraAnimation.SetTrigger("StartCutscene");

        yield return new WaitForSeconds(2f);

        stealCash.SetActive(true);

        yield return new WaitForSeconds(2.5f);

        stealCash.SetActive(false);
        cameraAnimation.SetTrigger("EndCutscene");

        yield return new WaitForSeconds(2f);

        playerController.enabled = true;
        cameraControl.enabled = true;
        cameraAnimation.enabled = false;
    }
}
