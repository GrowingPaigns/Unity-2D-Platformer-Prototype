using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings:")]
    [Space]

    [SerializeField] private Animator animator;         // Used to change animation values (and thereby animations) depending on different conditions
    [SerializeField] private float walkSpeed;           // Enemy walk speed
    [SerializeField] private float chaseSpeed;          // Sprint speed
    [SerializeField] private BoxCollider2D groundCheck; // Ground collision checker and layermask
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private BoxCollider2D wallCheck;   // Wall collision checker and layermask
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask rampLayer;

    [Space]
    [Header("Enemy-to-Enemy Interaction Settings:")]
    [Space]

    [SerializeField] private Collider2D hitbox;         // The main collider on the small enemy gameobject
    [SerializeField] private BoxCollider2D headbox;     // Will be later used with the player to create a mario-gomba stomp effect (just for small enemies)

    [Space]
    [Header("Player Interaction Settings:")]
    [Space]

    [SerializeField] private float chasingRaycastDist;  // Used to detect the player gameobject when we want the enemy to chase them
    [SerializeField] private float attackingRaycastDist;// Used to detect the player gameobject when we want the enemy to perform an attack
    public bool isKnockbackPaused = false;              // Knockback interaction from being attacked
    
    private float knockbackPauseTimer = 0f;             // Set by methods in the PlayerAttack class

    private SmallEnemyHealth health;
    private Rigidbody2D enemyRigidbody;

    private bool isMovingRight = true;
    private bool isWaiting = false;

    private float waitTime = 0f;
    private float moveTime = 0f;

    private bool isDetectionEnabled = true;

    private bool isAttacking = false; // Add this variable to track the attack state.
    private float attackDuration = 1f; // The duration of the attack animation.
    private float attackCooldownTimer = 0f; // Timer to track the cooldown
    private bool isAttackCooldown = false; // Indicates whether the attack is on cooldown


    private GameObject player;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        enemyRigidbody = GetComponent<Rigidbody2D>();
        SetRandomMovementTime();

        health = GetComponent<SmallEnemyHealth>();
    }

    private void Update()
    {
        if (isAttacking)
        {
            return;
        }

        if (isKnockbackPaused)
        {
            knockbackPauseTimer -= Time.deltaTime;

            if (knockbackPauseTimer <= 0f && GroundCollision())
            {
                isKnockbackPaused = false;
                isDetectionEnabled = true;
                enemyRigidbody.velocity = Vector2.zero;
                EnableMovement();
            }
            else
            {
                return;
            }
        }

        if (RampCollision())
        {
            enemyRigidbody.gravityScale = 0;
        }
        else
        {
            enemyRigidbody.gravityScale = 4;
        }

        if (isWaiting)
        {
            waitTime -= Time.deltaTime;
            if (waitTime <= 0f)
            {
                isWaiting = false;
                SetRandomMovementTime();
                Flip();
            }
        }


        if (!isWaiting && !isKnockbackPaused)
        {
            EnableMovement();
            if (!GroundCollision())
            {
                Flip();
            }

            if (WallCollision())
            {
                Flip();
            }

            

            if (isDetectionEnabled && PlayerInRange(chasingRaycastDist))
            {

                Vector3 runDirection = player.transform.position - transform.position;

                if (health.knockbackCounter < 1)
                {
                    enemyRigidbody.velocity = new Vector2(runDirection.normalized.x * chaseSpeed, enemyRigidbody.velocity.y);
                    animator.SetFloat("HorizSpeed", Mathf.Abs(enemyRigidbody.velocity.x));
                    FlipTowardsMovement();
                } else
                {
                    enemyRigidbody.velocity = new Vector2(runDirection.normalized.x * chaseSpeed/2, enemyRigidbody.velocity.y);
                    animator.SetFloat("HorizSpeed", Mathf.Abs(enemyRigidbody.velocity.x));
                    FlipTowardsMovement();
                }

                
            }
            else if (GroundCollision())
            {
                
                moveTime -= Time.deltaTime;
                if (moveTime <= 0f)
                {
                    isWaiting = true;
                    enemyRigidbody.velocity = Vector2.zero;
                    waitTime = Random.Range(0.5f, 2f); // Adjust the range as desired for the wait time
                    animator.SetFloat("HorizSpeed", 0);

                }
                else
                {

                    if (health.knockbackCounter < 1)
                    {
                        enemyRigidbody.velocity = new Vector2((isMovingRight ? 1 : -1) * walkSpeed, enemyRigidbody.velocity.y);
                        animator.SetFloat("HorizSpeed", Mathf.Abs(enemyRigidbody.velocity.x));
                    } else
                    {
                        enemyRigidbody.velocity = new Vector2((isMovingRight ? 1 : -1) * walkSpeed/2, enemyRigidbody.velocity.y);
                        animator.SetFloat("HorizSpeed", Mathf.Abs(enemyRigidbody.velocity.x));
                    }
                    
                }

                
            }
        }


        if (isAttackCooldown)
        {
            // Decrement the attackCooldownTimer.
            attackCooldownTimer -= Time.deltaTime;

            // Check if the cooldown has ended.
            if (attackCooldownTimer <= 0f)
            {
                isAttackCooldown = false;
            }
        }
        else
        {
            if (!isAttacking && PlayerInRange(attackingRaycastDist) && !isKnockbackPaused)
            {
                // Start attacking when the player is in range and not currently attacking or being knocked back.
                StartCoroutine(AttackCoroutine());

                // Set the attack cooldown timer to your desired delay (e.g., 1 second).
                float cooldownDelay = 1f; // Adjust as needed
                attackCooldownTimer = cooldownDelay;

                // Set isAttackCooldown to true to prevent re-entering the if statement.
                isAttackCooldown = true;
            }
        }

    }


    private IEnumerator AttackCoroutine()
    {
        enemyRigidbody.velocity = Vector2.zero;
        isAttacking = true;
        animator.SetBool("Attacking", true);

        // Wait for the attackDuration.
        yield return new WaitForSeconds(attackDuration);

        // End the attack animation.
        animator.SetBool("Attacking", false);
        isAttacking = false;
    }

    private bool PlayerInRange(float raycastDist)
    {
        
        if (player != null)
        {
            Vector3 direction = player.transform.position - transform.position;
            

            // Check if the player is directly above the enemy
            if (direction.sqrMagnitude <= 0.01f)
            {
                return false;
            }

            int rayCount = 10; // Number of rays in the cone
            float coneAngle = 45f; // Angle of the cone in degrees
            float angleIncrement = coneAngle / (rayCount - 1); // Angle increment between rays

            for (int i = 0; i < rayCount; i++)
            {
                float angle = -coneAngle / 2f + i * angleIncrement; // Calculate the angle of the current ray

                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up); // Rotate the direction vector by the angle
                Vector3 rotatedDirection = rotation * direction;

                // Apply a vertical offset to the rotatedDirection
                rotatedDirection += Vector3.up * 0.1f; // Adjust the vertical offset as desired

                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, rotatedDirection.normalized, raycastDist);

                bool playerDetected = false;

                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject != gameObject && !IsChildCollider(hit.collider, transform))
                    {
                        if (hit.collider.gameObject == player)
                        {
                            // Calculate the angle between the rotatedDirection and the direction to the player
                            float angleToPlayer = Vector3.Angle(direction, rotatedDirection);

                            // Exclude the rays with angles greater than a certain threshold (e.g., 75 degrees)
                            if (angleToPlayer <= 75f)
                            {
                                // Draw a line between the enemy and the detected player
                                Debug.DrawLine(transform.position, hit.collider.transform.position, Color.red);
                                playerDetected = true;
                                break;
                            }
                        }
                    }
                }

                if (playerDetected)
                {
                    return true;
                }

            }

            return false; 
        }

        return false;
    }

    private bool IsChildCollider(Collider2D collider, Transform parentTransform)
    {
        Transform colliderTransform = collider.transform;

        while (colliderTransform != null)
        {
            if (colliderTransform == parentTransform)
            {
                return true;
            }

            colliderTransform = colliderTransform.parent;
        }

        return false;
    }

    private bool RampCollision()
    {
        // Check if the player is touching the ramp layer
        bool onRamp = groundCheck.IsTouchingLayers(rampLayer);

        return onRamp;
    }

    public bool GroundCollision()
    {
        // Check if the enemy is touching the ground layer
        bool grounded = groundCheck.IsTouchingLayers(groundLayer);

        // Check if the enemy is not touching the wall layer
        bool onWall = groundCheck.IsTouchingLayers(wallLayer);

        // Check if the enemy is touching the ramp layer
        bool onRamp = groundCheck.IsTouchingLayers(rampLayer);

        // Return true if the enemy's feet are grounded on a surface
        return grounded || onWall || onRamp;
    }

    private bool WallCollision()
    {
        return wallCheck.IsTouchingLayers(wallLayer) || wallCheck.IsTouchingLayers(groundLayer);
    }

    private void Flip()
    {
        isMovingRight = !isMovingRight;
        Vector3 enemyScale = transform.localScale;
        enemyScale.x *= -1;
        transform.localScale = enemyScale;

        if (!isWaiting && GroundCollision())
        {
            enemyRigidbody.velocity = new Vector2((isMovingRight ? 1 : -1) * walkSpeed, enemyRigidbody.velocity.y);
        }
    }

    private void FlipTowardsMovement()
    {
        if (enemyRigidbody.velocity.x > 0 && !isMovingRight)
        {
            Flip();
        }
        else if (enemyRigidbody.velocity.x < 0 && isMovingRight)
        {
            Flip();
        }
    }

    private void SetRandomMovementTime()
    {
        moveTime = Random.Range(1f, 8f); // Adjust the range as desired for the random movement time
    }

    // Used in PlayerAttack class to disable player detection while the enemy is being attacked 
    public void DisableDetection(float duration)
    {
        isDetectionEnabled = false;
        StartCoroutine(PauseKnockback(duration));
    }

    public void EnableDetection()
    {
        isDetectionEnabled = true;
    }

    private IEnumerator PauseKnockback(float duration)
    {
        isKnockbackPaused = true;
        knockbackPauseTimer = duration;
        yield return new WaitForSeconds(duration);
    }

    // Used by PlayerAttack class to disable the enemy's movement
    public void DisableMovement()
    {
        isKnockbackPaused = true;
    }

    // Enable the enemy's movement
    public void EnableMovement()
    {
        isKnockbackPaused = false;
        isDetectionEnabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(hitbox, collision.collider);
            Physics2D.IgnoreCollision(headbox, collision.collider);
        }
    }
}