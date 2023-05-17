using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* This class includes all player movement behaviors besides sliding as of now 
 * 
 * List of Movement Features:
 * - L/R horizontal movement with 'A', 'D' (including sprinting with 'Shift')
 * - Jumping with 'Space' (with variable height depending on press length) 
 * - Wall Climbing ('W' when pressed against a wall/sliding)
 * - Wall Jumping (press 'Space' when sliding on a wall)
 * - Quality of Life Features:
 *     - When pressed against a wall and falling, the player will slide slowly
 *     - If the player does not want to slide, they can press 'S' to rapidly descend
 */


public class PlayerMovement : MonoBehaviour
{
    // -- standard movement variables --
    public int walkSpeed;
    public int sprintSpeed;
    private bool isSprinting;
    public int jumpForce;
    private bool facingRight = true;  // used to flip the character model (dont know if this works for sprites yet, but it works for colliders)
    // ---------------------------------

    private Rigidbody2D rb;           // determines the physics of the player
    private float horizontalInput;    // carries the values to determine L/R movement
    
    public float jumpTime;            // the time that a player can hold jump to go higher
    private float jumpTimeCounter;    // need this to be able to reset the time a player can hold jump for
    private bool isJumping;           // check if the player is in the air

    public Transform groundCheck;     // used to check if we are on the ground
    public float groundCheckRadius;     // radius of check
    public LayerMask groundLayer;       // objects on the 'layer' to check for

    public Transform wallCheck;       // used to check if we are next to a wall
    public float wallCheckRadius;       // radius of check
    public LayerMask wallLayer;         // objects on the 'layer' to check for

    private bool isWallSliding;       // checks if a character has collided with a wall + is falling
    public float wallSlidingSpeed;      // lowers the speed of the fall to make it look like sliding

    public float wallClimbingSpeed;   // speed for climbing

    private bool isWallJumping;       // check if we have jumped to stop player input for 'x' seconds
    private float wallJumpingDirection;     // redirects the player away from the wall
    // the next two variables are used to count down wall jump duration
    private float wallJumpingTime = 0.2f;
    private float wallJumpCounter;
    public float walljumpingDuration = 0.3f; // how long it will take before L/R input is received again after a wall jump
    public Vector2 wallJumpPower = new Vector2(); // the strength of the wall jump in x and y directions



    // Start is called before the first frame update
    void Start()
    {
        // initialize the physics for our player
        rb = GetComponent<Rigidbody2D>();
    }

    /* Called at fixed time intervals, typically every 0.02 seconds (or 50 times per second 
     * [can be changed in settings]). Generally used to update physics calculations/apply 
     * force to objects. Ensures that there will be consistent physics calculations regardless 
     * of framerate (at least up to 60 frames)*/
    private void FixedUpdate()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal"); // checks for L/R input

        if (!isWallJumping) // if the character is grounded or in the air...
        {
            if (isSprinting) // ... do this for sprinting
            {
                rb.velocity = new Vector2(horizontalInput * sprintSpeed, rb.velocity.y);
            }
            else // ... do this for regular movement
            {
                rb.velocity = new Vector2(horizontalInput * walkSpeed, rb.velocity.y);
            }
        }
        // flips the character model depending on movement direction
        if (horizontalInput > 0 && facingRight)
        {
            Flip();
        } else if (horizontalInput < 0 && !facingRight) {

            Flip();
        }
    }
    
    /* called once per frame during runtime. used to handle things like user input, animations,
     * and other game logic */
    private void Update()
    {
        
        // Calculates short jump (tapping space)
        if (GroundCollision() && Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb.velocity = Vector2.up * jumpForce;
        }

        // Calculates large jump (holding space
        if (Input.GetKey(KeyCode.Space) && isJumping == true)
        {
            if (jumpTimeCounter > 0)
            {
                rb.velocity = Vector2.up * jumpForce;
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isSprinting = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprinting = false;
        }

        // call outside functions to handle wall slide/jump
        WallSlide();
        WallJump();

        // New code for moving up the wall
        if (WallCollision() && Input.GetKey(KeyCode.W))
        {
            rb.velocity = new Vector2(rb.velocity.x, wallClimbingSpeed);
        }
    }

    /* Checks if the Player is standing on a surface */
    private bool GroundCollision()
    {
        // Check if the player is touching the ground layer
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Check if the player is not touching the wall layer
        bool onWall = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, wallLayer);

        // Return true if the player is grounded and not on the wall
        return grounded || onWall;
    }

    /* Handles flippage of the character */
    private void Flip()
    {
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;

        facingRight = !facingRight;
    }

    /* Checks if the hitbox has collided with a wall */
    private bool WallCollision()
    {
        return Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
    }

    /* Calculates speed at which player should slide on wall and handles dropping */
    private void WallSlide()
    {
        // if in the air and in contact with the wall...
        if (WallCollision() && !GroundCollision())
        {
            if (Input.GetKey(KeyCode.S)) // ... drops the player at normal speed instead of sliding
            {
                isWallSliding = false;
                rb.velocity = new Vector2(horizontalInput * walkSpeed, rb.velocity.y);
            } 
            else // ... causes the player to slide
            {
                isWallSliding = true;
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
            }
            
        } else
        {
            isWallSliding = false;
        }
    }

    /* Handles wall jump mechanisms */
    private void WallJump()
    {
        if (isWallSliding) 
        {
            // set direction opposite of current player x direction
            wallJumpingDirection = -transform.localScale.x;
            wallJumpCounter = wallJumpingTime; 

            CancelInvoke(nameof(StopWallJump));
        } else
        {
            // gives player extra time to wall jump after leaving the wall
            wallJumpCounter -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space) && wallJumpCounter > 0f)
        {
            isWallJumping = true;
            // gathers wall jump power from the public Vector2 variable
            rb.velocity = new Vector2(-wallJumpingDirection * wallJumpPower.x, wallJumpPower.y);
            
            wallJumpCounter = 0f; // player has used up their wall jump

            // if player orientation is different from wall jump direction, update the way they are facing
            if (transform.localScale.x != wallJumpingDirection)
            {
                facingRight = !facingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
            // isWallJumping set to false after x number of seconds, makes sure user cant spam jump along one wall
            Invoke(nameof(StopWallJump), walljumpingDuration);
        }

    }

    private void StopWallJump()
    {
        isWallJumping = false;
    }

    /* draws the wall/floor colliders for debug reference */
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
    }
}