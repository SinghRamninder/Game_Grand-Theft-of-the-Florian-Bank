using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CameraMovementStep
    {
        [Tooltip("The transform representing the position to move the camera to. Create an empty game object as a child to adjust.")]
        public Transform targetTransform;

        [Tooltip("The orthographic size of the camera for this step.")]
        public float orthoSize = 5f;

        [Tooltip("How long it takes to move to this position in seconds.")]
        public float moveDuration = 1.5f;

        [Tooltip("Optional: if higher than 0, the Y axis position moves first for this duration, then X axis and zoom begin.")]
        public float yOnlyTime = 0f;

        [Tooltip("How many seconds to wait at this position before continuing.")]
        public float waitTime = 1f;
    }

    [Header("Camera Movements")]
    [Tooltip("Add steps here to structure your end cutscene.")]
    public List<CameraMovementStep> cameraSteps = new List<CameraMovementStep>();

    [Header("Player Auto Move")]
    [Tooltip("If checked, the player will start moving after the camera steps have finished.")]
    [HideInInspector] public bool EnablePlayerMovement = true;
    [HideInInspector] public Transform player;
    [HideInInspector] public PlayerMovement playerMovement;
    public Vector2 moveDirection = Vector2.right;
    public float moveDistance = 2f;
    public float moveSpeed = 2f;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;

        if (player == null)
            player = GameObject.FindFirstObjectByType<PlayerMovement>().transform;

        if (playerMovement == null)
            playerMovement = GameObject.FindFirstObjectByType<PlayerMovement>();
    }

    public IEnumerator PlayCutsceneSequence()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) yield break;

        StartCoroutine(StartPlayerMovement());

        foreach (var step in cameraSteps)
        {
            if (step.targetTransform == null) continue;

            Vector3 startPos = cam.transform.position;
            Vector3 endPos = new Vector3(step.targetTransform.position.x, step.targetTransform.position.y, startPos.z);
            float startOrtho = cam.orthographicSize;
            float endOrtho = step.orthoSize;

            float yOnlyTime = Mathf.Clamp(step.yOnlyTime, 0f, step.moveDuration);
            float xyAndZoomTime = step.moveDuration - yOnlyTime;
            float t = 0f;

            // Y only move
            if (yOnlyTime > 0f)
            {
                while (t < yOnlyTime)
                {
                    t += Time.deltaTime;
                    float frac = Mathf.Clamp01(t / yOnlyTime);
                    frac = Smooth01(frac);
                    float curY = Mathf.Lerp(startPos.y, endPos.y, frac);
                    cam.transform.position = new Vector3(startPos.x, curY, startPos.z);
                    yield return null;
                }
            }

            // X, final Y, and Zoom
            t = 0f;
            Vector3 intermediateStartPos = cam.transform.position;
            while (t < xyAndZoomTime)
            {
                t += Time.deltaTime;
                float frac = Mathf.Clamp01(t / xyAndZoomTime);
                frac = Smooth01(frac);

                float curX = Mathf.Lerp(intermediateStartPos.x, endPos.x, frac);
                float curY = Mathf.Lerp(intermediateStartPos.y, endPos.y, frac);
                cam.transform.position = new Vector3(curX, curY, startPos.z);
                cam.orthographicSize = Mathf.Lerp(startOrtho, endOrtho, frac);

                yield return null;
            }

            cam.transform.position = endPos;
            cam.orthographicSize = endOrtho;

            if (step.waitTime > 0f)
            {
                yield return new WaitForSeconds(step.waitTime);
            }
        }
    }

    private IEnumerator StartPlayerMovement()
    {
        if (player == null)
        {
            yield break;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        Vector2 dir = moveDirection.sqrMagnitude > 0.0001f ? moveDirection.normalized : Vector2.right;
        Vector2 startPos = player.position;
        Vector2 targetPos = startPos + dir * Mathf.Max(0f, moveDistance);

        while (Vector2.Distance(player.position, targetPos) > 0.01f)
        {
            Vector2 newPos = Vector2.MoveTowards(player.position, targetPos, moveSpeed * Time.deltaTime);
            player.position = new Vector3(newPos.x, newPos.y, player.position.z);
            yield return null;
        }

        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    private float Smooth01(float t)
    {
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
                Gizmos.DrawLine(cameraSteps[i].targetTransform.position, cameraSteps[i + 1].targetTransform.position);
            }
        }

        if (EnablePlayerMovement)
        {
            Transform pRef = player;
            if (pRef == null)
            {
                GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
                if (taggedPlayer != null) pRef = taggedPlayer.transform;
            }

            if (pRef != null)
            {
                Vector3 start = pRef.position;
                Vector2 dir = moveDirection.sqrMagnitude > 0.0001f ? moveDirection.normalized : Vector2.right;
                Vector3 end = start + (Vector3)(dir * Mathf.Max(0f, moveDistance));

                Gizmos.color = Color.red;
                Gizmos.DrawLine(start, end);
                Gizmos.DrawWireSphere(end, 0.25f);
            }
        }
    }
}
