using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject player;
    
    [Tooltip("Control the speed of player")]
    [SerializeField] private float playerSpeed = 3f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
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
    }
}
