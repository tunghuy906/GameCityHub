using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class AIControllerFT : MonoBehaviour
{
	[Header("Move Settings")]
	public float MoveSpeed = 5f;
	public float jumpImpulse = 10f;
	public float dashSpeed = 15f;
	public float airWalkSpeed = 3f;
	public float dashTime = 0.2f;
	public float dashCooldown = 1f;

	[Header("AI Settings")]
	public Transform target;              // Player
	public float attackRange = 1.5f;
	public float detectionRange = 10f;
	public float jumpChance = 0.1f;       // Xác suất nhảy
	public float dashDistance = 5f;       // Nếu xa hơn giá trị này thì dash

	private Vector2 moveInput;
	private TouchingDirections touchingDirections;
	private Rigidbody2D rb;
	private Animator animator;

	private bool _isMoving = false;
	private bool isDashing = false;
	private bool canDash = true;
	private bool _isFacingRight = true;

	private float lastAttackTime;
	public float attackCooldown = 1f;

	// --- Properties giữ nguyên từ PlayerControllerFT ---
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
						if (isDashing) return dashSpeed;
						else return MoveSpeed;
					}
					else
					{
						return airWalkSpeed;
					}
				}
				else return 0;
			}
			else return 0;
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
		get { return animator.GetBool(AnimationStrings.canMove); }
	}

	// --- Unity Methods ---
	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		touchingDirections = GetComponent<TouchingDirections>();
	}

	private void FixedUpdate()
	{
		if (!isDashing)
		{
			rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y);
		}
		animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
	}

	private void Update()
	{
		if (target == null) return;

		float distance = Vector2.Distance(transform.position, target.position);

		// Nếu ngoài detection range thì đứng yên
		if (distance > detectionRange)
		{
			moveInput = Vector2.zero;
			IsMoving = false;
			return;
		}

		// --- Di chuyển về phía player ---
		if (Mathf.Abs(target.position.x - transform.position.x) > attackRange)
		{
			float dir = target.position.x > transform.position.x ? 1 : -1;
			moveInput = new Vector2(dir, 0);
			IsMoving = true;
			SetFacingDirection(moveInput);

			// Cơ hội dash khi đủ xa
			if (distance > dashDistance && canDash)
				StartCoroutine(Dash());

			// Cơ hội nhảy random
			if (touchingDirections.IsGrounded && Random.value < jumpChance * Time.deltaTime)
				Jump();
		}
		else
		{
			moveInput = Vector2.zero;
			IsMoving = false;
			TryAttack();
		}
	}

	// --- AI Actions ---
	private void Jump()
	{
		if (touchingDirections.IsGrounded && CanMove)
		{
			animator.SetTrigger(AnimationStrings.jump);
			rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
		}
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

	private void TryAttack()
	{
		if (Time.time > lastAttackTime + attackCooldown)
		{
			animator.SetTrigger(AnimationStrings.attack);
			lastAttackTime = Time.time;
		}
	}

	private void SetFacingDirection(Vector2 moveInput)
	{
		if (moveInput.x > 0 && !IsFacingRight)
		{
			IsFacingRight = true;
		}
		else if (moveInput.x < 0 && IsFacingRight)
		{
			IsFacingRight = false;
		}
	}
	private void OnEnable()
	{
		PlayerControllerFT.OnPlayerJump += HandlePlayerJump;
	}

	private void OnDisable()
	{
		PlayerControllerFT.OnPlayerJump -= HandlePlayerJump;
	}

	private void HandlePlayerJump()
	{
		// Enemy chỉ nhảy khi đang đứng đất & trong phạm vi detection
		if (touchingDirections.IsGrounded && target != null)
		{
			float distance = Vector2.Distance(transform.position, target.position);
			if (distance <= detectionRange)
			{
				Jump();
			}
		}
	}
}
