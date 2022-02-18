using UnityEngine;

/// <summary>
/// One of the main classes responsible for player movement
/// </summary>
public class CharacterController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Maximum movement speed in m/s")] public float speed = 5.0f;
    [Tooltip("Maximum turning speed in deg/s")] public float angularSpeed = 540.0f;
    [Tooltip("The rate of change of velocity")] public float acceleration = 50.0f;
    [Tooltip("The rate at which the character's slows down")] public float deceleration = 20.0f;
    [Tooltip("Affects movement control, higher values allow faster changes in direction")] public float groundFriction = 8f;
    [Tooltip("Should brakingFriction be used to slow the character? ")] public bool useBrakingFriction;
    [Tooltip("Friction applied when braking")] public float brakingFriction = 8f;
    [Tooltip("Friction applied when in the air")] public float airFriction;
    [Tooltip("The amount of control available to the player in the air"), Range(0, 1)] public float airControl = 0.2f;

    [Header("Crouch")]
    [Tooltip("The character's capsule height while standing.")] public float standingHeight = 2.0f;
    [Tooltip("The character's capsule height while crouching.")] public float crouchingHeight = 1.0f;

    [Header("Jump")]
    [Tooltip("The initial jump height in meters")] public float baseJumpHeight = 1.5f;
    [Tooltip("The extra jump time in seconds")] public float extraJumpTime = 1f;
    [Tooltip("Acceleration while jump button is held down, given in meters / sec^2")] public float extraJumpPower = 25.0f;
    [Tooltip("How early before hitting the ground you can press jump again")] public float jumpPreGroundedToleranceTime = 0.15f;
    [Tooltip("How long after leaving the ground you can press jump again")] public float jumpPostGroundedToleranceTime = 0.15f;
    [Tooltip("Maximum mid-air jumps")] public float maxMidAirJumps = 1;

    [Header("First Person")]
    [Tooltip("Speed when moving forward")] public float forwardSpeed = 5.0f;
    [Tooltip("Speed when moving backwards")] public float backwardSpeed = 3.0f;
    [Tooltip("Speed when moving sideways")] public float strafeSpeed = 4.0f;
    [Tooltip("Speed multiplier while running")] public float runSpeedMultiplier = 2.0f;

    [Header("Headbob")]
    [Tooltip("The animator responsible for headbobbing")] public Animator cameraAnimator;
    [Tooltip("The speed the animation plays")] public float cameraAnimSpeed = 1.0f;

    [HideInInspector] public bool canJump = true;
    [HideInInspector] public bool jump;
    [HideInInspector] public bool isJumping;

    [HideInInspector] public bool updateJumpTimer;
    [HideInInspector] public float jumpTimer;
    [HideInInspector] public float jumpButtonHeldDownTimer;
    [HideInInspector] public float jumpUngroundedTimer;

    [HideInInspector] public int midAirJumpCount;

    [HideInInspector] public bool allowVerticalMovement;
    [HideInInspector] public bool restoreVelocityOnResume = true;

    [HideInInspector] public CharacterMovement movement;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Transform cameraPivotTransform;
    [HideInInspector] public Transform cameraTransform;
    [HideInInspector] public MouseLook mouseLook;

    [HideInInspector] public bool isFalling { get { return !movement.groundDetection.groundHit.isOnGround && movement.velocity.y < 0.0001f; } }
    [HideInInspector] public Vector3 moveDirection;
    [HideInInspector] public Vector3 oldMoveDirection;

    [HideInInspector] public bool pause;
    [HideInInspector] public bool isPaused;
    [HideInInspector] public bool crouch;
    [HideInInspector] public bool isCrouching;

    [HideInInspector] private int _verticalParamId;
    [HideInInspector] private int _horizontalParamId;

    void Pause()
    {
        if (pause && !isPaused)
        {
            movement.Pause(true);
            isPaused = true;

            cameraAnimator.SetFloat(_verticalParamId, 0);
            cameraAnimator.SetFloat(_horizontalParamId, 0);
        }
        else if (!pause && isPaused)
        {
            movement.Pause(false, restoreVelocityOnResume);
            isPaused = false;
        }
    }

    public void RotateTowards(Vector3 direction, bool onlyLateral = true) { movement.Rotate(direction, angularSpeed, onlyLateral); }
    public void RotateTowardsMoveDirection(bool onlyLateral = true) { RotateTowards(moveDirection, onlyLateral); }
    public void RotateTowardsVelocity(bool onlyLateral = true) { RotateTowards(movement.velocity, onlyLateral); }

    void Jump()
    {
        if (isJumping)
        {
            if (!movement.wasGrounded && movement.groundDetection.groundHit.isOnGround)
                isJumping = false;
        }

        if (jump && canJump && movement.groundDetection.groundHit.isOnGround)
        {
            canJump = false;
            isJumping = true;
            updateJumpTimer = true;

            movement.ApplyVerticalImpulse(Mathf.Sqrt(2.0f * baseJumpHeight * movement.gravity.magnitude));
            movement.DisableGrounding();
        }
    }

    void MidAirJump()
    {
        if (movement.groundDetection.groundHit.isOnGround) midAirJumpCount = 0;

        if (jump && canJump && !movement.groundDetection.groundHit.isOnGround && midAirJumpCount < maxMidAirJumps)
        {
            midAirJumpCount++;

            canJump = false;
            isJumping = true;
            updateJumpTimer = true;

            movement.ApplyVerticalImpulse(Mathf.Sqrt(2.0f * baseJumpHeight * movement.gravity.magnitude));
            movement.DisableGrounding();
        }
    }

    void Crouch()
    {
        if (crouch)
        {
            if (isCrouching) return;

            movement.SetCapsuleHeight(crouchingHeight);
            isCrouching = true;
        }
        else
        {
            if (!isCrouching || !movement.ClearanceCheck(standingHeight)) return;

            movement.SetCapsuleHeight(standingHeight);
            isCrouching = false;
        }
    }

    Vector3 CalcDesiredVelocity() { speed = GetTargetSpeed(); return transform.TransformDirection(moveDirection * speed); }

    void Move()
    {
        var desiredVelocity = CalcDesiredVelocity();

        var currentFriction = movement.groundDetection.groundHit.isOnGround ? groundFriction : airFriction;
        var currentBrakingFriction = useBrakingFriction ? brakingFriction : currentFriction;

        movement.Move(desiredVelocity, speed, acceleration, deceleration, currentFriction, currentBrakingFriction, !allowVerticalMovement);

        Jump();
        MidAirJump();

        if (oldMoveDirection != moveDirection)
        {
            oldMoveDirection = moveDirection;
        }
    }

    float GetTargetSpeed()
    {
        var targetSpeed = forwardSpeed;

        if (moveDirection.x > 0.0f || moveDirection.x < 0.0f) targetSpeed = strafeSpeed;
        if (moveDirection.z < 0.0f) targetSpeed = backwardSpeed;
        if (moveDirection.z > 0.0f) targetSpeed = forwardSpeed;

        return targetSpeed;
    }

    void AnimateView()
    {
        var yScale = isCrouching ? Mathf.Clamp01(crouchingHeight / standingHeight) : 1.0f;
        cameraPivotTransform.localScale = Vector3.MoveTowards(cameraPivotTransform.localScale, new Vector3(1.0f, yScale, 1.0f), 5.0f * Time.deltaTime);
    }

    void AnimateCamera()
    {
        var lateralVelocity = Vector3.ProjectOnPlane(movement.velocity, transform.up);
        var normalizedSpeed = Mathf.InverseLerp(0.0f, forwardSpeed, lateralVelocity.magnitude);

        cameraAnimator.speed = Mathf.Max(0.5f, cameraAnimSpeed * normalizedSpeed);

        const float dampTime = 0.1f;

        cameraAnimator.SetFloat(_verticalParamId, moveDirection.z, dampTime, Time.deltaTime);
        cameraAnimator.SetFloat(_horizontalParamId, moveDirection.x, dampTime, Time.deltaTime);
    }

    void Animate() { AnimateCamera(); }
    void UpdateRotation() { mouseLook.LookRotation(movement, cameraTransform); }

    void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        movement.platformUpdatesRotation = true;

        animator = GetComponentInChildren<Animator>();
        mouseLook = GetComponent<MouseLook>();

        cameraPivotTransform = transform.Find("Camera_Pivot");
        cameraTransform = GetComponentInChildren<Camera>().transform;

        mouseLook.Init(transform, cameraTransform);
        moveDirection = new Vector3();

        _verticalParamId = Animator.StringToHash("vertical");
        _horizontalParamId = Animator.StringToHash("horizontal");
    }

    void FixedUpdate()
    {
        Pause();
        if (isPaused) return;

        Move();
        Crouch();
    }

    void Update()
    {
        moveDirection.x = Input.GetAxisRaw("Horizontal");
        moveDirection.z = Input.GetAxisRaw("Vertical");

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1.0f);

        if (jump && Input.GetButton("Jump") == false)
        {
            canJump = true;
            jumpButtonHeldDownTimer = 0.0f;
        }

        jump = Input.GetButton("Jump");
        crouch = Input.GetKey(KeyCode.LeftControl);

        if (jump) jumpButtonHeldDownTimer += Time.deltaTime;

        if (isPaused) return;

        UpdateRotation();
        Animate();
    }

    void LateUpdate()
    {
        AnimateView();
    }
}