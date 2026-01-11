using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DoorScript : MonoBehaviour
{
    [Header("Teleport Locations")]
    [SerializeField] private Transform teleportUp;
    [SerializeField] private Transform teleportDown;

    [Header("Settings")]
    [SerializeField] private KeyCode upKey = KeyCode.E;
    [SerializeField] private KeyCode downKey = KeyCode.Q;
    [SerializeField] private bool isUpAllow;
    [SerializeField] private bool isDownAllow;

    [Header("Key Required to Unlock")]
    [SerializeField] private string keyNameUp;
    [SerializeField] private string keyNameDown;
    [SerializeField] private bool isUpUnlocked = false;
    [SerializeField] private bool isDownUnlocked = false;

    [Header("Indicators")]
    [SerializeField] private SpriteRenderer upIndicator;
    [SerializeField] private SpriteRenderer downIndicator;
    [SerializeField] private Light2D indicatorLight;
    [SerializeField] private float blinkDuration;

    private Transform player;
    private bool playerInside = false;
    private PickPoket pickPoket;
    private Coroutine blinkRoutine;

    private void Update()
    {
        if (!playerInside) return;

        if (Input.GetKeyDown(upKey) && isUpAllow)
        {
            if (isUpUnlocked)
            {
                player.position = teleportUp.position;
            }
            else
            {
                if (pickPoket.hasKey(keyNameUp))
                {
                    StartCoroutine(doorUnlocked(upIndicator, true, teleportUp));
                }
                else
                {
                    if (blinkRoutine != null)
                        StopCoroutine(blinkRoutine);

                    blinkRoutine = StartCoroutine(BlinkRoutine());
                }
            }
        }

        if (Input.GetKeyDown(downKey) && isDownAllow)
        {
            if (isDownUnlocked)
            {
                player.position = teleportDown.position;
            }
            else
            {
                if (pickPoket.hasKey(keyNameDown))
                {
                    StartCoroutine(doorUnlocked(downIndicator, false, teleportDown));
                }
                else
                {
                    if (blinkRoutine != null)
                        StopCoroutine(blinkRoutine);

                    blinkRoutine = StartCoroutine(BlinkRoutine());
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            pickPoket = other.GetComponent<PickPoket>();
            playerInside = true;
            player = other.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            pickPoket = null;
            playerInside = false;
            player = null;
        }
    }

    private IEnumerator doorUnlocked(SpriteRenderer indicator, bool isUp, Transform teleportPosition)
    {
        ColorUtility.TryParseHtmlString("#04C100", out Color green);
        indicator.color = green;

        yield return new WaitForSeconds(1f);

        if (isUp)
            isUpUnlocked = true;
        else
            isDownUnlocked = true;

        if (player != null)
            player.position = teleportPosition.position;
    }

    private IEnumerator BlinkRoutine()
    {
        // Blink 1
        yield return Fade(0f, 1f);
        yield return Fade(1f, 0f);

        // Blink 2
        yield return Fade(0f, 1f);
        yield return Fade(1f, 0f);

        blinkRoutine = null;
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < blinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / blinkDuration;
            indicatorLight.intensity = Mathf.Lerp(from, to, t);
            yield return null;
        }

        indicatorLight.intensity = to;
    }
}
