using UnityEngine;

public class NoiseEmitter : MonoBehaviour
{
    [Header("Noise Settings")]
    [Tooltip("How fast the player must move to start generating noise rings.")]
    [SerializeField] private float minSpeedToMakeNoise = 1.0f;
    [Tooltip("How often (in seconds) a new noise ring is spawned while moving.")]
    [SerializeField] private float stepInterval = 0.25f;

    [Tooltip("How far the noise ring expands when the player is walking.")]
    [SerializeField] private float ringRadiusWalk = 3f;
    [Tooltip("How far the noise ring expands when the player is running.")]
    [SerializeField] private float ringRadiusRun = 6f;

    [Tooltip("How strong the noise is (e.g., how much attention it draws from guards) when the player is walking.")]
    [HideInInspector] public float strengthWalk = 0.5f;
    [Tooltip("How strong the noise is (e.g., how much attention it draws from guards) when the player is running.")]
    [HideInInspector] public float strengthRun = 1.0f;

    [Header("Advanced Settings (Can be ignored)")]
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private NoiseRing ringPrefab;
    [HideInInspector] public LayerMask guardMask;

    private float stepTimer;

    [HideInInspector] public bool noSoundRing = false;

    private void Reset()
    {
        playerRb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        guardMask = LayerMask.GetMask("Guard");
    }

    private void Update()
    {
        if (noSoundRing)
        {
            return;
        }

        if ((playerRb == null || ringPrefab == null)) return;

        float speed = playerRb.linearVelocity.magnitude;

        if (speed < minSpeedToMakeNoise)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer += Time.deltaTime;
        if (stepTimer >= stepInterval)
        {
            stepTimer = 0f;

            bool running = speed > GetComponent<PlayerMovement>().normalSpeed;

            float radius = running ? ringRadiusRun : ringRadiusWalk;
            float strength = running ? strengthRun : strengthWalk;

            SpawnRing(radius, strength);
        }
    }

    private void SpawnRing(float radius, float strength)
    {
        NoiseRing ring = Instantiate(ringPrefab, transform.position, Quaternion.identity);
        ring.Configure(radius, strength, guardMask);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("No Sound"))
        {
            noSoundRing = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("No Sound"))
        {
            noSoundRing = false;
        }
    }
}
