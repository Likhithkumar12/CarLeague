using UnityEngine;

public class CameraDistanceRayCaster : MonoBehaviour
{
    [SerializeField] private Transform CameraTransform;
    [SerializeField] private Transform CameraTargetTransform;
    public LayerMask layerMask = Physics.AllLayers;
    public float minimumDistanceFromObstacles = 0.2f;
    public float smoothingFactor = 25f;
    public float SphereRadius = 0.5f;

    private Transform _tr;
    private float currentDistance;

    private void Awake()
    {
        _tr = transform;
        layerMask &= ~(1 << LayerMask.NameToLayer("Ignore Raycast"));
        currentDistance = (CameraTargetTransform.position - _tr.position).magnitude;
    }

    void LateUpdate()
    {
        Vector3 castDirection = CameraTargetTransform.position - _tr.position;
        float targetDistance = GetCameraDistance(castDirection);
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smoothingFactor);
        CameraTransform.position = _tr.position + castDirection.normalized * currentDistance;
    }

    float GetCameraDistance(Vector3 castDirection)
    {
        float maxDistance = castDirection.magnitude;
        if (Physics.SphereCast(new Ray(_tr.position, castDirection.normalized), SphereRadius, out RaycastHit hit,
                maxDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            return Mathf.Max(0f, hit.distance - minimumDistanceFromObstacles);
        }
        return maxDistance;
    }
}