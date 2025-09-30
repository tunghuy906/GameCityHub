using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public float moveSpeed = 5f;

	private Rigidbody2D rb;
	private SpriteRenderer spriteRenderer;
	private Animator animator;

	private float moveInput;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
	}

	void Update()
	{
		
		moveInput = Input.GetAxisRaw("Horizontal");

		
		rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

		
		if (moveInput > 0)
		{
			spriteRenderer.flipX = false;
		}
		else if (moveInput < 0)
		{
			spriteRenderer.flipX = true;
		}

		
		bool isMoving = moveInput != 0;
		animator.SetBool("isMoving", isMoving);
	}
}
