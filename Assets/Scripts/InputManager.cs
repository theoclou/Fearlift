using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public static event Action OnResetRequested;

    private InputSystem_Actions controls;

    private void Awake()
    {
        controls = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Player.ResetScene.performed += OnResetPerformed;
    }

    private void OnDisable()
    {
        controls.Player.ResetScene.performed -= OnResetPerformed;
        controls.Disable();
    }

    private void OnResetPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnResetRequested?.Invoke();
    }
}
