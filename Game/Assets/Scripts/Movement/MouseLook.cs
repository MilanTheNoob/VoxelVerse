using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Tooltip("How fast the cursor moves in response to mouse lateral (x-axis) movement.")]
    public float lateralSensitivity = 2.0f;
    [Tooltip("How fast the cursor moves in response to mouse vertical (y-axis) movement.")]
    public float verticalSensitivity = 2.0f;
    [Tooltip("Approximately the time (in seconds) it will take to reach the target.\n")]
    public float smoothTime = 5.0f;
    [Tooltip("The minimum pitch angle (in degrees).")]
    public float minPitchAngle = -90.0f;
    [Tooltip("The maximum pitch angle (in degrees).")]
    public float maxPitchAngle = 90.0f;

    protected bool _isCursorLocked = true;
    [HideInInspector] public bool isInverted = false;

    protected Quaternion characterTargetRotation;
    protected Quaternion cameraTargetRotation;

    public virtual void Init(Transform characterTransform, Transform cameraTransform)
    {
        characterTargetRotation = characterTransform.localRotation;
        cameraTargetRotation = cameraTransform.localRotation;
    }

    public virtual void LookRotation(CharacterMovement movement, Transform cameraTransform)
    {
        var yaw = (isInverted ? -Input.GetAxis("Mouse X") : Input.GetAxis("Mouse X")) * lateralSensitivity;
        var pitch = (isInverted ? -Input.GetAxis("Mouse Y") : Input.GetAxis("Mouse Y")) * verticalSensitivity;

        var yawRotation = Quaternion.Euler(0.0f, yaw, 0.0f);
        var pitchRotation = Quaternion.Euler(-pitch, 0.0f, 0.0f);

        characterTargetRotation *= yawRotation;
        cameraTargetRotation *= pitchRotation;

        cameraTargetRotation = ClampPitch(cameraTargetRotation);

        if (movement.platformUpdatesRotation && movement.isOnPlatform && movement.platformAngularVelocity != Vector3.zero)
        {
            characterTargetRotation *= Quaternion.Euler(movement.platformAngularVelocity * Mathf.Rad2Deg * Time.deltaTime);
        }

        movement.rotation = Quaternion.Slerp(movement.rotation, characterTargetRotation, smoothTime * Time.deltaTime);
        cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, cameraTargetRotation, smoothTime * Time.deltaTime);
    }

    protected Quaternion ClampPitch(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        var pitch = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        pitch = Mathf.Clamp(pitch, minPitchAngle, maxPitchAngle);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * pitch);

        return q;
    }
}