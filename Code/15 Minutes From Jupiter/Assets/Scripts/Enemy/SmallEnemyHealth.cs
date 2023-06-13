using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallEnemyHealth : MonoBehaviour
{
    private int knockbackCounter = 0;
    private Animator animator;
    private EnemyMovement movement;
    [SerializeField] private float knockbackDuration;

    private void Start()
    {
        movement = GetComponent<EnemyMovement>();
        animator = GetComponent<Animator>();

        
    }

    private void Update()
    {
        if (movement.isKnockbackPaused)
        {
            Debug.Log("Knockback is paused");
            IncrementKnockbackCounter();
            movement.isKnockbackPaused = false;
        }
    }

    public void IncrementKnockbackCounter()
    {
        knockbackCounter++;

        if (knockbackCounter >= 2)
        {

            animator.SetBool("Dead", true);
            StartCoroutine(DestroyAfterDelay(0.53f));
        }
        else if (knockbackCounter == 1)
        {
            animator.SetBool("Injured", true);
        }

    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
