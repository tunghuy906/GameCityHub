using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]
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
	public float dashDistance = 5f;
	[SerializeField] private float blockChance = 0.3f;

	private Vector2 moveInput;
	private TouchingDirections touchingDirections;
	private Rigidbody2D rb;
	private Animator animator;
	private Damageable damageable;

	private bool _isMoving = false;
	private bool isDashing = false;
	private bool canDash = true;
	private bool _isFacingRight = true;

	private float lastAttackTime;
	private int attackStep = 0;
	float comboResetTime = 0.8f; 
								 

	// --- Properties giống Player ---
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
					else return airWalkSpeed;
				}
				else return 0;
			}
			else return 0;
		}
	}

	private bool _isBlocking = false;
	public bool IsBlocking
	{
		get
		{
			return _isBlocking;
		}
		private set
		{
			_isBlocking = value;
			animator.SetBool(AnimationStrings.isBlocking, value);
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
	public bool CanMove => animator.GetBool(AnimationStrings.canMove);

	public bool IsAlive => animator.GetBool(AnimationStrings.isAlive);

	// --- Unity Methods ---
	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		touchingDirections = GetComponent<TouchingDirections>();
		damageable = GetComponent<Damageable>();
	}

	private void FixedUpdate()
	{
		if (!isDashing && !damageable.LockVelocity)
		{
			rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y);
		}
		animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
	}

	private void Update()
	{
		if (!IsAlive || target == null) return;

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

			// Dash nếu đủ xa
			if (distance > dashDistance && canDash)
				StartCoroutine(Dash());
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
		if (Time.time > lastAttackTime + comboResetTime)
			attackStep = 0; // reset combo nếu quá lâu không đánh

		attackStep++;
		if (attackStep > 3) attackStep = 1; // loop lại 1 → 3

		animator.SetInteger("attackStep", attackStep);
		animator.SetTrigger("attack");

		lastAttackTime = Time.time;
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
	public void TryBlock()
	{
		if (Random.value < blockChance && !IsBlocking) // không đang block thì mới block
		{
			StartCoroutine(BlockRoutine());
		}
	}

	private IEnumerator BlockRoutine()
	{
		IsBlocking = true;   // bật block
		Debug.Log("AI START BLOCK");

		// Giữ block trong 0.5 - 1 giây
		yield return new WaitForSeconds(Random.Range(0.5f, 1f));

		IsBlocking = false;  // tắt block
		Debug.Log("AI END BLOCK");
	}
	// --- Nhận damage giống Player ---
	public void OnHit(int damage, Vector2 knockback)
	{
		rb.velocity = new Vector2(knockback.x, rb.velocity.y + knockback.y);
	}

	// --- Lắng nghe Player Jump để nhảy theo ---
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
