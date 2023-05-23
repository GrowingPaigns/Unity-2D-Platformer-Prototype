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
    public bool facingRight = true;  // used to flip the character model (dont know if this works for sprites yet, but it works for colliders)
    // ---------------------------------

    private Rigidbody2D rb;           // determines the physics of the player
    private float horizontalInput;    // carries the values to determine L/R movement

    public float jumpTime;            // the time that a player can hold jump to go higher
    private float jumpTimeCounter;    // need this to be able to reset the time a player can hold jump for
    private bool isJumping;           // check if the player is in the air

    public BoxCollider2D groundCheck;     // used to check if we are on the ground
    public LayerMask groundLayer;       // objects on the 'layer' to check for

    public BoxCollider2D wallCheck;       // used to check if we are next to a wall
    public LayerMask wallLayer;         // objects on the 'layer' to check for

    private bool isWallSliding;       // checks if a character has collided with a wall + is falling
    public float wallSlidingSpeed;      // lowers the speed of the fall to make it look like sliding

    public float wallClimbingSpeed;   // speed for climbing

    private bool isWallJumping;       // check if we have jumped to stop player input for 'x' seconds
    private float wallJumpingDirection;     // redirects the player away from the wall
    // the next two variables are used to count down wall jump duration
    private float wallJumpingTime = 0.2f;
    private float wallJumpCounter;
    public float walljumpingDuration; // how long it will take before L/R input is received again after a wall jump
    public Vector2 wallJumpPower = new Vector2(); // the strength of the wall jump in x and y directions

    public bool canMove = true;
    public float coyoteTime;
    private float coyoteTimeCounter;

    public Animator animator;

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

        if (canMove && !isWallJumping) // if the character is grounded or in the air...
        {
            
            

            if (isSprinting) // ... do this for sprinting
            {
                rb.velocity = new Vector2(horizontalInput * sprintSpeed, rb.velocity.y);
            }
            else // ... do this for regular movement
            {
                rb.velocity = new Vector2(horizontalInput * walkSpeed, rb.velocity.y);
            }

            animator.SetFloat("HorizSpeed", Mathf.Abs(horizontalInput));
        }

        


        // flips the character model depending on movement direction
        if (horizontalInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && facingRight)
        {

            Flip();
        }

        

        if (GroundCollision()) // Check if the player is grounded
        {
            coyoteTimeCounter = coyoteTime;
            isWallJumping = false; // Reset the isWallJumping flag
            animator.SetBool("WallJumping", false);
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        if (WallCollision())
        {
            isWallJumping = false;
            animator.SetBool("WallJumping", false);
        }
       

        animator.SetBool("Grounded", GroundCollision());
        animator.SetFloat("VertSpeed", rb.velocity.y);
    }

    /* called once per frame during runtime. used to handle things like user input, animations,
     * and other game logic */
    private void Update()
    {



        // Calculates short jump (tapping space)
        if (GroundCollision() && coyoteTimeCounter > 0f && Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;

            rb.velocity = Vector2.up * jumpForce;

        }
        else if (!GroundCollision() && coyoteTimeCounter > 0f && Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb.velocity = Vector2.up * jumpForce;
            coyoteTimeCounter = 0f;
            
        }

        // Calculates large jump (holding space
        if (Input.GetKey(KeyCode.Space) && isJumping == true)
        {
            if (!GroundCollision() && coyoteTimeCounter > 0f && jumpTimeCounter > 0)
            {
                rb.velocity = Vector2.up * jumpForce;
                jumpTimeCounter -= Time.deltaTime;
                coyoteTimeCounter = 0f;
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

        // New code for climbing up the wall
        if (WallCollision() && Input.GetKey(KeyCode.W))
        {
            if (!isWallJumping) // Only allow movement if not wall jumping
            {
                rb.velocity = new Vector2(rb.velocity.x, wallClimbingSpeed);
                horizontalInput = Input.GetAxisRaw("Horizontal");

                // Allow horizontal movement while climbing
                if (horizontalInput != 0)
                {
                    rb.velocity = new Vector2(horizontalInput * walkSpeed, rb.velocity.y);
                }
            }
        }


    }

    /* Checks if the Player is standing on a surface */
    private bool GroundCollision()
    {
        // Check if the player is touching the ground layer
        bool grounded = groundCheck.IsTouchingLayers(groundLayer);

        // Check if the player is not touching the wall layer
        bool onWall = groundCheck.IsTouchingLayers(wallLayer);

        // Return true if the player is grounded and not on the wall
        return grounded || onWall;
    }

    /* Checks if the hitbox has collided with a wall */
    private bool WallCollision()
    {
        return wallCheck.IsTouchingLayers(wallLayer);
    }

    /* Handles flippage of the character */
    public void Flip()
    {
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;

        facingRight = !facingRight;
    }

    

    /* Calculates speed at which player should slide on wall and handles dropping */
    private void WallSlide()
    {
        isWallJumping = false;
        // if in the air and in contact with the wall...
        if (WallCollision() && !GroundCollision())
        {
            
            GetComponent<SpriteRenderer>().flipX = true;
            horizontalInput = Input.GetAxisRaw("Horizontal"); // Added this line to check for L/R input

            if (horizontalInput != 0)
            {
                if (WallCollision())
                {
                    isWallSliding = true;
                    rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));

                }
               
            }
            else if (Input.GetKey(KeyCode.S)) // ... drops the player at normal speed instead of sliding
            {
                
                rb.velocity = new Vector2(horizontalInput * walkSpeed, rb.velocity.y);
            }
            else // Wall slide behavior
            {
                isWallSliding = true;
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    WallJump();
                }
            }

        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = false;
            isWallSliding = false;
        }

        animator.SetBool("WallSliding", isWallSliding);

    }

    /* Handles wall jump mechanisms */
    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            // set direction opposite of current player x direction
            wallJumpingDirection = -transform.localScale.x;
            wallJumpCounter = wallJumpingTime;
        }
        else
        {
            // gives player extra time to wall jump after leaving the wall
            wallJumpCounter -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space) && wallJumpCounter > 0f)
        {
            isWallSliding = false;
            isWallJumping = true;
            wallJumpCounter = 0f; // player has used up their wall jump
            animator.SetBool("WallJumping", isWallJumping);

            // if player orientation is different from wall jump direction, update the way they are facing
            if (transform.localScale.x != wallJumpingDirection)
            {
                Flip(); // Use the existing Flip() method to flip the character
            }

            rb.velocity = new Vector2(wallJumpingDirection * wallJumpPower.x, wallJumpPower.y);
            Debug.Log("Velocity: " + rb.velocity);
            // Disable player input for the duration of walljumpingDuration
            StartCoroutine(PauseInputForDuration(walljumpingDuration));
        }
    }

    private IEnumerator PauseInputForDuration(float duration)
    {
        canMove = false; // Disable player movement

        yield return new WaitForSecondsRealtime(duration);

        canMove = true; // Enable player movement again
        isWallJumping = false;
        animator.SetBool("WallJumping", isWallJumping);
    }


}