using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DOFController : MonoBehaviour
{
    public LayerMask layer;
    public float focusSpeed = 8f;
    public PostProcessVolume postProcess;
    DepthOfField dof;

    Ray ray;
    RaycastHit hit;

    bool isHit;
    float hitDistance;  

    void Start()
    {
        postProcess.profile.TryGetSettings(out dof);
    }

    void Update()
    {
        ray = new Ray(transform.position, transform.forward * 100);
        isHit = false;

        if (Physics.Raycast(ray, out hit, 100f, layer))
        {
            isHit = true;
            hitDistance = Vector3.Distance(transform.position, hit.point);
        }
        else
        {
            if (hitDistance < 100f) hitDistance++;
        }

        dof.focusDistance.value = Mathf.Lerp(dof.focusDistance.value, hitDistance, Time.deltaTime * focusSpeed);
    }
}
