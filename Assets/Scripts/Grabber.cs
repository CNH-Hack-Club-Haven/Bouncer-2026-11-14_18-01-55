using UnityEngine;
using UnityEngine.InputSystem;

public class Grabber : MonoBehaviour
{
    public InputActionAsset actions;
    public bool isGrabbing;

    private InputAction grabAction;

    private void Awake()
    {
        grabAction = actions.FindActionMap("Player").FindAction("Grab");
    }

    private void Update()
    {
        if (grabAction.WasPressedThisFrame())
        {
            // Replace your old Input.GetButtonDown block with this
        }
    }
}
