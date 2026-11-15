using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PullBack))]
public class LineDrawer : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform basePoint;
    public float heightOffset = 0.5f;
    public Vector3 playerOffset = new(0f, -0.4f, 0f);

    [Header("Emission Pulse")]
    public float emissionIntensity = 6f;
    public float minPulseSpeed = 4f;
    public float maxPulseSpeed = 22f;

    private LineRenderer line;
    private NavMeshPath path;
    private NavMeshAgent agent;
    private PullBack pullBack;
    private Material lineMat;
    private Color whiteHDR, redHDR;
    private float pulsePhase;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        lineMat = line.material;
        lineMat.EnableKeyword("_EMISSION");

        whiteHDR = Color.white * emissionIntensity;
        redHDR = new Color(200f / 255f, 0f, 0f) * emissionIntensity;

        path = new NavMeshPath();
        agent = GetComponent<NavMeshAgent>();
        pullBack = GetComponent<PullBack>();
    }

    private void Update()
    {
        if (!basePoint) return;

        int areaMask = agent ? agent.areaMask : NavMesh.AllAreas;

        // Calculate Player -> BasePoint so it matches the NavMeshAgent's exact route
        if (agent && agent.enabled && agent.hasPath)
        {
            ApplyCorners(agent.path.corners);
        }
        else if (NavMesh.SamplePosition(transform.position, out NavMeshHit start, 3f, areaMask) &&
                 NavMesh.SamplePosition(basePoint.position, out NavMeshHit end, 3f, areaMask) &&
                 NavMesh.CalculatePath(start.position, end.position, areaMask, path))
        {
            ApplyCorners(path.corners);
        }

        float ratio = pullBack ? pullBack.RemainingRatio : 1f;
        if (ratio <= 0.5f)
        {
            pulsePhase += Time.deltaTime * Mathf.Lerp(minPulseSpeed, maxPulseSpeed, Mathf.InverseLerp(0.5f, 0f, ratio));
            lineMat.SetColor("_EmissionColor", Color.Lerp(whiteHDR, redHDR, (1f - Mathf.Cos(pulsePhase)) * 0.5f));
        }
        else
        {
            pulsePhase = 0f;
            lineMat.SetColor("_EmissionColor", whiteHDR);
        }
    }

    private void ApplyCorners(Vector3[] rawCorners)
    {
        if (rawCorners == null || rawCorners.Length < 2) return;

        // Reverse corner direction (point a to point b flips to point b to point a)
        int len = rawCorners.Length;
        Vector3[] corners = new Vector3[len];
        for (int i = 0; i < len; i++)
        {
            corners[i] = rawCorners[len - 1 - i];
            if (i > 0 && i < len - 1) corners[i] += Vector3.up * heightOffset;
        }

        corners.SetValue(basePoint.position, 0);
        corners[len - 1] = transform.position + playerOffset;

        line.positionCount = len;
        line.SetPositions(corners);
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
