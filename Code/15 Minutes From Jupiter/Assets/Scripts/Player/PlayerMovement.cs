using System;
using System.Collections;
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
    [Header("Movement Settings:")]
    [Space]
    [SerializeField] private Animator animator;         // Used to play different animations based on movement

    [SerializeField] private int walkSpeed;             // Regular movement speed
    [SerializeField] private int sprintSpeed;           // Sprinting movement speed

    [SerializeField] private int jumpForce;             // Regular jump speed
    [SerializeField] private float coyoteTime;          // Time after walking off a ledge which a user can still jump during
    public float stamina, maxStamina;

    [Space]
    [Header("Adv. Movement Settings:")]
    [Space]

    [SerializeField] private float wallJumpDuration;    // How long it will take before L/R input is received again after a wall jump

    [SerializeField] private float wallSlidingSpeed;    // Lowers the speed of the fall to make it look like sliding
    [SerializeField] private float wallClimbingSpeed;   // Speed for climbing
    [SerializeField] private Vector2 wallJumpPower;     // The strength of the wall jump in x and y directions

    [SerializeField] private float horizDashPower;
    [SerializeField] private float vertDashPower;
    [SerializeField] private float dashTime;

    [SerializeField] private float slidingSpeed;


    [Space]
    [Header("Bool Checks for Adv. Movement and Sprite Flipping:")]
    [Space]

    public bool canMove = true;     // Used in wall jumping (necessary to make th player jump)
    public bool isHurt = false;
    public bool isDashing = false;  // Switches on and off the dash mechanic in combo with the dash cooldown 
    public bool facingRight = true; // Used to flip the character model
    [SerializeField] private bool canDash = true;     // Determines if a player can dash

    [Space]
    [Header("Movement Collision Checks:")]
    [Space]

    [SerializeField] private LayerMask groundLayer;     // Objects with the 'ground' layer we want to check for
    [SerializeField] private BoxCollider2D wallCheck;   // Used to check if we are next to a wall
    [SerializeField] private LayerMask wallLayer;       // Objects with the 'wall' layer we want to check for
    [SerializeField] private LayerMask rampLayer;

    [Space]
    [Header("Other:")]
    [Space]

    [SerializeField] TrailRenderer trail;               // Trail rendered during sprinting and dashing
    [SerializeField] private Transform attackTranform;
    public static event Action OnPlayerDash;

    [Space]

    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float rayLength;
    private RaycastHit2D rayHit2D;

    /* -- Private Fields -- 
     * Used only within this class
     */
    private Rigidbody2D playerRigidbody;          // determines the physics of the player
    private bool isSprinting;        // Used to change between 
    private float horizontalInput;   // carries the values to determine L/R movement
    private float verticalInput;     // carries the values to determine U/D movement

    private bool isWallSliding;      // checks if a character has collided with a wall + is 
    private bool isWallJumping;      // check if we have jumped to stop player input for 'x' seconds
    private float wallJumpingDirection; // redirects the player away from the wall

    private float coyoteTimeCounter; // Counts down the coyote time jump
    private int dashCount = 0;

    private GameObject[] enemies;

    private bool isSliding = false;
    private bool isJumping = false;
    private float originalGravity = 0;

    private SpriteRenderer spriteRenderer;

    /* --------------------------------- */



    private void DrainStamina(float amount)
    {
        stamina -= amount;
        OnPlayerDash?.Invoke();

        if (stamina <= 0)
        {
            return;
        }
    }

    private void RegainStamina()
    {
        stamina = maxStamina;
        OnPlayerDash?.Invoke();

        if (stamina <= 0)
        {
            return;
        }
    }


    // Start is called before the first frame update
    void Start()
    {

        // initialize the physics for our player
        playerRigidbody = GetComponent<Rigidbody2D>();
        originalGravity = playerRigidbody.gravityScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }



    /* called once per frame during runtime. used to handle things like user input, animations,
     * and other game logic */
    private void Update()
    {
        if (isWallJumping && !isWallSliding)
        {
            canMove = false;
            float timer = 0;

            while (timer < 0.15f)
            {
                timer += Time.deltaTime;
            }

            if (WallCollision())
            {
                WallSlide();
            }
        }

        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (!canMove)
        {
            canDash = true;
            if (GroundCollision())
            {
                canMove = true;
                animator.SetBool("Grounded", true);
                if (Input.GetKeyDown(KeyCode.S) && horizontalInput != 0)
                {
                    Slide();
                }
            }


            if (Input.GetMouseButtonDown(1) && canDash)
            {
                StartCoroutine(Dash());
            }
            return;
        }

        // if the player is doing any of these things dont listen for any input
        if (isSliding || isWallJumping || isDashing)
        {
            return;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal"); // checks for L/R input
        verticalInput = Input.GetAxisRaw("Vertical"); // checks for L/R input

        if (canMove && !isWallJumping) // if the character is on the ground or in the air...
        {

            if (isSprinting) // ... do this for sprinting
            {
                playerRigidbody.velocity = new Vector2(horizontalInput * sprintSpeed, playerRigidbody.velocity.y);
                if (trail != null)
                {
                    trail.emitting = true;
                }
            }
            else // ... do this for regular movement
            {
                playerRigidbody.velocity = new Vector2(horizontalInput * walkSpeed, playerRigidbody.velocity.y);
                if (trail != null)
                {
                    trail.emitting = false;
                }

            }

            // used in attackAnimator to switch between idle/moving animations
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


        animator.SetFloat("VertSpeed", playerRigidbody.velocity.y);   // and other states (idle, running)



        if (Input.GetKey(KeyCode.LeftShift)) // start sprinting
        {
            isSprinting = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift)) // stop sprinting
        {
            isSprinting = false;
        }

        // call outside functions to handle wall slide/jump


        WallJump();


        if (Input.GetMouseButtonDown(1) && canDash) // perform dash
        {
            StartCoroutine(Dash());

        }

        // Calculates jump (tapping space)
        if ((GroundCollision()) && coyoteTimeCounter > 0f && Input.GetKeyDown(KeyCode.Space))
        {
            playerRigidbody.velocity = Vector2.up * jumpForce;
            animator.SetBool("Jumping", true);
            StartCoroutine(StopJump());
        }
        else if (!(GroundCollision()) && coyoteTimeCounter > 0f && Input.GetKeyDown(KeyCode.Space))
        {
            playerRigidbody.velocity = Vector2.up * jumpForce;
            coyoteTimeCounter = 0f;
            animator.SetBool("Jumping", true);
            StartCoroutine(StopJump());

        }

        if (RampCollision() && horizontalInput == 0)
        {
            playerRigidbody.isKinematic = true;
            playerRigidbody.velocity = new Vector2(0, 0);

            if (Input.GetKey(KeyCode.Space))
            {
                playerRigidbody.velocity = Vector2.up * jumpForce;
                animator.SetBool("Jumping", true);
                StartCoroutine(StopJump());
            }
        }
        else
        {
            playerRigidbody.isKinematic = false;
        }

        if (GroundCollision())
        {
            animator.SetBool("Grounded", true); // used in combo with vert speed to switch between falling

            dashCount = 0;
            RegainStamina();

            // Reset the isWallJumping flag(s) and the coyote time variable
            coyoteTimeCounter = coyoteTime;
            isWallSliding = false;
            animator.SetBool("WallSliding", isWallSliding);
            canDash = true;
            canMove = true;
            if (Input.GetKeyDown(KeyCode.S) && horizontalInput != 0)
            {
                Slide();
            }
        }
        else
        {

            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        if ((GroundCollision()) && WallCollision())
        {
            animator.SetBool("Grounded", true); // used in combo with vert speed to switch between falling
        }


        if (WallCollision())
        {

            if (!GroundCollision())
            {
                spriteRenderer.flipX = true;
            }

            isSliding = false;
            animator.SetBool("IsSliding", isSliding);

            WallSlide();
        }
        else
        {
            spriteRenderer.flipX = false;
            isWallSliding = false;
            animator.SetBool("WallSliding", isWallSliding);
            animator.SetBool("Grounded", GroundCollision());
        }


    }

    private IEnumerator StopJump()
    {
        yield return new WaitForSeconds(0.4f);
        isJumping = false;
        animator.SetBool("Jumping", isJumping);

    }


    /* Checks if the Player is standing on a surface */
    public bool GroundCollision()
    {

        bool grounded = false;

        rayHit2D = Physics2D.Raycast(rayOrigin.position, -Vector2.up, rayLength, groundLayer | rampLayer | wallLayer);

        if (rayHit2D != false)
        {
            // Check if the player is touching the ground layer
            grounded = true;

            Debug.DrawRay(rayOrigin.position, -Vector2.up * rayLength, Color.green);
            
        }
        else
        {
            // Draw a ray to visualize the ray's direction (no hit)
            Debug.DrawRay(rayOrigin.position, -Vector2.up * rayLength, Color.red);
        }
        
        animator.SetBool("Grounded", grounded);

        // Return true if the player's feet are grounded on a surface
        return grounded;
    }

    public bool RampCollision()
    {

        bool grounded = false;

        rayHit2D = Physics2D.Raycast(rayOrigin.position, -Vector2.up, rayLength, rampLayer);

        if (rayHit2D != false)
        {
            // Check if the player is touching the ground layer
            grounded = true;

            Debug.DrawRay(rayOrigin.position, -Vector2.up * rayLength, Color.green);

        }
        else
        {
            // Draw a ray to visualize the ray's direction (no hit)
            Debug.DrawRay(rayOrigin.position, -Vector2.up * rayLength, Color.red);
        }

        animator.SetBool("Grounded", grounded);

        // Return true if the player's feet are grounded on a surface
        return grounded;
    }

    /* Checks if the hitbox has collided with a wall */
    private bool WallCollision()
    {
        return wallCheck.IsTouchingLayers(wallLayer) /*|| wallCheck.IsTouchingLayers(treeLayer)*/;
    }


    /* Handles flippage of the character */
    public void Flip()
    {

        // Invert the sign of the scale's X component
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;

        // Reverse the facing direction flag
        facingRight = !facingRight;



    }

    private void Slide()
    {
        canMove = false;
        playerRigidbody.gravityScale += 0.8f;
        isSliding = true;
        animator.SetBool("IsSliding", isSliding);


        /*regPlayerCol.enabled = false;
        slidePlayerCol.enabled = true;*/

        if (horizontalInput < 0)
        {
            playerRigidbody.velocity = (Vector2.left * slidingSpeed);
        }
        else if (horizontalInput > 0)
        {
            playerRigidbody.velocity = (Vector2.right * slidingSpeed);
        }

        StartCoroutine(StopSlide(0.6f));
    }

    private IEnumerator StopSlide(float duration)
    {

        float elapsedTime = 0f;


        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        while (playerRigidbody.velocity.y > 0.01 || playerRigidbody.velocity.y < -0.01)
        {
            yield return null;
        }

        canMove = true;
        playerRigidbody.gravityScale = originalGravity;
        isSliding = false;
        animator.SetBool("IsSliding", isSliding);

    }

    /* Calculates speed at which player should slide on wall and handles dropping */
    private void WallSlide()
    {
        // if in the air and in contact with the wall...
        if (WallCollision() && !GroundCollision())
        {
            isWallSliding = true;
            spriteRenderer.flipX = true;
            canMove = true;


            if (!Input.GetKey(KeyCode.Space))
            {
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                {
                    if (Input.GetKey(KeyCode.W)) // wall climbing with horizontal input
                    {
                        isWallSliding = true;
                        canMove = true;
                        isWallJumping = false;
                        animator.SetBool("WallJumping", isWallJumping);

                        playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, wallClimbingSpeed);

                        if (horizontalInput != 0)
                        {
                            playerRigidbody.velocity = new Vector2(horizontalInput * walkSpeed, playerRigidbody.velocity.y);
                        }

                    }
                    else
                    {
                        isWallSliding = true;
                        playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, Mathf.Clamp(playerRigidbody.velocity.y, -wallSlidingSpeed, float.MaxValue));
                    }

                }
                else if (Input.GetKey(KeyCode.W)) // wall climbing with horizontal input
                {
                    isWallSliding = true;
                    canMove = true;
                    isWallJumping = false;
                    animator.SetBool("WallJumping", isWallJumping);

                    playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, wallClimbingSpeed);

                    if (horizontalInput != 0)
                    {
                        playerRigidbody.velocity = new Vector2(horizontalInput * walkSpeed, playerRigidbody.velocity.y);
                    }

                }
                else if (Input.GetKey(KeyCode.S)) // ... drops the player at normal speed instead of sliding
                {

                    playerRigidbody.velocity = new Vector2(horizontalInput * walkSpeed, playerRigidbody.velocity.y);
                    isWallSliding = false;
                }
                else // Wall slide behavior
                {
                    isWallSliding = false;

                }
            }
            else if (Input.GetKey(KeyCode.Space) && horizontalInput == 0)
            {
                isWallSliding = false;
                spriteRenderer.flipX = false;
            }

        }
        else if (GroundCollision())
        {
            isWallSliding = false;
            spriteRenderer.flipX = false;
        }

        animator.SetBool("WallSliding", isWallSliding);

    }

    /* Handles wall jump mechanisms */
    private void WallJump()
    {
        

        if (WallCollision() && !GroundCollision() && Input.GetKeyDown(KeyCode.Space) && horizontalInput == 0)
        {
            canMove = false;
            // set direction opposite of current player x direction
            wallJumpingDirection = -transform.localScale.x;
            isWallSliding = false;
            animator.SetBool("WallSliding", isWallSliding);
            isWallJumping = true;
            animator.SetBool("WallJumping", isWallJumping);


            // if player orientation is different from wall jump direction, update the way they are facing
            if (transform.localScale.x != wallJumpingDirection)
            {
                Flip(); // Use the existing Flip() method to flip the character

            }

            Debug.Log("WallJumpPower " + wallJumpPower);
            // Disable player input for the duration of wallJumpDuration


            playerRigidbody.velocity = new Vector2(wallJumpingDirection * wallJumpPower.x, wallJumpPower.y);
            StartCoroutine(PauseInputForDuration(wallJumpDuration));


            Debug.Log("Direction + Power.x: " + wallJumpingDirection * wallJumpPower.x + " Power.y " + wallJumpPower.y);



        }
    }


    public IEnumerator PauseInputForDuration(float duration) // pause input for wall jump
    {
        Debug.Log("PauseInputForDuration called with duration: " + duration);

        canMove = false;
        // Wait for the specified duration
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (GroundCollision())
            {
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // The coroutine will resume execution after the specified duration

        isWallJumping = false;
        animator.SetBool("WallJumping", isWallJumping);

    }


    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        isWallJumping = false;
        animator.SetBool("WallJumping", isWallJumping);
        // Ignore collisions with objects tagged as "Enemy"
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);


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
            if (stamina > 0)
            {
                DrainStamina(1);
            }

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

            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);
            yield break;
        }

        if (isDashing && (GroundCollision()))
        {
            playerRigidbody.gravityScale = 0f;
        }


        playerRigidbody.velocity = new Vector2(dashDirection.x, dashDirection.y);
        if (trail != null)
        {
            trail.emitting = true;
        }
        float startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < dashTime)
        {
            if (WallCollision())
            {
                break; // Exit the loop if WallCollision() is true
            }

            yield return null;
            elapsedTime = Time.time - startTime;
        }


        if (trail != null)
        {
            trail.emitting = false;
        }
        playerRigidbody.gravityScale = originalGravity;
        canMove = true;
        isDashing = false;
        canDash = true;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);

    }

}

