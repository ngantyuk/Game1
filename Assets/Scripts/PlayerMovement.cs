using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    #region Movement Settings
    [Header("Speeds")]
    [SerializeField] [Tooltip("How fast the player walks.")]
    private float walkSpeed = 3.0f;

    [SerializeField] [Tooltip("How fast the player jogs.")]
    private float jogSpeed = 6.0f;

    [SerializeField] [Tooltip("How fast the player sprints.")]
    private float sprintSpeed = 10.0f;

    [SerializeField] [Tooltip("How high the player can jump.")]
    private float jumpHeight = 2.0f;
    #endregion

    #region Physics & Grounding
    [Header("Physics")]
    [SerializeField] [Tooltip("The strength of gravity pulling the player down.")]
    private float gravityValue = -19.81f; // Slightly stronger gravity feels "snappier"

    [SerializeField] [Tooltip("Transform representing the position of the ground check.")]
    private Transform groundCheck;

    [SerializeField] [Tooltip("Which layers count as 'Ground'.")]
    private LayerMask groundMask;

    [SerializeField] [Tooltip("Distance from groundCheck to look for the ground.")]
    private float groundDistance = 0.4f;
    #endregion

    #region Private Variables
    private CharacterController controller; // Reference to the CharacterController component
    private Vector2 moveInput;              // Stores our X and Y movement from the controller
    private Vector3 playerVelocity;         // Used for jumping and gravity
    private bool isGrounded;                // Are we touching the floor?
    private float currentSpeed;             // The speed we are currently moving at
    private bool isSprinting;               // Is the sprint key held?
    private bool isJogging;                 // Is the jog key held?
    #endregion

    private void Start()
    {
        // Get the components from the GameObject this script is attached to
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed; // Start at walk speed by default
    }

    private void Update()
    {
        CheckGrounded();
        HandleMovement();
        HandleGravity();
    }

    #region Input Methods
    // These methods are called by the "Player Input" component via "Send Messages"
    
    public void OnMove(InputValue value)
    {
        // Read the 2D vector (WASD or Joystick)
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        // Only jump if we are on the ground
        if (value.isPressed && isGrounded)
        {
            // Physics formula for jump height: velocity = sqrt(height * -2 * gravity)
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
        }
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    public void OnJog(InputValue value)
    {
        isJogging = value.isPressed;
    }
    #endregion

    #region Movement Logic
    private void CheckGrounded()
    {
        // Check if the groundCheck sphere overlaps with anything on the Ground layer
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Reset velocity when grounded so gravity doesn't build up infinitely
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; 
        }
    }

    private void HandleMovement()
    {
        // Determine current speed based on input priority: Sprint > Jog > Walk
        if (isSprinting) currentSpeed = sprintSpeed;
        else if (isJogging) currentSpeed = jogSpeed;
        else currentSpeed = walkSpeed;

        // Convert 2D input into 3D movement (X = Left/Right, Z = Forward/Back)
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        
        // Move the CharacterController
        controller.Move(move * Time.deltaTime * currentSpeed);

        // Optional: Rotate the player to face the direction they are moving
        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }
    }

    private void HandleGravity()
    {
        // Apply gravity over time
        playerVelocity.y += gravityValue * Time.deltaTime;

        // Apply the vertical velocity (jumping/falling) to the controller
        controller.Move(playerVelocity * Time.deltaTime);
    }
    #endregion
}

/*
================================================================================
IMPLEMENTATION GUIDE (STEP-BY-STEP)
================================================================================

1. PREPARE THE ENVIRONMENT:
   - Go to 'Edit' > 'Project Settings' > 'Tags and Layers'.
   - Add a Layer named "Player" and a Layer named "Ground".
   - Add a Tag named "Player" and a Tag named "Ground".

2. PREPARE THE GROUND:
   - Select your floor/ground object.
   - Set its Tag to "Ground" and its Layer to "Ground".

3. PREPARE THE PLAYER:
   - Drag your FBX model into the scene.
   - Set its Tag to "Player" and its Layer to "Player".
   - Add a 'Character Controller' component to the model.
   - Adjust the 'Center' and 'Height' of the Character Controller to fit your model.
   - Add a 'Player Input' component (comes with the Input System package).

4. CREATE THE INPUT ACTIONS:    
   - In the 'Player Input' component, click 'Create Actions...'. Save the file.
   - Open that file and ensure you have Actions named:
     * "Move" (Value, Vector2)
     * "Jump" (Button)
     * "Sprint" (Button)
     * "Jog" (Button)
   - Ensure the 'Behavior' on the Player Input component is set to "Send Messages".

5. SET UP THE SCRIPT:
   - Attach this 'PlayerController' script to your player model.
   - Right-click your player in the Hierarchy, select 'Create Empty'. 
   - Rename it "GroundCheck" and move it to the very bottom (feet) of the model.
   - Drag that "GroundCheck" object into the 'Ground Check' slot in the script inspector.
   - In the 'Ground Mask' dropdown in the inspector, select ONLY the "Ground" layer.

6. CONTROLLER SETUP:
   - Because we used the Unity Input System, as long as you mapped your Input Actions 
     to "Gamepad" as well as "Keyboard", a controller will work automatically!
================================================================================
*/