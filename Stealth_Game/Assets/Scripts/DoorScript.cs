using System.Collections;
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
    [SerializeField] private string keyNameUp1;
    [SerializeField] private string keyNameUp2;
    [SerializeField] private string keyNameDown1;
    [SerializeField] private string keyNameDown2;
    [SerializeField] private bool isUpUnlocked = false;
    [SerializeField] private bool isDownUnlocked = false;

    [Header("Indicators")]
    [SerializeField] private SpriteRenderer upIndicator;
    [SerializeField] private SpriteRenderer downIndicator;
    [SerializeField] private Light2D indicatorLight;
    [SerializeField] private float blinkDuration;

    [Header("Locks")]
    [SerializeField] private GameObject upLock1;
    [SerializeField] private GameObject upLock2;
    [SerializeField] private GameObject downLock1;
    [SerializeField] private GameObject downLock2;

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
                if (pickPoket.hasKey(keyNameUp1))
                {
                    if (keyNameUp1 != null && upLock1 != null)
                        pickPoket.PlayUseKeyFly(keyNameUp1, upLock1.transform.position);

                    if (keyNameUp2 != null && upLock2 != null)
                        pickPoket.PlayUseKeyFly(keyNameUp2, upLock2.transform.position);

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
                if (pickPoket.hasKey(keyNameDown1))
                {
                    if (keyNameDown1 != null && downLock1 != null)
                        pickPoket.PlayUseKeyFly(keyNameDown1, downLock1.transform.position);

                    if (keyNameDown2 != null && downLock2 != null)
                        pickPoket.PlayUseKeyFly(keyNameDown2, downLock2.transform.position);

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
        yield return new WaitForSeconds(1f);

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

    public void lockAllDoors()
    {
        ColorUtility.TryParseHtmlString("#C10000", out Color red);

        isUpUnlocked = false;
        isDownUnlocked = false;

        if (upIndicator != null)
        {
            upIndicator.color = red;
        }
        if (downIndicator != null)
        {
            downIndicator.color = red;
        }

    }
}
