using UnityEngine;

public class DW_ThirdPersonController : MonoBehaviour {
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip runAnimation;
    public AnimationClip jumpPoseAnimation;

    public float walkMaxAnimationSpeed = 0.75f;
    public float trotMaxAnimationSpeed = 1.0f;
    public float runMaxAnimationSpeed = 1.0f;
    public float jumpAnimationSpeed = 1.15f;
    public float landAnimationSpeed = 1.0f;

    private Animation _animation;

    private enum CharacterState {
        Idle = 0,
        Walking = 1,
        Trotting = 2,
        Running = 3,
        Jumping = 4,
    }

    private CharacterState _characterState;

    // Camera that is watching the controller
    public Camera Camera;
    // The speed when walking
    public float WalkSpeed = 2.0f;
    // after trotAfterSeconds of walking we trot with trotSpeed
    public float TrotSpeed = 4.0f;
    // when pressing "Fire3" button (cmd) we start running
    public float RunSpeed = 6.0f;

    public float InAirControlAcceleration = 3.0f;

    // How high do we jump when pressing jump and letting go immediately
    public float JumpHeight = 0.5f;

    // The gravity for the character
    public float Gravity = 20.0f;
    // The gravity in controlled descent mode
    public float SpeedSmoothing = 10.0f;
    public float RotateSpeed = 500.0f;
    public float TrotAfterSeconds = 3.0f;

    public bool CanJump = true;

    [HideInInspector]
    public float VerticalSpeed = 0.0f;

    private const float JumpRepeatTime = 0.05f;
    private const float JumpTimeout = 0.15f;
    private const float GroundedTimeout = 0.25f;

    // The camera doesnt start following the target immediately but waits for a split second to avoid too much waving around.
    private float _lockCameraTimer = 0.0f;

    // The current move direction in x-z
    private Vector3 _moveDirection = Vector3.zero;
    // The current vertical speed

    // The current x-z move speed
    private float _moveSpeed = 0.0f;

    // The last collision flags returned from controller.Move
    private CollisionFlags _collisionFlags;

    // Are we jumping? (Initiated with jump button and not grounded yet)
    private bool _jumping = false;
    private bool _jumpingReachedApex = false;

    // Are we moving backwards (This locks the camera to not do a 180 degree spin)
    private bool _movingBack = false;
    // Is the user pressing any keys?
    private bool _isMoving = false;
    // When did the user start walking (Used for going into trot after a while)
    private float _walkTimeStart = 0.0f;
    // Last time the jump button was clicked down
    private float _lastJumpButtonTime = -10.0f;
    // Last time we performed a jump
    private float _lastJumpTime = -1.0f;

    private Vector3 _inAirVelocity = Vector3.zero;

    private float _lastGroundedTime = 0.0f;
    private bool _isControllable = true;

    private void Awake() {
        _moveDirection = transform.TransformDirection(Vector3.forward);

        _animation = GetComponent<Animation>();
        if (!_animation) {
            Debug.Log("The character you would like to control doesn't have animations. Moving her might look weird.");
        }

        if (!idleAnimation) {
            _animation = null;
            Debug.Log("No idle animation found. Turning off animations.");
        }
        if (!walkAnimation) {
            _animation = null;
            Debug.Log("No walk animation found. Turning off animations.");
        }
        if (!runAnimation) {
            _animation = null;
            Debug.Log("No run animation found. Turning off animations.");
        }
        if (!jumpPoseAnimation && CanJump) {
            _animation = null;
            Debug.Log("No jump animation found and the character has canJump enabled. Turning off animations.");
        }
    }

    private void UpdateSmoothedMovementDirection() {
        bool grounded = IsGrounded();

        // Forward vector relative to the camera along the x-z plane	
        Vector3 forward = Camera.transform.TransformDirection(Vector3.forward);
        forward.y = 0;
        forward = forward.normalized;

        // Right vector relative to the camera
        // Always orthogonal to the forward vector
        Vector3 right = new Vector3(forward.z, 0, -forward.x);

        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        // Are we moving backwards or looking backwards
        if (v < -0.2f) {
            _movingBack = true;
        } else {
            _movingBack = false;
        }

        bool wasMoving = _isMoving;
        _isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

        // Target direction relative to the camera
        Vector3 targetDirection = h * right + v * forward;

        // Grounded controls
        if (grounded) {
            // Lock camera for short period when transitioning moving & standing still
            _lockCameraTimer += Time.deltaTime;
            if (_isMoving != wasMoving) {
                _lockCameraTimer = 0.0f;
            }

            // We store speed and direction seperately,
            // so that when the character stands still we still have a valid forward direction
            // moveDirection is always normalized, and we only update it if there is user input.
            if (targetDirection != Vector3.zero) {
                // If we are really slow, just snap to the target direction
                if (_moveSpeed < WalkSpeed * 0.9f && grounded) {
                    _moveDirection = targetDirection.normalized;
                }
                    // Otherwise smoothly turn towards it
                else {
                    _moveDirection = Vector3.RotateTowards(_moveDirection, targetDirection,
                                                           RotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);

                    _moveDirection = _moveDirection.normalized;
                }
            }

            // Smooth the speed based on the current target direction
            float curSmooth = SpeedSmoothing * Time.deltaTime;

            // Choose target speed
            //* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways
            float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);

            _characterState = CharacterState.Idle;

            // Pick speed modifier
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                targetSpeed *= RunSpeed;
                _characterState = CharacterState.Running;
            } else if (Time.time - TrotAfterSeconds > _walkTimeStart) {
                targetSpeed *= TrotSpeed;
                _characterState = CharacterState.Trotting;
            } else {
                targetSpeed *= WalkSpeed;
                _characterState = CharacterState.Walking;
            }

            _moveSpeed = Mathf.Lerp(_moveSpeed, targetSpeed, curSmooth);

            // Reset walk time start when we slow down
            if (_moveSpeed < WalkSpeed * 0.3f) {
                _walkTimeStart = Time.time;
            }
        }
            // In air controls
        else {
            // Lock camera while in air
            if (_jumping) {
                _lockCameraTimer = 0.0f;
            }

            if (_isMoving) {
                _inAirVelocity += targetDirection.normalized * Time.deltaTime * InAirControlAcceleration;
            }
        }
    }

    private void ApplyJumping() {
        // Prevent jumping too fast after each other
        if (_lastJumpTime + JumpRepeatTime > Time.time) {
            return;
        }

        if (IsGrounded()) {
            // Jump
            // - Only when pressing the button down
            // - With a timeout so you can press the button slightly before landing		
            if (CanJump && Time.time < _lastJumpButtonTime + JumpTimeout) {
                VerticalSpeed = CalculateJumpVerticalSpeed(JumpHeight);
                SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private void ApplyGravity() {
        if (_isControllable) // don't move player at all if not controllable.
        {
            // Apply gravity
            // When we reach the apex of the jump we send out a message
            if (_jumping && !_jumpingReachedApex && VerticalSpeed <= 0.0f) {
                _jumpingReachedApex = true;
                SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
            }

            if (IsGrounded()) {
                VerticalSpeed = 0.0f;
            } else {
                VerticalSpeed -= Gravity * Time.deltaTime;
            }
        }
    }

    private float CalculateJumpVerticalSpeed(float targetJumpHeight) {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2f * targetJumpHeight * Gravity);
    }

    private void DidJump() {
        _jumping = true;
        _jumpingReachedApex = false;
        _lastJumpTime = Time.time;
        _lastJumpButtonTime = -10;

        _characterState = CharacterState.Jumping;
    }

    private void Update() {
        if (!_isControllable) {
            // kill all inputs if not controllable.
            Input.ResetInputAxes();
        }

        if (Camera == null) {
            Camera = Camera.main ?? Camera.allCameras[0];
        }

        if (Input.GetButtonDown("Jump")) {
            _lastJumpButtonTime = Time.time;
        }

        UpdateSmoothedMovementDirection();

        // Apply gravity
        // - extra power jump modifies gravity
        // - controlledDescent mode modifies gravity
        ApplyGravity();

        // Apply jumping logic
        ApplyJumping();

        // Calculate actual motion
        Vector3 movement = _moveDirection * _moveSpeed + new Vector3(0, VerticalSpeed, 0) + _inAirVelocity;
        movement *= Time.deltaTime;

        // Move the controller
        CharacterController controller = GetComponent<CharacterController>();
        _collisionFlags = controller.Move(movement);

        // ANIMATION sector
        if (_animation) {
            if (_characterState == CharacterState.Jumping) {
                if (!_jumpingReachedApex) {
                    _animation[jumpPoseAnimation.name].speed = jumpAnimationSpeed;
                    _animation[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
                    _animation.CrossFade(jumpPoseAnimation.name);
                } else {
                    _animation[jumpPoseAnimation.name].speed = -landAnimationSpeed;
                    _animation[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
                    _animation.CrossFade(jumpPoseAnimation.name);
                }
            } else {
                if (controller.velocity.sqrMagnitude < 0.1f) {
                    _animation.CrossFade(idleAnimation.name);
                } else {
                    if (_characterState == CharacterState.Running) {
                        _animation[runAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0f,
                                                                          runMaxAnimationSpeed);
                        _animation.CrossFade(runAnimation.name);
                    } else if (_characterState == CharacterState.Trotting) {
                        _animation[walkAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0f,
                                                                           trotMaxAnimationSpeed);
                        _animation.CrossFade(walkAnimation.name);
                    } else if (_characterState == CharacterState.Walking) {
                        _animation[walkAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0f,
                                                                           walkMaxAnimationSpeed);
                        _animation.CrossFade(walkAnimation.name);
                    }
                }
            }
        }
        // ANIMATION sector

        // Set rotation to the move direction
        if (IsGrounded()) {
            transform.rotation = Quaternion.LookRotation(_moveDirection);
        } else {
            Vector3 xzMove = movement;
            xzMove.y = 0;
            if (xzMove.sqrMagnitude > 0.001f) {
                transform.rotation = Quaternion.LookRotation(xzMove);
            }
        }

        // We are in jump mode but just became grounded
        if (IsGrounded()) {
            _lastGroundedTime = Time.time;
            _inAirVelocity = Vector3.zero;
            if (_jumping) {
                _jumping = false;
                SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) {
        //	Debug.DrawRay(hit.point, hit.normal);
        if (hit.moveDirection.y > 0.01f) {
            return;
        }
    }

    private float GetSpeed() {
        return _moveSpeed;
    }

    private bool IsJumping() {
        return _jumping;
    }

    private bool IsGrounded() {
        return (_collisionFlags & CollisionFlags.CollidedBelow) != 0;
    }

    private Vector3 GetDirection() {
        return _moveDirection;
    }

    private bool IsMovingBackwards() {
        return _movingBack;
    }

    private float GetLockCameraTimer() {
        return _lockCameraTimer;
    }

    private bool IsMoving() {
        return Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f;
    }

    private bool HasJumpReachedApex() {
        return _jumpingReachedApex;
    }

    private bool IsGroundedWithTimeout() {
        return _lastGroundedTime + GroundedTimeout > Time.time;
    }

    private void Reset() {
        gameObject.tag = "Player";
    }
}