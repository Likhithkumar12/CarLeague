using UnityEngine;

public class CarCameraTarget : MonoBehaviour
{
    public CarController carController;
    public float rotationSmoothSpeed = 5f;

    private Rigidbody carRb;
    private float storedYRotation;

    void Start()
    {
        carRb = carController.GetComponent<Rigidbody>();
        storedYRotation = carController.transform.eulerAngles.y;
    }

    void FixedUpdate()
    {
        transform.position = carRb.position;
        
        if (carController.IsGrounded())
        {
            storedYRotation = Mathf.LerpAngle(storedYRotation, carController.transform.eulerAngles.y,
                Time.deltaTime * rotationSmoothSpeed);
        }

        transform.rotation = Quaternion.Euler(0, storedYRotation, 0);
    }
}