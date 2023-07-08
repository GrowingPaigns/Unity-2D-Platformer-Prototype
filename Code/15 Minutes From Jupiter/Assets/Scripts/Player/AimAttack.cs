using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimAttack : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;
    private Transform aimTransform;
    [SerializeField] private GameObject weaponParent;
    [SerializeField] private Animator animator;
    private float rotationSpeed = 100000f; // Adjust this value to control the rotation speed
    private float radius = 2f; // Adjust this value to set the desired radius
    private Ray mousePosition;
    private bool isAttacking = false; // Flag to track the attack state
    private bool isAnimationPlaying = false; // Flag to track if the attack animation is playing

    private Quaternion initialRotation; // Store the initial rotation of the weaponParent

    private bool isInputPaused = false; // Flag to track input pause state
    private float inputPauseDuration = 0.34f; // Duration to pause input

    private bool shouldFlip = false;

    private void Start()
    {
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        playerAttack = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAttack>();
        mousePosition = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Store the initial rotation of the weaponParent
        initialRotation = weaponParent.transform.rotation;
    }

    private void Update()
    {
        if (isInputPaused)
        {
            return; // Skip input processing when input is paused
        }

        if (Input.GetMouseButtonDown(0) && !isAnimationPlaying)
        {
            
            
            isAttacking = true; // Start the attack
            isAnimationPlaying = true; // Set the animation playing flag

            mousePosition = Camera.main.ScreenPointToRay(Input.mousePosition);

            Vector3 mousePos = mousePosition.GetPoint(radius);
            mousePos.z = transform.position.z; // Lock the z-axis position of the mouse
            Vector2 lookDir = new Vector2();

            if (playerAttack.lockedEnemy != null)
            {
                lookDir = playerAttack.lockedEnemy.transform.position - transform.position;
                
            } else
            {
                lookDir = mousePos - transform.position;
                
            }

            Debug.Log(lookDir);

            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

            if (!playerMovement.facingRight && lookDir.x > 0)
            {
                shouldFlip = true;
            }
            else if (playerMovement.facingRight && lookDir.x < 0)
            {
                shouldFlip = true;
            }

            if (!playerMovement.facingRight) // Adjust the angle based on player's facing direction
            {
                angle += 180f;
                // Flip the y-axis of the sprite
                Vector3 scale = weaponParent.transform.localScale;
                scale.y *= -1;
                weaponParent.transform.localScale = scale;
            }



            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            weaponParent.transform.rotation = Quaternion.RotateTowards(initialRotation, rotation, rotationSpeed * Time.deltaTime);

            isInputPaused = true; // Pause input
            
            StartCoroutine(ResumeInputAfterDelay(inputPauseDuration)); // Resume input after the specified duration

            animator.SetBool("Attacking", true);


        }

        if (Input.GetMouseButtonUp(0) && isAttacking) // Check if the mouse button was released
        {
            //StartCoroutine(ResetAttackAnimation()); // Start the coroutine to reset the animation
            

        }
    }


    private IEnumerator ResumeInputAfterDelay(float delay)
    {
        playerMovement.isDashing = true;
        yield return new WaitForSeconds(delay);
        isInputPaused = false; // Resume input after the specified delay

        if (isAttacking)
        {
            animator.SetBool("Attacking", false);
            isAttacking = false; // Stop the attack
        }

        if (shouldFlip)
        {
            playerMovement.Flip();
            shouldFlip = false;
        }
        playerMovement.isDashing = false;
        isAnimationPlaying = false; // Reset the animation playing flag
    }
}