using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerControllerFT : MonoBehaviour
{
    public float MoveSpeed = 5f;
	public float jumpImpulse = 10f;
	public float dashSpeed = 15f;
	public float airWalkSpeed = 3f;
	public float dashTime = 0.2f;
	public float dashCooldown = 0f;
	public static event System.Action OnPlayerJump;

	Vector2 moveInput;
	TouchingDirections touchingDirections;

    private bool _isMoving = false;
	private bool isDashing = false;
	private bool canDash = true;

	Rigidbody2D rb;
	Animator animator;

	public float CurrentMoveSpeed
	{
		get
		{
			if (CanMove)
			{
				if (IsMoving && !touchingDirections.IsOnWall)
				{
					if (touchingDirections.IsGrounded)
					{
						if (isDashing)
						{
							return dashSpeed;
						}
						else
						{
							return MoveSpeed;
						}
					}
					else
					{
						return airWalkSpeed;
					}
				}
				else
				{
					return 0;
				}
			}
			else
			{
				return 0;
			}
		}
	}

	public bool IsMoving
	{
		get { return _isMoving; }
		private set
		{
			_isMoving = value;
			animator.SetBool(AnimationStrings.isMoving, value);
		}
	}
	private bool _isFacingRight = true;
	public bool IsFacingRight
	{
		get { return _isFacingRight; }
		private set
		{
			if (_isFacingRight != value)
			{
				transform.localScale *= new Vector2(-1, 1);
			}
			_isFacingRight = value;
		}
	}
	public bool CanMove
	{
		get
		{
			return animator.GetBool(AnimationStrings.canMove);

		}
	}

	public bool IsAlive
	{
		get
		{
			return animator.GetBool(AnimationStrings.isAlive);
		}
	}

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		touchingDirections = GetComponent<TouchingDirections>();
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void FixedUpdate()
	{
		if (!isDashing)
		{
			rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y);
		}

		animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
	}

	public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

		if(IsAlive)
		{
        IsMoving = moveInput != Vector2.zero;

		SetFacingDirection(moveInput);
		}
		else
		{
			IsMoving = false;
		}

    }

	public void OnJump(InputAction.CallbackContext context)
	{
		if(context.started && touchingDirections.IsGrounded && CanMove)
		{
			animator.SetTrigger(AnimationStrings.jump);
			rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);

			OnPlayerJump?.Invoke();
		}
	}
	public void OnDash(InputAction.CallbackContext context)
	{
		if (context.performed && canDash)
		{
			StartCoroutine(Dash());
		}
	}

	public void OnAttack(InputAction.CallbackContext context)
	{
		if(context.started)
		{
			animator.SetTrigger(AnimationStrings.attack);
		}
	}

	public void OnHit(int damage, Vector2 knockback)
	{
		rb.velocity = new Vector2(knockback.x, knockback.y + knockback.y);
	}
	private IEnumerator Dash()
	{
		canDash = false;
		isDashing = true;
		animator.SetBool(AnimationStrings.isDashing, true);

		float originalGravity = rb.gravityScale;
		rb.gravityScale = 0;
		rb.velocity = new Vector2(IsFacingRight ? dashSpeed : -dashSpeed, 0);

		yield return new WaitForSeconds(dashTime);

		rb.gravityScale = originalGravity;
		isDashing = false;
		animator.SetBool(AnimationStrings.isDashing, false);

		yield return new WaitForSeconds(dashCooldown);
		canDash = true;
	}

	private void SetFacingDirection(Vector2 moveInput)
	{
		if(moveInput.x > 0 && !IsFacingRight)
		{
			IsFacingRight = true;
		}
		else if (moveInput.x < 0 && IsFacingRight)
		{
			IsFacingRight = false;
		}
	}
}
