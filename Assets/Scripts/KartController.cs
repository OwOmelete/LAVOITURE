using System;
using TMPro;
using UnityEngine;

public class KartController : MonoBehaviour
{
    [SerializeField] private Rigidbody sphere;

    private bool OnGround;
    float speed, currentSpeed;
    float rotate, currentRotate;
    int driftDirection;
    float driftPower;
    int driftMode = 0;
    private float steering;
    private float airSpin;
    private bool canDrift;
    private bool isDrifting;
    private bool startDrift;
    private driftDir currentDriftDir;
    private bool isBoosting;

    enum driftDir
    {
        left,
        right,
        none
    }
    

    [Header("Controls")]
    
    public float maxSpeed = 30f;
    public float boostSpeed = 60f;
    public float boostImpulse = 60f;
    public float acceleration = 12f;
    
    public float groundSteering = 80f;
    public float airSteering = 80f;
    
    public float gravity = 10f;
    [SerializeField] private float jumpForce = 1;
    [SerializeField] private float raycastDistance;
    
    [Header("Drift")]
    
    [SerializeField] private float driftAngle;
    [SerializeField] private float driftBaseSteering;
    [SerializeField] private float driftMaxSteering;
    [SerializeField] private float driftMinSteering;
    
    
    [Header("visuals")]
    
    [SerializeField] private float wheelsRotationAmount;
    public float carOffset;
    public float wheelRotationMult = 1;

    [Header("references")]
    
    public Transform kartNormal;
    public Transform kartModel;
    [SerializeField] private GameObject frontLeftWheel;
    [SerializeField] private GameObject frontLeftWheelPivot;
    [SerializeField] private GameObject frontRightWheel;
    [SerializeField] private GameObject frontRightWheelPivot;
    [SerializeField] private GameObject backLeftWheel;
    [SerializeField] private GameObject backRightWheel;
    [SerializeField] private TMP_Text speedCounter;

    private void Start()
    {
        steering = groundSteering;
    }

    void Update()
    {
        transform.position = sphere.transform.position - new Vector3(0,carOffset,0);

        if (Input.GetKey(KeyCode.W))
        {
            if (isBoosting)
            {
                speed = boostSpeed;
            }
            else
            {
                speed = maxSpeed;
                
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isBoosting = true;
            Boost();
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isBoosting = false;
        }
        
        if (Input.GetKeyDown(KeyCode.Space) && OnGround)
        {
            float dir = Input.GetAxisRaw("Horizontal");
            sphere.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            airSpin = 70f * dir;

            canDrift = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            canDrift = false;
            if (isDrifting)
            {
                Boost();
            }
            isDrifting = false;
        }
        
        Steering();
        
        currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * acceleration);
        speed = 0f;
        currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 4f);
        rotate = 0f;
        
        
    }

    private void FixedUpdate()
    {
        Debug.Log(currentDriftDir);
        
        RaycastHit hitOn;
        RaycastHit hitNear;

        Physics.Raycast(transform.position + (transform.up*.1f), Vector3.down, out hitOn, 1.1f);

        //Normal Rotation
        OnGround = Physics.Raycast(transform.position + (transform.up * .1f), Vector3.down, out hitNear, raycastDistance);
        
        if (!OnGround && Mathf.Abs(airSpin) > 0.1f)
        {
            float spinStep = 70f * Time.deltaTime; // vitesse de rotation
            float spinAmount = Mathf.Min(spinStep, Mathf.Abs(airSpin)) * Mathf.Sign(airSpin);
            kartModel.Rotate(Vector3.up, spinAmount, Space.Self);
            airSpin -= spinAmount;
        }
        
        if (OnGround)
        {
            if (canDrift && !isDrifting && startDrift)
            {
                startDrift = false;
                canDrift = false;
                isDrifting = true;

                float dir = Input.GetAxisRaw("Horizontal");
                if (dir < 0)
                {
                    currentDriftDir = driftDir.left;
                }
                else if (dir > 0)
                {
                    currentDriftDir = driftDir.right;
                }
                else
                {
                    currentDriftDir = driftDir.none;
                }
            }
            steering = groundSteering;

            if (isDrifting && currentDriftDir != driftDir.none)
            {
                if (currentDriftDir == driftDir.left)
                {
                    Vector3 angle = Vector3.Lerp(kartModel.transform.forward, kartModel.transform.right, driftAngle).normalized;
                
                    sphere.AddForce(angle * currentSpeed, ForceMode.Acceleration);
                }
                else if (currentDriftDir == driftDir.right)
                {
                    Vector3 angle = Vector3.Lerp(kartModel.transform.forward, -kartModel.transform.right, driftAngle).normalized;
                
                    sphere.AddForce(angle * currentSpeed, ForceMode.Acceleration);
                }
                
            }
            else
            {
                sphere.AddForce(kartModel.transform.forward * currentSpeed, ForceMode.Acceleration);
            }
            
            
        }
        else
        {
            if (canDrift) startDrift = true;
            steering = airSteering;
        }
        
        
        
        wheelRotation(frontLeftWheel, true, frontLeftWheelPivot);
        wheelRotation(frontRightWheel, true, frontRightWheelPivot);
        wheelRotation(backLeftWheel);
        wheelRotation(backRightWheel);

        
        
        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        float grav = sphere.linearVelocity.y;

        Vector3 Velocity = new Vector3(sphere.linearVelocity.x, 0f, sphere.linearVelocity.z);
        if (OnGround)
        {
            if (isDrifting && currentDriftDir!=driftDir.none)
            {
                if (currentDriftDir == driftDir.left)
                {
                    Vector3 angle = Vector3.Lerp(kartModel.transform.forward, kartModel.transform.right, driftAngle).normalized;
                    Velocity = angle * Velocity.magnitude;
                }
                else
                {
                    Vector3 angle = Vector3.Lerp(kartModel.transform.forward, -kartModel.transform.right, driftAngle).normalized;
                    Velocity = angle * Velocity.magnitude;
                }
                
            }
            else
            {
                Velocity = kartModel.transform.forward.normalized * Velocity.magnitude;
                
            }
        }

        sphere.linearVelocity = new Vector3(Velocity.x, grav, Velocity.z);

        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles,
            new Vector3(0, transform.eulerAngles.y + currentRotate, 0), Time.deltaTime * 5f);
        
        
        kartNormal.up = Vector3.Lerp(kartNormal.up, hitNear.normal, Time.deltaTime * 8.0f);
        
        kartNormal.Rotate(0, transform.eulerAngles.y, 0);

        speedCounter.text = isDrifting.ToString() + sphere.linearVelocity.magnitude.ToString();
    }

    void wheelRotation(GameObject wheel, bool isFrontWheel = false, GameObject pivot = null)
    {
        if (isFrontWheel && pivot)
        {
            pivot.transform.localRotation = Quaternion.Lerp(pivot.transform.localRotation, Quaternion.Euler(wheel.transform.rotation.x,
                Input.GetAxisRaw("Horizontal") * wheelsRotationAmount, pivot.transform.rotation.z), 0.2f); 
        }
        wheel.transform.Rotate(Vector3.right, sphere.linearVelocity.magnitude * wheelRotationMult * Time.deltaTime, Space.Self);
        //wheel.transform.localRotation = Quaternion.Euler(wheel.transform.localRotation.x + sphere.linearVelocity.magnitude * wheelRotationMult,wheel.transform.localRotation.y, wheel.transform.localRotation.z );
    }

    void Steering()
    {
        int dir = Input.GetAxisRaw("Horizontal") > 0 ? 1 : -1;
        float amount = Mathf.Abs(Input.GetAxisRaw("Horizontal"));

        if (isDrifting)
        {
            if (currentDriftDir == driftDir.left)
            {
                if (dir < 0)
                {
                    rotate = dir * driftMaxSteering * amount - driftBaseSteering;
                }
                else
                {
                    rotate = dir * driftMinSteering * amount - driftBaseSteering;
                }
                    
            }
            else
            {
                if (dir > 0)
                {
                    rotate = dir * driftMaxSteering * amount + driftBaseSteering;
                }
                else
                {
                    rotate = dir * driftMinSteering * amount + driftBaseSteering;
                }
            }
        }
        else
        {
            rotate = (dir * steering) * amount;
        }
    }

    void Boost()
    {
        sphere.AddForce(kartModel.transform.forward * boostImpulse, ForceMode.Impulse);
    }


}
