using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Grabber : MonoBehaviour
{
    [Header("Input & State")]
    public InputActionAsset actions;
    public bool isGrabbing;

    [Header("Detection")]
    public float grabDistance = 2f;
    public LayerMask handleLayer;

    [Header("Hand & Offsets")]
    public Transform hand;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    InputAction grab;
    Transform target;
    Rigidbody rb;
    readonly Collider[] hits = new Collider[16];

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grab = actions.FindActionMap("Player").FindAction("Grab");
    }

    void OnEnable() => grab.Enable();
    void OnDisable() => grab.Disable();

    void Update()
    {
        if (grab.WasPressedThisFrame())
            SetGrab(FindClosest());
        else if (grab.WasReleasedThisFrame() && isGrabbing)
            SetGrab(null);

        if (!isGrabbing) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 1f - Mathf.Exp(-10f * Time.deltaTime));
    }

    void LateUpdate()
    {
        if (!isGrabbing) return;

        float t = 1f - Mathf.Exp(-15f * Time.deltaTime);
        hand.SetPositionAndRotation(
            Vector3.Lerp(hand.position, target.TransformPoint(positionOffset), t),
            Quaternion.Slerp(hand.rotation, target.rotation * Quaternion.Euler(rotationOffset), t)
        );
    }

    Transform FindClosest()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, grabDistance, hits, handleLayer);
        Transform closest = null;
        float minDist = float.MaxValue;
        Vector3 pos = transform.position;

        for (int i = 0; i < count; i++)
        {
            float dist = (hits[i].transform.position - pos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = hits[i].transform;
            }
        }
        return closest;
    }

    void SetGrab(Transform newTarget)
    {
        target = newTarget;
        isGrabbing = target != null;
        rb.isKinematic = isGrabbing;
    }
}
