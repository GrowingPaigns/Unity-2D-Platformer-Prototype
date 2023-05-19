using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Rigidbody2D rb;
    private bool isDrawing;
    private Vector3 targetPosition;
    public float lineFillSpeed; // Adjust the fill speed as desired
    public float horizontalForceMultiplier;
    public float verticalForceMultiplier;
    public float launchPowerMultiplier;

    private PlayerMovement playerMovement;
    

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>(); // Add this line to assign the playerMovement 
        playerMovement.canMove = true;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.startWidth = 0.3f; // Adjust the line width as desired
        lineRenderer.endWidth = 0.1f; // Adjust the line width as desired
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
        lineRenderer.sortingLayerName = "Player"; // Assign the Player sorting layer

        SpriteRenderer playerRenderer = GetComponent<SpriteRenderer>();
        playerRenderer.sortingOrder = 1; // Assign a higher sorting order to the player sprite

        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isDrawing) // Check if the line is not already being drawn
            {
                StartDrawing();
            }
        }

        if (isDrawing && Input.GetMouseButton(0))
        {
            targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // Debug.Log(targetPosition);
            UpdateLinePositions();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDrawing)
            {
                StopDrawing();
                LaunchPlayer();
            }
        }

        if (isDrawing)
        {
            FillLine();
        }
    }

    void StartDrawing()
    {
        isDrawing = true;
        lineRenderer.enabled = true;
        lineRenderer.material.SetFloat("_FillAmount", 0f); // Reset the fill amount to 0
    }

    void StopDrawing()
    {
        isDrawing = false;
        lineRenderer.enabled = false;
    }

    void UpdateLinePositions()
    {
        Vector3 playerPosition = transform.position;
        lineRenderer.SetPosition(0, playerPosition);
        lineRenderer.SetPosition(1, Vector3.Lerp(playerPosition, targetPosition, lineRenderer.material.GetFloat("_FillAmount")));
    }

    void FillLine()
    {
        float fillAmount = Mathf.Clamp01(lineRenderer.material.GetFloat("_FillAmount") + (lineFillSpeed * Time.deltaTime));
        lineRenderer.material.SetFloat("_FillAmount", fillAmount); // Adjust the material property name as needed
    }

    public float launchDuration = 1.0f; // Adjust the duration as needed

    void LaunchPlayer()
    {
        playerMovement.canMove = false;

        // Get the end position of the line renderer
        Vector3 targetPosition = lineRenderer.GetPosition(1);

        // Calculate launch direction
        Vector2 launchDirection = (targetPosition - transform.position).normalized;
        Debug.Log("Launch Dir: " + launchDirection);

        if (launchDirection.x > 0 && playerMovement.facingRight)
        {
            playerMovement.Flip();
        }
        else if (launchDirection.x < 0 && !playerMovement.facingRight)
        {
            playerMovement.Flip();
        }

        // Apply launch force
        float launchPowerX = launchDirection.x * launchPowerMultiplier * horizontalForceMultiplier;
        float launchPowerY = launchDirection.y * launchPowerMultiplier * verticalForceMultiplier;
        // Apply horizontal and vertical multipliers separately
        float launchForceX = launchPowerX;
        float launchForceY = launchPowerY;
        Debug.Log("Launch Force X: " + launchForceX);
        Debug.Log("Launch Force Y: " + launchForceY);
        // Adjust the launch forces based on the target position
        
        Debug.Log("Launch Force X2: " + launchForceX);
        // Apply the forces to the player's Rigidbody2D
        Vector2 launchForce = new Vector2(launchForceX, launchForceY);
        rb.velocity = Vector2.zero; // Reset the current velocity
        rb.AddForce(launchForce, ForceMode2D.Impulse);
        StartCoroutine(EnableMovementAfterLaunch());
    }

    IEnumerator EnableMovementAfterLaunch()
    {
        yield return new WaitForSeconds(launchDuration); // Adjust the duration if needed
        playerMovement.canMove = true; // Enable movement after launch

    }
}