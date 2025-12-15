using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HearingRadius : MonoBehaviour
{
    [SerializeField] private SecurityOfficerScript securityGuard;
    [SerializeField] private int segments = 64;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private Color lineColor = new Color(0f, 1f, 0f, 0.4f);

    private LineRenderer lr;
    private float lastRadius;
    private Vector3 lastPos;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();

        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = segments;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lineColor;
        lr.endColor = lineColor;

        lastRadius = -1f;
        lastPos = Vector3.positiveInfinity;

        DrawCircle();
    }

    private void Update()
    {
        if (securityGuard == null) return;

        float r = securityGuard.hearingRadius;
        Vector3 p = transform.position;

        // only redraw if something actually changed
        if (!Mathf.Approximately(r, lastRadius) || (p - lastPos).sqrMagnitude > 0.000001f)
        {
            DrawCircle();
            lastRadius = r;
            lastPos = p;
        }
    }

    private void DrawCircle()
    {
        if (securityGuard == null) return;

        float radius = securityGuard.hearingRadius;
        Vector3 center = transform.position;

        float step = 2f * Mathf.PI / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = step * i;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            lr.SetPosition(i, center + new Vector3(x, y, 0f));
        }
    }
}
