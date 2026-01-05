using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speeds")]
    public float normalSpeed = 3f;
    [SerializeField] private float sprintSpeed = 5f;
    [SerializeField] private float sneakSpeed = 1.5f;
    [SerializeField] private Animator playerAnimation;

    private float moveSpeed;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private float inputX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        playerAnimation = GetComponent<Animator>();

        moveSpeed = normalSpeed;
    }

    void Update()
    {
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
            moveSpeed = sneakSpeed;
        if (Input.GetKeyUp(KeyCode.LeftControl))
            moveSpeed = normalSpeed;
    }

    void FixedUpdate()
    {
        Vector2 v = rb.linearVelocity;

        v.x = movementInput.x * moveSpeed;

        if (rb.linearVelocity.x != 0)
        {
            playerAnimation.SetBool("Walk", true);
        }
        else
        {
            playerAnimation.SetBool("Walk", false);
        }

            rb.linearVelocity = v;
    }
}
