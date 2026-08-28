using System.Collections;
using UnityEngine;

namespace EvolveGames
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Player Controller")]
        [SerializeField] public Transform Camera;
        [Range(1, 10)] public float walkingSpeed = 3.0f;
        [Range(0.1f, 8)] public float CroughSpeed = 1.0f;
        [SerializeField, Range(2, 20)] float RuningSpeed = 4.0f;
        [SerializeField, Range(0, 20)] float jumpSpeed = 6.0f;
        [SerializeField, Range(0, 1)] float jumpDelay = 0.5f;
        [Range(0.1f, 10)] public float lookSpeed = 2.0f;
        [SerializeField, Range(10, 120)] float lookXLimit = 80.0f;

        [Space(20)]
        [Header("Advance Settings")]
        [SerializeField] float RunningFOV = 65.0f;
        [SerializeField] float SpeedToFOV = 4.0f;

        [Header("Crouch Settings")]
        [SerializeField] float CroughHeight = 0.8f;
        [SerializeField] float crouchTransitionSpeed = 12.0f;
        [SerializeField] private CapsuleCollider playerCapsuleCollider;

        [Header("Head Clearance")]
        [SerializeField] private Vector3 headCheckOffset = new Vector3(0, 0.05f, 0);
        [SerializeField] private float headCheckRadius = 0.25f;
        [SerializeField] private float headCheckExtraHeight = 0.1f;

        [Header("Head Status")]
        public bool isHeadBlocked = false;

        [SerializeField] public float gravity = 20.0f;
        [SerializeField] float timeToRunning = 0.3f;
        [HideInInspector] public CharacterController characterController;

        [Space(20)]
        public bool canMove = true;
        public bool canJump = true;
        public bool CanRun = true;
        public bool CanCrough = true;

        [Space(20)]
        public bool isMoving = false;
        public bool isJumping = false;
        public bool isRunning = false;
        public bool isCroughing = false;
        public bool isPause = false;
        public bool isPauseCamera = false;
        public bool isPauseBody = false;

        [Header("Jump Restrictions")]
        [SerializeField] private LayerMask noJumpLayers = 0;

        [Header("Ground Check")]
        [SerializeField] private Vector3 groundCheckOffset = new Vector3(0, -0.9f, 0);
        [SerializeField] private Vector3 groundCheckScale = new Vector3(0.4f, 0.4f, 0.4f);

        [Header("Input Controls")]
        [SerializeField] KeyCode CroughKey = KeyCode.LeftControl;

        [HideInInspector] public float vertical;
        [HideInInspector] public float horizontal;
        [HideInInspector] public float Lookvertical;
        [HideInInspector] public float Lookhorizontal;
        [HideInInspector] public Vector3 moveDirection = Vector3.zero;

        private FlashLight flashLight;
        private HeadBob headBob;
        private MovementEffects movementEffects;
        private Camera cam;

        private float originalHeight;
        private Vector3 originalCenter;

        private float originalCapsuleColliderHeight;
        private Vector3 originalCapsuleColliderCenter;

        private float originalFOV;
        private float originalWalkSpeed;
        private float originalGravity;

        private float rotationX = 0;
        private float RunningValue;
        private bool JumpOnce;
        private bool once;

        private int playerLayer;
        private LayerMask groundMask;

        private bool isFocusPaused = false;
        private Transform focusTarget;
        private Vector2 focusMouseOffset;

        [Header("Focus Pause Settings")]
        [SerializeField] private float focusLookSpeed = 2.0f;
        [SerializeField] private float focusReturnSpeed = 3f;
        [SerializeField] private float focusMaxAngle = 30f;

        public void ChangeLookXLimit(float x) => lookXLimit = x;

        public void Pause()
        {
            isPause = true;
            canMove = false;
            isMoving = false;
            CanRun = false;
            canJump = false;
            CanCrough = false;
            if (characterController != null) characterController.enabled = false;
            if (playerCapsuleCollider != null) playerCapsuleCollider.enabled = false;
            ToggleEffects(false);
        }

        public void UnPause()
        {
            if (characterController != null) characterController.enabled = true;
            if (playerCapsuleCollider != null) playerCapsuleCollider.enabled = true;
            isPause = false;
            canMove = true;
            CanRun = true;
            if (!isCroughing) canJump = true;
            CanCrough = true;
            ToggleEffects(true);
        }

        public void PauseCamera()
        {
            isPauseCamera = true;
            ToggleEffects(false);
        }

        public void UnPauseCamera()
        {
            isPauseCamera = false;
            ToggleEffects(true);
        }

        public void PauseBody()
        {
            isPauseBody = true;
            canMove = false;
            isMoving = false;
            CanRun = false;
            canJump = false;
            CanCrough = false;
            if (characterController != null) characterController.enabled = false;
            if (playerCapsuleCollider != null) playerCapsuleCollider.enabled = false;
            ToggleEffects(false);
        }

        public void UnPauseBody()
        {
            isPauseBody = false;
            canMove = true;
            CanRun = true;
            if (!isCroughing) canJump = true;
            CanCrough = true;
            if (characterController != null) characterController.enabled = true;
            if (playerCapsuleCollider != null) playerCapsuleCollider.enabled = true;
            ToggleEffects(true);
        }

        public void PauseFocus(Transform target)
        {
            if (target == null) return;
            focusTarget = target;
            isFocusPaused = true;
            Pause();
            focusMouseOffset = Vector2.zero;
        }

        public void UnPauseFocus()
        {
            isFocusPaused = false;
            focusTarget = null;
            focusMouseOffset = Vector2.zero;
            UnPause();
        }

        public void HideMouse()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void UnHideMouse()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        void Awake()
        {
            characterController = GetComponent<CharacterController>();
            cam = GetComponentInChildren<Camera>();
            HideMouse();

            if (playerCapsuleCollider == null)
            {
                playerCapsuleCollider = GetComponent<CapsuleCollider>();
            }

            headBob = FindFirstObjectByType<HeadBob>();
            movementEffects = FindFirstObjectByType<MovementEffects>();
            flashLight = FindFirstObjectByType<FlashLight>();

            originalHeight = characterController.height;
            originalCenter = characterController.center;

            if (playerCapsuleCollider != null)
            {
                originalCapsuleColliderHeight = playerCapsuleCollider.height;
                originalCapsuleColliderCenter = playerCapsuleCollider.center;
            }

            originalFOV = cam.fieldOfView;
            originalWalkSpeed = walkingSpeed;
            originalGravity = gravity;

            RunningValue = walkingSpeed;

            playerLayer = gameObject.layer;
            groundMask = ~(1 << playerLayer);

            characterController.enabled = true;
            moveDirection = Vector3.zero;

            AudioListener.volume = 0;
            StartCoroutine(DelayEnableAudio());
        }

        IEnumerator DelayEnableAudio()
        {
            yield return new WaitForSeconds(0.5f);
            AudioListener.volume = 1;
        }

        IEnumerator DelayJump()
        {
            yield return new WaitForSeconds(jumpDelay);
            if (!isCroughing) canJump = true;
        }

        private bool IsGroundedOnNoJumpLayer()
        {
            if (!characterController.isGrounded) return false;

            Vector3 center = transform.position + groundCheckOffset;
            float radius = groundCheckScale.x;

            Collider[] hits = Physics.OverlapSphere(center, radius, groundMask, QueryTriggerInteraction.Ignore);
            if (hits.Length > 0)
            {
                int layer = hits[0].gameObject.layer;
                return (noJumpLayers.value & (1 << layer)) != 0;
            }
            return false;
        }

        private bool CanStandUp()
        {
            Vector3 currentTop = transform.position + characterController.center +
                                 Vector3.up * (characterController.height * 0.5f) +
                                 headCheckOffset;

            Vector3 standingTop = transform.position + originalCenter +
                                  Vector3.up * (originalHeight * 0.5f) +
                                  headCheckOffset +
                                  Vector3.up * headCheckExtraHeight;

            float radius = headCheckRadius > 0 ? headCheckRadius : characterController.radius;

            return !Physics.CheckCapsule(currentTop, standingTop, radius, groundMask, QueryTriggerInteraction.Ignore);
        }

        void Update()
        {
            isHeadBlocked = !CanStandUp();

            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
            Lookvertical = Input.GetAxis("Mouse Y");
            Lookhorizontal = Input.GetAxis("Mouse X");

            if (isFocusPaused)
            {
                UpdateFocusCamera();
                return;
            }

            // Camera rotation: always allowed unless isPauseCamera or isPause (full pause)
            if (!isPauseCamera && !isPause)
            {
                UpdateCameraRotation();
            }

            // If body is paused, stop here (no movement, no crouch, no FOV update)
            if (isPauseBody) return;

            UpdateMovementAndGravity();
            UpdateCrouch();

            UpdateFOV();
        }

        private void UpdateMovementAndGravity()
        {
            bool inputActive = (Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f);
            isMoving = inputActive && canMove && !isPause && !isPauseBody;

            bool isGrounded = characterController.isGrounded;

            if (isGrounded)
            {
                if (JumpOnce)
                {
                    JumpOnce = false;
                    StartCoroutine(DelayJump());
                }

                if (moveDirection.y < 0)
                {
                    moveDirection.y = -2f;
                }

                isJumping = false;
                if (!isCroughing && !isPause) CanCrough = true;

                if (isCroughing) canJump = false;

                if (Input.GetButton("Jump") && canMove && canJump && !IsGroundedOnNoJumpLayer() && !isCroughing)
                {
                    JumpOnce = true;
                    moveDirection.y = jumpSpeed;
                    canJump = false;
                    isJumping = true;
                }
            }
            else
            {
                isJumping = true;
                CanCrough = false;
                moveDirection.y -= gravity * Time.deltaTime;
            }

            bool wantRun = CanRun && Input.GetKey(KeyCode.LeftShift) && !isCroughing && isMoving;
            isRunning = wantRun;

            float targetSpeed = walkingSpeed;
            if (isCroughing)
            {
                targetSpeed = CroughSpeed;
            }
            else if (isRunning)
            {
                targetSpeed = RuningSpeed;
            }

            if (!isMoving)
            {
                RunningValue = targetSpeed;
            }
            else
            {
                float accelRate = timeToRunning > 0 ? (RuningSpeed / timeToRunning) : 100f;
                RunningValue = Mathf.MoveTowards(RunningValue, targetSpeed, accelRate * Time.deltaTime);
            }

            float speed = canMove ? RunningValue : 0f;
            Vector3 targetVelocity = (transform.forward * vertical + transform.right * horizontal) * speed;

            moveDirection.x = targetVelocity.x;
            moveDirection.z = targetVelocity.z;

            if (!isPause)
            {
                characterController.Move(moveDirection * Time.deltaTime);
            }
        }

        private void UpdateCrouch()
        {
            if (Input.GetKeyDown(CroughKey) && !isPause)
            {
                if (isCroughing)
                {
                    if (characterController.isGrounded && CanStandUp())
                    {
                        isCroughing = false;
                        once = true;
                    }
                }
                else
                {
                    if (CanCrough)
                    {
                        isCroughing = true;
                        canJump = false;
                        once = true;
                    }
                }
            }

            // Auto‑sit removed

            // --- LERP ---
            float targetHeight = isCroughing ? CroughHeight : originalHeight;
            float targetSpeed = isCroughing ? CroughSpeed : originalWalkSpeed;

            float newHeight = Mathf.Lerp(characterController.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
            characterController.height = newHeight;
            walkingSpeed = Mathf.Lerp(walkingSpeed, targetSpeed, crouchTransitionSpeed * Time.deltaTime);

            float heightDelta = originalHeight - newHeight;
            Vector3 newCenter = originalCenter;
            newCenter.y -= heightDelta * 0.5f;
            characterController.center = newCenter;

            if (playerCapsuleCollider != null)
            {
                float capsuleTargetHeight = isCroughing ? (CroughHeight / originalHeight) * originalCapsuleColliderHeight : originalCapsuleColliderHeight;
                float newCapsuleHeight = Mathf.Lerp(playerCapsuleCollider.height, capsuleTargetHeight, crouchTransitionSpeed * Time.deltaTime);
                playerCapsuleCollider.height = newCapsuleHeight;

                float capsuleDelta = originalCapsuleColliderHeight - newCapsuleHeight;
                Vector3 newCapsuleCenter = originalCapsuleColliderCenter;
                newCapsuleCenter.y -= capsuleDelta * 0.5f;
                playerCapsuleCollider.center = newCapsuleCenter;
            }

            if (Camera != null)
            {
                Vector3 camPos = Camera.localPosition;
                camPos.y = characterController.center.y + (characterController.height * 0.5f) - (characterController.radius * 0.5f);
                Camera.localPosition = camPos;
            }

            if (!isCroughing && characterController.height >= originalHeight - 0.001f && once)
            {
                once = false;
                StartCoroutine(DelayJump());
            }
        }

        private void UpdateCameraRotation()
        {
            rotationX += -Lookvertical * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            Camera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

            transform.Rotate(Vector3.up * Lookhorizontal * lookSpeed);
        }

        private void UpdateFocusCamera()
        {
            if (focusTarget == null)
            {
                UnPauseFocus();
                return;
            }

            Vector3 dir = focusTarget.position - Camera.position;
            if (dir.sqrMagnitude < 0.0001f) return;

            Quaternion targetRot = Quaternion.LookRotation(dir);

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            focusMouseOffset.x += mouseX * focusLookSpeed * Time.deltaTime;
            focusMouseOffset.y += -mouseY * focusLookSpeed * Time.deltaTime;

            focusMouseOffset = Vector2.Lerp(focusMouseOffset, Vector2.zero, focusReturnSpeed * Time.deltaTime);
            focusMouseOffset = Vector2.ClampMagnitude(focusMouseOffset, focusMaxAngle);

            Quaternion offsetRot = Quaternion.Euler(focusMouseOffset.y, focusMouseOffset.x, 0);
            Camera.rotation = targetRot * offsetRot;
        }

        private void UpdateFOV()
        {
            float targetFOV = (isRunning && isMoving) ? RunningFOV : originalFOV;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, SpeedToFOV * Time.deltaTime);
        }

        private void ToggleEffects(bool enable)
        {
            if (headBob != null) headBob.Enabled = enable;
            if (movementEffects != null) movementEffects.enabled = enable;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            Vector3 groundCenter = transform.position + groundCheckOffset;
            float groundRadius = groundCheckScale.x;
            Gizmos.DrawSphere(groundCenter, groundRadius);
            Gizmos.DrawWireSphere(groundCenter, groundRadius);

            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                float standHeight = (originalHeight > 0) ? originalHeight : cc.height;
                Vector3 standCenter = (originalHeight > 0) ? originalCenter : cc.center;

                Vector3 currentTop = transform.position + cc.center +
                                     Vector3.up * (cc.height * 0.5f) +
                                     headCheckOffset;

                Vector3 standingTop = transform.position + standCenter +
                                      Vector3.up * (standHeight * 0.5f) +
                                      headCheckOffset +
                                      Vector3.up * headCheckExtraHeight;

                float radius = (headCheckRadius > 0) ? headCheckRadius : cc.radius;

                Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
                Gizmos.DrawSphere(currentTop, radius);
                Gizmos.DrawSphere(standingTop, radius);
                Gizmos.DrawLine(currentTop, standingTop);

                Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                int segments = 10;
                for (int i = 0; i <= segments; i++)
                {
                    float t = i / (float)segments;
                    Vector3 point = Vector3.Lerp(currentTop, standingTop, t);
                    Gizmos.DrawWireSphere(point, radius);
                }
            }
        }
    }
}