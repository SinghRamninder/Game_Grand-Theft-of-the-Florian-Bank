using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class lasers : MonoBehaviour
{
    [Header("Other Settings")]
    public float lightBlinkDuration = 0.3f;

    [Header("Advanced Settings (Can be ignored)")]
    public Light2D indicatorLight;
    public GameObject lasersGameobject;
    [HideInInspector] public string keyName;
    [HideInInspector] public GameObject lockTransform;
    public GameObject instructionText;
    public GameObject instructionKey;

    private Coroutine blinkRoutine;

    private bool playerNear = false;
    private PickPoket pickPoket;
    private bool isLaserWorking = true;

    private void Start()
    {
        if (instructionText == null)
        {
            var lvlRef = GameObject.FindFirstObjectByType<LevelReferences>();

            if (lvlRef != null)
            {
                instructionText = lvlRef.laserDeactivatedText;
            }
        }
    }

    private void Update()
    {
        if (!isLaserWorking)
        {
            instructionKey.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Z) && isLaserWorking && playerNear)
        {
            if (pickPoket.hasKey(keyName))
            {
                if (lockTransform != null)
                {
                    pickPoket.PlayUseKeyFly(keyName, lockTransform.transform.position);
                }

                lasersGameobject.SetActive(false);
                StartCoroutine(showandhidetext());

                isLaserWorking = false;
            }
            else
            {
                if (blinkRoutine != null)
                    StopCoroutine(blinkRoutine);

                blinkRoutine = StartCoroutine(BlinkRoutine());
            }
        }
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            pickPoket = other.GetComponent<PickPoket>();
            playerNear = true;
            instructionKey.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            pickPoket = null;
            playerNear = false;
            instructionKey.SetActive(false);
        }
    }

    IEnumerator showandhidetext()
    {
        instructionText.SetActive(true);

        yield return new WaitForSeconds(3f);

        instructionText.SetActive(false);
    }
}
