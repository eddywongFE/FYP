using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform playerTransform;
    public float speed;

    public float minX; // Left Boundary
    public float maxX; // Right Boundary
    public float minY; // Bottom Boundary
    public float maxY; // Top Boundary

    void Start() { // Init player character position
        transform.position = playerTransform.position;
    }

    void Update() { // Follow player's movement
        if (playerTransform != null) {
            float clampedX = Mathf.Clamp(playerTransform.position.x, minX, maxX);
            float clampedY = Mathf.Clamp(playerTransform.position.y, minY, maxY);

            transform.position = Vector2.Lerp(transform.position, new Vector2(clampedX, clampedY), speed);
        }
    }
}
