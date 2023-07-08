using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float chaseSpeed;
    [SerializeField] private BoxCollider2D groundCheck;
    [SerializeField] private BoxCollider2D wallCheck;
    [SerializeField] private BoxCollider2D hitbox;
    [SerializeField] private BoxCollider2D headbox;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float raycastDistance;

    private SmallEnemyHealth health;
    private Rigidbody2D rb;
    private bool isMovingRight = true;
    private bool isWaiting = false;
    private float waitTime = 0f;
    private float moveTime = 0f;
    private bool isDetectionEnabled = true;
    public bool isKnockbackPaused = false;
    private float knockbackPauseTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SetRandomMovementTime();

        health = GetComponent<SmallEnemyHealth>();
    }

    private void Update()
    {
        if (isKnockbackPaused)
        {
            knockbackPauseTimer -= Time.deltaTime;

            if (knockbackPauseTimer <= 0f && GroundCollision())
            {
                isKnockbackPaused = false;
                isDetectionEnabled = true;
                rb.velocity = Vector2.zero;
                EnableMovement();
            }
            else
            {
                return;
            }
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

            if (isDetectionEnabled && PlayerInRange())
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                Vector3 direction = player.transform.position - transform.position;

                if (health.knockbackCounter < 1)
                {
                    rb.velocity = new Vector2(direction.normalized.x * chaseSpeed, rb.velocity.y);
                    animator.SetFloat("HorizSpeed", Mathf.Abs(rb.velocity.x));
                    FlipTowardsMovement();
                } else
                {
                    rb.velocity = new Vector2(direction.normalized.x * chaseSpeed/2, rb.velocity.y);
                    animator.SetFloat("HorizSpeed", Mathf.Abs(rb.velocity.x));
                    FlipTowardsMovement();
                }
                
            }
            else if (GroundCollision())
            {
                moveTime -= Time.deltaTime;
                if (moveTime <= 0f)
                {
                    isWaiting = true;
                    rb.velocity = Vector2.zero;
                    waitTime = Random.Range(0.5f, 2f); // Adjust the range as desired for the wait time
                    animator.SetFloat("HorizSpeed", 0);
                }
                else
                {
                    if (health.knockbackCounter < 1)
                    {
                        rb.velocity = new Vector2((isMovingRight ? 1 : -1) * walkSpeed, rb.velocity.y);
                        animator.SetFloat("HorizSpeed", Mathf.Abs(rb.velocity.x));
                    } else
                    {
                        rb.velocity = new Vector2((isMovingRight ? 1 : -1) * walkSpeed/2, rb.velocity.y);
                        animator.SetFloat("HorizSpeed", Mathf.Abs(rb.velocity.x));
                    }
                    
                }
            }
        }
    }

    private bool PlayerInRange()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
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

                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, rotatedDirection.normalized, raycastDistance);

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

            return false; // Move this line outside the foreach loop
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

    private bool GroundCollision()
    {
        return groundCheck.IsTouchingLayers(groundLayer) || groundCheck.IsTouchingLayers(wallLayer);
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
            rb.velocity = new Vector2((isMovingRight ? 1 : -1) * walkSpeed, rb.velocity.y);
        }
    }

    private void FlipTowardsMovement()
    {
        if (rb.velocity.x > 0 && !isMovingRight)
        {
            Flip();
        }
        else if (rb.velocity.x < 0 && isMovingRight)
        {
            Flip();
        }
    }

    private void SetRandomMovementTime()
    {
        moveTime = Random.Range(1f, 8f); // Adjust the range as desired for the random movement time
    }

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

    // Disable the enemy's movement
    public void DisableMovement()
    {
        isKnockbackPaused = true;
        knockbackPauseTimer = 0.33f; // Set the duration of knockback pause

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