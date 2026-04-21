using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class CarController : NetworkBehaviour
{
    [Header("Driving")]
    public float motorTorque = 2000f;
    public float brakeTorque = 2000f;
    public float maxSpeed = 20f;
    public float boostMaxSpeed = 34.5f;
    public float steeringRange = 30f;
    public float steeringRangeAtMaxSpeed = 10f;
    public float centreOfGravityOffset = -1f;

    [Header("Wall/Ceiling Driving")]
    public float surfaceGravityForce = 30f;
    public float surfaceCheckDistance = 2f;
    private Vector3 surfaceNormal = Vector3.up;
    private bool isOnSurface;

    [Header("Jump & Flip")]
    public float jumpForce = 600f;

    [Header("Aerial Control")]
    public float airRollTorque = 200f;
    public float aerialPitchTorque = 200f;
    public float aerialYawTorque = 150f;
    public float maxAngularSpeed = 5f;

    [Header("Drift")]
    public float driftSteerMultiplier = 1.5f;
    public float driftSidewaysFriction = 0.5f;
    private float normalSidewaysFriction;

    [Header("Boost")]
    public float boostForce = 1500f;
    public float maxBoostFuel = 100f;
    public float boostConsumptionRate = 30f;

    // ── Networked State ───────────────────────────────────────────────────────
    [Networked] private float NetworkedBoostFuel { get; set; }
    [Networked] private NetworkBool NetworkedIsBoosting { get; set; }

    // ── Private State ─────────────────────────────────────────────────────────
    private WheelController[] wheels;
    private Rigidbody rb;
    private bool isGrounded;
    private bool hasJumped;
    private bool isDrifting;
    private bool isBoosting;

    private bool xInputReleasedAfterJump = true;
    private bool yInputReleasedAfterJump = true;
    private bool rollInputReleasedAfterJump = true;

    // Cached input values for this tick
    private float xinput, yinput, rollInput;
    private bool jumpPressed, drifting;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass += Vector3.up * centreOfGravityOffset;
        wheels = GetComponentsInChildren<WheelController>();

        if (wheels.Length > 0)
            normalSidewaysFriction = wheels[0].WheelCollider.sidewaysFriction.stiffness;

        NetworkedBoostFuel = maxBoostFuel;

        // Disable physics simulation on non-authority clients
        // Fusion's NetworkRigidbody handles syncing position/rotation
        Debug.Log($"[Car Spawned] Name: {gameObject.name} | HasInputAuthority: {HasInputAuthority} | CameraManager exists: {CameraManager.Instance != null}");

        if (HasInputAuthority)
        {
            // Our car — simulate physics normally
            rb.isKinematic = false;
            CameraManager.Instance?.AssignToLocalCar(this);
        }
        else
        {
            // Remote car — NetworkTransform drives position
            // Physics must be kinematic so it doesn't fight NetworkTransform
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    // ── Fusion Tick — runs on all clients in sync ─────────────────────────────

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;
        // Only the authority (the owning player) drives physics
        if (!GetInput(out CarInputData input)) return;

        // Unpack input
        xinput    = input.Move.y;
        yinput    = input.Move.x;
        rollInput = input.Roll;
        jumpPressed  = input.Jump;
        isBoosting = input.Boost;
        drifting     = input.Drift;

        CheckGrounded();
        CheckSurface();
        TrackInputReleaseAfterJump();
        HandleSurfaceGravity();

        if (jumpPressed) HandleJump();

        // Landing detection
        if ((isGrounded || isOnSurface) && hasJumped && rb.linearVelocity.y <= 0.1f)
        {
            hasJumped = false;
            xInputReleasedAfterJump = true;
            yInputReleasedAfterJump = true;
            rollInputReleasedAfterJump = true;
        }

        bool onAnySurface = isGrounded || isOnSurface;
        isDrifting = onAnySurface && drifting;
        HandleDrift(isDrifting);

        if (onAnySurface)
            HandleDriving();
        else
            HandleAerialControl();

        HandleBoost();
    }

    // ── Wheel visuals update — runs every frame on all clients ────────────────

    //Input Tracking ────────────────────────────────────────────────────────

    private void TrackInputReleaseAfterJump()
    {
        if (!hasJumped) return;

        if (!xInputReleasedAfterJump && Mathf.Abs(xinput) < 0.1f)
            xInputReleasedAfterJump = true;
        if (!yInputReleasedAfterJump && Mathf.Abs(yinput) < 0.1f)
            yInputReleasedAfterJump = true;
        if (!rollInputReleasedAfterJump && Mathf.Abs(rollInput) < 0.1f)
            rollInputReleasedAfterJump = true;
    }

    // ── Physics Methods (unchanged logic, cleaner structure) ──────────────────

    void CheckGrounded()
    {
        isGrounded = false;
        foreach (var wheel in wheels)
        {
            if (wheel.WheelCollider.isGrounded) { isGrounded = true; return; }
        }
    }

    void CheckSurface()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, surfaceCheckDistance))
        {
            if (Vector3.Dot(hit.normal, transform.up) > 0.5f)
            {
                isOnSurface = true;
                surfaceNormal = hit.normal;
                return;
            }
        }
        isOnSurface = false;
        surfaceNormal = Vector3.up;
    }

    void HandleSurfaceGravity()
    {
        if (Vector3.Dot(surfaceNormal, Vector3.up) >= 0.7f) return;

        bool hasInput = Mathf.Abs(xinput) > 0.1f || Mathf.Abs(yinput) > 0.1f;
        if (hasInput)
        {
            rb.AddForce(-Physics.gravity, ForceMode.Acceleration);
            rb.AddForce(-surfaceNormal * surfaceGravityForce, ForceMode.Acceleration);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation,
                Runner.DeltaTime * 8f);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation,
                Runner.DeltaTime * 3f);
        }
    }

    void HandleJump()
    {
        if ((!isGrounded && !isOnSurface) || hasJumped) return;

        foreach (var wheel in wheels)
        {
            wheel.WheelCollider.motorTorque = 0;
            wheel.WheelCollider.brakeTorque = brakeTorque;
        }

        rb.angularVelocity = Vector3.zero;
        rb.AddForce(surfaceNormal * jumpForce, ForceMode.Impulse);
        hasJumped = true;

        xInputReleasedAfterJump   = Mathf.Abs(xinput) < 0.1f;
        yInputReleasedAfterJump   = Mathf.Abs(yinput) < 0.1f;
        rollInputReleasedAfterJump = Mathf.Abs(rollInput) < 0.1f;
    }

    void HandleDrift(bool isDrift)
    {
        foreach (var wheel in wheels)
        {
            var sf = wheel.WheelCollider.sidewaysFriction;
            sf.stiffness = isDrift ? driftSidewaysFriction : normalSidewaysFriction;
            wheel.WheelCollider.sidewaysFriction = sf;
        }
    }

    void HandleDriving()
    {
        if (hasJumped) return;

        float currentMaxSpeed = isBoosting ? boostMaxSpeed : maxSpeed;
        float forwardSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
        float speedFactor = Mathf.InverseLerp(0, currentMaxSpeed, Mathf.Abs(forwardSpeed));
        float currentMotorTorque = Mathf.Lerp(motorTorque, motorTorque * 0.1f, speedFactor);
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor)
                                  * (isDrifting ? driftSteerMultiplier : 1f);
        bool isAccelerating = Mathf.Sign(xinput) == Mathf.Sign(forwardSpeed);

        foreach (var wheel in wheels)
        {
            if (wheel.steerable)
                wheel.WheelCollider.steerAngle = yinput * currentSteerRange;

            if (isAccelerating)
            {
                if (wheel.motorized)
                    wheel.WheelCollider.motorTorque = xinput * currentMotorTorque;
                wheel.WheelCollider.brakeTorque = 0;
            }
            else
            {
                wheel.WheelCollider.brakeTorque = Mathf.Abs(xinput) * brakeTorque;
                wheel.WheelCollider.motorTorque = 0;
            }
        }

        if (rb.linearVelocity.magnitude > currentMaxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * currentMaxSpeed;
    }

    void HandleAerialControl()
    {
        foreach (var wheel in wheels)
        {
            wheel.WheelCollider.motorTorque = 0;
            wheel.WheelCollider.brakeTorque = 0;
        }

        float pitch = xInputReleasedAfterJump ? xinput : 0f;
        float yaw   = yInputReleasedAfterJump ? yinput : 0f;
        float roll  = rollInputReleasedAfterJump ? rollInput : 0f;

        rb.AddTorque(transform.right   * pitch * aerialPitchTorque, ForceMode.Force);
        rb.AddTorque(transform.up      * yaw   * aerialYawTorque,   ForceMode.Force);
        rb.AddTorque(transform.forward * roll  * airRollTorque,     ForceMode.Force);

        if (rb.angularVelocity.magnitude > maxAngularSpeed)
            rb.angularVelocity = rb.angularVelocity.normalized * maxAngularSpeed;
    }

    void HandleBoost()
    {
       // isBoosting = boostPressed && NetworkedBoostFuel > 0f;
        NetworkedIsBoosting = isBoosting;

        if (!isBoosting) return;

        NetworkedBoostFuel = Mathf.Max(NetworkedBoostFuel - boostConsumptionRate * Runner.DeltaTime, 0f);

        if (isGrounded)
        {
            Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, surfaceNormal).normalized;
            rb.AddForce(flatForward * boostForce, ForceMode.Force);

            if (rb.linearVelocity.magnitude > 1f)
            {
                Vector3 hVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                float vVel = rb.linearVelocity.y;
                rb.linearVelocity = Vector3.Slerp(hVel.normalized, flatForward, 0.15f)
                                    * hVel.magnitude + Vector3.up * vVel;
            }
        }
        else
        {
            rb.AddForce(transform.forward * boostForce, ForceMode.Force);
        }

        if (rb.linearVelocity.magnitude > boostMaxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * boostMaxSpeed;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void AddBoostFuel(float amount) =>
        NetworkedBoostFuel = Mathf.Min(NetworkedBoostFuel + amount, maxBoostFuel);

    public float GetBoostFuel() => NetworkedBoostFuel;
    public bool IsGrounded() => isGrounded;
    public bool IsOnSurface() => isOnSurface;
    public Vector3 GetSurfaceNormal() => surfaceNormal;
}