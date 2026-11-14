using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class SimpleMove : MonoBehaviour
{
    public InputActionAsset actions;
    public Rigidbody rb;
    public CinemachineCamera vcam;

    public float speed = 5f;
    public float jumpForce = 5f;
    public float dashSpeed = 18f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float dashFov = 80f;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private float normalFov;
    private float dashTimer;
    private float dashCooldownTimer;
    private bool isGrounded;
    private bool canAirJump;
    private bool canAirDash;

    private void Awake()
    {
        var map = actions.FindActionMap("Player");
        moveAction = map.FindAction("Move");
        jumpAction = map.FindAction("Jump");
        dashAction = map.FindAction("Dash");
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        normalFov = vcam.Lens.FieldOfView;
    }

    private void OnEnable()
    {
        actions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        actions.FindActionMap("Player").Disable();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = canAirJump = canAirDash = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    private void Update()
    {
        dashTimer -= Time.deltaTime;
        dashCooldownTimer -= Time.deltaTime;

        if (jumpAction.WasPressedThisFrame() && (isGrounded || canAirJump))
        {
            if (!isGrounded)
            {
                canAirJump = false;
            }

            isGrounded = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (dashAction.WasPressedThisFrame() && dashCooldownTimer <= 0f && (isGrounded || canAirDash))
        {
            if (!isGrounded)
            {
                canAirDash = false;
            }

            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
        }

        float targetFov = dashTimer > 0f ? dashFov : normalFov;
        vcam.Lens.FieldOfView = Mathf.Lerp(vcam.Lens.FieldOfView, targetFov, 15f * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 forward = Vector3.ProjectOnPlane(vcam.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(vcam.transform.right, Vector3.up).normalized;
        Vector3 dir = Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);

        if (dashTimer > 0f && dir == Vector3.zero)
        {
            dir = forward;
        }

        float currentSpeed = dashTimer > 0f ? dashSpeed : speed;
        rb.linearVelocity = new Vector3(dir.x * currentSpeed, rb.linearVelocity.y, dir.z * currentSpeed);
    }
}
