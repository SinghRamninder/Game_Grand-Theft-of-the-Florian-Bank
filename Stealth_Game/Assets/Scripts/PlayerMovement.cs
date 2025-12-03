using UnityEngine;

public class PlayerMovement : MonoBehaviour
{   
    [Tooltip("Control the speed of player")]
    [SerializeField] private float normalSpeed = 3f;

    [Tooltip("Player Speed while sprinting")]
    [SerializeField] private float sprintSpeed;

    [Tooltip("Player Speed while sneaking")]
    [SerializeField] private float sneakSpeed;

    private float playerSpeed;

    void Start()
    {
        playerSpeed = normalSpeed;
    }

    void Update()
    {
        float distance = Input.GetAxisRaw("Horizontal");
        transform.Translate(new Vector3 (distance,0f,0f) * playerSpeed * Time.deltaTime,Space.World);

        if (distance > 0)
        {
            transform.rotation = Quaternion.Euler(0,180,0);
            //For animation//
        }
        else if (distance < 0)
        {
            transform.rotation = Quaternion.Euler(0,0,0);
            //For animation//
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            playerSpeed = sprintSpeed;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            playerSpeed = normalSpeed;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            playerSpeed = sneakSpeed;
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            playerSpeed = normalSpeed;
        }
    }
}
