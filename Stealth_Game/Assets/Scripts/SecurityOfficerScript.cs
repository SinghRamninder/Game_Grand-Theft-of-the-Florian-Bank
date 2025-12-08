using System.Collections;
using UnityEngine;

public class SecurityOfficerScript : MonoBehaviour
{
    [Tooltip("Control the movement speed of the guard")]
    [SerializeField] private float speed;

    [Tooltip("Guard speed during chase")]
    [SerializeField] private float chaseSpeed;

    [Tooltip("Guard will move between point A to point B (Enter x value)")]
    [SerializeField] private GameObject pointA;
    [Tooltip("Guard will move between point A to point B (Enter x value)")]
    [SerializeField] private GameObject pointB;

    [Tooltip("How far guard can see")]
    [SerializeField] private float maxVisibiltiy;

    [SerializeField] private float hearingRadius;

    [SerializeField] private float maxQuiteSpeed;
    [SerializeField] private float minQuiteSpeed;

    private float currentQuiteSpeed;

    [SerializeField] private LayerMask playerMask;

    [SerializeField] private GameObject player;

    private Rigidbody2D rb;
    private Rigidbody2D playerRb;
    private Vector2 currentTarget;
    private bool chasePlayer = false;
    private bool playerOutOfVision = true;
    private Vector2 playerCurrentPos;
    private SpriteRenderer sr;

    void Start()
    {
        pointA.GetComponent<SpriteRenderer>().enabled = false;
        pointB.GetComponent<SpriteRenderer>().enabled = false;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        player = GameObject.FindGameObjectWithTag("Player");
        playerRb = player.GetComponent<Rigidbody2D>();

        currentTarget = new Vector2(pointA.transform.position.x, rb.position.y);
    }


    void Update()
    {
        Vector2 direction = new Vector2(1,0);
        if (transform.rotation.eulerAngles.y == 0) direction = Vector2.left;
        if (Mathf.Abs(transform.rotation.eulerAngles.y) == 180) direction = Vector2.right;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxVisibiltiy, playerMask);

        if (hit.collider!= null && hit.collider.CompareTag("Player"))
        {
            playerCurrentPos = new Vector2(hit.collider.transform.position.x, transform.position.y);
            chasePlayer = true;
            playerOutOfVision = false;
            GetComponent<SpriteRenderer>().color = Color.red;
            Debug.DrawLine(transform.position, hit.collider.transform.position, Color.red);
        }
        else
        {
            playerOutOfVision = true;
        }

        HandleHearing();
    }

    private void FixedUpdate()
    {
        if (!chasePlayer)
        {
            Vector2 newPos = Vector2.MoveTowards(rb.position, currentTarget, speed * Time.fixedDeltaTime);

            rb.MovePosition(newPos);

            float targetX;
            if (Vector2.Distance(rb.position, currentTarget) < 0.05f)
            {
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

                currentTarget = new Vector2(targetX, rb.position.y);
            }
        }
        else
        {
            Vector2 newPos = Vector2.MoveTowards(rb.position, playerCurrentPos, chaseSpeed * Time.fixedDeltaTime);

            rb.MovePosition(newPos);

            if (Vector2.Distance(rb.position, playerCurrentPos) < 0.05f && playerOutOfVision)
            {
                StartCoroutine(GuardChaseToNormal());
            }
        }
    }

    private void OnCollisionEnter2D (Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("GAME OVER!!");
            Time.timeScale = 0f;
        }
    }

    private IEnumerator GuardChaseToNormal()
    {
        yield return new WaitForSeconds(2f);
        GetComponent<SpriteRenderer>().color = Color.white;
        chasePlayer = false;
    }

    private void HandleHearing()
    {
        if (player == null || playerRb == null)
        {
            Debug.LogWarning("[Hearing] Player or playerRb is null!");
            return;
        }

        float distance = Vector2.Distance(transform.position, player.transform.position);
        Debug.Log($"[Hearing] Distance to player: {distance}");

        if (distance > hearingRadius)
        {
            if (sr != null)
            {
                sr.color = Color.white;
            }
            Debug.Log("[Hearing] Player outside hearing radius, resetting color to white.");
            return;
        }

        float currentDistance = Mathf.Clamp(distance, 0f, hearingRadius);

        float t = (currentDistance / hearingRadius);

        currentQuiteSpeed = Mathf.Lerp(minQuiteSpeed, maxQuiteSpeed, t);

        float playerSpeed = playerRb.linearVelocity.magnitude;

        Debug.Log(
            $"[Hearing] currentDistance={currentDistance}, t={t}, " +
            $"quietSpeed={currentQuiteSpeed}, playerSpeed={playerSpeed}"
        );

        float noiseFactor = (currentQuiteSpeed > 0f)
            ? Mathf.Clamp01(playerSpeed / currentQuiteSpeed)
            : 1f;

        if (sr != null)
        {
            Color targetColor = Color.Lerp(Color.white, Color.red, noiseFactor);
            sr.color = targetColor;
            Debug.Log($"[Hearing] noiseFactor={noiseFactor}, newColor={targetColor}");
        }

        if (playerSpeed > currentQuiteSpeed)
        {
            //float newY = (transform.eulerAngles.y == 0) ? 180f : 0f;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            Debug.Log("[Hearing] Player too loud! Guard turned around.");
        }
        else
        {
            Debug.Log("[Hearing] Player is quiet enough. No reaction.");
        }
    }

}
