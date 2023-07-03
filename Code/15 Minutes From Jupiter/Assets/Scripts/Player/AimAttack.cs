using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimAttack : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private Transform aimTransform;
    [SerializeField] private GameObject weaponParent;
    [SerializeField] private Animator animator;
    private float rotationSpeed = 100000f; // Adjust this value to control the rotation speed
    private float radius = 2f; // Adjust this value to set the desired radius
    private Ray mousePosition;
    private bool isAttacking = false; // Flag to track the attack state
    private bool isAnimationPlaying = false; // Flag to track if the attack animation is playing

    private Quaternion initialRotation; // Store the initial rotation of the weaponParent

    private void Start()
    {
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        mousePosition = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Store the initial rotation of the weaponParent
        initialRotation = weaponParent.transform.rotation;
    }

    private void Update()
    {

        if (Input.GetMouseButtonDown(0) && !isAnimationPlaying)
        {
            isAttacking = true; // Start the attack
            isAnimationPlaying = true; // Set the animation playing flag

            mousePosition = Camera.main.ScreenPointToRay(Input.mousePosition);

            Vector3 mousePos = mousePosition.GetPoint(radius);
            mousePos.z = transform.position.z; // Lock the z-axis position of the mouse

            Vector3 lookDir = mousePos - transform.position;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

            if (!playerMovement.facingRight) // Adjust the angle based on player's facing direction
            {
                angle += 180f;
                // Flip the y-axis of the sprite
                Vector3 scale = weaponParent.transform.localScale;
                scale.y *= -1;
                weaponParent.transform.localScale = scale;

                //playerAnimator.SetBool("AttackingBehind", true);
            }

            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            weaponParent.transform.rotation = Quaternion.RotateTowards(initialRotation, rotation, rotationSpeed * Time.deltaTime);

            animator.SetBool("Attacking", true);
            //playerAnimator.SetBool("AttackingInfront", true);
        }

        if (Input.GetMouseButtonUp(0)) // Check if the mouse button was released
        {
            if (isAttacking)
            {
                StartCoroutine(ResetAttackAnimation()); // Start the coroutine to reset the animation
            }
        }
    }

    private IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(0.33f); // Adjust the delay as needed
        animator.SetBool("Attacking", false);
        //playerAnimator.SetBool("AttackingInfront", false);
        //playerAnimator.SetBool("AttackingBehind", false);
        isAttacking = false; // Stop the attack
        isAnimationPlaying = false; // Reset the animation playing flag
    }
}