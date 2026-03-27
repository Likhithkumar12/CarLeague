using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [SerializeField] private CinemachineCamera cinemachineCamera;

    private Transform cameraTarget;
    private Rigidbody targetRb;
    private CarController targetCar;
    private float storedYRotation;
    private bool initialized;

    void Awake()
    {
        Instance = this;
        cameraTarget = new GameObject("CameraFollowTarget").transform;
        cinemachineCamera.Follow = cameraTarget;
        cinemachineCamera.LookAt = cameraTarget;
    }

    public void AssignToLocalCar(CarController car)
    {
        targetCar = car;
        targetRb = car.GetComponent<Rigidbody>();
        storedYRotation = car.transform.eulerAngles.y;
        cameraTarget.position = targetRb.position;
        cameraTarget.rotation = Quaternion.Euler(0, storedYRotation, 0);
        initialized = true;
        Debug.Log("[CameraManager] Following: " + car.gameObject.name);
    }

    void FixedUpdate()
    {
        if (!initialized || targetRb == null) return;

        cameraTarget.position = targetRb.position;

        if (targetCar.IsGrounded())
        {
            storedYRotation = Mathf.LerpAngle(
                storedYRotation,
                targetCar.transform.eulerAngles.y,
                Time.deltaTime * 5f
            );
        }

        cameraTarget.rotation = Quaternion.Euler(0, storedYRotation, 0);
    }
}