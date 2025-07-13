using System.Collections;
using System.Collections.Generic;

using System.Linq;

using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;

#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController_RobotSphere : MonoBehaviour
    {
        [Header("Player")]
        public bool canPlay = false;

        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        int  directionValue = 0;
        private int prevDirection = 0;
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        [SerializeField] Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        [Header("Can double/triple jump")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool canJump = true;
        public int jumpCount = 0;
        public int jumpLimit = 1;

        [Header("Dash Settings")]
        bool dashUsed = false;
        public float dashForce = 10f;
        public float dashDuration = 0.2f;
        public float dashCooldown = 1f;
        public bool canDash = true;

        private bool isDashing = false;
        private float dashTimer;
        private float dashCooldownTimer;
        private Vector3 dashDirection;

        [SerializeField] int dashLimit = 0;

        public float doublePressThreshold = 0.3f;
        private float lastKeyPressTime = -1f;
        private KeyCode lastKeyPressed = KeyCode.None;

        [Header("Shield abilities")]
        [SerializeField] List<Shield> shieldList;

        [Header("Main Chip abilities")]
        public int has_OS = 0;
        public int has_Jump = 0;
        public int has_Vision = 0;
        public int has_Shield = 0;
        public int has_Logic = 0;
        public int has_Dash = 0;

        [Header("Other Chip abilities")]
        [SerializeField] int has_Overclocking = 0;

        [Header("Other Chip abilities")]
        [SerializeField] KeyCode jump_KeyCode = KeyCode.Space;
        [SerializeField] KeyCode sprint_KeyCode = KeyCode.LeftShift;
        [SerializeField] KeyCode moveLeft_KeyCode = KeyCode.A;
        [SerializeField] KeyCode moveRight_KeyCode = KeyCode.D;

        [Header("VFX")]
        [SerializeField] ParticleSystem dash_VFX;
        [SerializeField] ParticleSystem death_VFX;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        void ResetVFX()
        {
            dash_VFX.Stop();
        }

        private void Awake()
        {
            ResetVFX();

            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = _animator != null;
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            StartCoroutine(Open());
        }

        List<KeyCode> inputList = new List<KeyCode>();

        private void Update()
        {
            _hasAnimator = _animator != null;

            JumpAndGravity();

            if(!canPlay)
            {
                return;
            }

            // Handle jump input before checking grounded state
            if(Input.GetKeyDown(jump_KeyCode))
                Jump();

            
            HandleDash();

            GroundedCheck();
            Move();
        }

        private void LateUpdate()
        {
            // CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }

            if (Grounded && _verticalVelocity <= 0f)
            {
                jumpCount = 0;
            }
        }

        private void CameraRotation()
        {
            // // if there is an input and camera position is not fixed
            // if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            // {
            //     //Don't multiply mouse input by Time.deltaTime;
            //     float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            //     _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            //     _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            // }

            // // clamp our rotations so our values are limited 360 degrees
            // _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            // _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // // Cinemachine will follow this target
            // CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            //     _cinemachineTargetYaw, 0.0f);
        }

        public float SprintAcceleration = 0.01f;

        private void Move()
        {
            bool isSprinting = Input.GetKey(sprint_KeyCode);
            bool isMoving = Input.GetKey(moveLeft_KeyCode) || Input.GetKey(moveRight_KeyCode);

            // Debug.Log($"prevDirection: {prevDirection}");
            // Debug.Log($"directionValue: {directionValue}");

            if(isMoving)
            {
                if(isSprinting)
                {
                    AudioManager.Instance?.PlaySFX("Roll", 1);
                }
                else
                {
                    AudioManager.Instance?.PlaySFX("Walk", 1);
                }

                directionValue = Input.GetKey(moveLeft_KeyCode) ? -1 : 1;

                if(prevDirection != directionValue)
                {
                    // Debug.LogError($"prevDirection != directionValue");
                    SprintAcceleration = 0.01f + has_Overclocking * 0.01f * 2;
                }

                prevDirection = directionValue;
            }
            else
            {
                directionValue = 0;
                SprintAcceleration = 0.01f + has_Overclocking * 0.01f * 2;
            }

            Vector2 moveAxis = new Vector2(isMoving ? directionValue : 0, 0);
            
            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon
            if(isSprinting)
            {
                SprintAcceleration += 0.01f + has_Overclocking * 0.01f * 2;
            }
            else
            {
                SprintAcceleration = 0.01f + has_Overclocking * 0.01f * 2;
            }

            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = isSprinting ? SprintSpeed + (has_Overclocking * SprintSpeed * 1.5f) + SprintAcceleration : MoveSpeed + (has_Overclocking * MoveSpeed * 2);

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (moveAxis == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? moveAxis.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(moveAxis.x, 0.0f, moveAxis.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (moveAxis != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;

                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);
            
                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            Vector3 move = isDashing
                ? dashDirection * dashForce 
                : targetDirection.normalized * (_speed);

            _controller.Move(move * Time.deltaTime + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        void Jump()
        {
            canJump = has_Jump > 0 && jumpCount < jumpLimit;

            if(canJump)
            {
                // SFX
                AudioManager.Instance?.PlaySFX("Jump", 1, true);

                // Jump
                if (_jumpTimeoutDelta <= 0.0f)
                {

                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                Grounded = false;
                jumpCount++;
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                // _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        void HandleDash()
        {
            if (isDashing)
            {
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0f)
                {
                    isDashing = false;
                    canDash = false;
                    dashCooldownTimer = dashCooldown;
                }
            }
            else
            {
                dash_VFX.Stop();

                if (dashCooldownTimer > 0f)
                {
                    dashCooldownTimer -= Time.deltaTime;
                }

                if (dashCooldownTimer <= 0f)
                {
                    dashLimit = has_Dash;
                    float yAngle = transform.eulerAngles.y;
                    float tolerance = 10f; // adjust as needed

                    if (Mathf.Abs(Mathf.DeltaAngle(yAngle, 90f)) <= tolerance)
                    {
                        // Angle is close to 90 degrees
                        canDash = true;
                    }
                    else if (Mathf.Abs(Mathf.DeltaAngle(yAngle, 270f)) <= tolerance)
                    {
                        // Angle is close to 270 degrees
                        canDash = true;
                    }
                    else
                    {
                        canDash = false;
                    }
                }

                if(Input.GetKeyDown(moveLeft_KeyCode) || Input.GetKeyDown(moveRight_KeyCode))
                {
                    KeyCode keyPressed = Input.GetKeyDown(moveLeft_KeyCode) ? moveLeft_KeyCode : moveRight_KeyCode;

                    if(canDash || dashLimit > 0 && (lastKeyPressed == keyPressed))
                    {
                        if (Time.time - lastKeyPressTime <= doublePressThreshold)
                        {
                            Debug.Log("Double press detected!");
                            // Reset to avoid triple-press being detected as second double-press
                            lastKeyPressTime = -1f;

                            StartDash();
                        }
                        else
                        {
                            lastKeyPressTime = Time.time;
                        }
                    }

                    lastKeyPressed = Input.GetKeyDown(moveLeft_KeyCode) ? moveLeft_KeyCode : moveRight_KeyCode;
                }
            }
        }

        void StartDash()
        {
            dashLimit--;

            dash_VFX.Play();
            isDashing = true;
            dashTimer = dashDuration;

            // Use movement direction if available, otherwise forward
            Vector3 inputDir = new Vector3(_input.move.x, 0, _input.move.y).normalized;

            if (inputDir.sqrMagnitude > 0.1f)
            {
                dashDirection = Quaternion.Euler(0f, _mainCamera.transform.eulerAngles.y, 0f) * inputDir;
            }
            else
            {
                dashDirection = transform.forward;
            }

            // Optional: cancel vertical velocity while dashing
            _verticalVelocity = 0f;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        public void AssignChipsValues(List<Chip> _chipList)
        {
            has_OS              = _chipList.Where(chip => chip.chipType == ChipType.OS).Count();
            has_Jump            = _chipList.Where(chip => chip.chipType == ChipType.Jump).Count();
            has_Vision          = _chipList.Where(chip => chip.chipType == ChipType.Vision).Count();
            has_Shield          = _chipList.Where(chip => chip.chipType == ChipType.Shield).Count();
            has_Logic           = _chipList.Where(chip => chip.chipType == ChipType.Logic).Count();
            has_Overclocking    = _chipList.Where(chip => chip.chipType == ChipType.Overclock).Count();
            has_Dash            = _chipList.Where(chip => chip.chipType == ChipType.Dash).Count();

            ActivateChips();
        }

        public void ActivateChips()
        {
            if(has_OS == 0 && has_Overclocking == 0)
            {
                StartCoroutine(Suicide());
            }

            // Assign overclocking effect
            if(has_Overclocking > 0)
            {
                has_Jump        = has_Jump   * (has_Jump + 1);
                has_Shield      = has_Shield * (has_Shield + 1);
                has_Dash        = has_Dash   * (has_Dash + 1);
                has_Logic       = 1;
            }

            // Assign logic effect
            if(has_Logic > 0)
            {
                ResetKeyCodes();
            }
            else
            {
                ScrambleKeyCodes();
            }

            jumpLimit           = has_Jump;
            dashLimit           = has_Dash;

            foreach(Shield shield in shieldList)
            {
                shield.gameObject.SetActive(false);
            }
            
            for(int i = 0; i < has_Shield; i++)
            {
                shieldList[i].gameObject.SetActive(true);
            }
        }

        void ScrambleKeyCodes()
        {
            List<KeyCode> shuffleKeys = new List<KeyCode>
            {
                jump_KeyCode,
                sprint_KeyCode,
                moveLeft_KeyCode,
                moveRight_KeyCode,
            };

            shuffleKeys.Shuffle();
            
            jump_KeyCode        = shuffleKeys[0];
            sprint_KeyCode      = shuffleKeys[1];
            moveLeft_KeyCode    = shuffleKeys[2];
            moveRight_KeyCode   = shuffleKeys[3];
        }

        void ResetKeyCodes()
        {
            jump_KeyCode = KeyCode.Space;
            sprint_KeyCode = KeyCode.LeftShift;
            moveLeft_KeyCode = KeyCode.A;
            moveRight_KeyCode = KeyCode.D;
        }

        public IEnumerator Open()
        {
            _animator.Play("Open");

            yield return new WaitForSeconds(3);

            canPlay = true;
        }

        public void Reset()
        {
            StartCoroutine(Open());
        }

        public IEnumerator Suicide()
        {
            canPlay = false;
            
            _animator.Play("Shutdown");

            yield return new WaitForSeconds(2);

            yield return null;
             CheckpointManager.Instance.OnPlayerDeath();
            // Death sequence, replay from next checkpoint
        }

        public IEnumerator Death()
        {
            canPlay = false;

            _animator.Play("Close");

            // Play explosion VFX and SFX
            death_VFX.Play();

            yield return new WaitForSeconds(2);

            yield return null;
            CheckpointManager.Instance?.OnPlayerDeath();
            // Death sequence, replay from next checkpoint
        }

        public void TakeDamage()
        {
            AudioManager.Instance?.PlaySFX("Damage", 1, true);
            if(has_Shield > 0)
            {
                shieldList[has_Shield - 1].BreakShield();

                has_Shield--;
            }
            else
            {
                StartCoroutine(Death());
            }
        }
    }
}

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random rng = new System.Random(); // You can also use UnityEngine.Random if preferred
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1); // 0 <= k <= n
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}