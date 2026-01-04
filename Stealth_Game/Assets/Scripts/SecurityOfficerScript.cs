using System.Collections;
using UnityEngine;

public class SecurityOfficerScript : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private VisionCone2D visionCone;
    [SerializeField] private GameObject suspicionIcon;

    [Header("Movement")]
    [Tooltip("Control the movement speed of the guard")]
    [SerializeField] private float speed = 2f;

    [Tooltip("Guard speed during chase")]
    [SerializeField] private float chaseSpeed = 4f;

    [Tooltip("Guard will move between point A to point B (Enter x value)")]
    [SerializeField] private GameObject pointA;

    [Tooltip("Guard will move between point A to point B (Enter x value)")]
    [SerializeField] private GameObject pointB;

    [Header("Vision")]
    [Tooltip("How far guard can see")]
    [SerializeField] private float maxVisibiltiy = 6f;
    [SerializeField] private LayerMask playerMask;

    [Header("Hearing Reaction Timings (Inspector Editable)")]
    [SerializeField] private float turnDelayAfterHearing = 1f; // wait before turning
    [SerializeField] private float lookDuration = 2f;          // stay turned

    [Header("Other")]
    [SerializeField] private GameObject gameOverDisplay;

    // Your existing public fields (kept so other scripts don't break)
    public bool hasKey;
    public GameObject key;
    public string keyName;
    public float hearingRadius;

    private Rigidbody2D rb;
    private Rigidbody2D playerRb;
    private GameObject player;

    private Vector2 currentTarget;
    private Vector2 playerCurrentPos;

    private bool playerOutOfVision = true;
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
        playerRb = player ? player.GetComponent<Rigidbody2D>() : null;

        if (pointA != null)
            currentTarget = new Vector2(pointA.transform.position.x, rb.position.y);

        if (gameOverDisplay) gameOverDisplay.SetActive(false);
        Time.timeScale = 1f;

        if (!visionCone)
            visionCone = GetComponentInChildren<VisionCone2D>();

        if (suspicionIcon)
            suspicionIcon.SetActive(false);
    }

    void Update()
    {
        // Facing logic same style as your original:
        // y == 0 -> left, y == 180 -> right
        Vector2 direction = Vector2.right;
        if (Mathf.Approximately(transform.rotation.eulerAngles.y, 0f)) direction = Vector2.left;
        else if (Mathf.Abs(transform.rotation.eulerAngles.y) >= 179f) direction = Vector2.right;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxVisibiltiy, playerMask);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            playerCurrentPos = new Vector2(hit.collider.transform.position.x, transform.position.y);
            playerOutOfVision = false;

            // Switch to chase (this also cancels suspicious cleanly)
            SetChase(true);

            if (visionCone != null) visionCone.SetAlert();
            Debug.DrawLine(transform.position, hit.collider.transform.position, Color.red);
        }
        else
        {
            playerOutOfVision = true;

            // Only normal cone if NOT chasing
            if (state != GuardState.Chase && visionCone != null)
                visionCone.SetNormal();
        }
    }

    private void FixedUpdate()
    {
        // Freeze movement during suspicious (prevents the �walk somewhere then rotate� weirdness)
        if (state == GuardState.Suspicious)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            return;
        }

        if (state != GuardState.Chase)
        {
            // PATROL
            Vector2 newPos = Vector2.MoveTowards(rb.position, currentTarget, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            if (Vector2.Distance(rb.position, currentTarget) < 0.05f)
            {
                float targetX;

                if (pointA != null && Mathf.Approximately(currentTarget.x, pointA.transform.position.x))
                {
                    targetX = pointB.transform.position.x;
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                }
                else
                {
                    targetX = pointA.transform.position.x;
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                }

                currentTarget = new Vector2(targetX, rb.position.y);
            }
        }
        else
        {
            // CHASE
            Vector2 newPos = Vector2.MoveTowards(rb.position, playerCurrentPos, chaseSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            if (Vector2.Distance(rb.position, playerCurrentPos) < 0.05f && playerOutOfVision)
            {
                bullAnimation.SetBool("StopAnimation", true);
                StartCoroutine(GuardChaseToNormal());
            }
        }
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
        bullAnimation.SetBool("StopAnimation", false);

        if (visionCone != null)
            visionCone.SetNormal();
    }

    private void SetChase(bool enabled)
    {
        if (enabled)
        {
            state = GuardState.Chase;

            // IMPORTANT FIX:
            // If chase starts while we were suspicious, make sure animation is not stuck paused
            if (bullAnimation != null)
                bullAnimation.SetBool("StopAnimation", false);

            // Cancel suspicious if running
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

    // CALLED BY YOUR NOISE RING SCRIPT
    public void HearNoise(Vector2 noisePosition)
    {
        // Don�t interrupt chase
        if (state == GuardState.Chase) return;

        // Restart suspicious cleanly
        if (suspiciousRoutine != null) StopCoroutine(suspiciousRoutine);
        suspiciousRoutine = StartCoroutine(SuspiciousReaction(noisePosition));
    }

    private IEnumerator SuspiciousReaction(Vector2 noisePosition)
    {
        state = GuardState.Suspicious;

        if (suspicionIcon) suspicionIcon.SetActive(true);

        // Pause walk animation while suspicious
        if (bullAnimation != null)
            bullAnimation.SetBool("StopAnimation", true);

        Quaternion originalRot = transform.rotation;

        // Wait before turning
        yield return new WaitForSeconds(turnDelayAfterHearing);

        // If during the wait we started chasing, stop this routine
        if (state == GuardState.Chase)
        {
            suspiciousRoutine = null;
            yield break;
        }

        // Turn toward noise
        float dir = noisePosition.x - transform.position.x;
        transform.rotation = (dir > 0f) ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);

        // Stay turned
        yield return new WaitForSeconds(lookDuration);

        // If we started chasing while looking, do NOT restore rotation or icon
        if (state == GuardState.Chase)
        {
            suspiciousRoutine = null;
            yield break;
        }

        // Restore
        transform.rotation = originalRot;

        if (suspicionIcon) suspicionIcon.SetActive(false);

        // Resume patrol animation
        if (bullAnimation != null)
            bullAnimation.SetBool("StopAnimation", false);

        state = GuardState.Patrol;
        suspiciousRoutine = null;
    }
}
