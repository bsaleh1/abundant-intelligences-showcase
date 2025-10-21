/// First Person Sphere Script
/// Purpose: 3D movement Controller for a Rigidbody designed for a planatary surface
/// How to use it: WASD for movement, mouse pad for camera look

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FPSphere : MonoBehaviour
{
    // Planet center 
    public Transform sphereCenter;

    // Movement variables
    public float speed = 5;
    public bool canRun = true;
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    // Look variables
    [Header("Look")]
    public float mouseSensitivity = 100f;
    public float rollLimit = 90f;
    public float alignSpeed = 10f; 

    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();
    public bool IsRunning { get; private set; }

    private Rigidbody rigidbody;
    private float zRotation = 0f; 
    private float xRotation = 0f;
    public float pitchLimit = 90f;

    public Transform cameraTransform;

    void Awake()
    {
        // Rigidbody setup
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = false; 
        rigidbody.freezeRotation = true; 
       
        // Cursor setup
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // CAMERA PITCH (X-Axis) 
        // Vertical mouse movement controls pitch for the camera ONLY
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -pitchLimit, pitchLimit);

        // Apply Pitch xRotation to the camera
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // BODY YAW 
        // Rotate the rigidbody horizontally based on mouseX
        transform.Rotate(Vector3.up * mouseX, Space.Self);
    }

    void FixedUpdate()
    {
        // Get the surface normal from sphere center
        Vector3 normal = (transform.position - sphereCenter.position).normalized;

        // PLANET ALIGNMENT (Body xRotation and zRotation)
        // Calculate the rotation needed to align the body's 'up' vector with the surface normal to rotate the body on the X and Z axes 
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, normal) * transform.rotation;
       
        // Apply the rotation to the body (Body xRotation and zRotation)
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, alignSpeed * Time.deltaTime);
       
        // GRAVITY
        rigidbody.AddForce(-normal * 100f);

        // MOVEMENT SPEED
        IsRunning = canRun && Input.GetKey(runningKey);
        float moveSpeed = speedOverrides.Count > 0 ? speedOverrides[^1]() : (IsRunning ? runSpeed : speed);

        // MOVEMENT INPUT
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Compute local tangent vectors (relative to the planet's surface)
        Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, normal).normalized;
        Vector3 cameraRight = Vector3.Cross(normal, cameraForward).normalized;

        // Calculate desired movement direction
        Vector3 moveDir = (cameraRight * h + cameraForward * v).normalized;

        // Apply movements 
        Vector3 targetVelocity = moveDir * moveSpeed;
        Vector3 radialVelocity = normal * Vector3.Dot(rigidbody.velocity, normal);
        rigidbody.velocity = targetVelocity + radialVelocity;
    }
}