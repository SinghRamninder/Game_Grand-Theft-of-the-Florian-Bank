using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class NoiseRing : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private int segments = 64;

    [Header("Life")]
    [SerializeField] private float lifeTime = 0.6f;
    [SerializeField] private float startRadius = 0.1f;

    [Header("Collision")]
    [SerializeField] private LayerMask guardMask;

    private LineRenderer lr;
    private CircleCollider2D col;

    private float elapsed;
    private float endRadius;
    private float strength;
    private bool hasAlerted;

    // Better than reflection: configure the ring properly
    public void Configure(float endRadius, float strength, LayerMask guardMask)
    {
        this.endRadius = endRadius;
        this.strength = strength;
        this.guardMask = guardMask;
    }

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        col = GetComponent<CircleCollider2D>();

        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = segments + 1;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifeTime);

        float radius = Mathf.Lerp(startRadius, endRadius, t);

        DrawCircle(radius);
        col.radius = radius;

        // fade out
        float alpha = 1f - t;
        var c = lr.startColor;
        c.a = alpha;
        lr.startColor = c;
        lr.endColor = c;

        if (elapsed >= lifeTime)
            Destroy(gameObject);
    }

    private void DrawCircle(float radius)
    {
        float angleStep = 360f / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            lr.SetPosition(i, transform.position + new Vector3(x, y, 0f));
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasAlerted) return;

        // Only react to guards in the mask
        if (((1 << other.gameObject.layer) & guardMask) == 0) return;

        if (other.TryGetComponent<SecurityOfficerScript>(out var guard))
        {
            hasAlerted = true;
            guard.HearNoise(transform.position);
        }
    }
}
