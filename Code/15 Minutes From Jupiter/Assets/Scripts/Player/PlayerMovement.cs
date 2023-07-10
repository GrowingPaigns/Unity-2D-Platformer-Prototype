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
    /* -- Serealized Fields -- 
     * Serialized fields have the benefit of being able to be private variables which can still be seen
     * outside of the script (i.e. in the inspector window of unity). Without these specifiers, we would 
     * need to make the variables public.
     * 
     * After we finalize these mechanics, all the serialized field specifiers can be removed. They are 
     * simply used to make testing easier at the moment.
     */
    [SerializeField] private Animator animator;         // Used to play different animations based on movement

    [SerializeField] private int walkSpeed;             // Regular movement speed
    [SerializeField] private int sprintSpeed;           // Sprinting movement speed

    [SerializeField] private int jumpForce;             // Regular jump speed
    [SerializeField] private float coyoteTime;          // Time after walking off a ledge which a user can still jump during

    [SerializeField] private BoxCollider2D groundCheck; // Used to check if we are on the ground
    [SerializeField] private LayerMask groundLayer;     // Objects with the 'ground' layer we want to check for
    [SerializeField] private BoxCollider2D wallCheck;   // Used to check if we are next to a wall
    [SerializeField] private LayerMask wallLayer;       // Objects with the 'wall' layer we want to check for
    
    [SerializeField] private BoxCollider2D dashCheck;   // 

    [SerializeField] private float wallJumpingTime;     // Time after exiting the wall that the player can still wall jump
    [SerializeField] private float wallJumpDuration;    // How long it will take before L/R input is received again after a wall jump

    [SerializeField] private float wallSlidingSpeed;    // Lowers the speed of the fall to make it look like sliding
    [SerializeField] private float wallClimbingSpeed;   // Speed for climbing
    [SerializeField] private Vector2 wallJumpPower;     // The strength of the wall jump in x and y directions

    [SerializeField] private float slowMotionTimeScale; // How much we want to slow down time by when dashing

    [SerializeField] TrailRenderer trail;               // Trail rendered during sprinting and dashing
    /* --------------------------------- */


    [SerializeField] private float horizDashPower;
    [SerializeField] private float vertDashPower;
    [SerializeField] private float dashTime;
    [SerializeField] private Transform attackTranform;



    /* -- Private Fields -- 
     * Used only within this class
     */
    private Rigidbody2D rb;          // determines the physics of the player

    private bool isSprinting;        // Used to change between 
    public bool canMove = true;     // Used in wall jumping (necessary to make th player jump)

    public bool facingRight = true; // Used to flip the character model
    private float horizontalInput;   // carries the values to determine L/R movement
    private float verticalInput;     // carries the values to determine U/D movement

    private bool isWallSliding;      // checks if a character has collided with a wall + is 
    private bool isWallJumping;      // check if we have jumped to stop player input for 'x' seconds
    private float wallJumpCounter;     // Used to count down the time
    private float wallJumpingDirection; // redirects the player away from the wall

    private float coyoteTimeCounter; // Counts down the coyote time jump

    private bool canDash = true;     // Determines if a player can dash
    public bool isDashing = false;  // Switches on and off the dash mechanic in combo with the dash cooldown 
   
    private int dashCount = 0;

    private GameObject[] enemies;

    /* --------------------------------- */






    // Start is called before the first frame update
    void Start()
    {

        // initialize the physics for our player
        rb = GetComponent<Rigidbody2D>();
    }




    /* Called at fixed time intervals, typically every 0.02 seconds (or 50 times per second 
     * [can be changed in settings]). 
     * 
     * Generally used to update physics calculations/apply force to objects. Ensures that 
     * there will be consistent physics calculations regardless of framerate (at least up to 
     * 60 frames)
     */
    private void FixedUpdate()
    {
       


        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (isDashing) // if the player is dashing dont listen for any input
        {
            return;
        }

        if (!canMove)
        {
            return;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal"); // checks for L/R input
        verticalInput = Input.GetAxisRaw("Vertical"); // checks for L/R input

        if (canMove && !isWallJumping) // if the character is grounded or in the air...
        {

            if (isSprinting) // ... do this for sprinting
            {
                rb.velocity = new Vector2(horizontalInput * sprintSpeed, rb.velocity.y);
                trail.emitting = true;
            }
            else // ... do this for regular movement
            {
                rb.velocity = new Vector2(horizontalInput * walkSpeed, rb.velocity.y);
                trail.emitting = false;
            }

            // used in animator to switch between idle/moving animations
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
            dashCount = 0;
            // Reset the isWallJumping flag(s) and the coyote time variable
            coyoteTimeCounter = coyoteTime;
            isWallJumping = false;
            animator.SetBool("WallJumping", false);
            canDash = true;
            canMove = true;

            foreach (GameObject enemy in enemies)
            {
                Physics2D.IgnoreCollision(enemy.GetComponent<Collider2D>(), GetComponent<Collider2D>(), false);
            }
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }


        if (WallCollision())
        {
            animator.SetBool("WallJumping", false);
            
            isWallJumping = false;
            canMove = true;
        }



        animator.SetBool("Grounded", GroundCollision()); // used in combo with vert speed to switch between falling
        animator.SetFloat("VertSpeed", rb.velocity.y);   // and other states (idle, running)
    }

    /* called once per frame during runtime. used to handle things like user input, animations,
     * and other game logic */
    private void Update()
    {

        // Calculates jump (tapping space)
        if (GroundCollision() && coyoteTimeCounter > 0f && Input.GetKeyDown(KeyCode.Space))
        {
            rb.velocity = Vector2.up * jumpForce;
        }
        else if (!GroundCollision() && coyoteTimeCounter > 0f && Input.GetKeyDown(KeyCode.Space))
        {
            rb.velocity = Vector2.up * jumpForce;
            coyoteTimeCounter = 0f;

        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) // start sprinting
        {
            isSprinting = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift)) // stop sprinting
        {
            isSprinting = false;
        }

        // call outside functions to handle wall slide/jump
        WallSlide();
        WallJump();
        if (isWallSliding)
        {
            isWallJumping = false;
        }

        if (Input.GetMouseButtonDown(1) && canDash) // perform dash
        {
            StartCoroutine(Dash());

        }



    }

    /* Checks if the Player is standing on a surface */
    public bool GroundCollision()
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
        // Invert the sign of the scale's X component
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        this.transform.localScale = currentScale;
        // Reverse the facing direction flag
        facingRight = !facingRight;

    }



    /* Calculates speed at which player should slide on wall and handles dropping */
    private void WallSlide()
    {
        isWallJumping = false;
        // if in the air and in contact with the wall...
        if (WallCollision() && !GroundCollision() && !Input.GetKey(KeyCode.Space))
        {
            GetComponent<SpriteRenderer>().flipX = true;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                if (Input.GetKey(KeyCode.W)) // wall climbing with horizontal input
                {
                    isWallSliding = true;
                    rb.velocity = new Vector2(rb.velocity.x, wallClimbingSpeed);

                    if (horizontalInput != 0)
                    {
                        rb.velocity = new Vector2(horizontalInput * walkSpeed, rb.velocity.y);
                    }

                } 
                else
                {
                    isWallSliding = true;
                    rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
                }

            }
            else if (Input.GetKey(KeyCode.W)) // wall climbing with horizontal input
            {
                isWallSliding = true;
                rb.velocity = new Vector2(rb.velocity.x, wallClimbingSpeed);

                if (horizontalInput != 0)
                {
                    rb.velocity = new Vector2(horizontalInput * walkSpeed, rb.velocity.y);
                }

            }
            else if (Input.GetKey(KeyCode.S)) // ... drops the player at normal speed instead of sliding
            {

                rb.velocity = new Vector2(horizontalInput * walkSpeed, rb.velocity.y);
                isWallSliding = false;
            }
            else // Wall slide behavior
            {
                isWallSliding = false;


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

        if (WallCollision() && Input.GetKeyDown(KeyCode.Space) && wallJumpCounter > 0f)
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

            Debug.Log("WallJumpPower " + wallJumpPower);
            // Disable player input for the duration of wallJumpDuration

            
            canMove = false; // Enable player movement again
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpPower.x, wallJumpPower.y);
            StartCoroutine(PauseInputForDuration(wallJumpDuration));
            

            Debug.Log("Direction + Power.x: " + wallJumpingDirection * wallJumpPower.x + " Power.y " + wallJumpPower.y);

            

        }
    }

    public IEnumerator PauseInputForDuration(float duration) // pause input for wall jump
    {
        

        yield return new WaitForSeconds(duration);

        
        isWallJumping = false;
        animator.SetBool("WallJumping", isWallJumping);
        canMove = true; // Enable player movement again

        

    }


    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        // Ignore collisions with objects tagged as "Enemy"
        
        foreach (GameObject enemy in enemies)
        {
            Physics2D.IgnoreCollision(enemy.GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);
        }

        float originalGravity = rb.gravityScale;

        Vector2 dashDirection = Vector2.zero;

        // Flag to check if the dash direction has been set
        bool dashDirectionSet = false;

        if (Input.GetKey(KeyCode.A))
        {
            dashDirection.x = -horizDashPower; // Dash to the left
            dashDirectionSet = true;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            dashDirection.x = horizDashPower; // Dash to the right
            dashDirectionSet = true;
        }

        if (Input.GetKey(KeyCode.W))
        {
            dashDirection.y = vertDashPower; // Dash upwards
            dashDirectionSet = true;
        }
        else if (Input.GetKey(KeyCode.S) && !GroundCollision())
        {
            dashDirection.y = -vertDashPower * 2; // Dash downwards
            dashDirectionSet = true;
        }

        if (dashDirectionSet)
        {
            dashCount++; // Increment dashCount only once if a dash direction is set

        }
        else
        {
            canDash = false;
            isDashing = false;
            yield break;
        }


        if (dashCount > 3)
        {
            
            canMove = true;
            isDashing = false;


            yield break;
        }



        rb.gravityScale = 0f;
        rb.velocity = new Vector2(dashDirection.x, dashDirection.y);
        trail.emitting = true;

        float startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < dashTime)
        {
            if (WallCollision() /*|| DashCollision()*/)
            {
                break; // Exit the loop if WallCollision() is true
            }

            yield return null;
            elapsedTime = Time.time - startTime;
        }



        

        trail.emitting = false;
        rb.gravityScale = originalGravity;

        isDashing = false;
        canDash = true;

        
    }


}

