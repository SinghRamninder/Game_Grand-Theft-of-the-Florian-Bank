using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DoorScript : MonoBehaviour
{
    [Header("Teleport Locations")]
    [HideInInspector] public Transform teleportUp;
    [HideInInspector] public Transform teleportDown;

    [Header("Settings")]
    [HideInInspector] public KeyCode upKey = KeyCode.E;
    [HideInInspector] public KeyCode downKey = KeyCode.Q;
    [HideInInspector] public bool isUpAllow;
    [HideInInspector] public bool isDownAllow;

    [Header("Key Required to Unlock")]
    [HideInInspector] public string keyNameUp1;
    [HideInInspector] public string keyNameUp2;
    [HideInInspector] public string keyNameDown1;
    [HideInInspector] public string keyNameDown2;
    [HideInInspector] public bool isUpUnlocked = false;
    [HideInInspector] public bool isDownUnlocked = false;

    [Header("Indicators")]
    [HideInInspector] public SpriteRenderer upIndicator;
    [HideInInspector] public SpriteRenderer downIndicator;
    [HideInInspector] public Light2D indicatorLight;

    [Header("Locks")]
    [HideInInspector] public GameObject upLock1;
    [HideInInspector] public GameObject upLock2;
    [HideInInspector] public GameObject downLock1;
    [HideInInspector] public GameObject downLock2;

    [Header("Other Settings")]
    public float lightBlinkDuration = 0.3f;
    public float teleportTimeDelay = 1f;

    private Transform player;
    private bool playerInside = false;
    private PickPoket pickPoket;
    private Coroutine blinkRoutine;

    private bool isUpCalled = false;
    private bool isDownCalled = false;

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
                if (pickPoket.hasKey(keyNameUp1) && !isUpCalled)
                {
                    if (keyNameUp1 != null && upLock1 != null)
                        pickPoket.PlayUseKeyFly(keyNameUp1, upLock1.transform.position);

                    if (keyNameUp2 != null && upLock2 != null)
                        pickPoket.PlayUseKeyFly(keyNameUp2, upLock2.transform.position);

                    StartCoroutine(doorUnlocked(upIndicator, true, teleportUp));

                    isUpCalled = true;
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
                if (pickPoket.hasKey(keyNameDown1) && !isDownCalled)
                {
                    if (keyNameDown1 != null && downLock1 != null)
                        pickPoket.PlayUseKeyFly(keyNameDown1, downLock1.transform.position);

                    if (keyNameDown2 != null && downLock2 != null)
                        pickPoket.PlayUseKeyFly(keyNameDown2, downLock2.transform.position);

                    StartCoroutine(doorUnlocked(downIndicator, false, teleportDown));

                    isDownCalled = true;
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

        yield return new WaitForSeconds(teleportTimeDelay);

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

        while (elapsed < lightBlinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lightBlinkDuration;
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

    public void ChangeIsCalled()
    {
        isUpCalled = false;
        isDownCalled = false;
    }
}
