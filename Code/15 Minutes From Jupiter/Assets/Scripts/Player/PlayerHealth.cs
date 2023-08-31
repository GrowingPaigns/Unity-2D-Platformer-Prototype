using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHealth : MonoBehaviour
{
    public static event Action OnPlayerDamage;

    [Header("Player Damage Settings:")]
    [Space]

    [SerializeField] private float playerMovementDelay;
    [SerializeField] private float invincibilityTime;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private GameObject player;

    [Space]
    [Header("Player Health Settings:")]
    [Space]

    public float health;
    public float maxHealth;

    private PlayerMovement playerMovement;
    private Rigidbody2D playerRigidbody;
    private Vector2 collisionDirection = new Vector2();


    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerRigidbody = GetComponent<Rigidbody2D>();

    }

    private void Update()
    {
        if (!playerMovement.canMove)
        {
            return;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (!playerMovement.isHurt)
            {
                Debug.Log("NOT Ignoring Collision");
                string enemyType = collision.gameObject.name;
                float damageAmount = 0;

                switch (enemyType)
                {
                    case ("Hatch"):
                        damageAmount = 1f;
                        break;
                }

                TakeDamage(collision, damageAmount);
            }
            else
            {
                Debug.Log("Ignoring Collision");
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
                
            }
        }
        
    }

    private void TakeDamage(Collision2D collision, float amount)
    {
        playerRigidbody.isKinematic = false;
        health -= amount;
        OnPlayerDamage?.Invoke();

        if (health <= 0)
        {
            health = 0;
            Debug.Log("UR DED");
            Destroy(gameObject);
            return;
        }

        // Calculate the direction of the collision
        collisionDirection = transform.position - collision.transform.position;
        collisionDirection.Normalize();

        // Apply velocity in the opposite direction of the collision
        playerMovement.canMove = false;
        playerRigidbody.velocity = new Vector2(collisionDirection.x * 12f, 7f);
        Debug.Log(playerRigidbody.velocity);
        
        // Prevent collisions with enemy GameObjects for a specific duration
        StartCoroutine(PauseInput(playerMovementDelay));
        // Start flickering the sprite
        StartCoroutine(FlickerSprite(invincibilityTime));
    }

    private IEnumerator PauseInput(float duration) // pause input for wall jump
    {
        // Wait for the specified duration
        yield return new WaitForSeconds(duration);

        playerMovement.canMove = true;
    }

    private IEnumerator FlickerSprite(float duration)
    {
        // Number of times to flicker the sprite
        int flickerCount = Mathf.FloorToInt(duration / 0.2f); // Flicker every 0.2 seconds

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer.color;

        playerMovement.isHurt = true;

        for (int i = 0; i < flickerCount; i++)
        {
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
            yield return new WaitForSeconds(0.1f);

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }

        spriteRenderer.color = originalColor;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);
        playerMovement.isHurt = false;
    }

}
