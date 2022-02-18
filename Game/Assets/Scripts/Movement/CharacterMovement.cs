using System.Collections;
using UnityEngine;

/// <summary>
/// An important part of Character Movement responsible for the lower level part of moving the player
/// </summary>
public class CharacterMovement : MonoBehaviour
{
    #region Singleton
    public static CharacterMovement instance;
    void Awake() { instance = this; }
    #endregion

    [Header("Basic Settings")]
    [Tooltip("The maximum lateral speed this character can move")] public float maxLateralSpeed = 10.0f;
    [Tooltip("The gravity applied to this character.")] public Vector3 gravity = new Vector3(0, -20f, 0);

    static readonly Collider[] OverlappedColliders = new Collider[8];
    Coroutine _lateFixedUpdateCoroutine;

    Vector3 _normal;
    float _referenceCastDistance;

    bool _forceUnground;
    float _forceUngroundTimer;
    bool _performGroundDetection = true;

    Vector3 _savedVelocity;
    Vector3 _savedAngularVelocity;

    bool useGravity = true;
    bool isGod;
    public bool Testing;

    public bool wasGrounded { get { return groundDetection.prevGroundHit.isOnGround && groundDetection.prevGroundHit.isValidGround; } }
    public bool wasOnGround { get { return groundDetection.prevGroundHit.isOnGround; } }

    [HideInInspector] public bool isOnPlatform;
    [HideInInspector] public bool nearStep;

    public Vector3 platformVelocity;
    public Vector3 platformAngularVelocity;
    public bool platformUpdatesRotation;

    public Vector3 velocity { get { return cachedRigidbody.velocity - platformVelocity; } set { cachedRigidbody.velocity = value + platformVelocity; } }
    public float forwardSpeed { get { return Vector3.Dot(velocity, transform.forward); } }
    public Quaternion rotation { get { return cachedRigidbody.rotation; } set { cachedRigidbody.MoveRotation(value); }}

    [HideInInspector] public GroundDetection groundDetection;
    Rigidbody cachedRigidbody;
    CharacterController controller;


    public void Pause(bool pause, bool restoreVelocity = true)
    {
        if (pause)
        {
            _savedVelocity = cachedRigidbody.velocity;
            _savedAngularVelocity = cachedRigidbody.angularVelocity;

            cachedRigidbody.isKinematic = true;
        }
        else
        {
            cachedRigidbody.isKinematic = false;

            if (restoreVelocity)
            {
                cachedRigidbody.AddForce(_savedVelocity, ForceMode.VelocityChange);
                cachedRigidbody.AddTorque(_savedAngularVelocity, ForceMode.VelocityChange);
            }
            else
            {
                cachedRigidbody.AddForce(Vector3.zero, ForceMode.VelocityChange);
                cachedRigidbody.AddTorque(Vector3.zero, ForceMode.VelocityChange);
            }

            cachedRigidbody.WakeUp();
        }
    }

    public void SetCapsuleDimensions(Vector3 capsuleCenter, float capsuleRadius, float capsuleHeight)
    {
        groundDetection.capsuleCollider.center = capsuleCenter;
        groundDetection.capsuleCollider.radius = capsuleRadius;
        groundDetection.capsuleCollider.height = Mathf.Max(capsuleRadius * 0.5f, capsuleHeight);
    }

    public void SetCapsuleDimensions(float capsuleRadius, float capsuleHeight)
    {
        groundDetection.capsuleCollider.center = new Vector3(0.0f, capsuleHeight * 0.5f, 0.0f);
        groundDetection.capsuleCollider.radius = capsuleRadius;
        groundDetection.capsuleCollider.height = Mathf.Max(capsuleRadius * 0.5f, capsuleHeight);
    }

    public void SetCapsuleHeight(float capsuleHeight)
    {
        capsuleHeight = Mathf.Max(groundDetection.capsuleCollider.radius * 2.0f, capsuleHeight);

        groundDetection.capsuleCollider.center = new Vector3(0.0f, capsuleHeight * 0.5f, 0.0f);
        groundDetection.capsuleCollider.height = capsuleHeight;
    }

    void OverlapCapsule(Vector3 bottom, Vector3 top, float radius, out int overlapCount, LayerMask overlappingMask, QueryTriggerInteraction queryTriggerInteraction)
    {
        int colliderCount = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, OverlappedColliders, overlappingMask, queryTriggerInteraction);

        overlapCount = colliderCount;
        for (int i = 0; i < colliderCount; i++)
        {
            Collider overlappedCollider = OverlappedColliders[i];

            if (overlappedCollider != null && overlappedCollider != groundDetection.capsuleCollider) continue;
            if (i < --overlapCount) OverlappedColliders[i] = OverlappedColliders[overlapCount];
        }
    }

    public Collider[] OverlapCapsule(Vector3 position, Quaternion rotation, out int overlapCount, LayerMask overlapMask, QueryTriggerInteraction queryTrigger = QueryTriggerInteraction.Ignore)
    {
        float height = groundDetection.capsuleCollider.height * 0.5f - groundDetection.capsuleCollider.radius;

        Vector3 topSphereCenter = groundDetection.capsuleCollider.center + Vector3.up * height;
        Vector3 bottomSphereCenter = groundDetection.capsuleCollider.center - Vector3.up * height;

        Vector3 top = position + rotation * topSphereCenter;
        Vector3 bottom = position + rotation * bottomSphereCenter;

        int colliderCount = Physics.OverlapCapsuleNonAlloc(bottom, top, groundDetection.capsuleCollider.radius, OverlappedColliders, overlapMask, queryTrigger);

        overlapCount = colliderCount;
        for (int i = 0; i < colliderCount; i++)
        {
            Collider overlappedCollider = OverlappedColliders[i];

            if (overlappedCollider != null && overlappedCollider != groundDetection.capsuleCollider) continue;
            if (i < --overlapCount) OverlappedColliders[i] = OverlappedColliders[overlapCount];
        }

        return OverlappedColliders;
    }

    public bool ClearanceCheck(float clearanceHeight)
    {
        const float kTolerance = 0.01f;
        float radius = Mathf.Max(kTolerance, groundDetection.capsuleCollider.radius - kTolerance);

        float height = Mathf.Max(radius * 2.0f + kTolerance, clearanceHeight - kTolerance);
        float halfHeight = height * 0.5f;

        Vector3 center = new Vector3(0.0f, halfHeight, 0.0f);
        Vector3 up = transform.rotation * Vector3.up;

        Vector3 localBottom = center - up * Mathf.Max(0.0f, halfHeight - kTolerance) + up * radius;
        Vector3 localTop = center + up * halfHeight - up * radius;

        Vector3 bottom = transform.position + transform.rotation * localBottom;
        Vector3 top = transform.position + transform.rotation * localTop;

        int overlapCount;
        OverlapCapsule(bottom, top, radius, out overlapCount, groundDetection.overlapMask, groundDetection.triggerInteraction);

        return overlapCount == 0;
    }

    private void OverlapRecovery(ref Vector3 probingPosition, Quaternion probingRotation)
    {
        int overlapCount;
        Collider[] overlappedColliders = groundDetection.OverlapCapsule(probingPosition, probingRotation, out overlapCount);

        for (int i = 0; i < overlapCount; i++)
        {
            Rigidbody overlappedColliderRigidbody = overlappedColliders[i].attachedRigidbody;
            Transform overlappedColliderTransform = overlappedColliders[i].transform;

            if (overlappedColliderRigidbody != null) continue;

            float distance;
            Vector3 direction;
            if (!Physics.ComputePenetration(groundDetection.capsuleCollider, probingPosition, probingRotation, overlappedColliders[i],
                overlappedColliderTransform.position, overlappedColliderTransform.rotation, out direction, out distance)) continue;

            probingPosition += direction * distance;
        }
    }

    public bool ComputeGroundHit(Vector3 probingPosition, Quaternion probingRotation, out GroundHit groundHitInfo, float scanDistance = Mathf.Infinity)
    {
        groundHitInfo = groundDetection.groundHit;
        return groundDetection.ComputeGroundHit(probingPosition, probingRotation, scanDistance);
    }

    public bool ComputeGroundHit(out GroundHit hitInfo, float scanDistance = Mathf.Infinity) { return ComputeGroundHit(transform.position, transform.rotation, out hitInfo, scanDistance); }

    public void Rotate(Vector3 direction, float angularSpeed, bool onlyLateral = true)
    {
        if (onlyLateral) direction = Vector3.ProjectOnPlane(direction, transform.up);
        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction, transform.up);
        Quaternion newRotation = Quaternion.Slerp(cachedRigidbody.rotation, targetRotation, angularSpeed * Mathf.Deg2Rad * Time.deltaTime);

        cachedRigidbody.MoveRotation(newRotation);
    }

    public void ApplyDrag(float drag, bool onlyLateral = true)
    {
        Vector3 up = transform.up;
        Vector3 v = onlyLateral ? Vector3.ProjectOnPlane(velocity, up) : velocity;

        cachedRigidbody.AddForce(-drag * v.magnitude * v, ForceMode.Acceleration);
    }

    public void ApplyForce(Vector3 force, ForceMode forceMode = ForceMode.Force) { cachedRigidbody.AddForce(force, forceMode); }
    public void ApplyVerticalImpulse(float impulse) { cachedRigidbody.velocity = Vector3.ProjectOnPlane(cachedRigidbody.velocity, transform.up) + transform.up * impulse; }
    public void ApplyImpulse(Vector3 impulse) { cachedRigidbody.velocity += impulse - Vector3.Project(cachedRigidbody.velocity, transform.up); }

    public void DisableGrounding(float time = 0.1f) { _forceUnground = true; _forceUngroundTimer = time; groundDetection.castDistance = 0.0f; }
    public void DisableGroundDetection() { _performGroundDetection = false; }
    public void EnableGroundDetection() { _performGroundDetection = true; }
    
    void ResetGroundInfo()
    {
        groundDetection.ResetGroundInfo();

        isOnPlatform = false;
        platformVelocity = Vector3.zero;
        platformAngularVelocity = Vector3.zero;

        _normal = transform.up;
    }

    void DetectGround()
    {
        ResetGroundInfo();

        if (_performGroundDetection)
        {
            if (_forceUnground || _forceUngroundTimer > 0.0f)
            {
                _forceUnground = false;
                _forceUngroundTimer -= Time.deltaTime;
            }
            else
            {
                groundDetection.DetectGround();
                groundDetection.castDistance = groundDetection.groundHit.isOnGround ? _referenceCastDistance : 0.0f;
            }
        }

        if (!groundDetection.groundHit.isOnGround) return;

        if (groundDetection.groundHit.isValidGround) _normal = groundDetection.groundHit.isOnLedgeSolidSide ? transform.up : groundDetection.groundHit.groundNormal;
        else _normal = Vector3.Cross(Vector3.Cross(transform.up, groundDetection.groundHit.groundNormal), transform.up).normalized;

        Rigidbody otherRigidbody = groundDetection.groundHit.groundRigidbody;
        if (otherRigidbody == null) return;

        if (otherRigidbody.isKinematic)
        {
            isOnPlatform = true;
            platformVelocity = otherRigidbody.GetPointVelocity(groundDetection.groundHit.groundPoint);
            platformAngularVelocity = Vector3.Project(otherRigidbody.angularVelocity, transform.up);
        }
        else _normal = transform.up;
    }

    void PreventGroundPenetration()
    {
        if (groundDetection.groundHit.isOnGround) return;

        Vector3 v = velocity;
        float speed = v.magnitude;

        Vector3 direction = speed > 0.0f ? v / speed : Vector3.zero;
        float distance = speed * Time.deltaTime;

        RaycastHit hitInfo;
        if (!groundDetection.FindGround(direction, out hitInfo, distance)) return;

        float remainingDistance = distance - hitInfo.distance;
        if (remainingDistance <= 0.0f) return;

        Vector3 velocityToGround = direction * (hitInfo.distance / Time.deltaTime);
        Vector3 remainingLateralVelocity = Vector3.ProjectOnPlane(v - velocityToGround, transform.up);

        remainingLateralVelocity = MathLibrary.GetTangent(remainingLateralVelocity, hitInfo.normal, transform.up) * remainingLateralVelocity.magnitude;
        Vector3 newVelocity = velocityToGround + remainingLateralVelocity;

        cachedRigidbody.velocity = newVelocity;
        groundDetection.castDistance = _referenceCastDistance;
    }

    void ApplyGroundMovement(Vector3 desiredVelocity, float maxDesiredSpeed, float acceleration, float deceleration, float friction, float brakingFriction)
    {
        var up = transform.up;
        var deltaTime = Time.deltaTime;

        var v = wasGrounded ? velocity : Vector3.ProjectOnPlane(velocity, up);

        var desiredSpeed = desiredVelocity.magnitude;
        var speedLimit = desiredSpeed > 0.0f ? Mathf.Min(desiredSpeed, maxDesiredSpeed) : maxDesiredSpeed;

        var desiredDirection = MathLibrary.GetTangent(desiredVelocity, _normal, up);
        var desiredAcceleration = desiredDirection * (acceleration * deltaTime);

        if (desiredAcceleration.isZero() || v.isExceeding(speedLimit))
        {
            v = MathLibrary.GetTangent(v, _normal, up) * v.magnitude;
            v = v * Mathf.Clamp01(1f - brakingFriction * deltaTime);
            v = Vector3.MoveTowards(v, desiredVelocity, deceleration * deltaTime);
        }
        else
        {
            v = MathLibrary.GetTangent(v, _normal, up) * v.magnitude;
            v = v - (v - desiredDirection * v.magnitude) * Mathf.Min(friction * deltaTime, 1.0f);
            v = Vector3.ClampMagnitude(v + desiredAcceleration, speedLimit);
        }

        if (useGravity) velocity += v - velocity;
    }

    void ApplyAirMovement(Vector3 desiredVelocity, float maxDesiredSpeed, float acceleration, float deceleration, float friction, float brakingFriction, bool onlyLateral = true)
    {
        var up = transform.up;
        var v = onlyLateral ? Vector3.ProjectOnPlane(velocity, up) : velocity;

        if (onlyLateral) desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, up);

        if (groundDetection.groundHit.isOnGround)
        {
            if (Vector3.Dot(desiredVelocity, _normal) <= 0.0f)
            {
                var maxLength = Mathf.Min(desiredVelocity.magnitude, maxDesiredSpeed);
                var lateralVelocity = Vector3.ProjectOnPlane(velocity, up);

                desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, _normal) + Vector3.Project(lateralVelocity, _normal);
                desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxLength);
            }

        }

        var desiredSpeed = desiredVelocity.magnitude;
        var speedLimit = desiredSpeed > 0.0f ? Mathf.Min(desiredSpeed, maxDesiredSpeed) : maxDesiredSpeed;

        var deltaTime = Time.deltaTime;

        var desiredDirection = desiredSpeed > 0.0f ? desiredVelocity / desiredSpeed : Vector3.zero;
        var desiredAcceleration = desiredDirection * (acceleration * deltaTime);

        if (desiredAcceleration.isZero() || v.isExceeding(speedLimit))
        {
            if (groundDetection.groundHit.isOnGround && onlyLateral)
            {
            }
            else
            {
                v = v * Mathf.Clamp01(1f - brakingFriction * deltaTime);
                v = Vector3.MoveTowards(v, desiredVelocity, deceleration * deltaTime);
            }
        }
        else
        {
            v = v - (v - desiredDirection * v.magnitude) * Mathf.Min(friction * deltaTime, 1.0f);
            v = Vector3.ClampMagnitude(v + desiredAcceleration, speedLimit);
        }

        if (onlyLateral) velocity += Vector3.ProjectOnPlane(v - velocity, up);
        else velocity += v - velocity;

        if (useGravity) velocity += gravity * Time.deltaTime;
    }

    void ApplyMovement(Vector3 desiredVelocity, float maxDesiredSpeed, float acceleration, float deceleration, float friction, float brakingFriction, bool onlyLateral)
    {
        if (groundDetection.groundHit.isOnGround) ApplyGroundMovement(desiredVelocity, maxDesiredSpeed, acceleration, deceleration, friction, brakingFriction);
        else ApplyAirMovement(desiredVelocity, maxDesiredSpeed, acceleration, deceleration, friction, brakingFriction, onlyLateral);
    }

    private void LimitLateralVelocity()
    {
        var lateralVelocity = Vector3.ProjectOnPlane(velocity, transform.up);
        if (lateralVelocity.sqrMagnitude > maxLateralSpeed * maxLateralSpeed)
            cachedRigidbody.velocity += lateralVelocity.normalized * maxLateralSpeed - lateralVelocity;
    }

    private void LimitVerticalVelocity()
    {
        if (groundDetection.groundHit.isOnGround) return;

        var up = transform.up;
        var verticalSpeed = Vector3.Dot(velocity, up);

        if (verticalSpeed < -40) cachedRigidbody.velocity += up * (-40 - verticalSpeed);
        if (verticalSpeed > 40)  cachedRigidbody.velocity += up * (40 - verticalSpeed);
    }

    void Step()
    {
        const float raycastDst = 0.9f;

        Vector3 forwardTD = transform.TransformDirection(Vector3.forward);
        Vector3 backwardTD = transform.TransformDirection(Vector3.back);

        Vector3 leftTD = transform.TransformDirection(1.5f, 0, 1);
        Vector3 rightTD = transform.TransformDirection(-1.5f, 0, 1);

        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z), forwardTD, raycastDst))
        {
            if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1.1f, transform.position.z), forwardTD, raycastDst) &&
            !Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 2.1f, transform.position.z), forwardTD, raycastDst) &&
            !Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), transform.TransformDirection(Vector3.up), 1.5f) &&
            controller.moveDirection.z > 0f)
            { StartCoroutine(PerformStep()); return; }
        }
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z), backwardTD, raycastDst))
        {
            if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1.1f, transform.position.z), backwardTD, raycastDst) &&
            !Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 2.1f, transform.position.z), backwardTD, raycastDst) &&
            !Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), transform.TransformDirection(Vector3.up), 1.5f) &&
            controller.moveDirection.z < 0f)
            { StartCoroutine(PerformStep()); return; }
        }

        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z), leftTD, raycastDst))
        {
            if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1.1f, transform.position.z), leftTD, raycastDst) &&
            !Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 2.1f, transform.position.z), leftTD, raycastDst) &&
            !Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), transform.TransformDirection(Vector3.up), 1.5f) &&
            controller.moveDirection.x > 0f)
            { StartCoroutine(PerformStep()); return; }
        }
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z), rightTD, raycastDst))
        {
            if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1.1f, transform.position.z), rightTD, raycastDst) &&
            !Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 2.1f, transform.position.z), rightTD, raycastDst) &&
            !Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), transform.TransformDirection(Vector3.up), 1.5f) &&
            controller.moveDirection.x < 0f)
            { StartCoroutine(PerformStep()); return; }
        }

        /*
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z), transform.TransformDirection(Vector3.back), raycastDst) &&
            !Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1.1f, transform.position.z), transform.TransformDirection(Vector3.back), raycastDst) &&
            controller.moveDirection.z < 0f) { StartCoroutine(PerformStep(new Vector3(0, stepSize, 0))); return; }

        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z), transform.TransformDirection(1.5f, 0, 1), raycastDst) &&
            !Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1.1f, transform.position.z), transform.TransformDirection(1.5f, 0, 1), raycastDst) &&
            controller.moveDirection.x > 0f) { StartCoroutine(PerformStep(new Vector3(0, stepSize, 0))); return; }

        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z), transform.TransformDirection(-1.5f, 0, 1), raycastDst) &&
            !Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1.1f, transform.position.z), transform.TransformDirection(-1.5f, 0, 1), raycastDst) &&
            controller.moveDirection.z < 0f) { StartCoroutine(PerformStep(new Vector3(0, stepSize, 0))); return; }
        */
    }

    IEnumerator PerformStep()
    {
        const float stepSize = 2f;
        groundDetection.capsuleCollider.enabled = false;

        useGravity = false;
        cachedRigidbody.velocity += new Vector3(0, stepSize, 0);
        cachedRigidbody.velocity += transform.forward * 2;

        yield return new WaitForSeconds(0.1f);

        cachedRigidbody.isKinematic = false;
        cachedRigidbody.velocity = transform.forward;

        groundDetection.capsuleCollider.enabled = true;

        useGravity = true;
    }

    public void Move(Vector3 desiredVelocity, float maxDesiredSpeed, float acceleration, float deceleration,
        float friction, float brakingFriction, bool onlyLateral = true)
    {
        if (Testing)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                isGod = !isGod;

                useGravity = !isGod;
            }
        }

        if (transform.position.y < -5)
        {
            transform.position = new Vector3(transform.position.x, 100, transform.position.z);
        }

        Step();

        DetectGround();
        ApplyMovement(desiredVelocity, maxDesiredSpeed, acceleration, deceleration, friction, brakingFriction, onlyLateral);

        LimitLateralVelocity();
        LimitVerticalVelocity();

        PreventGroundPenetration();
    }

    void SnapToPlatform(ref Vector3 probingPosition, ref Quaternion probingRotation)
    {
        if (_performGroundDetection == false || _forceUnground || _forceUngroundTimer > 0.0f) return;

        GroundHit hitInfo;
        if (!ComputeGroundHit(probingPosition, probingRotation, out hitInfo, groundDetection.castDistance)) return;

        var otherRigidbody = hitInfo.groundRigidbody;
        if (otherRigidbody == null || !otherRigidbody.isKinematic) return;

        var up = probingRotation * Vector3.up;
        var groundedPosition = probingPosition - up * hitInfo.groundDistance;

        var pointVelocity = otherRigidbody.GetPointVelocity(groundedPosition);
        cachedRigidbody.velocity = velocity + pointVelocity;

        var deltaVelocity = pointVelocity - platformVelocity;
        groundedPosition += Vector3.ProjectOnPlane(deltaVelocity, up) * Time.deltaTime;

        if (hitInfo.isOnLedgeSolidSide) groundedPosition = MathLibrary.ProjectPointOnPlane(groundedPosition, hitInfo.groundPoint, up);

        probingPosition = groundedPosition;

        if (platformUpdatesRotation == false || otherRigidbody.angularVelocity == Vector3.zero) return;

        var yaw = Vector3.Project(otherRigidbody.angularVelocity, up);
        var yawRotation = Quaternion.Euler(yaw * (Mathf.Rad2Deg * Time.deltaTime));

        probingRotation *= yawRotation;
    }

    private IEnumerator LateFixedUpdate()
    {
        var waitTime = new WaitForFixedUpdate();

        while (true)
        {
            yield return waitTime;

            var p = transform.position;
            var q = transform.rotation;

            OverlapRecovery(ref p, q);
            if (groundDetection.groundHit.isOnGround && isOnPlatform) SnapToPlatform(ref p, ref q);

            cachedRigidbody.MovePosition(p);
            cachedRigidbody.MoveRotation(q);
        }
    }

    public void OnEnable()
    {
        groundDetection = GetComponent<GroundDetection>();
        cachedRigidbody = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();

        if (_lateFixedUpdateCoroutine != null) StopCoroutine(_lateFixedUpdateCoroutine);
        _lateFixedUpdateCoroutine = StartCoroutine(LateFixedUpdate());
    }

    public void OnDisable() { if (_lateFixedUpdateCoroutine != null) StopCoroutine(_lateFixedUpdateCoroutine); }
}
