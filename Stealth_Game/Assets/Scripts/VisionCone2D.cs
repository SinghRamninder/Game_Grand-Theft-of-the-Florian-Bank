using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisionCone2D : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.25f);
    [SerializeField] private Color alertColor = new Color(1f, 0f, 0f, 0.35f);

    [Header("Shape")]
    public float viewDistance = 6f;
    [Range(1f, 179f)] public float viewAngle = 60f;
    private int segments = 40;

    [Header("Optional: stop cone when something blocks it")]
    [SerializeField] private bool useObstacles = false;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Follow")]
    private Transform guardTransform;
    public Vector2 localOffset = Vector2.zero;

    private Mesh _mesh;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        _mesh = new Mesh { name = "VisionConeMesh" };
        GetComponent<MeshFilter>().mesh = _mesh;

        meshRenderer = GetComponent<MeshRenderer>();

        if (!guardTransform)
            guardTransform = transform.parent;

        SetNormal(); // default color
    }


    void LateUpdate()
    {
        DrawCone();
    }

    public void SetDistance(float d) => viewDistance = Mathf.Max(0f, d);
    public void SetAngle(float a) => viewAngle = Mathf.Clamp(a, 1f, 179f);

    private void DrawCone()
    {
        if (!guardTransform) return;

        // In your guard script: y=0 => looking LEFT, y=180 => looking RIGHT
        bool facingRight = Mathf.Abs(guardTransform.eulerAngles.y - 180f) < 1f;
        Vector2 forward = facingRight ? Vector2.right : Vector2.left;

        Vector3 origin = guardTransform.TransformPoint((Vector3)localOffset);

        int vertexCount = segments + 2; // center + points around edge
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[segments * 3];

        vertices[0] = transform.InverseTransformPoint(origin);

        float half = viewAngle * 0.5f;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = Mathf.Lerp(-half, half, t);

            Vector2 dir = Rotate(forward, angle).normalized;

            float dist = viewDistance;

            if (useObstacles)
            {
                RaycastHit2D hit = Physics2D.Raycast(origin, dir, viewDistance, obstacleMask);
                if (hit.collider != null) dist = hit.distance;
            }

            Vector3 worldPoint = origin + (Vector3)(dir * dist);
            vertices[i + 1] = transform.InverseTransformPoint(worldPoint);
        }

        int triIndex = 0;
        for (int i = 0; i < segments; i++)
        {
            triangles[triIndex++] = 0;
            triangles[triIndex++] = i + 1;
            triangles[triIndex++] = i + 2;
        }

        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateBounds();
    }

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    public void SetAlert()
    {
        if (meshRenderer != null)
            meshRenderer.material.color = alertColor;
    }

    public void SetNormal()
    {
        if (meshRenderer != null)
            meshRenderer.material.color = normalColor;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Try to find the origin reference just like Awake() and DrawCone() do
        Transform refTransform = guardTransform;
        if (!refTransform && transform.parent) refTransform = transform.parent;
        if (!refTransform) refTransform = transform;

        bool facingRight = Mathf.Abs(refTransform.eulerAngles.y - 180f) < 1f;
        Vector2 forward = facingRight ? Vector2.right : Vector2.left;
        Vector3 origin = refTransform.TransformPoint((Vector3)localOffset);

        // Make the Gizmo slightly opaque so it's easy to see but not overwhelming
        Gizmos.color = new Color(normalColor.r, normalColor.g, normalColor.b, 1f);

        float half = viewAngle * 0.5f;
        Vector3 prevPoint = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = Mathf.Lerp(-half, half, t);

            Vector2 dir = Rotate(forward, angle).normalized;
            float dist = viewDistance;

            if (useObstacles)
            {
                RaycastHit2D hit = Physics2D.Raycast(origin, dir, viewDistance, obstacleMask);
                if (hit.collider != null) dist = hit.distance;
            }

            Vector3 worldPoint = origin + (Vector3)(dir * dist);

            // Draw the curved edge of the cone
            if (i > 0) Gizmos.DrawLine(prevPoint, worldPoint);

            // Draw the two straight side edges from the origin
            if (i == 0 || i == segments) Gizmos.DrawLine(origin, worldPoint);

            prevPoint = worldPoint;
        }
    }
#endif
}
