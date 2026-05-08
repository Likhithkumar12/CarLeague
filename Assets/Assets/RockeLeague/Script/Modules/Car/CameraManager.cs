using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [SerializeField] private CinemachineCamera cinemachineCamera;

      Transform cameraTarget;
    private CarController targetCar;
    private float storedYRotation;
    private bool initialized;

    void Awake()
    {
        // Singleton protection
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;


        cameraTarget = new GameObject("CameraFollowTarget").transform;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void AssignToLocalCar(CarController car)
    {
        if (car == null)
        {
            Debug.LogError("[CameraManager] AssignToLocalCar called with null car!");
            return;
        }

        targetCar = car;

       
        storedYRotation = car.transform.eulerAngles.y;
        cameraTarget.position = car.transform.position;
        cameraTarget.rotation = Quaternion.Euler(0, storedYRotation, 0);

        
        cinemachineCamera.Follow = cameraTarget;
        cinemachineCamera.LookAt = cameraTarget;

        initialized = true;
        Debug.Log($"[CameraManager] Following: {car.gameObject.name} | Y: {storedYRotation}");
    }

    void LateUpdate()
    {
      
        if (!initialized || targetCar == null) return;

        
        cameraTarget.position = targetCar.transform.position;

       
        if (targetCar.IsGrounded() || targetCar.IsOnSurface())
        {
         
            storedYRotation = Mathf.LerpAngle(
                storedYRotation,
                targetCar.transform.eulerAngles.y,
                Time.deltaTime * 5f
            );
        }
        else
        {
          
            storedYRotation = Mathf.LerpAngle(
                storedYRotation,
                targetCar.transform.eulerAngles.y,
                Time.deltaTime * 0.5f
            );
        }

        cameraTarget.rotation = Quaternion.Euler(0, storedYRotation, 0);
    }
}