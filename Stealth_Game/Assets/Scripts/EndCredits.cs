using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndCredits : MonoBehaviour
{
    [System.Serializable]
    public class CreditStep
    {
        public TMP_Text text;

        [Header("Timing")]
        public float delayBefore = 0.5f; // wait before this text appears
        public float holdTime = 2f;      // how long it stays visible
    }

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool enableDebugGizmos = true;
    [SerializeField] private bool logEachFrameCameraProgress = false;
    [SerializeField] private float cameraLogInterval = 0.25f;

    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnce = true;
    private bool triggered;

    [Header("Player Auto Move")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Vector2 moveDirection = Vector2.right;
    [SerializeField] private float moveDistance = 2f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Camera Move + Zoom")]
    [SerializeField] private Camera targetCamera; // if null -> Camera.main
    [SerializeField] private Vector3 cameraTargetPosition = new Vector3(0f, 0f, -10f);
    [SerializeField] private float cameraMoveSpeed = 5f;
    [SerializeField] private float cameraTargetOrthoSize = 4f;
    [SerializeField] private float cameraZoomSpeed = 5f;
    [SerializeField] private float cameraReachPosThreshold = 0.01f;
    [SerializeField] private float cameraReachZoomThreshold = 0.01f;

    [Header("Black Fade Image")]
    [Tooltip("Fullscreen black UI Image. We'll animate its alpha.")]
    [SerializeField] private Image blackFadeImage;
    [SerializeField] private float startCreditsDelay = 1f;
    [SerializeField] private float blackFadeInDuration = 0.8f;

    [Header("Credits (scene objects)")]
    [Tooltip("Drag your credit TMP Text objects here in order, and set delay/hold per line.")]
    [SerializeField] private CreditStep[] credits;

    [Tooltip("Fade duration for each text (fade in and fade out).")]
    [SerializeField] private float textFadeDuration = 0.4f;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

    [Header("End")]
    [SerializeField] private float waitBeforeSceneLoad = 2f;
    [SerializeField] private string sceneToLoad = "Start";

    private CinemachineBrain brain;

    // Debug helpers
    private float lastCamLogTime = -999f;

    private void DLog(string msg)
    {
        if (!enableDebugLogs) return;
        Debug.Log($"[EndCredits] {msg}", this);
    }

    private void DWarn(string msg)
    {
        if (!enableDebugLogs) return;
        Debug.LogWarning($"[EndCredits] {msg}", this);
    }

    private void DErr(string msg)
    {
        if (!enableDebugLogs) return;
        Debug.LogError($"[EndCredits] {msg}", this);
    }

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
            brain = targetCamera.GetComponent<CinemachineBrain>();

        DLog("Awake() started.");

        // Validate refs
        if (targetCamera == null) DWarn("Target Camera is NULL (Camera.main not found).");
        if (brain == null) DWarn("CinemachineBrain is NULL on targetCamera (if you're not using Cinemachine, this is fine).");
        if (audioManager == null) DWarn("AudioManager is NULL (audio calls will be skipped).");

        // Ensure black fade starts transparent
        if (blackFadeImage != null)
        {
            Color c = blackFadeImage.color;
            blackFadeImage.color = new Color(c.r, c.g, c.b, 0f);
            blackFadeImage.gameObject.SetActive(true);

            var canvas = blackFadeImage.GetComponentInParent<Canvas>();
            if (canvas != null)
                DLog($"BlackFadeImage canvas: '{canvas.name}', renderMode={canvas.renderMode}, sortingOrder={canvas.sortingOrder}");
            else
                DWarn("BlackFadeImage has NO parent Canvas.");

            DLog($"BlackFadeImage initial alpha set to 0. Current alpha={blackFadeImage.color.a}");
        }
        else
        {
            DWarn("BlackFadeImage is NULL (no black fade will be shown).");
        }

        // Hide all credit texts
        if (credits != null)
        {
            DLog($"Credits array size: {credits.Length}");

            for (int i = 0; i < credits.Length; i++)
            {
                var step = credits[i];
                if (step == null)
                {
                    DWarn($"Credits[{i}] is NULL.");
                    continue;
                }

                if (step.text == null)
                {
                    DWarn($"Credits[{i}] text is NULL. (Drag a TMP_Text into this slot)");
                    continue;
                }

                step.text.alpha = 0f;
                step.text.gameObject.SetActive(false);

                var canvas = step.text.GetComponentInParent<Canvas>();
                DLog($"Credits[{i}] text='{step.text.name}', hidden. ParentCanvas='{(canvas ? canvas.name : "NONE")}'");
            }
        }
        else
        {
            DWarn("Credits array is NULL.");
        }

        // Player refs
        if (player == null) DWarn("Player Transform is NULL (will try to auto-fill on trigger).");
        if (playerMovement == null) DWarn("PlayerMovement is NULL (will try to auto-fill on trigger).");

        DLog("Awake() finished.");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DLog($"OnTriggerEnter2D by '{collision.name}', tag='{collision.tag}'");

        if (triggerOnce && triggered)
        {
            DLog("Trigger ignored: already triggered and Trigger Once is ON.");
            return;
        }

        if (!string.IsNullOrEmpty(playerTag) && !collision.CompareTag(playerTag))
        {
            DLog($"Trigger ignored: collider tag '{collision.tag}' != required '{playerTag}'.");
            return;
        }

        if (player == null) player = collision.transform;
        if (playerMovement == null) playerMovement = collision.GetComponent<PlayerMovement>();

        if (player == null) DWarn("Auto-fill player failed (player still NULL).");
        if (playerMovement == null) DWarn("Auto-fill PlayerMovement failed (playerMovement still NULL).");

        Trigger();
    }

    public void Trigger()
    {
        if (triggerOnce && triggered)
        {
            DLog("Trigger() ignored: already triggered and Trigger Once is ON.");
            return;
        }

        triggered = true;
        DLog("Trigger() STARTED.");

        if (brain != null)
        {
            brain.enabled = false;
            DLog("CinemachineBrain disabled.");
        }
        else
        {
            DWarn("CinemachineBrain not found. Skipping disable.");
        }

        if (audioManager != null)
        {
            DLog("Audio: PlayEndCredits + StopSFX");
            audioManager.PlayEndCredits(1f);
            audioManager.StopSFX();
        }

        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        DLog("Sequence() START.");

        Coroutine movePlayerCo = StartCoroutine(MovePlayerThenDisableInput());
        Coroutine moveCameraCo = StartCoroutine(MoveAndZoomCamera());

        DLog("Waiting for camera move to complete...");
        if (moveCameraCo != null)
            yield return moveCameraCo;
        DLog("Camera move COMPLETE.");

        DLog("Waiting for player move to complete...");
        if (movePlayerCo != null)
            yield return movePlayerCo;
        DLog("Player move COMPLETE.");

        if (startCreditsDelay > 0f)
        {
            DLog($"StartCreditsDelay: waiting {startCreditsDelay}s");
            yield return new WaitForSeconds(startCreditsDelay);
        }

        if (blackFadeImage != null)
        {
            DLog($"Black fade IN: duration {blackFadeInDuration}s (0 -> 1)");
            yield return StartCoroutine(FadeImageAlpha(blackFadeImage, 0f, 1f, blackFadeInDuration));
            DLog($"Black fade IN done. Current alpha={blackFadeImage.color.a}");
        }
        else
        {
            DWarn("BlackFadeImage is NULL, skipping fade.");
        }

        // Credits
        if (credits != null && credits.Length > 0)
        {
            DLog("Starting credits loop...");

            for (int i = 0; i < credits.Length; i++)
            {
                CreditStep step = credits[i];

                if (step == null)
                {
                    DWarn($"Credits[{i}] step is NULL, skipping.");
                    continue;
                }

                if (step.text == null)
                {
                    DWarn($"Credits[{i}] text is NULL, skipping.");
                    continue;
                }

                DLog($"Credits[{i}] '{step.text.name}' delayBefore={step.delayBefore}, holdTime={step.holdTime}");

                if (step.delayBefore > 0f)
                    yield return new WaitForSeconds(step.delayBefore);

                step.text.gameObject.SetActive(true);

                DLog($"Credits[{i}] fade IN start (duration {textFadeDuration}s)");
                yield return StartCoroutine(FadeTMPAlpha(step.text, 0f, 1f, textFadeDuration));
                DLog($"Credits[{i}] fade IN done. alpha={step.text.alpha}");

                if (step.holdTime > 0f)
                    yield return new WaitForSeconds(step.holdTime);

                DLog($"Credits[{i}] fade OUT start (duration {textFadeDuration}s)");
                yield return StartCoroutine(FadeTMPAlpha(step.text, 1f, 0f, textFadeDuration));
                DLog($"Credits[{i}] fade OUT done. alpha={step.text.alpha}");

                step.text.gameObject.SetActive(false);
            }

            DLog("Credits loop DONE.");
        }
        else
        {
            DWarn("Credits list is empty or NULL. No texts will show.");
        }

        // NEW END: keep black screen, wait, then load scene
        DLog($"Credits finished. Waiting {waitBeforeSceneLoad}s then loading scene '{sceneToLoad}'...");

        if (waitBeforeSceneLoad > 0f)
            yield return new WaitForSeconds(waitBeforeSceneLoad);

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            DErr("sceneToLoad is empty. Not loading any scene.");
            yield break;
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator MovePlayerThenDisableInput()
    {
        if (player == null)
        {
            DWarn("MovePlayerThenDisableInput: player is NULL. Skipping.");
            yield break;
        }

        DLog($"Player auto move: dir={moveDirection}, distance={moveDistance}, speed={moveSpeed}");

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            DLog("PlayerMovement disabled for auto-move.");
        }
        else
        {
            DWarn("PlayerMovement is NULL. Auto-move will still move Transform, but input won't be disabled by this script.");
        }

        Vector2 dir = moveDirection.sqrMagnitude > 0.0001f ? moveDirection.normalized : Vector2.right;
        Vector2 startPos = player.position;
        Vector2 targetPos = startPos + dir * Mathf.Max(0f, moveDistance);

        DLog($"Player startPos={startPos}, targetPos={targetPos}");

        while (Vector2.Distance(player.position, targetPos) > 0.01f)
        {
            Vector2 newPos = Vector2.MoveTowards(player.position, targetPos, moveSpeed * Time.deltaTime);
            player.position = new Vector3(newPos.x, newPos.y, player.position.z);
            yield return null;
        }

        if (playerMovement != null)
            playerMovement.enabled = false;

        DLog("Player auto move COMPLETE. PlayerMovement remains disabled.");
    }

    private IEnumerator MoveAndZoomCamera()
    {
        if (targetCamera == null)
        {
            DWarn("MoveAndZoomCamera: targetCamera is NULL. Skipping.");
            yield break;
        }

        Transform camT = targetCamera.transform;
        Vector3 targetPos = cameraTargetPosition;

        DLog($"Camera move: fromPos={camT.position}, toPos={targetPos}, moveSpeed={cameraMoveSpeed}");
        DLog($"Camera zoom: fromOrtho={targetCamera.orthographicSize}, toOrtho={cameraTargetOrthoSize}, zoomSpeed={cameraZoomSpeed}");

        while (Vector3.Distance(camT.position, targetPos) > cameraReachPosThreshold ||
               Mathf.Abs(targetCamera.orthographicSize - cameraTargetOrthoSize) > cameraReachZoomThreshold)
        {
            camT.position = Vector3.MoveTowards(camT.position, targetPos, cameraMoveSpeed * Time.deltaTime);

            targetCamera.orthographicSize = Mathf.MoveTowards(
                targetCamera.orthographicSize,
                cameraTargetOrthoSize,
                cameraZoomSpeed * Time.deltaTime
            );

            if (logEachFrameCameraProgress && Time.time - lastCamLogTime >= cameraLogInterval)
            {
                lastCamLogTime = Time.time;

                float posDist = Vector3.Distance(camT.position, targetPos);
                float zoomDist = Mathf.Abs(targetCamera.orthographicSize - cameraTargetOrthoSize);

                DLog($"Camera progress: posDist={posDist:F3}, zoomDist={zoomDist:F3}, camPos={camT.position}, ortho={targetCamera.orthographicSize:F3}");
            }

            yield return null;
        }

        DLog($"Camera reached target. Final pos={camT.position}, ortho={targetCamera.orthographicSize:F3}");
    }

    private IEnumerator FadeImageAlpha(Image img, float from, float to, float duration)
    {
        if (img == null)
        {
            DWarn("FadeImageAlpha: img is NULL.");
            yield break;
        }

        if (!img.gameObject.activeInHierarchy)
            DWarn($"FadeImageAlpha: '{img.name}' is inactive in hierarchy (won't be visible).");

        var canvas = img.GetComponentInParent<Canvas>();
        if (canvas == null)
            DWarn($"FadeImageAlpha: '{img.name}' has no parent Canvas.");
        else
            DLog($"FadeImageAlpha: '{img.name}' parentCanvas='{canvas.name}', renderMode={canvas.renderMode}, sortingOrder={canvas.sortingOrder}");

        Color c = img.color;

        if (duration <= 0f)
        {
            img.color = new Color(c.r, c.g, c.b, to);
            DLog($"FadeImageAlpha immediate set: '{img.name}' alpha={to}");
            yield break;
        }

        float t = 0f;
        img.color = new Color(c.r, c.g, c.b, from);
        DLog($"FadeImageAlpha start: '{img.name}' from={from} to={to} duration={duration}");

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            img.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        img.color = new Color(c.r, c.g, c.b, to);
        DLog($"FadeImageAlpha done: '{img.name}' alpha={img.color.a}");
    }

    private IEnumerator FadeTMPAlpha(TMP_Text tmp, float from, float to, float duration)
    {
        if (tmp == null)
        {
            DWarn("FadeTMPAlpha: tmp is NULL.");
            yield break;
        }

        if (duration <= 0f)
        {
            tmp.alpha = to;
            yield break;
        }

        float t = 0f;
        tmp.alpha = from;

        while (t < duration)
        {
            t += Time.deltaTime;
            tmp.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }

        tmp.alpha = to;
    }

    private void OnDrawGizmosSelected()
    {
        if (!enableDebugGizmos) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(cameraTargetPosition, 0.4f);

        if (player != null)
        {
            Vector3 start = player.position;
            Vector2 dir = moveDirection.sqrMagnitude > 0.0001f ? moveDirection.normalized : Vector2.right;
            Vector3 end = start + (Vector3)(dir * Mathf.Max(0f, moveDistance));

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, 0.25f);
        }
    }
}
