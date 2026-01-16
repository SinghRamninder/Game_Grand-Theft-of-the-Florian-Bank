using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class lasers : MonoBehaviour
{
    [SerializeField] private Light2D indicatorLight;
    [SerializeField] private float blinkDuration;
    [SerializeField] private GameObject lasersGameobject;
    [SerializeField] private string keyName;
    [SerializeField] private GameObject lockTransform;

    private Coroutine blinkRoutine;

    private bool playerNear = false;
    private PickPoket pickPoket;
    private bool isLaserWorking = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z) && isLaserWorking && playerNear)
        {
            if (pickPoket.hasKey(keyName))
            {
                pickPoket.PlayUseKeyFly(keyName, lockTransform.transform.position);

                lasersGameobject.SetActive(false);

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

        while (elapsed < blinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / blinkDuration;
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
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            pickPoket = null;
            playerNear = false;
        }
    }
}
