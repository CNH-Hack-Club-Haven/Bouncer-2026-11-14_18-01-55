using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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
        {
            target = Physics.OverlapSphere(transform.position, grabDistance, handleLayer)
                .OrderBy(h => (h.transform.position - transform.position).sqrMagnitude)
                .FirstOrDefault()?.transform;

            SetGrab(target != null);
        }
        else if (grab.WasReleasedThisFrame() && isGrabbing)
        {
            SetGrab(false);
        }

        if (!isGrabbing) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
    }

    void LateUpdate()
    {
        if (!isGrabbing) return;

        hand.position = Vector3.Lerp(hand.position, target.TransformPoint(positionOffset), 15f * Time.deltaTime);
        hand.rotation = Quaternion.Slerp(hand.rotation, target.rotation * Quaternion.Euler(rotationOffset), 15f * Time.deltaTime);
    }

    void SetGrab(bool state)
    {
        isGrabbing = state;
        if (rb) rb.isKinematic = state;
        if (!state) target = null;
    }
}
