using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class LineDrawer : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform basePoint;
    public float heightOffset = 0.5f;
    public Vector3 playerOffset = new Vector3(0f, -0.4f, 0f);

    [Header("Emission Pulse")]
    public float emissionIntensity = 6f;
    public float minPulseSpeed = 4f;
    public float maxPulseSpeed = 22f;

    private LineRenderer line;
    private NavMeshPath path;
    private PullBack pullBack;
    private Material lineMat;
    private Color whiteHDR;
    private Color redHDR;
    private float pulsePhase;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        lineMat = line.material;
        lineMat.EnableKeyword("_EMISSION");

        // Starts at RGB(255, 255, 255) and pulses to RGB(200, 0, 0) at intensity 6
        whiteHDR = Color.white * emissionIntensity;
        redHDR = new Color(200f / 255f, 0f, 0f) * emissionIntensity;

        path = new NavMeshPath();
        pullBack = GetComponent<PullBack>();
    }

    private void Update()
    {
        if (!basePoint) return;

        if (NavMesh.SamplePosition(basePoint.position, out NavMeshHit start, 3f, NavMesh.AllAreas) &&
            NavMesh.SamplePosition(transform.position, out NavMeshHit end, 3f, NavMesh.AllAreas) &&
            NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path) &&
            path.corners.Length >= 2)
        {
            Vector3[] corners = path.corners;

            for (int i = 1; i < corners.Length - 1; i++)
                corners[i] += Vector3.up * heightOffset;

            corners = basePoint.position;
            corners[corners.Length - 1] = transform.position + playerOffset;

            line.positionCount = corners.Length;
            line.SetPositions(corners);
        }

        UpdatePulse();
    }

    private void UpdatePulse()
    {
        if (!pullBack) return;

        float remainingRatio = pullBack.RemainingRatio;

        if (remainingRatio <= 0.5f)
        {
            float urgency = Mathf.InverseLerp(0.5f, 0f, remainingRatio);
            float speed = Mathf.Lerp(minPulseSpeed, maxPulseSpeed, urgency);

            // Uses -Cos so wave starts smoothly at 0 (white) when pulsePhase == 0
            pulsePhase += Time.deltaTime * speed;
            float wave = (1f - Mathf.Cos(pulsePhase)) * 0.5f;

            lineMat.SetColor("_EmissionColor", Color.Lerp(whiteHDR, redHDR, wave));
        }
        else
        {
            pulsePhase = 0f;
            lineMat.SetColor("_EmissionColor", whiteHDR);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tether"))
        {
            basePoint = other.transform;
            if (pullBack) pullBack.ResetTimer();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Tether") && pullBack)
            pullBack.ResetTimer();
    }
}
