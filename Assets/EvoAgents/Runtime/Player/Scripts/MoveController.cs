using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CharacterController), typeof(PlayerInput))]
public class MoveController : MonoBehaviour {
    [Range(2f, 10f)] public float movementSpeed = 4f;
    [Range(1f, 2f)] public float jumpForce = 1.7f;
    [Range(1f, 5f)] public float runMultiplier = 2f;
    [Range(0f, 1f)] public float turnSmoothTime = 0.1f;
    public const float gravity = -9.81f;
    public Transform controllerCamera;
    public PlayerAnimate playerAnimate;
    public CameraToggle cameraToggle;
    public CinemachineFreeLook freeLookCamera;

    private Vector3 velocity, direction;
    private float turnSmoothVelocity, speed;
    private CharacterController characterController;
    private PlayerAnimate.AnimationState animationState;
    private bool isCameraToggled = false, hasSprint = false, hasCursorLocked;

    public Vector3 Velocity => velocity;

    private void Awake() {
        characterController = GetComponent<CharacterController>();
        if (speed == 0f) speed = movementSpeed;
    }

    private void Start() {
        playerAnimate.PlayerAnimationChecked += OnPlayerAnimationChecked;
        cameraToggle.CameraToggleEvent += OnCameraToggled;
    }

    private void OnCameraToggled(CameraToggle.CameraState state) {
        if (state == CameraToggle.CameraState.Locked) isCameraToggled = false;
        else if (state == CameraToggle.CameraState.Unlocked) isCameraToggled = true;
    }

    private void OnPlayerAnimationChecked(PlayerAnimate sender) {
        sender.SetCurrentAnimation(animationState);
    }

    public void SelectDirection(Vector2 keyboardDirection) {
        direction = new Vector3(keyboardDirection.x, 0f, keyboardDirection.y).normalized;
    }
    public void OnMovement(InputAction.CallbackContext context) {
        if (isCameraToggled || !hasCursorLocked) { direction = Vector3.zero; animationState = PlayerAnimate.AnimationState.idle; return; }
        SelectDirection(context.ReadValue<Vector2>());
        if (hasSprint && direction.magnitude >= 0.1f) animationState = PlayerAnimate.AnimationState.isRunning;
        else if (direction.magnitude >= 0.1f) animationState = PlayerAnimate.AnimationState.isWalking;
        else animationState = PlayerAnimate.AnimationState.idle;
    }
    public void OnToggleCamera(InputAction.CallbackContext context) {
        if (!hasCursorLocked) return;
        if (context.performed && context.ReadValue<float>() == 1f) 
            cameraToggle.cameraUnlocked = !cameraToggle.cameraUnlocked;
    }
    public void OnTakeHeight(InputAction.CallbackContext context) {
        if (isCameraToggled || !hasCursorLocked) { velocity.y = -2; return; }
        if (context.performed && characterController.isGrounded) velocity.y = Mathf.Sqrt(jumpForce * (-2f * gravity));
    }
    public void OnSprint(InputAction.CallbackContext context) {
        if (isCameraToggled || !hasCursorLocked) { speed = movementSpeed; animationState = PlayerAnimate.AnimationState.idle; return; }
        if (context.ReadValue<float>() == 1f) {
            if (context.performed && !hasSprint) {
                speed = movementSpeed * runMultiplier;
                hasSprint = true;
                return;
            }
            speed = movementSpeed;
            hasSprint = false;
        }
    }

    private bool hasInputProvider = false;
    private CinemachineInputProvider inputProvider;
    public InputActionReference actionReference;
    void Update() {
        hasCursorLocked = !cameraToggle.cursorUnlocked;

        if (isCameraToggled || !hasCursorLocked) {
            if (!hasInputProvider)
                hasInputProvider = freeLookCamera.TryGetComponent(out inputProvider);
            if (!hasCursorLocked) {
                inputProvider.XYAxis = null;
            } 
            return;
        }
        if(hasInputProvider && inputProvider.XYAxis == null) 
            inputProvider.XYAxis = actionReference;
        if (characterController.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (direction.magnitude >= 0.1f && !isCameraToggled) {
            //Debug.Log(direction);
            float targetAngle = Mathf.Atan2(this.direction.x, this.direction.z) * Mathf.Rad2Deg + controllerCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            var newDirection = (Quaternion.AngleAxis(targetAngle, Vector3.up) * Vector3.forward).normalized;
            characterController.Move(newDirection * speed * Time.deltaTime);
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}
