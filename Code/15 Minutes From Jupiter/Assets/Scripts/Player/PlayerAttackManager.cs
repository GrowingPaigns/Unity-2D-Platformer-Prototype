using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerAttackManager : MonoBehaviour
{
    [SerializeField] private GameObject weaponParent;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator attackAnimator;

    [SerializeField] private bool isRaycastLocked = false;
    [SerializeField] private GameObject lockedEnemy;
    [SerializeField] private GameObject hitObject;
    [SerializeField] private float cameraShakeDuration = 0.1f;
    [SerializeField] private float cameraShakeMagnitude = 0.08f;
    [SerializeField] private float knockbackSpeed;
    [SerializeField] private float smashForceMultiplier;


    [SerializeField] private float horizJumpForce;
    [SerializeField] private float vertJumpForce;
    [SerializeField] private float maxDistance;
    [SerializeField] private float attackCooldown;

    private float rotationSpeed = 100000f;
    private float radius = 2f;
    private Ray mousePosition;
    private bool isAttacking = false;
    private bool isAnimationPlaying = false;
    private bool isInputPaused = false;
    private float inputPauseDuration = 0.34f;
    private PlayerMovement playerMovement;
    private Rigidbody2D playerRigidbody;
    private bool shouldFlip = false;
    private Vector3 originalCameraPosition;
    private Plane cursorPlane;
    private float attackCooldownTimer = 0f;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerRigidbody = GetComponent<Rigidbody2D>();
        cursorPlane = new Plane(Vector3.forward, Vector3.zero);
        originalCameraPosition = Camera.main.transform.localPosition;
    }

    private void Update()
    {
        if (isInputPaused)
            return;

        if (Input.GetMouseButtonDown(0) && !isAnimationPlaying && !Input.GetKey(KeyCode.S))
        {
            isAttacking = true;
            isAnimationPlaying = true;
            mousePosition = Camera.main.ScreenPointToRay(Input.mousePosition);

            Vector3 mousePos = mousePosition.GetPoint(radius);
            mousePos.z = transform.position.z;
            Vector2 lookDir = new Vector2();

            if (lockedEnemy != null)
            {
                lookDir = lockedEnemy.transform.position - transform.position;
            }
            else
            {
                lookDir = mousePos - transform.position;
            }

            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

            if (!playerMovement.facingRight && lookDir.x > 0)
                shouldFlip = true;
            else if (playerMovement.facingRight && lookDir.x < 0)
                shouldFlip = true;

            if (!playerMovement.facingRight)
            {
                angle += 180f;
                Vector3 scale = weaponParent.transform.localScale;
                scale.y *= -1;
                weaponParent.transform.localScale = scale;
            }

            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            weaponParent.transform.rotation = Quaternion.RotateTowards(weaponParent.transform.rotation, rotation, rotationSpeed * Time.deltaTime);
            isInputPaused = true;
            StartCoroutine(ResumeInputAfterDelay(inputPauseDuration));
            attackAnimator.SetBool("Attacking", true);
        }

        if (Input.GetMouseButtonUp(0) && isAttacking)
        {
            StartCoroutine(ResetAttackAnimation());
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (lockedEnemy != null)
            {
                isRaycastLocked = false;
                lockedEnemy = null;
            }
            else
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);

                if (hitCollider != null)
                {
                    GameObject hitObject = hitCollider.gameObject;

                    if (hitObject.CompareTag("Enemy") && hitObject.transform.parent == null)
                    {
                        isRaycastLocked = true;
                        lockedEnemy = hitObject;
                    }
                }
                else
                {
                    isRaycastLocked = false;
                    lockedEnemy = null;
                }
            }
        }

        if (isRaycastLocked && lockedEnemy != null && Input.GetMouseButtonDown(0))
        {
            playerAnimator.SetBool("Attacking", true);
            Vector2 direction = lockedEnemy.transform.position - transform.position;
            direction.Normalize();
            playerMovement.PauseInputForDuration(attackCooldown);

            if (direction.y >= 0.80f)
                playerRigidbody.AddForce(new Vector2(direction.x * horizJumpForce, vertJumpForce * 2), ForceMode2D.Impulse);
            else
                playerRigidbody.AddForce(new Vector2(direction.x * horizJumpForce, vertJumpForce), ForceMode2D.Impulse);

            if (direction.x < 0 && playerMovement.facingRight)
                shouldFlip = true;
            else if (direction.x > 0 && !playerMovement.facingRight)
                shouldFlip = true;

            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, maxDistance);
            Debug.DrawRay(transform.position, direction * maxDistance, Color.red, 0.1f);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject.CompareTag("Enemy") && hit.transform.parent == null)
                {
                    if (hit.collider.gameObject != lockedEnemy)
                        lockedEnemy = hit.collider.gameObject;

                    hitObject = hit.collider.gameObject;
                    isRaycastLocked = true;
                    isInputPaused = true;

                    Rigidbody2D enemyRigidbody = hit.collider.gameObject.GetComponent<Rigidbody2D>();
                    EnemyMovement enemyMovement = hit.collider.gameObject.GetComponent<EnemyMovement>();
                    SmallEnemyHealth enemyHealth = hit.collider.gameObject.GetComponent<SmallEnemyHealth>();

                    if (enemyRigidbody != null)
                    {
                        enemyMovement.DisableMovement();
                        Vector2 knockbackDirection = enemyRigidbody.transform.position - transform.position;
                        knockbackDirection.Normalize();
                        Vector2 knockbackForce = knockbackDirection * knockbackSpeed;
                        enemyRigidbody.velocity = knockbackForce;
                        Vector2 upwardForce = Vector2.up * (knockbackSpeed / 2);
                        enemyRigidbody.AddForce(upwardForce, ForceMode2D.Impulse);
                        enemyHealth.IncrementKnockbackCounter();
                        enemyMovement.DisableDetection(attackCooldown);
                        enemyMovement.isKnockbackPaused = true;
                        StartCoroutine(ShakeCamera(cameraShakeDuration, cameraShakeMagnitude));
                    }
                }
            }

            attackCooldownTimer = attackCooldown;
            StartCoroutine(ResetAttackAnimation());
        }

        if (isRaycastLocked && lockedEnemy == null)
        {
            isRaycastLocked = false;
            lockedEnemy = null;
        }
    }

    private IEnumerator ResumeInputAfterDelay(float delay)
    {
        playerMovement.isDashing = true;
        yield return new WaitForSeconds(delay);
        isInputPaused = false;

        if (isAttacking)
        {
            attackAnimator.SetBool("Attacking", false);
            isAttacking = false;
        }

        if (shouldFlip)
        {
            playerMovement.Flip();
            shouldFlip = false;
        }

        playerMovement.isDashing = false;
        isAnimationPlaying = false;
    }

    private IEnumerator ShakeCamera(float duration, float magnitude)
    {
        originalCameraPosition = Camera.main.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            Camera.main.transform.localPosition = originalCameraPosition + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalCameraPosition;
    }

    private IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(attackCooldown);
        playerAnimator.SetBool("Attacking", false);
        playerMovement.isDashing = false;
    }
}