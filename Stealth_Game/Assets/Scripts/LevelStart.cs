using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class LevelStart : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private PlayerMovement playerScript;

    [Header("Step 1")]
    [SerializeField] private Transform pointA;
    [SerializeField] private float orthoSizeA = 6f;
    [SerializeField] private float moveDurationA = 1.5f;
    [SerializeField] private float yOnlyTime = 0.6f;
    [SerializeField] private float waitAtA = 1f;

    [Header("Step 2")]
    [SerializeField] private Transform pointB;
    [SerializeField] private float orthoSizeB = 5f;
    [SerializeField] private float moveDurationB = 1.5f;
    [SerializeField] private float waitAtB = 0f;

    [Header("Options")]
    [SerializeField] private bool enablePlayerAfterSequence = true;
    [SerializeField] private bool enableCinemachineAfterSequence = true;

    [Header("Canvas")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject keyInventory;

    private Camera cam;

    private void Start()
    {
        if (playerScript != null) playerScript.enabled = false;

        if (keyInventory != null) keyInventory.SetActive(false);

        var brain = mainCamera != null ? mainCamera.GetComponent<CinemachineBrain>() : null;
        if (brain != null) brain.enabled = false;

        cam = mainCamera != null ? mainCamera.GetComponent<Camera>() : null;

        StartCoroutine(IntroSequence(brain));
    }

    private IEnumerator IntroSequence(CinemachineBrain brain)
    {
        if (mainCamera == null || cam == null) yield break;

        if (pointA != null)
            yield return MoveToA_YStartsFirstThenXZoom(pointA.position, orthoSizeA, moveDurationA, yOnlyTime);

        if (waitAtA > 0f)
            yield return new WaitForSeconds(waitAtA);

        if (pointB != null)
            yield return MoveAndZoom(pointB.position, orthoSizeB, moveDurationB);

        if (waitAtB > 0f)
            yield return new WaitForSeconds(waitAtB);

        if (enableCinemachineAfterSequence && brain != null)
            brain.enabled = true;

        if (enablePlayerAfterSequence && playerScript != null)
            playerScript.enabled = true;

        if (mainMenu != null)
            Destroy(mainMenu);

        if (keyInventory != null) keyInventory.SetActive(true);

        Destroy(gameObject);
    }

    private IEnumerator MoveToA_YStartsFirstThenXZoom(Vector3 targetWorldPos, float targetOrthoSize, float totalDuration, float yOnlyDuration)
    {
        Vector3 startPos = mainCamera.transform.position;
        float startSize = cam.orthographicSize;

        Vector3 targetPos = new Vector3(targetWorldPos.x, targetWorldPos.y, startPos.z);

        if (totalDuration <= 0f)
        {
            mainCamera.transform.position = targetPos;
            cam.orthographicSize = targetOrthoSize;
            yield break;
        }

        float total = Mathf.Max(0.0001f, totalDuration);
        float yOnly = Mathf.Clamp(yOnlyDuration, 0f, totalDuration);
        float xPhase = Mathf.Max(0.0001f, totalDuration - yOnly);

        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            float g = Mathf.Clamp01(elapsed / total);                       // overall progress (for Y)
            float h = Mathf.Clamp01((elapsed - yOnly) / xPhase);            // X+Zoom progress (starts after yOnlyTime)

            float yP = Smooth01(g);
            float xzP = Smooth01(h);

            float y = Mathf.Lerp(startPos.y, targetPos.y, yP);
            float x = Mathf.Lerp(startPos.x, targetPos.x, xzP);

            mainCamera.transform.position = new Vector3(x, y, startPos.z);
            cam.orthographicSize = Mathf.Lerp(startSize, targetOrthoSize, xzP);

            yield return null;
        }

        mainCamera.transform.position = targetPos;
        cam.orthographicSize = targetOrthoSize;
    }

    private IEnumerator MoveAndZoom(Vector3 targetWorldPos, float targetOrthoSize, float duration)
    {
        Vector3 startPos = mainCamera.transform.position;
        float startSize = cam.orthographicSize;

        Vector3 endPos = new Vector3(targetWorldPos.x, targetWorldPos.y, startPos.z);

        if (duration <= 0f)
        {
            mainCamera.transform.position = endPos;
            cam.orthographicSize = targetOrthoSize;
            yield break;
        }

        float t = 0f;
        float dur = Mathf.Max(0.0001f, duration);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float eased = Smooth01(t);

            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, eased);
            cam.orthographicSize = Mathf.Lerp(startSize, targetOrthoSize, eased);

            yield return null;
        }

        mainCamera.transform.position = endPos;
        cam.orthographicSize = targetOrthoSize;
    }

    private float Smooth01(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }
}
