using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class GroundDetection : MonoBehaviour
{
    [Tooltip("Layers to be considered as 'ground' (walkables).")]
    public LayerMask groundMask = 1;
    [Tooltip("The maximum angle (in degrees) that will be accounted as 'ground'.")]
    public float groundLimit = 60.0f;
    [Tooltip("The maximum height (in meters) for a valid step.")]
    public float stepOffset = 0.5f;
    [Tooltip("The maximum horizontal distance (in meters) a character can stand on a ledge without slide down.")]
    public float ledgeOffset;
    [Tooltip("Determines the maximum length of the cast.")]
    public float castDistance = 0.5f;
    [Tooltip("Should Triggers be considered as 'ground'?")]
    public QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

    static readonly Collider[] OverlappedColliders = new Collider[16];

    const float kBackstepDistance = 0.05f;
    const float kMinLedgeDistance = 0.05f;
    const float kHorizontalOffset = 0.001f;

    public GroundHit groundHit;
    public GroundHit prevGroundHit;

    public LayerMask overlapMask = -1;

    int _ignoreRaycastLayer = 2;
    int _cachedLayer;

    public QueryTriggerInteraction triggerInteraction { get { return _triggerInteraction; } set { _triggerInteraction = value; } }
    public float groundAngle { get { return !groundHit.isOnGround ? 0.0f : Vector3.Angle(groundHit.surfaceNormal, transform.up); } }

    public CapsuleCollider capsuleCollider;
    public Rigidbody cachedRigidbody;

    void InitializeOverlapMask()
    {
        overlapMask = 0;
        for (var i = 0; i < 32; i++) { if (!Physics.GetIgnoreLayerCollision(gameObject.layer, i)) overlapMask |= 1 << i; }
    }

    public Collider[] OverlapCapsule(Vector3 position, Quaternion rotation, out int overlapCount)
    {
        var center = capsuleCollider.center;
        var radius = capsuleCollider.radius;

        var height = capsuleCollider.height * 0.5f - radius;

        var topSphereCenter = center + Vector3.up * height;
        var bottomSphereCenter = center - Vector3.up * height;

        var top = position + rotation * topSphereCenter;
        var bottom = position + rotation * bottomSphereCenter;

        var colliderCount = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, OverlappedColliders, overlapMask, triggerInteraction);
        overlapCount = colliderCount;

        for (var i = 0; i < colliderCount; i++)
        {
            var overlappedCollider = OverlappedColliders[i];

            if (overlappedCollider != null && overlappedCollider != capsuleCollider) continue;
            if (i < --overlapCount) OverlappedColliders[i] = OverlappedColliders[overlapCount];
        }

        return OverlappedColliders;
    }

    bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float distance, float backstepDistance = kBackstepDistance)
    {
        origin = origin - direction * backstepDistance;

        var hit = Physics.Raycast(origin, direction, out hitInfo, distance + backstepDistance, groundMask,triggerInteraction);
        if (hit) hitInfo.distance = hitInfo.distance - backstepDistance;

        return hit;
    }

    bool SphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float distance, float backstepDistance = kBackstepDistance)
    {
        origin = origin - direction * backstepDistance;

        var hit = Physics.SphereCast(origin, radius, direction, out hitInfo, distance + backstepDistance, groundMask, triggerInteraction);
        if (hit) hitInfo.distance = hitInfo.distance - backstepDistance;

        return hit;
    }

    bool CapsuleCast(Vector3 bottom, Vector3 top, float radius, Vector3 direction, out RaycastHit hitInfo, float distance, float backstepDistance = kBackstepDistance)
    {
        top = top - direction * backstepDistance;
        bottom = bottom - direction * backstepDistance;

        var hit = Physics.CapsuleCast(bottom, top, radius, direction, out hitInfo, distance + backstepDistance, groundMask, triggerInteraction);
        if (hit) hitInfo.distance = hitInfo.distance - backstepDistance;

        return hit;
    }

    void DisableRaycastCollisions()
    {
        _cachedLayer = gameObject.layer;
        gameObject.layer = _ignoreRaycastLayer;
    }

    void EnableRaycastCollisions() { gameObject.layer = _cachedLayer; }

    public bool ComputeGroundHit(Vector3 position, Quaternion rotation, float distance = Mathf.Infinity)
    {
        var up = rotation * Vector3.up;

        RaycastHit hitInfo;
        if (BottomSphereCast(position, rotation, out hitInfo, distance) && Vector3.Angle(hitInfo.normal, up) < 89.0f)
        {
            groundHit.SetFrom(hitInfo);
            DetectLedgeAndSteps(position, rotation, distance, hitInfo.point, hitInfo.normal);

            groundHit.isOnGround = true;
            groundHit.isValidGround = !groundHit.isOnLedgeEmptySide && Vector3.Angle(groundHit.surfaceNormal, up) < groundLimit;

            return true;
        }

        if (!BottomRaycast(position, rotation, out hitInfo, distance)) return false;

        groundHit.SetFrom(hitInfo);
        groundHit.surfaceNormal = hitInfo.normal;

        groundHit.isOnGround = true;
        groundHit.isValidGround = Vector3.Angle(groundHit.surfaceNormal, /*Vector3.up*/up) < groundLimit;

        return true;
    }

    bool BottomSphereCast(Vector3 position, Quaternion rotation, out RaycastHit hitInfo, float distance, float backstepDistance = kBackstepDistance)
    {
        var radius = capsuleCollider.radius;

        var height = Mathf.Max(0.0f, capsuleCollider.height * 0.5f - radius);
        var center = capsuleCollider.center - Vector3.up * height;

        var origin = position + rotation * center;
        var down = rotation * Vector3.down;

        return SphereCast(origin, radius, down, out hitInfo, distance, backstepDistance);
    }

    public void DetectGround()
    {
        DisableRaycastCollisions();
        ComputeGroundHit(transform.position, transform.rotation, castDistance);
        EnableRaycastCollisions();
    }

    public void ResetGroundInfo()
    {
        var up = transform.up;

        prevGroundHit = new GroundHit(groundHit);
        groundHit = new GroundHit
        {
            groundPoint = transform.position,
            groundNormal = up,
            surfaceNormal = up
        };
    }

    public bool FindGround(Vector3 direction, out RaycastHit hitInfo, float distance = Mathf.Infinity, float backstepDistance = kBackstepDistance)
    {
        var radius = capsuleCollider.radius;

        var height = Mathf.Max(0.0f, capsuleCollider.height * 0.5f - radius);
        var center = capsuleCollider.center - Vector3.up * height;

        var origin = transform.TransformPoint(center);

        var up = transform.up;
        if (!SphereCast(origin, radius, direction, out hitInfo, distance, backstepDistance) || Vector3.Angle(hitInfo.normal, /*Vector3.up*/up) >= 89.0f)
            return false;

        var p = transform.position - transform.up * hitInfo.distance;
        var q = transform.rotation;

        groundHit = new GroundHit(hitInfo);
        DetectLedgeAndSteps(p, q, castDistance, hitInfo.point, hitInfo.normal);

        groundHit.isOnGround = true;
        groundHit.isValidGround = !groundHit.isOnLedgeEmptySide && Vector3.Angle(groundHit.surfaceNormal, /*Vector3.up*/up) < groundLimit;

        return groundHit.isOnGround && groundHit.isValidGround;
    }

    public void DetectLedgeAndSteps(Vector3 position, Quaternion rotation, float distance, Vector3 point, Vector3 normal)
    {
        Vector3 up = rotation * Vector3.up, down = -up;
        var projectedNormal = Vector3.ProjectOnPlane(normal, up).normalized;

        var nearPoint = point + projectedNormal * kHorizontalOffset;
        var farPoint = point - projectedNormal * kHorizontalOffset;

        var ledgeStepDistance = Mathf.Max(kMinLedgeDistance, Mathf.Max(stepOffset, distance));

        RaycastHit nearHitInfo;
        var nearHit = Raycast(nearPoint, down, out nearHitInfo, ledgeStepDistance);
        var isNearGroundValid = nearHit && Vector3.Angle(nearHitInfo.normal, up) < groundLimit;

        RaycastHit farHitInfo;
        var farHit = Raycast(farPoint, down, out farHitInfo, ledgeStepDistance);
        var isFarGroundValid = farHit && Vector3.Angle(farHitInfo.normal, up) < groundLimit;

        if (farHit && !isFarGroundValid)
        {
            groundHit.surfaceNormal = farHitInfo.normal;

            RaycastHit secondaryHitInfo;
            if (BottomRaycast(position, rotation, out secondaryHitInfo, distance))
            {
                groundHit.SetFrom(secondaryHitInfo);
                groundHit.surfaceNormal = secondaryHitInfo.normal;
            }

            return;
        }

        if (isNearGroundValid && isFarGroundValid)
        {
            CharacterMovement.instance.nearStep = true;

            return;
        }

        var isOnLedge = isNearGroundValid != isFarGroundValid;
        if (!isOnLedge)  return;

        groundHit.surfaceNormal = isFarGroundValid ? farHitInfo.normal : nearHitInfo.normal;
        groundHit.ledgeDistance = Vector3.ProjectOnPlane(point - position, up).magnitude;

        if (isFarGroundValid && groundHit.ledgeDistance > ledgeOffset)
        {
            groundHit.isOnLedgeEmptySide = true;

            var radius = ledgeOffset;
            var offset = Mathf.Max(0.0f, capsuleCollider.height * 0.5f - radius);

            var bottomSphereCenter = capsuleCollider.center - Vector3.up * offset;
            var bottomSphereOrigin = position + rotation * bottomSphereCenter;

            RaycastHit hitInfo;
            if (SphereCast(bottomSphereOrigin, radius, down, out hitInfo, Mathf.Max(stepOffset, distance)))
            {
                var verticalSquareDistance = Vector3.Project(point - hitInfo.point, up).sqrMagnitude;
                if (verticalSquareDistance <= stepOffset * stepOffset) groundHit.isOnLedgeEmptySide = false;
            }
        }

        groundHit.isOnLedgeSolidSide = !groundHit.isOnLedgeEmptySide;
    }

    bool BottomRaycast(Vector3 position, Quaternion rotation, out RaycastHit hitInfo, float distance, float backstepDistance = kBackstepDistance)
    {
        var down = rotation * Vector3.down;
        return Raycast(position, down, out hitInfo, distance, backstepDistance) && SimulateSphereCast(position, rotation, hitInfo.normal, out hitInfo, distance, backstepDistance);
    }

    bool SimulateSphereCast(Vector3 position, Quaternion rotation, Vector3 normal, out RaycastHit hitInfo, float distance = Mathf.Infinity, float backstepDistance = kBackstepDistance)
    {
        var origin = position;
        var up = rotation * Vector3.up;

        var angle = Vector3.Angle(normal, up) * Mathf.Deg2Rad;
        if (angle > 0.0001f)
        {
            var radius = capsuleCollider.radius;

            var x = Mathf.Sin(angle) * radius;
            var y = (1.0f - Mathf.Cos(angle)) * radius;

            var right = Vector3.Cross(normal, up);
            var tangent = Vector3.Cross(right, normal);

            origin += Vector3.ProjectOnPlane(tangent, up).normalized * x + up * y;
        }

        return Raycast(origin, -up, out hitInfo, distance, backstepDistance);
    }

    void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        cachedRigidbody = GetComponent<Rigidbody>();

        _ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        InitializeOverlapMask();
    }
}
