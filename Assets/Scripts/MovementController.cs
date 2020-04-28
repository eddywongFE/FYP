using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{   
    public float speed; // Speed of player character
    public Joystick joystick; // Joystick controller reference

    private Rigidbody2D rb; // Collision control for player character
    private Animator anim; // Animation selection for player

    private Vector2 moveAmount; // Move amount when using joystick

    private void Start() { // Set component reference
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() { // Update player position
        Vector2 moveInput = new Vector2(joystick.Horizontal, joystick.Vertical); // Get positions
        moveAmount = moveInput.normalized * speed;

        // Update player character animation
        if (moveInput != Vector2.zero) {
            anim.SetBool("isRunning", true);
        } else {
            anim.SetBool("isRunning", false);
        }
    }

    private void FixedUpdate(){
      rb.MovePosition(rb.position + moveAmount * Time.fixedDeltaTime);
    }
}
