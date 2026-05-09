using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndManager : MonoBehaviour
{
    [HideInInspector] public string[] afterEndingOptions = { "Restart", "New Scene" };
    [HideInInspector] public SceneAsset selectedScene;
    [HideInInspector] public int afterEnding = 0;
    [HideInInspector] public float delayBeforeSceneLoad = 0f;

    [Header("End Sequence Dependencies")]
    [Tooltip("Filled automatically by editor or dynamically if left null")]
    [HideInInspector] public GameObject timerDisplay;
    [HideInInspector] public AfterStealMoney countdownScript;
    [HideInInspector] public GameObject keyInventoryDisplay;
    [HideInInspector] public GameObject playerGameObject;
    [HideInInspector] public AudioManager audioManager;
    [HideInInspector] public List<SecurityOfficerScript> allGuards = new List<SecurityOfficerScript>();

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(EndSequenceRoutine());
        }
    }

    private void Start()
    {
        if (timerDisplay == null && LevelReferences.instance != null)
            timerDisplay = LevelReferences.instance.timerDisplay;

        if (keyInventoryDisplay == null)
        {
            KeyInventoryUI ui = Object.FindFirstObjectByType<KeyInventoryUI>(FindObjectsInactive.Include);
            if (ui != null) keyInventoryDisplay = ui.gameObject;
        }

        if (playerGameObject == null)
        {
            PlayerMovement pm = Object.FindFirstObjectByType<PlayerMovement>(FindObjectsInactive.Include);
            if (pm != null) playerGameObject = pm.gameObject;
        }

        if (audioManager == null)
            audioManager = Object.FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include);

        if (countdownScript == null)
            countdownScript = Object.FindFirstObjectByType<AfterStealMoney>(FindObjectsInactive.Include);

        allGuards.AddRange(Object.FindObjectsByType<SecurityOfficerScript>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    }

    private IEnumerator EndSequenceRoutine()
    {
        // Handle global scene disable logic you requested should happen no matter what:
        if (timerDisplay != null) timerDisplay.SetActive(false);
        if (keyInventoryDisplay != null) keyInventoryDisplay.SetActive(false);

        if (countdownScript != null)
        {
            countdownScript.StopCountdown();
            countdownScript.enabled = false;
        }

        foreach (SecurityOfficerScript guard in allGuards)
        {
            if (guard != null)
            {
                guard.ForceStopChaseAndTurnAround();
            }
        }

        if (playerGameObject != null)
        {
            playerGameObject.tag = "Untagged";
            PlayerMovement pm = playerGameObject.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = false;
        }

        if (audioManager != null)
        {
            audioManager.PlayMusicOnce(audioManager.endCredits, 1f);
            audioManager.StopSFX();
        }

        // Disable Cinemachine Brain if applicable
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            var brain = mainCam.GetComponent<Unity.Cinemachine.CinemachineBrain>();
            if (brain != null) brain.enabled = false;
        }

        // Sub-sequence orchestration
        EndCutsceneManager cutsceneManager = GetComponentInChildren<EndCutsceneManager>(false); // active children only
        EndCreditsManager creditsManager = GetComponentInChildren<EndCreditsManager>(false); // active children only

        if (cutsceneManager != null)
        {
            yield return StartCoroutine(cutsceneManager.PlayCutsceneSequence());
        }

        if (creditsManager != null)
        {
            yield return StartCoroutine(creditsManager.PlayCreditsSequence());
        }

        if (delayBeforeSceneLoad > 0f)
        {
            yield return new WaitForSeconds(delayBeforeSceneLoad);
        }

        // Execute actual ending logic after things are disabled
        if (afterEndingOptions[afterEnding] == "Restart")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (afterEndingOptions[afterEnding] == "New Scene")
        {
            if (selectedScene != null)
            {
                SceneManager.LoadScene(selectedScene.name);
            }
            else
            {
                Debug.LogError("Add a scene in the inspector");
            }
        }
    }
}
