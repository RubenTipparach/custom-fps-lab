using UnityEngine;

public class CustomFPSController : MonoBehaviour
{
    public float speed = 5.0f;
    public float sensitivity = 2.0f;
    public float gravity = -9.81f;
    public float jumpForce = 5.0f;
    public float crouchHeight = 1.0f;
    public float standHeight = 2.0f;
    public float slopeLimit = 45.0f;
    public Transform groundCheck; // Assign the child GameObject in the inspector
    public LayerMask groundMask; // Assign a ground layer mask in the inspector

    private Rigidbody rb;
    private CapsuleCollider col;
    private bool isGrounded;
    private Vector3 colliderCenterOriginal;
    private float groundDistance = 0.4f; // Distance to check for ground
    private float cameraVerticalAngle = 0.0f;
    public Camera playerCamera;

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
        Move();
        Jump();
        Crouch();
        GroundCheck();
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

    void GroundCheck()
    {
        // Ground check ray starting from the groundCheck child transform
        isGrounded = Physics.Raycast(groundCheck.position, -Vector3.up, groundDistance, groundMask);
    }


    void Move()
    {
        // Movement
        float x = Input.GetAxis("Horizontal") * speed;
        float z = Input.GetAxis("Vertical") * speed;
        Vector3 move = transform.right * x + transform.forward * z;

        // Slope handling
        if (isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, col.height / 2 * slopeLimit))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle <= slopeLimit)
            {
                Vector3 slopeDir = Vector3.Cross(Vector3.up, hit.normal);
                move = Vector3.Cross(slopeDir, hit.normal).normalized * move.magnitude;
            }
        }

        rb.MovePosition(transform.position + new Vector3(move.x, rb.velocity.y, move.z));
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

    // Call this to apply gravity and handle grounded state
    // Call this to apply gravity and handle grounded state
    void FixedUpdate()
    {
        ApplyGravity();
    }

    void ApplyGravity()
    {
        // Apply custom gravity
        if (!isGrounded)
        {
            rb.AddForce(Vector3.up * gravity * rb.mass);
        }
    }
}
