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
    [SerializeField] private GameObject gameOverDisplay;

    public bool hasKey;
    public GameObject key;
    public string keyName;
    public float hearingRadius;

    private Rigidbody2D rb;
    private GameObject player;

    private Vector2 currentTarget;
    private Vector2 playerCurrentPos;

    public bool playerOutOfVision = true;
    private Animator bullAnimation;

    private Coroutine suspiciousRoutine;

    private enum GuardState { Patrol, Suspicious, Chase }
    private GuardState state = GuardState.Patrol;

    void Start()
    {
        bullAnimation = GetComponent<Animator>();

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
            maxDisVisibiltiy = visionCone.viewDistance;
            maxAngleVisibiltiy = visionCone.viewAngle;
        }

        if (suspicionIcon)
            suspicionIcon.SetActive(false);
    }

    void Update()
    {
        Vector2 disFromPlayer = player.transform.position - transform.position;
        if (disFromPlayer.magnitude > maxDisVisibiltiy) return;

        Vector2 guardFacing = Vector2.right;

        if (Mathf.Approximately(transform.rotation.eulerAngles.y, 0f))
            guardFacing = Vector2.left;
        else if (Mathf.Abs(transform.rotation.eulerAngles.y) >= 179f)
            guardFacing = Vector2.right;

        if (Vector2.Angle(guardFacing, disFromPlayer) > maxAngleVisibiltiy) return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, disFromPlayer.normalized, disFromPlayer.magnitude, playerMask);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            playerCurrentPos = new Vector2(hit.collider.transform.position.x, 0f);
            playerOutOfVision = false;

            SetChase(true);

            if (visionCone != null) visionCone.SetAlert();
            Debug.DrawLine(transform.position, hit.collider.transform.position, Color.red);
        }
        else
        {
            playerOutOfVision = true;

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
            transform.rotation = (dx > 0f) ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);

        if (Mathf.Abs(dx) <= reachXThreshold)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (state != GuardState.Chase)
            {
                SwapPatrolTarget();
            }
            else
            {
                if (playerOutOfVision)
                {
                    if (bullAnimation != null) bullAnimation.SetBool("StopAnimation", true);
                    StartCoroutine(GuardChaseToNormal());
                }
            }

            return;
        }

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
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            targetX = pointA.transform.position.x;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        currentTarget = new Vector2(targetX, 0f);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (gameOverDisplay) gameOverDisplay.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private IEnumerator GuardChaseToNormal()
    {
        yield return new WaitForSeconds(2f);

        SetChase(false);

        if (bullAnimation != null)
            bullAnimation.SetBool("StopAnimation", false);

        if (visionCone != null)
            visionCone.SetNormal();
    }

    private void SetChase(bool enabled)
    {
        if (enabled)
        {
            state = GuardState.Chase;

            if (bullAnimation != null)
                bullAnimation.SetBool("StopAnimation", false);

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

        if (suspiciousRoutine != null) StopCoroutine(suspiciousRoutine);
        suspiciousRoutine = StartCoroutine(SuspiciousReaction(noisePosition));
    }

    private IEnumerator SuspiciousReaction(Vector2 noisePosition)
    {
        state = GuardState.Suspicious;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (suspicionIcon) suspicionIcon.SetActive(true);

        if (bullAnimation != null)
            bullAnimation.SetBool("StopAnimation", true);

        Quaternion originalRot = transform.rotation;

        yield return new WaitForSeconds(turnDelayAfterHearing);

        if (state == GuardState.Chase)
        {
            suspiciousRoutine = null;
            yield break;
        }

        float dir = noisePosition.x - transform.position.x;
        transform.rotation = (dir > 0f) ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);

        yield return new WaitForSeconds(lookDuration);

        if (state == GuardState.Chase)
        {
            suspiciousRoutine = null;
            yield break;
        }

        transform.rotation = originalRot;

        if (suspicionIcon) suspicionIcon.SetActive(false);

        if (bullAnimation != null)
            bullAnimation.SetBool("StopAnimation", false);

        state = GuardState.Patrol;
        suspiciousRoutine = null;
    }

    public void ForceStopChaseToPatrol()
    {
        if (state != GuardState.Chase) return;

        SetChase(false);

        if (bullAnimation != null)
            bullAnimation.SetBool("StopAnimation", false);

        if (visionCone != null)
            visionCone.SetNormal();

        playerOutOfVision = true;
    }
}
