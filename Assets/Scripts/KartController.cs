using System;
using UnityEngine;

public class KartController : MonoBehaviour
{
    [SerializeField] private Rigidbody sphere;

    float speed, currentSpeed;
    float rotate, currentRotate;
    int driftDirection;
    float driftPower;
    int driftMode = 0;

    public Transform kartNormal;
    public Transform kartModel;
    public float carOffset;
    public float acceleration = 30f;
    public float steering = 80f;
    public float gravity = 10f;

    // Update is called once per frame
    void Update()
    {
        transform.position = sphere.transform.position - new Vector3(0,carOffset,0);

        if (Input.GetKey(KeyCode.W))
        {
            Debug.Log("hihi");
            speed = acceleration;
        }

        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            int dir = Input.GetAxisRaw("Horizontal") > 0 ? 1 : -1;
            float amount = Mathf.Abs((Input.GetAxisRaw("Horizontal")));
            Steer(dir, amount);
        }

        currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * 12f);
        speed = 0f;
        currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 4f);
        rotate = 0f;
    }

    private void FixedUpdate()
    {
        sphere.AddForce(kartModel.transform.forward * currentSpeed, ForceMode.Acceleration);
        
        
        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        float grav = sphere.linearVelocity.y;
        
        sphere.linearVelocity = (kartModel.transform.forward.normalized * sphere.linearVelocity.magnitude);

        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles,
            new Vector3(0, transform.eulerAngles.y + currentRotate, 0), Time.deltaTime * 5f);
        
        RaycastHit hitOn;
        RaycastHit hitNear;

        Physics.Raycast(transform.position + (transform.up*.1f), Vector3.down, out hitOn, 1.1f);
        Physics.Raycast(transform.position + (transform.up * .1f)   , Vector3.down, out hitNear, 2.0f);

        //Normal Rotation
        kartNormal.up = Vector3.Lerp(kartNormal.up, hitNear.normal, Time.deltaTime * 8.0f);
        kartNormal.Rotate(0, transform.eulerAngles.y, 0);
    }

    void Steer(int direction, float amount)
    {
        rotate = (direction * steering) * amount;
    }
}
