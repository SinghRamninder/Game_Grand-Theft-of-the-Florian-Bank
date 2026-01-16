using System.Collections;
using UnityEngine;

public class SecurityOfficerScript : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private VisionCone2D visionCone;
    [SerializeField] private GameObject suspicionIcon;

    [Header("Movement")]
    [Tooltip("Control the movement speed of the guard")]
    public float speed = 2f;

    [Tooltip("Guard speed during chase")]
    public float chaseSpeed = 4f;

    [Tooltip("Guard will move between point A to point B (Enter x value)")]
    [SerializeField] private GameObject pointA;

    [Tooltip("Guard will move between point A to point B (Enter x value)")]
    [SerializeField] private GameObject pointB;

    [Tooltip("How close guard must be to target X to switch/stop")]
    [SerializeField] private float reachXThreshold = 0.05f;

    [Header("Vision")]
    [Tooltip("How far guard can see")]
    private float maxDisVisibiltiy;
    private float maxAngleVisibiltiy;
    [SerializeField] private LayerMask playerMask;

    [Header("Hearing Reaction Timings (Inspector Editable)")]
    [SerializeField] private float turnDelayAfterHearing = 1f;
    [SerializeField] private float lookDuration = 2f;

    [Header("Other")]
    public GameObject gameOverDisplay;
    [SerializeField] private float waitTimeAtTarget = 2f;
    [SerializeField] private float chaseLoseSightDuration = 2f;
    [SerializeField] private ShowKeyInstruction showKeyInstruction;

    public bool hasKey;
    public GameObject key;
    public string keyName;
    public float hearingRadius;

    [Header("Rotation Smoothness")]
    [SerializeField] private float turnDuration = 0.12f;

    private Rigidbody2D rb;
    private GameObject player;

    private Vector2 currentTarget;
    private Vector2 playerCurrentPos;

    public bool playerOutOfVision = true;
    private Animator guardAnimation;

    private Coroutine suspiciousRoutine;
    private Coroutine turnRoutine;
    private Coroutine waitRoutine;
    private Coroutine chaseLoseRoutine;

    private float currentTurnTargetY = 0f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool isWaiting = false;

    private enum GuardState { Patrol, Suspicious, Chase }
    private GuardState state = GuardState.Patrol;

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.cyan;
    //    Gizmos.DrawSphere(transform.position, 0.1f);
    //}

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        guardAnimation = GetComponent<Animator>();

        if (pointA) pointA.GetComponent<SpriteRenderer>().enabled = false;
        if (pointB) pointB.GetComponent<SpriteRenderer>().enabled = false;

        rb = GetComponent<Rigidbody2D>();

        player = GameObject.FindGameObjectWithTag("Player");

        if (pointA != null)
            currentTarget = new Vector2(pointA.transform.position.x, 0f);

        if (gameOverDisplay) gameOverDisplay.SetActive(false);
        Time.timeScale = 1f;

        if (!visionCone)
        {
            visionCone = GetComponentInChildren<VisionCone2D>();
        }

        maxDisVisibiltiy = visionCone.viewDistance;
        maxAngleVisibiltiy = visionCone.viewAngle;

        currentTurnTargetY = SnapToFacingY(transform.rotation.eulerAngles.y);

        if (suspicionIcon)
            suspicionIcon.SetActive(false);
    }

    void Update()
    {
        Vector2 disFromPlayer = player.transform.position - transform.position;

        if (disFromPlayer.magnitude > maxDisVisibiltiy)
        {
            playerOutOfVision = true;

            if (state == GuardState.Chase && chaseLoseRoutine == null)
                chaseLoseRoutine = StartCoroutine(ExitChaseAfterLoseSight());

            if (state != GuardState.Chase && visionCone != null) visionCone.SetNormal();
            return;
        }

        float yForFacing = (turnRoutine != null) ? currentTurnTargetY : transform.rotation.eulerAngles.y;
        Vector2 guardFacing = IsFacingRightFromY(yForFacing) ? Vector2.right : Vector2.left;

        if (Vector2.Angle(guardFacing, disFromPlayer) > maxAngleVisibiltiy)
        {
            playerOutOfVision = true;

            if (state == GuardState.Chase && chaseLoseRoutine == null)
                chaseLoseRoutine = StartCoroutine(ExitChaseAfterLoseSight());

            if (state != GuardState.Chase && visionCone != null) visionCone.SetNormal();
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, disFromPlayer.normalized, disFromPlayer.magnitude, playerMask);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            playerCurrentPos = new Vector2(hit.collider.transform.position.x, 0f);
            playerOutOfVision = false;

            if (chaseLoseRoutine != null)
            {
                StopCoroutine(chaseLoseRoutine);
                chaseLoseRoutine = null;
            }

            SetChase(true);

            if (visionCone != null) visionCone.SetAlert();
            Debug.DrawLine(transform.position, hit.collider.transform.position, Color.red);
        }
        else
        {
            playerOutOfVision = true;

            if (state == GuardState.Chase && chaseLoseRoutine == null)
                chaseLoseRoutine = StartCoroutine(ExitChaseAfterLoseSight());

            if (state != GuardState.Chase && visionCone != null)
                visionCone.SetNormal();
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        if (state == GuardState.Suspicious)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float moveSpeed = (state == GuardState.Chase) ? chaseSpeed : speed;
        float targetX = (state == GuardState.Chase) ? playerCurrentPos.x : currentTarget.x;

        float dx = targetX - rb.position.x;

        if (state == GuardState.Patrol)
            SmoothSetYRotation((dx > 0f) ? 180f : 0f);

        if (Mathf.Abs(dx) <= reachXThreshold && !isWaiting)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (state != GuardState.Chase)
            {
                if (guardAnimation != null) guardAnimation.SetBool("StopAnimation", true);
                if (waitRoutine != null) StopCoroutine(waitRoutine);
                waitRoutine = StartCoroutine(waitAtTarget());
                isWaiting = true;
            }
            else
            {
                if (playerOutOfVision)
                {
                    if (guardAnimation != null) guardAnimation.SetBool("StopAnimation", true);
                    StartCoroutine(GuardChaseToNormal());
                }
            }

            return;
        }

        if (isWaiting) return;

        float dir = Mathf.Sign(dx);
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    private void SwapPatrolTarget()
    {
        if (pointA == null || pointB == null) return;

        float targetX;

        if (Mathf.Approximately(currentTarget.x, pointA.transform.position.x))
        {
            targetX = pointB.transform.position.x;
            SmoothSetYRotation(180f);
        }
        else
        {
            targetX = pointA.transform.position.x;
            SmoothSetYRotation(0f);
        }

        currentTarget = new Vector2(targetX, 0f);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (gameOverDisplay) gameOverDisplay.SetActive(true);
            if (showKeyInstruction != null)
                showKeyInstruction.stopCouritineCall();
            Time.timeScale = 0f;
        }
    }

    private IEnumerator GuardChaseToNormal()
    {
        yield return new WaitForSeconds(2f);

        if (state != GuardState.Chase || !playerOutOfVision) yield break;

        SetChase(false);

        if (guardAnimation != null)
            guardAnimation.SetBool("StopAnimation", false);

        if (visionCone != null)
            visionCone.SetNormal();
    }

    private void SetChase(bool enabled)
    {
        if (enabled)
        {
            state = GuardState.Chase;

            if (waitRoutine != null)
            {
                StopCoroutine(waitRoutine);
                waitRoutine = null;
            }
            isWaiting = false;

            if (guardAnimation != null)
                guardAnimation.SetBool("StopAnimation", false);

            if (suspiciousRoutine != null)
            {
                StopCoroutine(suspiciousRoutine);
                suspiciousRoutine = null;
            }

            if (suspicionIcon) suspicionIcon.SetActive(false);
        }
        else
        {
            if (state == GuardState.Chase)
                state = GuardState.Patrol;
        }
    }

    public void HearNoise(Vector2 noisePosition)
    {
        if (state == GuardState.Chase || state == GuardState.Suspicious) return;

        if (waitRoutine != null)
        {
            StopCoroutine(waitRoutine);
            waitRoutine = null;
        }
        isWaiting = false;

        if (suspiciousRoutine != null) StopCoroutine(suspiciousRoutine);
        suspiciousRoutine = StartCoroutine(SuspiciousReaction(noisePosition));
    }

    private IEnumerator SuspiciousReaction(Vector2 noisePosition)
    {
        state = GuardState.Suspicious;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (suspicionIcon) suspicionIcon.SetActive(true);

        if (guardAnimation != null)
            guardAnimation.SetBool("StopAnimation", true);

        float originalFacingY = SnapToFacingY((turnRoutine != null) ? currentTurnTargetY : transform.rotation.eulerAngles.y);
        StopSmoothTurn();

        yield return new WaitForSeconds(turnDelayAfterHearing);

        if (state == GuardState.Chase)
        {
            suspiciousRoutine = null;
            yield break;
        }

        float dir = noisePosition.x - transform.position.x;
        SmoothSetYRotation((dir > 0f) ? 180f : 0f);

        yield return new WaitForSeconds(lookDuration);

        if (state == GuardState.Chase)
        {
            suspiciousRoutine = null;
            yield break;
        }

        SmoothSetYRotation(originalFacingY);

        if (suspicionIcon) suspicionIcon.SetActive(false);

        if (guardAnimation != null)
            guardAnimation.SetBool("StopAnimation", false);

        state = GuardState.Patrol;
        suspiciousRoutine = null;
    }

    public void ForceStopChaseToPatrol()
    {
        if (state != GuardState.Chase) return;

        if (chaseLoseRoutine != null)
        {
            StopCoroutine(chaseLoseRoutine);
            chaseLoseRoutine = null;
        }

        SetChase(false);

        if (guardAnimation != null)
            guardAnimation.SetBool("StopAnimation", false);

        if (visionCone != null)
            visionCone.SetNormal();

        playerOutOfVision = true;
    }

    public void ForceStopChaseAndTurnAround()
    {
        if (state == GuardState.Suspicious) return;

        if (suspiciousRoutine != null)
        {
            StopCoroutine(suspiciousRoutine);
            suspiciousRoutine = null;
        }

        if (chaseLoseRoutine != null)
        {
            StopCoroutine(chaseLoseRoutine);
            chaseLoseRoutine = null;
        }

        SetChase(false);

        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        float y = (turnRoutine != null) ? currentTurnTargetY : transform.rotation.eulerAngles.y;
        float snapped = SnapToFacingY(y);
        float newY = (Mathf.Abs(Mathf.DeltaAngle(snapped, 180f)) < 0.01f) ? 0f : 180f;
        SmoothSetYRotation(newY);

        if (suspicionIcon) suspicionIcon.SetActive(false);

        if (guardAnimation != null)
            guardAnimation.SetBool("StopAnimation", false);

        if (visionCone != null)
            visionCone.SetNormal();

        playerOutOfVision = true;
    }

    public void TeleportToStart()
    {
        if (suspiciousRoutine != null)
        {
            StopCoroutine(suspiciousRoutine);
            suspiciousRoutine = null;
        }

        if (waitRoutine != null)
        {
            StopCoroutine(waitRoutine);
            waitRoutine = null;
        }
        isWaiting = false;

        if (chaseLoseRoutine != null)
        {
            StopCoroutine(chaseLoseRoutine);
            chaseLoseRoutine = null;
        }

        state = GuardState.Patrol;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        StopSmoothTurn();
        transform.position = startPosition;
        transform.rotation = startRotation;
        currentTurnTargetY = SnapToFacingY(startRotation.eulerAngles.y);

        if (suspicionIcon) suspicionIcon.SetActive(false);

        if (guardAnimation != null)
            guardAnimation.SetBool("StopAnimation", false);

        if (visionCone != null)
            visionCone.SetNormal();

        playerOutOfVision = true;
    }

    private IEnumerator waitAtTarget()
    {
        yield return new WaitForSeconds(waitTimeAtTarget);

        if (state != GuardState.Patrol)
        {
            isWaiting = false;
            waitRoutine = null;
            yield break;
        }

        SwapPatrolTarget();

        if (guardAnimation != null)
            guardAnimation.SetBool("StopAnimation", false);

        isWaiting = false;
        waitRoutine = null;
    }

    private IEnumerator ExitChaseAfterLoseSight()
    {
        yield return new WaitForSeconds(chaseLoseSightDuration);

        if (state == GuardState.Chase && playerOutOfVision)
        {
            SetChase(false);

            if (guardAnimation != null)
                guardAnimation.SetBool("StopAnimation", false);

            if (visionCone != null)
                visionCone.SetNormal();
        }

        chaseLoseRoutine = null;
    }

    private void SmoothSetYRotation(float targetY)
    {
        float snappedTarget = SnapToFacingY(targetY);

        if (turnRoutine != null && Mathf.Abs(Mathf.DeltaAngle(currentTurnTargetY, snappedTarget)) < 0.01f)
            return;

        float currentY = transform.rotation.eulerAngles.y;
        currentTurnTargetY = snappedTarget;

        if (Mathf.Abs(Mathf.DeltaAngle(currentY, snappedTarget)) < 0.01f)
        {
            transform.rotation = Quaternion.Euler(0f, snappedTarget, 0f);
            return;
        }

        if (turnRoutine != null)
        {
            StopCoroutine(turnRoutine);
            visionCone.enabled = true;
            turnRoutine = null;
        }

        turnRoutine = StartCoroutine(SmoothTurnToY(snappedTarget));
    }

    private IEnumerator SmoothTurnToY(float targetY)
    {
        float startY = transform.rotation.eulerAngles.y;
        float t = 0f;
        float dur = Mathf.Max(0.0001f, turnDuration);

        visionCone.enabled = false;
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float y = Mathf.LerpAngle(startY, targetY, t);
            transform.rotation = Quaternion.Euler(0f, y, 0f);
            yield return null;
        }

        visionCone.enabled = true;
        transform.rotation = Quaternion.Euler(0f, targetY, 0f);
        turnRoutine = null;
    }

    private void StopSmoothTurn()
    {
        if (turnRoutine != null)
        {
            StopCoroutine(turnRoutine);
            visionCone.enabled = true;
            turnRoutine = null;
        }
    }

    private bool IsFacingRightFromY(float y)
    {
        float d0 = Mathf.Abs(Mathf.DeltaAngle(y, 0f));
        float d180 = Mathf.Abs(Mathf.DeltaAngle(y, 180f));
        return d180 < d0;
    }

    private float SnapToFacingY(float y)
    {
        return IsFacingRightFromY(y) ? 180f : 0f;
    }
}
