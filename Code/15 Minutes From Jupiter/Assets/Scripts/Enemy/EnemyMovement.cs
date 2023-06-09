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
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float raycastDistance;

    private Rigidbody2D rb;
    private bool isMovingRight = true;
    private bool isWaiting = false;
    private float waitTime = 0f;
    private float moveTime = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SetRandomMovementTime();
    }

    private void Update()
    {
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
        else
        {
            if (!GroundCollision())
            {
                Flip();
            }

            if (WallCollision())
            {
                Flip();
            }

            if (PlayerInRange())
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                Vector3 direction = player.transform.position - transform.position;
                rb.velocity = new Vector2(direction.normalized.x * chaseSpeed, rb.velocity.y);
                animator.SetFloat("HorizSpeed", Mathf.Abs(rb.velocity.x));
                FlipTowardsMovement();
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
                    rb.velocity = new Vector2((isMovingRight ? 1 : -1) * walkSpeed, rb.velocity.y);
                    animator.SetFloat("HorizSpeed", Mathf.Abs(rb.velocity.x));
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
            direction.y = 0f;

            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction.normalized, raycastDistance);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject == player)
                {
                    return true;
                }
            }

            Debug.DrawRay(transform.position, direction.normalized * raycastDistance, Color.red);
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
        rb.velocity = new Vector2((isMovingRight ? 1 : -1) * walkSpeed, rb.velocity.y);
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
}