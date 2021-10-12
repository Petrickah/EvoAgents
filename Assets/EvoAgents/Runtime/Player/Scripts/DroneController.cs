using UnityEngine;
using UnityEngine.InputSystem;

public class DroneController : MonoBehaviour
{
    [Range(4f, 8f)] public float cameraSpeed = 4.5f;
    [Range(1f, 4f)] public float cameraSpeedMultiplier = 2f;
    [Range(0f, 1f)] public float mouseSensitivity = 0.17f;
    [Range(0f, 1f)] public float androidMouseSensitivity = 0.25f;
    [Range(0f, 1f)] public float mouseFriction = 0.25f;
    [Range(0f, 1f)] public float androidMouseFriction = 0.75f;
    public float groundDistance = 20f;
    public Transform drone;
    public Transform droneCamera;
    public CameraToggle cameraToggle;
    private Vector3 direction, rotation;
    private Cinemachine.CinemachineVirtualCamera droneVirtualCamera;
    private float speed;
    private bool hasIncreasingHeight, hasDecreasingHeight, hasCameraUnlocked, hasSprint, hasCursorLocked;

    private void Awake() {
        if (speed == 0f) speed = cameraSpeed;
        droneVirtualCamera = droneCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>();
    }

    private void Start() {
        cameraToggle.CameraToggleEvent += OnCameraToggled;
    }

    private void OnCameraToggled(CameraToggle.CameraState state) {
        if (state == CameraToggle.CameraState.Unlocked) hasCameraUnlocked = true;
        else if (state == CameraToggle.CameraState.Locked) hasCameraUnlocked = false;
    }
    public void SelectDirection(Vector2 keyboardDirection) {
        direction = new Vector3(keyboardDirection.x, 0, keyboardDirection.y);
    }
    public void OnMovement(InputAction.CallbackContext callbackContext) {
        if (!hasCameraUnlocked || !hasCursorLocked) { direction = Vector3.zero; return; }
        var keyboardDirection = callbackContext.ReadValue<Vector2>();
        SelectDirection(keyboardDirection);
    }
    public void OnLook(InputAction.CallbackContext context) {
        if (!hasCameraUnlocked || !hasCursorLocked) { rotation = Vector3.zero; return; }
        var sensitivity = (Application.isMobilePlatform) ? androidMouseSensitivity : mouseSensitivity;
        var friction = (Application.isMobilePlatform) ? androidMouseFriction : mouseFriction;
        rotation = context.ReadValue<Vector2>().x * sensitivity * friction * Vector3.up + context.ReadValue<Vector2>().y * sensitivity * friction * Vector3.right;
        if (droneVirtualCamera != null) {
            var pov = droneVirtualCamera.GetCinemachineComponent<Cinemachine.CinemachinePOV>();
            pov.m_HorizontalAxis.m_InputAxisValue = rotation.y;
            pov.m_VerticalAxis.m_InputAxisValue = rotation.x;
        }
    }
    public void OnSprint(InputAction.CallbackContext context) {
        if (!hasCameraUnlocked || !hasCursorLocked) { speed = cameraSpeed; return; }
        if (context.ReadValue<float>() == 1f) {
            if (context.performed && !hasSprint) {
                speed = cameraSpeed * cameraSpeedMultiplier;
                hasSprint = true;
                return;
            }
            speed = cameraSpeed;
            hasSprint = false;
        }
    }
    public void OnTakeHeight(InputAction.CallbackContext callbackContext) {
        if (!hasCameraUnlocked || !hasCursorLocked) { 
            hasIncreasingHeight = false; 
            return; 
        }
        if (callbackContext.performed && callbackContext.ReadValue<float>()==1f) 
            hasIncreasingHeight = true;
        else
            hasIncreasingHeight = false;
    }
    public void OnDecreaseHeight(InputAction.CallbackContext callbackContext) {
        if (!hasCameraUnlocked || !hasCursorLocked) { hasDecreasingHeight = false; return; }
        if (callbackContext.performed && callbackContext.ReadValue<float>()==1f) hasDecreasingHeight = true;
        else hasDecreasingHeight = false;
    }

    private void LateUpdate() {
        if (!hasCameraUnlocked || !hasCursorLocked) { direction.y = 0f; return; }
        if (hasIncreasingHeight) direction.y = 1f;
        else if (hasDecreasingHeight) direction.y = -1f;
        else direction.y = 0f;

        if (direction.y == -1f && CameraIsOnGround()) direction.y = 0f;
    }

    private void Update() {
        hasCursorLocked = !cameraToggle.cursorUnlocked;
        if (!hasCameraUnlocked || !hasCursorLocked) return;
        var newDirection = (Quaternion.AngleAxis(droneCamera.eulerAngles.y, Vector3.up) * direction).normalized;
        drone.position += newDirection * speed * Time.deltaTime;
    }

    private bool CameraIsOnGround() {
        Ray ray = new Ray(drone.position, -1 * Vector3.up);
        return Physics.Raycast(ray, out RaycastHit hit, groundDistance) && hit.distance < 0.5f;
    }
}
