using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    private Grabber grabber;
    [HideInInspector] public bool isBeingPulled;

    [Header("References")]
    public InputActionAsset actions;
    public Rigidbody rb;
    public CinemachineCamera vcam;

    [Header("Movement & Jump")]
    public float speed = 5f;
    public float jumpForce = 5f;
    public float fallMultiplier = 2.2f;
    public float lowJumpMultiplier = 2f;

    [Header("Dash & FOV")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float dashFov = 80f;
    public float pullFov = 85f;

    [Header("Landing Dip")]
    public float landDipAmount = 0.25f;
    public float landDipSpeed = 12f;

    private InputActionMap playerMap;
    private InputAction moveAction, jumpAction, dashAction;
    private Vector3 baseCamOffset;
    private float normalFov, dashTimer, dashCooldownTimer, currentDip;
    private bool isGrounded, canAirDash;

    private void Awake()
    {
        playerMap = actions.FindActionMap("Player");
        moveAction = playerMap.FindAction("Move");
        jumpAction = playerMap.FindAction("Jump");
        dashAction = playerMap.FindAction("Dash");
        grabber = GetComponent<Grabber>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb.freezeRotation = true;
        normalFov = vcam.Lens.FieldOfView;
        baseCamOffset = vcam.transform.localPosition;
    }

    private void OnEnable() => playerMap.Enable();
    private void OnDisable() => playerMap.Disable();

    private void OnCollisionStay(Collision col)
    {
        for (int i = 0; i < col.contactCount; i++)
        {
            if (col.GetContact(i).normal.y > 0.5f)
            {
                if (!isGrounded) currentDip = -landDipAmount;
                isGrounded = canAirDash = true;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision col) => isGrounded = false;

    private void Update()
    {
        float dt = Time.deltaTime;
        dashTimer -= dt;
        dashCooldownTimer -= dt;

        if (!isBeingPulled)
        {
            if (jumpAction.WasPressedThisFrame() && isGrounded)
            {
                isGrounded = false;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }

            if (dashAction.WasPressedThisFrame() && dashCooldownTimer <= 0f && (isGrounded || canAirDash))
            {
                canAirDash &= isGrounded;
                dashTimer = dashDuration;
                dashCooldownTimer = dashCooldown;
            }
        }

        float targetFov = isBeingPulled ? pullFov : (dashTimer > 0f ? dashFov : normalFov);
        vcam.Lens.FieldOfView = Mathf.Lerp(vcam.Lens.FieldOfView, targetFov, 15f * dt);

        currentDip = Mathf.Lerp(currentDip, 0f, landDipSpeed * dt);
        vcam.transform.localPosition = baseCamOffset + Vector3.up * currentDip;
    }

    private void FixedUpdate()
    {
        if (isBeingPulled || (grabber && grabber.isGrabbing)) return;

        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 forward = Vector3.ProjectOnPlane(vcam.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(vcam.transform.right, Vector3.up).normalized;

        if (forward.sqrMagnitude > 0.01f)
            rb.MoveRotation(Quaternion.LookRotation(forward));

        Vector3 dir = Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
        if (dashTimer > 0f && dir == Vector3.zero) dir = forward;
        float currentSpeed = dashTimer > 0f ? dashSpeed : speed;
        rb.linearVelocity = new Vector3(dir.x * currentSpeed, rb.linearVelocity.y, dir.z * currentSpeed);

        float gravityMult = rb.linearVelocity.y < 0f ? fallMultiplier : (!jumpAction.IsPressed() ? lowJumpMultiplier : 1f);
        rb.linearVelocity += Vector3.up * Physics.gravity.y * (gravityMult - 1f) * Time.fixedDeltaTime;
    }
}
