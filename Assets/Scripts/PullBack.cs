using UnityEngine;
using UnityEngine.AI;

public class PullBack : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public PlayerMovement playerMovement;
    public Transform anchorPoint;
    public Grabber grabber;
    public LineRenderer lineRenderer;

    [Header("Settings")]
    public float stringHeightOffset = 0.6f;
    public float navSampleRadius = 2f;
    public float pullDuration = 5f;
    public float pullForce = 35f;
    public float maxPullSpeed = 20f;
    public float stopDistance = 1.5f;

    private NavMeshPath navPath;
    private Vector3[] corners = System.Array.Empty<Vector3>();
    private float pullTimer;
    private bool isPulled;

    private void Start()
    {
        navPath = new NavMeshPath();

        if (!anchorPoint)
        {
            anchorPoint = new GameObject("AnchorPoint").transform;
            anchorPoint.position = transform.position;
        }

        if (lineRenderer)
        {
            lineRenderer.startWidth = lineRenderer.endWidth = 0.05f;
            lineRenderer.useWorldSpace = true;
        }
    }

    private void Update()
    {
        if (!isPulled && grabber && grabber.isGrabbing)
            SetPull(true);

        if (!anchorPoint || !lineRenderer) return;

        if (NavMesh.SamplePosition(anchorPoint.position, out NavMeshHit start, navSampleRadius, NavMesh.AllAreas) &&
            NavMesh.SamplePosition(transform.position, out NavMeshHit end, navSampleRadius, NavMesh.AllAreas) &&
            NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, navPath) &&
            navPath.corners.Length >= 2)
        {
            corners = navPath.corners;
            Vector3 heightOffset = Vector3.up * stringHeightOffset;

            for (int i = 1; i < corners.Length - 1; i++)
                corners[i] += heightOffset;

            corners.SetValue(anchorPoint.position + heightOffset, 0);
            corners[corners.Length - 1] = transform.position + Vector3.up * (stringHeightOffset - 0.5f);

            lineRenderer.positionCount = corners.Length;
            lineRenderer.SetPositions(corners);
        }
    }

    private void FixedUpdate()
    {
        if (!isPulled || !anchorPoint) return;

        pullTimer -= Time.fixedDeltaTime;
        if (pullTimer <= 0f || Vector3.Distance(transform.position, anchorPoint.position) <= stopDistance)
        {
            SetPull(false);
            return;
        }

        Vector3 target = corners.Length >= 2 ? corners[corners.Length - 2] : anchorPoint.position;
        Vector3 dir = Vector3.ProjectOnPlane(target - transform.position, Vector3.up);

        if (dir.sqrMagnitude > 0.001f)
            rb.AddForce(dir.normalized * pullForce, ForceMode.Acceleration);

        Vector3 flatVel = Vector3.ClampMagnitude(new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z), maxPullSpeed);
        rb.linearVelocity = new Vector3(flatVel.x, rb.linearVelocity.y, flatVel.z);
    }

    public void SetPull(bool active)
    {
        isPulled = active;
        if (active) pullTimer = pullDuration;
        if (playerMovement) playerMovement.isBeingPulled = active;
    }

    public void SetNewAnchor(Transform newAnchor)
    {
        anchorPoint = newAnchor;
        SetPull(false);
    }
}
