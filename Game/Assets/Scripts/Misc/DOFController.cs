using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// A very basic script responsible for dynamic depth of field
/// Requires a post processing volume with a DOF on it
/// </summary>
public class DOFController : MonoBehaviour
{
    [Header("The layer in which to look for things to focus on")] public LayerMask layer;
    [Header("The speed in which the controller focuses onto new objects")] public float focusSpeed = 8f;

    [Space, Header("The pp volume in which the dof is stored in")] public PostProcessVolume postProcess;
    DepthOfField dof;

    Ray ray;
    RaycastHit hit;

    float hitDistance;  

    void Start() { postProcess.profile.TryGetSettings(out dof);}

    void Update()
    {
        ray = new Ray(transform.position, transform.forward * 100);

        if (Physics.Raycast(ray, out hit, 100f, layer)) hitDistance = Vector3.Distance(transform.position, hit.point);
        else { if (hitDistance < 100f) hitDistance++; }

        dof.focusDistance.value = Mathf.Lerp(dof.focusDistance.value, hitDistance, Time.deltaTime * focusSpeed);
    }
}
