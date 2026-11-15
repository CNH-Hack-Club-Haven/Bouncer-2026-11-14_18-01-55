using UnityEngine;
using UnityEngine.AI;

public class LineDrawer : MonoBehaviour
{
    public Transform basePoint;
    public LineRenderer line;
    public float heightOffset = 0.5f;
    public Vector3 playerOffset = new Vector3(0f, -0.4f, 0f);

    [HideInInspector] public Vector3[] currentCorners;
    [HideInInspector] public bool isPullingBack = false;

    private NavMeshPath path;
    private PullBack pullBack;

    private void Start()
    {
        path = new NavMeshPath();
        line.useWorldSpace = true;
        pullBack = GetComponent<PullBack>();
    }

    private void Update()
    {
        if (!basePoint) return;

        // Keep the line attached to the player while pulling back
        if (isPullingBack)
        {
            if (line.positionCount >= 2)
                line.SetPosition(line.positionCount - 1, transform.position + playerOffset);
            return;
        }

        if (NavMesh.SamplePosition(basePoint.position, out NavMeshHit start, 3f, NavMesh.AllAreas) &&
            NavMesh.SamplePosition(transform.position, out NavMeshHit end, 3f, NavMesh.AllAreas) &&
            NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path) &&
            path.corners.Length >= 2)
        {
            Vector3[] corners = path.corners;

            for (int i = 1; i < corners.Length - 1; i++)
                corners[i] += Vector3.up * heightOffset;

            corners[0] = basePoint.position;
            corners[corners.Length - 1] = transform.position + playerOffset;

            currentCorners = corners;

            line.positionCount = corners.Length;
            line.SetPositions(corners);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPullingBack && other.CompareTag("Tether"))
        {
            basePoint = other.transform;
            if (pullBack != null)
                pullBack.ResetTimer();
        }
    }
}
