using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraToggle : MonoBehaviour {
    public enum CameraState {
        Locked, Unlocked
    }
    public event Action<CameraState> CameraToggleEvent;

    public bool cameraUnlocked = false, cursorUnlocked = false;
    public Animator animatedTarget;

    public void OnCursorUnlock(InputAction.CallbackContext context) {
        if (context.ReadValueAsButton()) {
            if (Cursor.lockState != CursorLockMode.Locked) {
                Cursor.lockState = CursorLockMode.Locked;
                cursorUnlocked = false;
            }
            else if (Cursor.lockState == CursorLockMode.Locked) {
                Cursor.lockState = CursorLockMode.Confined;
                cursorUnlocked = true;
            }
        }
    }

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        if (cameraUnlocked) {
            animatedTarget.SetBool("isToggled", cameraUnlocked);
            CameraToggleEvent?.Invoke(CameraState.Unlocked);
        }
    }
    private void Update() {
        if (cameraUnlocked) {
            animatedTarget.SetBool("isToggled", true);
            CameraToggleEvent?.Invoke(CameraState.Unlocked);
            return;
        }
        animatedTarget.SetBool("isToggled", false);
        CameraToggleEvent?.Invoke(CameraState.Locked);
    }
}
