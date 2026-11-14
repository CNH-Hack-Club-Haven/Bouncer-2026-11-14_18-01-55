using UnityEngine;
using UnityEngine.AI;

public class StringPath : MonoBehaviour
{
    public Transform basePoint;
    public LineRenderer line;
    public float heightOffset = 0.5f;
    public Vector3 playerOffset = new Vector3(0f, -0.4f, 0f);

    private NavMeshPath path;

    private void Start()
    {
        path = new NavMeshPath();
        line.useWorldSpace = true;
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

            corners.SetValue(basePoint.position, 0);
            corners[corners.Length - 1] = transform.position + playerOffset;

            line.positionCount = corners.Length;
            line.SetPositions(corners);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tether"))
            basePoint = other.transform;
    }
}
