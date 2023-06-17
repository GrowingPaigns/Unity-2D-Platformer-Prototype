using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerAttack : MonoBehaviour
{
    private PlayerMovement playerMovement;
    [SerializeField] float maxDistance;
    [SerializeField] float knockbackSpeed;
    [SerializeField] float knockbackDuration;
    [SerializeField] float attackCooldown;

    private bool isKnockbackActive = false;
    private float knockbackTimer = 0f;
    public bool isRaycastLocked = false;
    public GameObject lockedEnemy;
    private float attackCooldownTimer = 0f;


    private Plane cursorPlane; // The plane on which the cursor will be positioned

    void Start()
    {
        // Initialize playerMovement reference if needed
        playerMovement = GetComponent<PlayerMovement>(); 
        cursorPlane = new Plane(Vector3.forward, Vector3.zero); // Define the cursor plane (e.g., XY plane)

    }


    void Update()
    {
        // Update the attack cooldown timer
        attackCooldownTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 playerToMouse = mousePosition - transform.position;
            playerToMouse.z = 0f; // Ensure the z-component is zero

            float moveDistance = 0.5f; // Adjust this value to control the movement distance

            // Calculate the target position to move towards
            Vector3 targetPosition = transform.position + playerToMouse.normalized * moveDistance;

            // Move the player towards the target position using a smooth movement
            StartCoroutine(MoveTowards(targetPosition));
        }


        if (Input.GetMouseButtonDown(0) && !isRaycastLocked && attackCooldownTimer <= 0f)
        {
            Vector3 hitPoint = new Vector3(0, 0, 0);
            Vector2 direction = new Vector2(0, 0);

            // Get the mouse position in the world
            Ray mousePosition = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (cursorPlane.Raycast(mousePosition, out float dist))
            {
                hitPoint = mousePosition.GetPoint(dist);
            }

            // Calculate the direction from the player to the mouse
            direction = hitPoint - transform.position;

            // Normalize the direction to get a unit vector
            direction.Normalize();

            // Ignore the player's collider when performing the raycast
            int layerMask = ~(1 << gameObject.layer);

            // Calculate the distance to the mouse
            float distance = Vector2.Distance(transform.position, hitPoint);

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

                    // Apply knockback velocity to the locked enemy
                    Rigidbody2D enemyRigidbody = hit.collider.gameObject.GetComponent<Rigidbody2D>();
                    if (enemyRigidbody != null)
                    {
                        ApplyKnockbackVelocity(enemyRigidbody, direction);
                    }

                    // Start the attack cooldown
                    attackCooldownTimer = attackCooldown;

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
                // Convert the mouse position to a world position
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // Perform an overlap check at the mouse position
                Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);

                if (hitCollider != null)
                {
                    GameObject hitObject = hitCollider.gameObject;

                    if (hitObject.CompareTag("Enemy") && hitObject.transform.parent == null)
                    {
                        // Perform actions based on the hit object (enemy)
                        Debug.Log("Hit object: " + hitObject.name);

                        // Lock the raycast to the hit enemy's position
                        isRaycastLocked = true;
                        lockedEnemy = hitObject;
                    }
                    else
                    {
                        // Perform actions for other objects (non-enemies)
                        Debug.Log("Hit object is not an enemy.");
                    }
                }
                else
                {
                    // No object was hit, unlock the raycast and return to following the mouse
                    isRaycastLocked = false;
                    lockedEnemy = null;
                }
            }
        }

        if (isRaycastLocked && lockedEnemy != null && Input.GetMouseButtonDown(0) && attackCooldownTimer <= 0f)
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

                    // Perform actions based on the hit object
                    GameObject hitObject = hit.collider.gameObject;
                    Debug.Log("Hit object: " + hitObject.name);

                    // Lock the raycast to the hit enemy's position
                    isRaycastLocked = true;
                    lockedEnemy = hitObject;

                    // Apply knockback velocity to the locked enemy
                    Rigidbody2D enemyRigidbody = lockedEnemy.GetComponent<Rigidbody2D>();
                    EnemyMovement enemyMovement = enemyRigidbody.GetComponent<EnemyMovement>();
                    if (enemyRigidbody != null)
                    {
                        ApplyKnockbackVelocity(enemyRigidbody, direction);
                        enemyMovement.EnableDetection();
                    }

                    // Start the attack cooldown
                    attackCooldownTimer = attackCooldown;
                }
            }
        }

        if (isRaycastLocked && lockedEnemy == null)
        {
            // Unlock the raycast and return to following the mouse
            isRaycastLocked = false;
            lockedEnemy = null;
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
        Vector2 knockbackVelocity = direction * knockbackSpeed;

        enemyRigidbody.velocity = knockbackVelocity;

        // Disable enemy detection temporarily
        EnemyMovement enemyMovement = enemyRigidbody.GetComponent<EnemyMovement>();
        if (enemyMovement != null)
        {
            enemyMovement.DisableDetection(knockbackDuration);
        }
    }

    private IEnumerator MoveTowards(Vector3 targetPosition)
    {
        float duration = 0.05f; // Adjust this value to control the movement duration
        float elapsedTime = 0f;
        Vector3 startingPosition = transform.position;

        while (elapsedTime < duration)
        {
            // Calculate the progress (0 to 1) based on the elapsed time and duration
            float progress = elapsedTime / duration;

            // Calculate the current position based on the starting and target positions
            Vector3 currentPosition = Vector3.Lerp(startingPosition, targetPosition, progress);

            // Move the player to the current position
            playerMovement.GetComponent<Rigidbody2D>().MovePosition(currentPosition);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Stop the player's movement by setting the velocity to zero
        playerMovement.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
    }


}