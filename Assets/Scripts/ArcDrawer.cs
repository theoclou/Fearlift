using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ArcDrawer : MonoBehaviour
{
    public float radius = 5f;
    public float angle = 90f; // Angle total de l'arc
    public int segments = 50; // Plus il y en a, plus c’est lisse
    public float startAngle = 0f; // En degrés

    void Start()
    {
        DrawArc();
    }

    void DrawArc()
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.positionCount = segments + 1;

        float angleStep = angle / segments;
        Vector3[] points = new Vector3[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            float rad = Mathf.Deg2Rad * currentAngle;
            float x = Mathf.Cos(rad) * radius;
            float y = Mathf.Sin(rad) * radius;
            points[i] = new Vector3(x, y, 0);
        }

        lr.useWorldSpace = false;
        lr.SetPositions(points);
    }
}
