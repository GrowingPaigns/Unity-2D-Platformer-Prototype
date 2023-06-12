using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCursor : MonoBehaviour
{
    private SpriteRenderer render;
    [SerializeField] private Sprite MouseCursorNormal;
    [SerializeField] private Sprite MouseCursorAim;
    [SerializeField] private Sprite MouseCursorClick;
    [SerializeField] private Animator animator;         // Used to play different animations based on movement

    private Camera mainCamera;
    private PlayerAttack player;
    private Plane cursorPlane; // The plane on which the cursor will be positioned

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        render = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAttack>();
        mainCamera = Camera.main;
        cursorPlane = new Plane(Vector3.forward, Vector3.zero); // Define the cursor plane (e.g., XY plane)

        EnableCursor(); // Call the method to set the initial sprite
    }

    // Update is called once per frame
    void Update()
    {
        if (player.isRaycastLocked)
        {
            Debug.Log("Lock Detected in Cursor Script");

            if (player.lockedEnemy != null)
            {
                DisableCursor();
                render.transform.position = player.lockedEnemy.transform.position;
                animator.SetBool("LockedOn", player.isRaycastLocked);
            }
        }
        else
        {
            Debug.Log("Lock Was Not Detected in Cursor Script");

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (cursorPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                render.transform.position = hitPoint;
            }

            if (Input.GetMouseButton(0))
            {
                Debug.Log("Test 1");
                animator.SetBool("Clicking", true);
            }
            else
            {
                Debug.Log("Test 2");
                animator.SetBool("Clicking", false);
            }

            animator.SetBool("LockedOn", player.isRaycastLocked);
        }
    }

    public void DisableCursor()
    {
        render.sprite = null;
    }

    public void EnableCursor()
    {
        render.sprite = MouseCursorAim;
    }
}