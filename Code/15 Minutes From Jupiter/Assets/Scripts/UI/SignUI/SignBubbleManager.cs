using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignBubbleManager : MonoBehaviour
{
    public Animator signAnimator; // Drag and drop your Animator component in the Inspector.

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Trigger the "Animation" boolean in the Animator.
            signAnimator.SetBool("OpenBubble", true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Trigger the "Animation" boolean in the Animator.
            signAnimator.SetBool("OpenBubble", false);
        }
    }

   
}
