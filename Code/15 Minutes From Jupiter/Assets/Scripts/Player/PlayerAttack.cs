using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerMovement playerMovement;
    [SerializeField] float maxDistance;
    [SerializeField] float knockbackSpeed;
    [SerializeField] float knockbackDuration;

    private bool isKnockbackActive = false;
    private float knockbackTimer = 0f;
    private bool isRaycastLocked = false;
    private GameObject lockedEnemy;

    void Start()
    {
        // Initialize playerMovement reference if needed
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (isKnockbackActive)
        {
            // Update the knockback timer
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                // End the knockback effect
                isKnockbackActive = false;
            }
        }

        if (Input.GetMouseButtonDown(0) && !isKnockbackActive && !isRaycastLocked)
        {
            // Get the mouse position in the world
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Calculate the direction from the player to the mouse
            Vector2 direction = mousePosition - transform.position;

            // Normalize the direction to get a unit vector
            direction.Normalize();

            // Ignore the player's collider when performing the raycast
            int layerMask = ~(1 << gameObject.layer);

            // Calculate the distance to the mouse
            float distance = Vector2.Distance(transform.position, mousePosition);

            // Adjust the distance based on the maximum distance
            distance = Mathf.Clamp(distance, 0f, maxDistance);

            // Cast a ray from the player towards the mouse direction with the adjusted distance
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, distance, layerMask);

            // Draw the raycast for debugging purposes
            Debug.DrawRay(transform.position, direction * distance, Color.red, 0.1f);

            // Process the raycast hits
            foreach (RaycastHit2D hit in hits)
            {
                // Ignore child objects of the hit enemy
                if (hit.collider.gameObject.CompareTag("Enemy") && hit.transform.parent == null)
                {
                    // Perform actions based on the hit object
                    GameObject hitObject = hit.collider.gameObject;
                    Debug.Log("Hit object: " + hitObject.name);

                    // Lock the raycast to the hit enemy's position
                    isRaycastLocked = true;
                    lockedEnemy = hitObject;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isRaycastLocked)
            {
                // Unlock the raycast and return to following the mouse
                isRaycastLocked = false;
                lockedEnemy = null;
            }
            else
            {
                // Lock the raycast to the nearest enemy's position
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, maxDistance);
                float nearestDistance = Mathf.Infinity;
                GameObject nearestEnemy = null;

                foreach (Collider2D collider in colliders)
                {
                    if (collider.gameObject.CompareTag("Enemy") && collider.transform.parent == null)
                    {
                        float distanceToEnemy = Vector2.Distance(transform.position, collider.transform.position);
                        if (distanceToEnemy < nearestDistance)
                        {
                            nearestDistance = distanceToEnemy;
                            nearestEnemy = collider.gameObject;
                        }
                    }
                }

                if (nearestEnemy != null)
                {
                    isRaycastLocked = true;
                    lockedEnemy = nearestEnemy;
                }
            }
        }

        if (isRaycastLocked && lockedEnemy != null && Input.GetMouseButtonDown(0))
        {
            // Update the direction to the locked enemy's position
            Vector2 direction = lockedEnemy.transform.position - transform.position;
            direction.Normalize();

            // Cast a ray from the player towards the locked enemy's position
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, maxDistance);

            // Draw the raycast for debugging purposes
            Debug.DrawRay(transform.position, direction * maxDistance, Color.red, 0.1f);

            // Process the raycast hits
            foreach (RaycastHit2D hit in hits)
            {
                // Ignore child objects of the hit enemy
                if (hit.collider.gameObject.CompareTag("Enemy") && hit.transform.parent == null)
                {
                    // Update the locked enemy if it changes position
                    if (hit.collider.gameObject != lockedEnemy)
                    {
                        lockedEnemy = hit.collider.gameObject;
                    }

                    // Apply knockback velocity to the locked enemy
                    Rigidbody2D enemyRigidbody = lockedEnemy.GetComponent<Rigidbody2D>();
                    if (enemyRigidbody != null)
                    {
                        ApplyKnockbackVelocity(enemyRigidbody, direction);
                    }
                }
            }
        }
    }

    private void ApplyKnockbackVelocity(Rigidbody2D enemyRigidbody, Vector2 direction)
    {
        // Reset the knockback timer and activate knockback effect
        knockbackTimer = knockbackDuration;
        isKnockbackActive = true;

        // Apply knockback velocity
        enemyRigidbody.velocity = Vector2.zero;

        // Calculate knockback velocity based on player position relative to the enemy
        Vector2 knockbackVelocity;
        float playerY = transform.position.y;
        float enemyY = enemyRigidbody.transform.position.y;

        if (playerY >= enemyY)
        {
            // Player is above the enemy, launch enemy downwards
            knockbackVelocity = new Vector2(direction.x, 1f) * knockbackSpeed;
        }
        else if (playerY <= enemyY)
        {
            // Player is below the enemy, launch enemy upwards
            knockbackVelocity = new Vector2(direction.x, 1f) * knockbackSpeed;
        }
        else
        {
            // Player is horizontal to the enemy, launch enemy up and away
            knockbackVelocity = direction * knockbackSpeed;
        }

        enemyRigidbody.velocity = knockbackVelocity;

        // Disable enemy detection temporarily
        EnemyMovement enemyMovement = enemyRigidbody.GetComponent<EnemyMovement>();
        if (enemyMovement != null)
        {
            enemyMovement.DisableDetection(knockbackDuration);
        }
    }
}