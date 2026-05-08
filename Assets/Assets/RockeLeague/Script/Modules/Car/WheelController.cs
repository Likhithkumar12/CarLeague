using Fusion;
using UnityEngine;

public class WheelController : NetworkBehaviour
{
    public Transform wheelModel;
    public WheelCollider WheelCollider;

    public bool steerable;
    public bool motorized;

    Vector3 position;
    Quaternion rotation;

    public override void FixedUpdateNetwork()
    {
        // Only the state authority simulates WheelCollider physics.
        // On proxy clients the WheelCollider is inactive (kinematic body),
        // so GetWorldPose() returns garbage values that cause wheel jitter.
        if (!HasStateAuthority) return;

        WheelCollider.GetWorldPose(out position, out rotation);
        wheelModel.transform.position = position;
        wheelModel.transform.rotation = rotation;
    }

    // Proxy clients: update wheel visuals in LateUpdate using the car's
    // synced transform instead of the WheelCollider's (invalid) pose.
    void LateUpdate()
    {
        if (HasStateAuthority) return;
        if (wheelModel == null) return;

        // Keep wheel model at the WheelCollider's local offset on the car body.
        // No spinning on proxy — acceptable trade-off vs jitter from invalid physics data.
        wheelModel.transform.position = WheelCollider.transform.TransformPoint(WheelCollider.center);
        wheelModel.transform.rotation = transform.rotation;
    }
}