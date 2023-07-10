using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float delay;
    [SerializeField] private GameObject[] hearts;
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
            if (hearts.Length > 0)
            {

                // Calculate the direction of the collision
                collisionDirection = transform.position - collision.transform.position;
                collisionDirection.Normalize();

                // Apply velocity in the opposite direction of the collision


                playerMovement.canMove = false;
                playerRigidbody.velocity = new Vector2(collisionDirection.x * 12f, 7f);
                Debug.Log(playerRigidbody.velocity);
                StartCoroutine(PauseInput(delay));
                // Get the last heart and destroy it
                GameObject lastHeart = hearts[hearts.Length - 1];
                Destroy(lastHeart);

                // Remove the last heart from the array
                Array.Resize(ref hearts, hearts.Length - 1);
            }
        }


    }
    private IEnumerator PauseInput(float duration) // pause input for wall jump
    {
        
        
        yield return new WaitForSeconds(duration);

        playerMovement.canMove = true;

    }
}
