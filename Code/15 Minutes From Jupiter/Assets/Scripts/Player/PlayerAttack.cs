using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack/Attack Movement Variables")]
    [Space]

    [SerializeField] private Animator playerAnimator;         // Used to play different animations based on movement

    [SerializeField] private float horizJumpForce;
    [SerializeField] private float vertJumpForce;
    
    [SerializeField] private float maxDistance;
    public float attackCooldown;
    

    [Space]
    [Header("Enemy/Camera Affecting Variables")]
    [Space]

    public bool isRaycastLocked = false;
    public GameObject lockedEnemy;
    public GameObject hitObject;
    public float cameraShakeDuration = 0.1f;
    public float cameraShakeMagnitude = 0.08f;

    [SerializeField] private float smashForceMultiplier;

    [SerializeField] private GameObject attackAnimation;

    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    
    private PlayerMovement playerMovement;
    private Rigidbody2D playerRigidbody;
    private float attackCooldownTimer = 0f;

    private Plane cursorPlane; // The plane on which the cursor will be positioned
    
    
    void Start()
    {
        // Initialize playerMovement reference if needed
        playerMovement = GetComponent<PlayerMovement>();
        playerRigidbody = GetComponent<Rigidbody2D>();
        cursorPlane = new Plane(Vector3.forward, Vector3.zero); // Define the cursor plane (e.g., XY plane)
        
        // Get the main camera reference
        mainCamera = Camera.main;
        playerAnimator.SetBool("Attacking", false);
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


            playerAnimator.SetBool("Attacking", true);
            playerMovement.isDashing = true;
            Vector3 hitPoint = new Vector3(0, 0, 0);
            Vector2 direction = new Vector2(0, 0);

            if (Input.GetKey(KeyCode.S))
            {
                playerAnimator.SetBool("Attacking", false);
                
                direction = Vector2.down; // Set the direction to straight up
                float forceHolder = vertJumpForce;
                vertJumpForce *= smashForceMultiplier;
                playerRigidbody.AddForce(new Vector2(direction.x * horizJumpForce, direction.y * vertJumpForce * 2), ForceMode2D.Impulse);
                vertJumpForce = forceHolder;

            }
            else
            {
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
            }



            playerMovement.PauseInputForDuration(attackCooldown);
            // Apply the jump force to the player's Rigidbody2D

            if (direction.y >= 0.80f)
            {
                playerRigidbody.velocity = new Vector2(direction.x * horizJumpForce, vertJumpForce * 1.2f);
            } else
            {
                playerRigidbody.velocity = new Vector2(direction.x * horizJumpForce, vertJumpForce);
            }

            if (direction.x < 0 && playerMovement.facingRight)
            {
                playerMovement.Flip();
            } else if (direction.x > 0 && !playerMovement.facingRight)
            {
                playerMovement.Flip();
            }
            
            // Start the attack cooldown
            attackCooldownTimer = attackCooldown;
            StartCoroutine(ResetAttackAnimation());

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

                    if (hitObject.CompareTag("Enemy"))
                    {
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

            playerAnimator.SetBool("Attacking", true);

            // Update the direction to the locked enemy's position
            Vector2 direction = lockedEnemy.transform.position - transform.position;
            direction.Normalize();

            playerMovement.PauseInputForDuration(attackCooldown);
            // Apply the jump force to the player's Rigidbody2D

            if (direction.y >= 0.80f)
            {
                playerRigidbody.velocity = new Vector2(direction.x * horizJumpForce, vertJumpForce * 1.2f);
            }
            else
            {
                playerRigidbody.velocity = new Vector2(direction.x * horizJumpForce, vertJumpForce);
            }

            if (direction.x < 0 && playerMovement.facingRight)
            {
                playerMovement.Flip();
            }
            else if (direction.x > 0 && !playerMovement.facingRight)
            {
                playerMovement.Flip();
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


    public IEnumerator ShakeCamera(float duration, float magnitude)
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
        playerAnimator.SetBool("Attacking", false);
        playerMovement.isDashing = false;
    }
}