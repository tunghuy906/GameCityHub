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

	[Header("Skill Settings")]
	[SerializeField] private float skill1Cooldown = 2f;
	[SerializeField] private float skill2Cooldown = 4f;
	[SerializeField] private float skill3Cooldown = 6f;

	private bool skill1OnCooldown = false;
	private bool skill2OnCooldown = false;
	private bool skill3OnCooldown = false;


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
	public float attackCooldown = 1.0f;

	// --- Properties ---
	public float CurrentMoveSpeed
	{
		get
		{
			if (CanMove && !IsBlocking)   // <<<<<< chặn move khi block
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
		get { return _isBlocking; }
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
			if (IsBlocking)
			{
				// khi block thì đứng yên, không di chuyển
				rb.velocity = new Vector2(0, rb.velocity.y);
			}
			else
			{
				rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y);
			}
		}
		animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
	}

	private void Update()
	{
		if (!IsAlive || target == null) return;

		if (IsBlocking)
		{
			// khi đang block thì không di chuyển/attack/dash
			moveInput = Vector2.zero;
			IsMoving = false;
			return;
		}

		float distance = Vector2.Distance(transform.position, target.position);

		// Nếu ngoài detection range thì đứng yên
		if (distance > detectionRange)
		{
			moveInput = Vector2.zero;
			IsMoving = false;
			return;
		}

		if (Mathf.Abs(target.position.x - transform.position.x) > attackRange)
		{
			float dir = target.position.x > transform.position.x ? 1 : -1;
			moveInput = new Vector2(dir, 0);
			IsMoving = true;
			SetFacingDirection(moveInput);

			// Dash random, không bám player nữa
			if (canDash && Random.value < 0.01f) // 1% cơ hội mỗi frame (tầm 1 lần vài giây)
				StartCoroutine(DashRandom());
		}
		else
		{
			moveInput = Vector2.zero;
			IsMoving = false;

			// Quyết định đánh thường hay skill
			if (Random.value < 0.7f)
				TryAttack();  // 70% đánh thường
			else
				TryUseSkill(); // 30% dùng skill
		}
	}

	// --- AI Actions ---
	private void Jump()
	{
		if (touchingDirections.IsGrounded && CanMove && !IsBlocking) // <<<< chặn khi block
		{
			animator.SetTrigger(AnimationStrings.jump);
			rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
		}
	}

	private IEnumerator DashRandom()
	{
		if (IsBlocking) yield break;

		canDash = false;
		isDashing = true;
		animator.SetBool(AnimationStrings.isDashing, true);

		float originalGravity = rb.gravityScale;
		rb.gravityScale = 0;

		// random hướng dash (trái hoặc phải)
		int dir = Random.value < 0.5f ? -1 : 1;
		rb.velocity = new Vector2(dir * dashSpeed, 0);
		IsFacingRight = dir > 0;

		yield return new WaitForSeconds(dashTime);

		rb.gravityScale = originalGravity;
		isDashing = false;
		animator.SetBool(AnimationStrings.isDashing, false);

		yield return new WaitForSeconds(dashCooldown);
		canDash = true;
	}

	private void TryAttack()
	{
		if (IsBlocking) return; // <<<< chặn attack khi block
		if (Time.time < lastAttackTime + attackCooldown)
			return;

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
		if (!IsBlocking && Random.value < blockChance)
		{
			StartCoroutine(BlockRoutine());
		}
	}

	private IEnumerator BlockRoutine()
	{
		IsBlocking = true;
		Debug.Log("AI START BLOCK");

		// Giữ block trong 0.5 - 1 giây
		yield return new WaitForSeconds(Random.Range(0.5f, 1f));

		IsBlocking = false;
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
	private IEnumerator Skill1CooldownRoutine()
	{
		skill1OnCooldown = true;
		yield return new WaitForSeconds(skill1Cooldown);
		skill1OnCooldown = false;
	}
	private IEnumerator Skill2CooldownRoutine()
	{
		skill2OnCooldown = true;
		yield return new WaitForSeconds(skill2Cooldown);
		skill2OnCooldown = false;
	}
	private IEnumerator Skill3CooldownRoutine()
	{
		skill3OnCooldown = true;
		yield return new WaitForSeconds(skill3Cooldown);
		skill3OnCooldown = false;
	}
	private void TryUseSkill()
	{
		if (IsBlocking || !IsAlive) return;

		// Random xác suất kích hoạt skill tổng thể
		float roll = Random.value;

		// 40% cơ hội dùng skill (chia đều 3 skill)
		if (roll < 0.4f)
		{
			int skillIndex = Random.Range(1, 4); // 1 → 3
			switch (skillIndex)
			{
				case 1:
					if (!skill1OnCooldown)
					{
						Debug.Log("AI dùng Skill 1 (tầm xa)");
						animator.SetTrigger(AnimationStrings.Skill1);
						StartCoroutine(Skill1CooldownRoutine());
					}
					break;

				case 2:
					if (!skill2OnCooldown)
					{
						Debug.Log("AI dùng Skill 2 (tầm gần)");
						animator.SetTrigger(AnimationStrings.Skill2);
						StartCoroutine(Skill2CooldownRoutine());
					}
					break;

				case 3:
					if (!skill3OnCooldown)
					{
						Debug.Log("AI dùng Skill 3 (tất sát)");
						animator.SetTrigger(AnimationStrings.Skill3);
						StartCoroutine(Skill3CooldownRoutine());
					}
					break;
			}
		}
		else
		{
			// 60% còn lại thì đánh thường
			TryAttack();
		}
	}
}
