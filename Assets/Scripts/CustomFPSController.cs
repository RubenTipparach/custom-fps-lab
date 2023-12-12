using UnityEditor.ShaderGraph;
using UnityEngine;

public class CustomFPSController : MonoBehaviour
{
    // Public variables for tuning the controller
    public float speed = 5.0f;
    public float airControl = 0.5f;  // Reduced control when in the air
    public float sensitivity = 2.0f;
    public float gravity = -9.81f;
    public float jumpForce = 5.0f;
    public float crouchHeight = 1.0f;
    public float standHeight = 2.0f;
    public float slopeLimit = 45.0f;
    public Transform groundCheck; // Assign the child GameObject in the inspector
    public LayerMask groundMask; // Assign a ground layer mask in the inspector
    public Camera playerCamera;
    public float groundCheckOffset = 0.1f; // Offset for the ground check ray

    // Private variables for internal state
    private Rigidbody rb;
    private CapsuleCollider col;
    private bool isGrounded;
    private Vector3 colliderCenterOriginal;
    private float groundDistance = 0.4f; // Distance to check for ground
    private float cameraVerticalAngle = 0.0f;
    private Vector3 horizontalVelocity; // Separate horizontal velocity


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        colliderCenterOriginal = col.center;
    }
    void Update()
    {
        MouseLook();
        Jump();
        Crouch();
        GroundCheck();
    }

    void FixedUpdate()
    {
        Move();
        ApplyGravity();
    }

    void MouseLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

        // Horizontal rotation
        transform.Rotate(0, mouseX, 0);

        // Vertical rotation
        cameraVerticalAngle -= mouseY;
        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -90.0f, 90.0f);
        playerCamera.transform.localEulerAngles = new Vector3(cameraVerticalAngle, 0, 0);
    }


    void Move()
    {
        // Get horizontal movement input
        float x = Input.GetAxis("Horizontal") * (isGrounded ? speed : airControl);
        float z = Input.GetAxis("Vertical") * (isGrounded ? speed : airControl);

        // Convert local movement direction to world space
        Vector3 move = transform.right * x + transform.forward * z;
        horizontalVelocity = new Vector3(move.x, 0f, move.z);

        // Apply the horizontal velocity
        if (isGrounded)
        {
            rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
        }
        else
        {
            // Allow for some air control
            rb.velocity += horizontalVelocity * Time.fixedDeltaTime;
            rb.velocity = new Vector3(
                Mathf.Clamp(rb.velocity.x, -speed, speed),
                rb.velocity.y,
                Mathf.Clamp(rb.velocity.z, -speed, speed)
            );
        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void Crouch()
    {
        if (Input.GetButtonDown("Crouch"))
        {
            col.height = crouchHeight;
            col.center = new Vector3(col.center.x, crouchHeight / 2, col.center.z);
            Camera.main.transform.localPosition = new Vector3(0, crouchHeight, 0); // Adjust the camera position
        }
        else if (Input.GetButtonUp("Crouch"))
        {
            col.height = standHeight;
            col.center = colliderCenterOriginal;
            Camera.main.transform.localPosition = new Vector3(0, standHeight, 0); // Adjust the camera position back
        }
    }


    void GroundCheck()
    {
        // Adjust the ray origin based on the offset and check for ground
        Vector3 rayOrigin = groundCheck.position + Vector3.up * groundCheckOffset;
        isGrounded = Physics.Raycast(rayOrigin, -Vector3.up, out RaycastHit hit, groundDistance + groundCheckOffset, groundMask);

        //if (isGrounded) { Debug.Log("shit is grounded."); }

        if (isGrounded && rb.velocity.y < 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }
    }

    void ApplyGravity()
    {
        // Apply custom gravity only if the player is not grounded
        if (!isGrounded)
        {
            rb.AddForce(Vector3.up * gravity * rb.mass);
        }
    }
}
