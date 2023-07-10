using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerAttack : MonoBehaviour
{
    private PlayerMovement playerMovement;
    [SerializeField] private float horizJumpForce;
    [SerializeField] private float vertJumpForce;
    private Rigidbody2D playerRigidbody;
    [SerializeField] private float maxDistance;
    [SerializeField] private float attackCooldown;
    [SerializeField] private Animator animator;         // Used to play different animations based on movement
    [SerializeField] private float knockbackSpeed;
    public bool isRaycastLocked = false;
    public GameObject lockedEnemy;
    public GameObject hitObject;
    private float attackCooldownTimer = 0f;

    public float cameraShakeDuration = 0.1f;
    public float cameraShakeMagnitude = 0.08f;

    private Camera mainCamera;
    private Vector3 originalCameraPosition;

    private Plane cursorPlane; // The plane on which the cursor will be positioned



    void Start()
    {
        // Initialize playerMovement reference if needed
        playerMovement = GetComponent<PlayerMovement>();
        playerRigidbody = GetComponent<Rigidbody2D>();
        cursorPlane = new Plane(Vector3.forward, Vector3.zero); // Define the cursor plane (e.g., XY plane)
        
        // Get the main camera reference
        mainCamera = Camera.main;
        animator.SetBool("Attacking", false);
    }


    void Update()
    {
        // Update the attack cooldown timer
        attackCooldownTimer -= Time.deltaTime;

        if (playerMovement.isDashing)
        {
            // Skip reading horizontal input while attacking
            return;
        }

        if (Input.GetMouseButtonDown(0) && !isRaycastLocked && attackCooldownTimer <= 0f)
        {
            animator.SetBool("Attacking", true);
            playerMovement.isDashing = true;
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

            

            playerMovement.PauseInputForDuration(attackCooldown);
            // Apply the jump force to the player's Rigidbody2D

            if (direction.y >= 0.80f)
            {
                playerRigidbody.AddForce(new Vector2(direction.x * horizJumpForce, vertJumpForce * 2), ForceMode2D.Impulse);
            } else
            {
                playerRigidbody.AddForce(new Vector2(direction.x * horizJumpForce, vertJumpForce), ForceMode2D.Impulse);
            }

            if (direction.x < 0 && playerMovement.facingRight)
            {
                playerMovement.Flip();
            } else if (direction.x > 0 && !playerMovement.facingRight)
            {
                playerMovement.Flip();
            }
            
            Debug.Log(direction);





            // Ignore the player's collider when performing the raycast
            int layerMask = ~(1 << gameObject.layer);

            // Calculate the distance to the mouse
            float distance = Vector2.Distance(transform.position, hitPoint);

            // Adjust the distance based on the maximum distance
            distance = Mathf.Clamp(distance, 0f, maxDistance);

            // Cast a wider ray by dividing it into segments
            int numSegments = 1; // Adjust the number of segments as desired
            float segmentWidth = 1f; // Adjust the segment width as desired
            float halfWidth = (numSegments - 1) * segmentWidth / 2f;
            Vector2 startPos = transform.position - transform.up * halfWidth;

            for (int i = 0; i < numSegments; i++)
            {
                Vector2 castPosition = startPos + (Vector2)transform.up * i * segmentWidth;

                // Cast a ray from the player towards the mouse direction with the adjusted distance
                RaycastHit2D[] hits = Physics2D.RaycastAll(castPosition, direction, distance, layerMask);

                // Draw the raycast for debugging purposes
                Debug.DrawRay(castPosition, direction * distance, Color.red, 0.1f);

                // Process the raycast hits
                foreach (RaycastHit2D hit in hits)
                {
                    // Ignore child objects of the hit enemy
                    if (hit.collider.gameObject.CompareTag("Enemy") && hit.transform.parent == null)
                    {
                        // Perform actions based on the hit object
                        hitObject = hit.collider.gameObject;
                        Debug.Log("Hit object: " + hitObject.name);

                        // Apply knockback velocity to the locked enemy
                        Rigidbody2D enemyRigidbody = hit.collider.gameObject.GetComponent<Rigidbody2D>();
                        EnemyMovement enemyMovement = hit.collider.gameObject.GetComponent<EnemyMovement>();
                        SmallEnemyHealth enemyHealth = hit.collider.gameObject.GetComponent<SmallEnemyHealth>();
                        if (enemyRigidbody != null)
                        {
                            Debug.Log("Knockback is paused");
                            enemyMovement.DisableMovement();

                            // Apply knockback velocity to the enemy
                            Vector2 knockbackDirection = enemyRigidbody.transform.position - transform.position;
                            knockbackDirection.Normalize();
                            // Calculate the knockback force by multiplying the knockback direction with the knockback speed
                            Vector2 knockbackForce = knockbackDirection * knockbackSpeed;
                            // Apply the knockback force to the enemy's Rigidbody2D
                            enemyRigidbody.velocity = knockbackForce;
                            // Apply an upward force to the enemy
                            Vector2 upwardForce = Vector2.up * (knockbackSpeed / 2);
                            enemyRigidbody.AddForce(upwardForce, ForceMode2D.Impulse);
                            // Increment the knockback counter, disable movement, etc.
                            enemyHealth.IncrementKnockbackCounter();
                            enemyMovement.DisableDetection(attackCooldown);
                            enemyMovement.isKnockbackPaused = true;
                            Debug.Log(enemyMovement.isKnockbackPaused);
                            // Shake the camera
                            StartCoroutine(ShakeCamera(cameraShakeDuration, cameraShakeMagnitude));
                        }
                    }
                }
            }

            StartCoroutine(ResetAttackAnimation());
            attackCooldownTimer = attackCooldown;
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

            animator.SetBool("Attacking", true);

            // Update the direction to the locked enemy's position
            Vector2 direction = lockedEnemy.transform.position - transform.position;
            direction.Normalize();

            playerMovement.PauseInputForDuration(attackCooldown);
            // Apply the jump force to the player's Rigidbody2D

            if (direction.y >= 0.80f)
            {
                playerRigidbody.AddForce(new Vector2(direction.x * horizJumpForce, vertJumpForce * 2), ForceMode2D.Impulse);
            }
            else
            {
                playerRigidbody.AddForce(new Vector2(direction.x * horizJumpForce, vertJumpForce), ForceMode2D.Impulse);
            }

            if (direction.x < 0 && playerMovement.facingRight)
            {
                playerMovement.Flip();
            }
            else if (direction.x > 0 && !playerMovement.facingRight)
            {
                playerMovement.Flip();
            }


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
                    hitObject = hit.collider.gameObject;
                    Debug.Log("Hit object: " + hitObject.name);

                    // Lock the raycast to the hit enemy's position
                    isRaycastLocked = true;
                    lockedEnemy = hitObject;

                    // Apply knockback velocity to the locked enemy
                    Rigidbody2D enemyRigidbody = hit.collider.gameObject.GetComponent<Rigidbody2D>();
                    EnemyMovement enemyMovement = hit.collider.gameObject.GetComponent<EnemyMovement>();
                    SmallEnemyHealth enemyHealth = hit.collider.gameObject.GetComponent<SmallEnemyHealth>();
                    if (enemyRigidbody != null)
                    {
                        Debug.Log("Knockback is paused");
                        enemyMovement.DisableMovement();

                        // Apply knockback velocity to the enemy
                        Vector2 knockbackDirection = enemyRigidbody.transform.position - transform.position;
                        knockbackDirection.Normalize();
                        // Calculate the knockback force by multiplying the knockback direction with the knockback speed
                        Vector2 knockbackForce = knockbackDirection * knockbackSpeed;
                        // Apply the knockback force to the enemy's Rigidbody2D
                        enemyRigidbody.velocity = knockbackForce;
                        // Apply an upward force to the enemy
                        Vector2 upwardForce = Vector2.up * (knockbackSpeed/2);
                        enemyRigidbody.AddForce(upwardForce, ForceMode2D.Impulse);
                        // Increment the knockback counter, disable movement, etc.
                        enemyHealth.IncrementKnockbackCounter();
                        enemyMovement.DisableDetection(attackCooldown);
                        enemyMovement.isKnockbackPaused = true;
                        Debug.Log(enemyMovement.isKnockbackPaused);
                        // Shake the camera
                        StartCoroutine(ShakeCamera(cameraShakeDuration, cameraShakeMagnitude));
                        
                    }
                }
            }

            // Start the attack cooldown
            attackCooldownTimer = attackCooldown;
            StartCoroutine(ResetAttackAnimation());

        }

        if (isRaycastLocked && lockedEnemy == null)
        {
            // Unlock the raycast and return to following the mouse
            isRaycastLocked = false;
            lockedEnemy = null;
        }


    }


    private IEnumerator ShakeCamera(float duration, float magnitude)
    {
        originalCameraPosition = mainCamera.transform.localPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.localPosition = originalCameraPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;

            yield return null;
        }

        mainCamera.transform.localPosition = originalCameraPosition;
    }


    private IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(attackCooldown); // Adjust the delay as needed
        animator.SetBool("Attacking", false);
        playerMovement.isDashing = false;
    }
}