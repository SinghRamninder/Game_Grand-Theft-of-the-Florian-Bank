using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

public class AfterStealMoney : MonoBehaviour
{
    [Tooltip("The actual money GameObject to hide when stolen.")]
    [HideInInspector] public GameObject money;

    [Tooltip("List of all alert lights that will blink when the alarm goes off. Add lights via the custom editor inspector.")]
    public List<Light2D> alertLights = new List<Light2D>();

    [Tooltip("The UI prompt or object telling the player to press a key.")]
    [HideInInspector] public GameObject instructionKey;

    [Tooltip("How fast the warning lights should blink after the steal.")]
    [SerializeField] private float alertLightBlinkSpeed = 1f;

    [Tooltip("Trigger to activate the end credits sequence.")]
    [HideInInspector] public GameObject endCreditTrigger;

    [Header("Countdown")]
    [Tooltip("Number of seconds the player has to escape before the timer runs out.")]
    public float countdownSeconds = 30f;
    private float remaining;

    [HideInInspector] public bool blink = false;
    private Coroutine blinkRoutine;

    private bool isNear;
    [HideInInspector] public PlayerMovement playerMovement;
    private AudioManager audioManager;

    private List<DoorScript> allElevators = new List<DoorScript>();
    private List<SecurityOfficerScript> allGuards = new List<SecurityOfficerScript>();
    private List<HiddingBin> allHidingBins = new List<HiddingBin>();
    private List<HiddingTable> allHidingTables = new List<HiddingTable>();
    private List<HiddingDesk> allHidingDesks = new List<HiddingDesk>();

    private GameObject timerCanvas;
    private TMP_Text timerText;
    private GameObject timeUpCanvas;
    private GameObject gameOverDisplay;

    private Coroutine countdownRoutine;
    private bool isTimeUp;

    private void Start()
    {
        playerMovement = Object.FindFirstObjectByType<PlayerMovement>();
        audioManager = Object.FindFirstObjectByType<AudioManager>();

        if (LevelReferences.instance != null)
        {
            gameOverDisplay = LevelReferences.instance.gameOverDisplay;
            timerCanvas = LevelReferences.instance.timerDisplay;
            timeUpCanvas = LevelReferences.instance.timesUpDisplay;

            if (timerCanvas != null)
            {
                timerText = timerCanvas.GetComponentInChildren<TMP_Text>(true);
            }
        }

        allElevators.AddRange(Object.FindObjectsByType<DoorScript>(FindObjectsSortMode.None));
        allGuards.AddRange(Object.FindObjectsByType<SecurityOfficerScript>(FindObjectsSortMode.None));

        allHidingBins.AddRange(Object.FindObjectsByType<HiddingBin>(FindObjectsSortMode.None));
        allHidingTables.AddRange(Object.FindObjectsByType<HiddingTable>(FindObjectsSortMode.None));
        allHidingDesks.AddRange(Object.FindObjectsByType<HiddingDesk>(FindObjectsSortMode.None));

        if (endCreditTrigger == null)
            endCreditTrigger = Object.FindFirstObjectByType<EndManager>(FindObjectsInactive.Include).gameObject;

        if (timerCanvas != null) timerCanvas.SetActive(false);
        if (timeUpCanvas != null) timeUpCanvas.SetActive(false);
    }

    private void Update()
    {
        // Blink lights
        if (blink && blinkRoutine == null)
        {
            blinkRoutine = StartCoroutine(Blink());
        }
        else if (!blink && blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
            foreach (var light in alertLights)
            {
                if (light) light.intensity = 0f;
            }
        }

        // Trigger heist
        if (Input.GetKeyDown(KeyCode.C) && isNear)
        {
            if (instructionKey) instructionKey.SetActive(false);
            if (money) money.SetActive(false);
            var col = GetComponent<BoxCollider2D>();
            if (col) col.enabled = false;

            blink = true;

            if (audioManager != null)
            {
                audioManager.SetSFXVolume(0.03f);
                if (audioManager.siren != null)
                    audioManager.PlaySFXLoop(audioManager.siren, 0f);

                audioManager.PlayChaseMusic(0f);
            }

            foreach (var guard in allGuards)
            {
                if (guard != null)
                    guard.speed += 2f;
            }

            foreach (var elevator in allElevators)
            {
                if (elevator != null)
                {
                    elevator.lockAllDoors();
                    elevator.ChangeIsCalled();
                }
            }

            if (CheckPoint.instance != null)
            {
                CheckPoint.instance.moneyStolen = true;
            }

            if (endCreditTrigger) endCreditTrigger.SetActive(true);

            StartCountdown();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (instructionKey) instructionKey.SetActive(true);
            isNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (instructionKey) instructionKey.SetActive(false);
            isNear = false;
        }
    }

    private IEnumerator Blink()
    {
        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * alertLightBlinkSpeed;
            float v = Mathf.PingPong(t, 1f);

            foreach (var light in alertLights)
            {
                if (light) light.intensity = v;
            }

            yield return null;
        }
    }

    public void StartCountdown(float seconds = -1f)
    {
        if (seconds > 0f)
            countdownSeconds = seconds;

        isTimeUp = false;

        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        countdownRoutine = StartCoroutine(Countdown());
    }

    public void StopCountdown(bool hideUI = true)
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }

        remaining = 0f;
        isTimeUp = false;

        if (hideUI)
        {
            if (timerCanvas != null) timerCanvas.SetActive(false);
            if (timeUpCanvas != null) timeUpCanvas.SetActive(false);
        }
    }

    public void ResetTimeUpUI()
    {
        isTimeUp = false;
        if (timeUpCanvas != null) timeUpCanvas.SetActive(false);
        if (timerCanvas != null) timerCanvas.SetActive(false);
        if (audioManager != null) audioManager.PlayChaseMusic();
    }

    public bool IsTimeUp()
    {
        return isTimeUp;
    }

    private IEnumerator Countdown()
    {
        if (timerCanvas != null) timerCanvas.SetActive(true);
        if (timeUpCanvas != null) timeUpCanvas.SetActive(false);

        remaining = Mathf.Max(0f, countdownSeconds);

        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;

            if (timerText != null)
            {
                int secondsInt = Mathf.CeilToInt(remaining);
                timerText.text = secondsInt.ToString();
            }

            yield return null;
        }

        remaining = 0f;
        if (timerText != null) timerText.text = "0";

        // Time up
        isTimeUp = true;

        // Stop player + music, show canvas
        if (playerMovement != null) playerMovement.enabled = false;
        if (audioManager != null) audioManager.StopMusic();

        if (timeUpCanvas != null) timeUpCanvas.SetActive(true);

        foreach (var bin in allHidingBins)
        {
            if (bin != null && bin.isHidden)
            {
                bin.removeHiding();
            }
        }

        foreach (var table in allHidingTables)
        {
            if (table != null && table.isHidden)
            {
                table.removeHiding();
            }
        }

        foreach (var desk in allHidingDesks)
        {
            if (desk != null && desk.isHidden)
            {
                desk.removeHiding();
            }
        }
    }
}
