using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*


The line Debug.Log("PlayerAttack is not null"); is not being reached at all

*/

public class SmallEnemyHealth : MonoBehaviour
{
    public int knockbackCounter = 0;
    private Animator animator;
    private PlayerAttack playerAttack;

    private Plane cursorPlane; // The plane on which the cursor will be positioned

    private void Start()
    {
        cursorPlane = new Plane(Vector3.forward, Vector3.zero); // Define the cursor plane (e.g., XY plane)
        playerAttack = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAttack>();
        animator = GetComponent<Animator>();

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
