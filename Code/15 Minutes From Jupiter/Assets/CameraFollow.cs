using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;  // The target object to follow
    public float smoothSpeed = 0.125f;  // The smoothness of camera movement
    public Vector3 offset;  // The offset from the target's position

    private Vector3 desiredPosition;  // The desired position of the camera

    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null)
            return;

        // Calculate the desired position with the offset
        desiredPosition = target.position + offset;

        // Smoothly move the camera towards the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
