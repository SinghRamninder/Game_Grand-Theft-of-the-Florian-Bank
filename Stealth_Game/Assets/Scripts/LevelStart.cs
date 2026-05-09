using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class LevelStart : MonoBehaviour
{
    [System.Serializable]
    public class CameraMovementStep
    {
        [Tooltip("The transform representing the position to move the camera to.")]
        public Transform targetTransform;
        
        [Tooltip("The orthographic size of the camera for this step. Represents how much the camera sees.")]
        public float orthoSize = 5f;
        
        [Tooltip("How long it takes to move to this position in seconds.")]
        public float moveDuration = 1.5f;

        [Tooltip("How many seconds to wait at this position before continuing.")]
        public float waitTime = 1f;

        [Tooltip("Optional: if higher than 0, the Y axis position moves first for this duration, then X axis and zoom begin.")]
        public float yOnlyTime = 0f;
    }

    [HideInInspector] public GameObject mainCamera;
    [HideInInspector] public PlayerMovement playerScript;

    [Header("Options")]
    [HideInInspector] public bool enablePlayerAfterSequence = true;
    [HideInInspector] public bool enableCinemachineAfterSequence = true;
    [HideInInspector] public bool startSequenceOnStart = true;

    [Header("Canvas")]
    [HideInInspector] public GameObject mainMenu;
    [HideInInspector] public GameObject keyInventory;

    [Header("Camera Movements")]
    [Tooltip("Add steps here to structure your intro cutscene. The camera will move sequentially to each transform.")]
    public List<CameraMovementStep> cameraSteps = new List<CameraMovementStep>();

    private Camera cam;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main.gameObject;

        if (playerScript == null)
            playerScript = GameObject.FindFirstObjectByType<PlayerMovement>();

        if (playerScript != null)
            playerScript.enabled = false;

        if (keyInventory == null)
        {
            var keyUI = GameObject.FindFirstObjectByType<KeyInventoryUI>();
            if (keyUI != null) keyInventory = keyUI.gameObject;
        }

        if (keyInventory != null)
            keyInventory.SetActive(false);

        var brain = mainCamera != null ? mainCamera.GetComponent<CinemachineBrain>() : null;

        if (brain != null)
            brain.enabled = false;

        cam = Camera.main;

        if (startSequenceOnStart)
        {
            StartCoroutine(IntroSequence(brain));
        }
    }

    private IEnumerator IntroSequence(CinemachineBrain brain)
    {
        if (mainCamera == null || cam == null) yield break;

        foreach (var step in cameraSteps)
        {
            if (step.targetTransform == null) continue;

            if (step.yOnlyTime > 0f)
            {
                yield return MoveToA_YStartsFirstThenXZoom(step.targetTransform.position, step.orthoSize, step.moveDuration, step.yOnlyTime);
            }
            else
            {
                yield return MoveAndZoom(step.targetTransform.position, step.orthoSize, step.moveDuration);
            }

            if (step.waitTime > 0f)
            {
                yield return new WaitForSeconds(step.waitTime);
            }
        }

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

            float g = Mathf.Clamp01(elapsed / total);
            float h = Mathf.Clamp01((elapsed - yOnly) / xPhase);

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

    private void OnDrawGizmos()
    {
        if (cameraSteps == null) return;

        float aspect = 16f / 9f;
        if (Application.isPlaying && cam != null)
        {
            aspect = cam.aspect;
        }
        else if (Camera.main != null)
        {
            aspect = Camera.main.aspect;
        }

        Gizmos.color = Color.cyan;

        foreach (var step in cameraSteps)
        {
            if (step.targetTransform != null)
            {
                float height = step.orthoSize * 2f;
                float width = height * aspect;
                Vector3 size = new Vector3(width, height, 0.1f);
                
                Gizmos.DrawWireCube(step.targetTransform.position, size);
                
                Gizmos.DrawSphere(step.targetTransform.position, 0.2f);
            }
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < cameraSteps.Count - 1; i++)
        {
            if (cameraSteps[i].targetTransform != null && cameraSteps[i + 1].targetTransform != null)
            {
                Gizmos.DrawLine(cameraSteps[i].targetTransform.position, cameraSteps[i+1].targetTransform.position);
            }
        }
    }
}
