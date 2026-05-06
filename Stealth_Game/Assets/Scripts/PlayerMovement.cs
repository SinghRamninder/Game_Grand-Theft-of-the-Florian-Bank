using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speeds")]
    [Tooltip("The default movement speed of the player while walking.")]
    public float normalSpeed = 3f;
    [Tooltip("The speed of the player when sprinting (holding Left Shift).")]
    public float sprintSpeed = 5f;
    [Tooltip("The speed of the player when sneaking or crouching (holding Left Control).")]
    public float sneakSpeed = 1.5f;

    private Animator playerAnimation;

    private float moveSpeed;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private float inputX;

    private bool externalMoveActive = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        playerAnimation = GetComponent<Animator>();

        moveSpeed = normalSpeed;
    }

    void Update()
    {
        if (externalMoveActive)
            return;

        inputX = Input.GetAxisRaw("Horizontal");
        movementInput = new Vector2(inputX, 0f).normalized;

        if (inputX > 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (inputX < 0)
            transform.rotation = Quaternion.Euler(0, 0, 0);

        if (Input.GetKeyDown(KeyCode.LeftShift))
            moveSpeed = sprintSpeed;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            moveSpeed = normalSpeed;

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            moveSpeed = sneakSpeed;
            playerAnimation.SetBool("Crouch", true);
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            moveSpeed = normalSpeed;
            playerAnimation.SetBool("Crouch", false);
        }
    }

    void FixedUpdate()
    {
        if (externalMoveActive)
            return;

        Vector2 v = rb.linearVelocity;

        v.x = movementInput.x * moveSpeed;

        if (rb.linearVelocity.x != 0)
            playerAnimation.SetBool("Walk", true);
        else
            playerAnimation.SetBool("Walk", false);

        rb.linearVelocity = v;
    }

    // =========================================================
    // NEW: External movement API (SAFE & MINIMAL)
    // =========================================================

    /// <summary>
    /// Moves the player slightly without touching input logic.
    /// </summary>
    /// <param name="direction">Vector2.left or Vector2.right</param>
    /// <param name="distance">World units to move</param>
    /// <param name="duration">Time in seconds</param>
    public void MoveExternally(Vector2 direction, float distance, float duration = 0.2f)
    {
        if (!gameObject.activeInHierarchy)
            return;

        StartCoroutine(ExternalMoveRoutine(direction.normalized, distance, duration));
    }

    private IEnumerator ExternalMoveRoutine(Vector2 direction, float distance, float duration)
    {
        externalMoveActive = true;

        float moved = 0f;
        float speed = distance / duration;

        // Optional: face direction
        if (direction.x > 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (direction.x < 0)
            transform.rotation = Quaternion.Euler(0, 0, 0);

        while (moved < distance)
        {
            float step = speed * Time.deltaTime;
            rb.MovePosition(rb.position + direction * step);
            moved += step;
            yield return null;
        }

        // Stop residual velocity
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        externalMoveActive = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (CheckPoint.instance != null)
        {
            if (collision.CompareTag("Checkpoint1"))
            {
                CheckPoint.instance.checkPoint1Exists(collision.transform);
            }
            if (collision.CompareTag("Checkpoint2"))
            {
                CheckPoint.instance.checkPoint2Exists(collision.transform);
            }
        }
    }
}
