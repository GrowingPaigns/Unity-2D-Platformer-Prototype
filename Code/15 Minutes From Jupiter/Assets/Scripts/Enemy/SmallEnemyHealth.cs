using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SmallEnemyHealth : MonoBehaviour
{
    public int knockbackCounter = 0;
    private Animator animator;
    private PlayerAttack playerAttack;
    private Collider2D boxCollider;


    private void Start()
    {
        playerAttack = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAttack>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<Collider2D>();

        if (playerAttack == null)
        {
            Debug.Log("PlayerAttack component is missing!");
        }
    }

    private void Update()
    {
        playerAttack = GetComponent<PlayerAttack>();

    }

    public void IncrementKnockbackCounter()
    {
        knockbackCounter++;

        if (knockbackCounter >= 2)
        {
            boxCollider.enabled = false;
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
