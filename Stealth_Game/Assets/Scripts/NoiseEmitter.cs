using UnityEngine;

public class NoiseEmitter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private NoiseRing ringPrefab;

    [Header("Guards")]
    [SerializeField] private LayerMask guardMask;

    [Header("Noise Settings")]
    [SerializeField] private float minSpeedToMakeNoise = 1.0f;
    [SerializeField] private float stepInterval = 0.25f;

    [SerializeField] private float ringRadiusWalk = 3f;
    [SerializeField] private float ringRadiusRun = 6f;

    [SerializeField] private float strengthWalk = 0.5f;
    [SerializeField] private float strengthRun = 1.0f;

    private float stepTimer;

    public bool noSoundRing = false;

    private void Reset()
    {
        playerRb = GetComponent<Rigidbody2D>();
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
