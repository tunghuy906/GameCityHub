using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class CatMovement : MonoBehaviour
{
	public float moveSpeed = 2f;
	private Transform player;

	private Rigidbody2D rb;
	private SpriteRenderer spriteRenderer;
	private Animator animator;

	private bool canMove = true;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();

		// đảm bảo không dùng root motion (nếu animation có translate)
		if (animator != null) animator.applyRootMotion = false;

		// mặc định cho phép di chuyển nhưng khoá xoay để không bị lật do physics
		rb.constraints = RigidbodyConstraints2D.FreezeRotation;
	}

	public void SetTarget(Transform p)
	{
		player = p;
	}

	// Gọi để bật/tắt di chuyển
	public void SetCanMove(bool value)
	{
		canMove = value;

		// cập nhật animation param (nếu dùng)
		if (animator != null)
			animator.SetBool("isIdle", !canMove);

		if (!canMove)
		{
			// dừng ngay lập tức, khoá vị trí
			rb.velocity = Vector2.zero;
			rb.angularVelocity = 0f;
			rb.constraints = RigidbodyConstraints2D.FreezeAll;
		}
		else
		{
			// cho phép di chuyển (nhưng vẫn khoá xoay)
			rb.constraints = RigidbodyConstraints2D.FreezeRotation;
		}
	}

	void FixedUpdate()
	{
		// physics logic phải chạy ở FixedUpdate
		if (!canMove || player == null)
		{
			// bảo đảm giữ yên trong physics loop
			if (rb.velocity != Vector2.zero)
			{
				rb.velocity = Vector2.zero;
				rb.angularVelocity = 0f;
			}
			return;
		}

		// hướng từ mèo -> player (dùng rb.position cho ổn định)
		Vector2 direction = ((Vector2)player.position - rb.position).normalized;

		// di chuyển bằng velocity (physics)
		rb.velocity = direction * moveSpeed;

		// lật sprite (có thể để ở FixedUpdate hoặc Update)
		if (direction.x > 0.01f) spriteRenderer.flipX = true;
		else if (direction.x < -0.01f) spriteRenderer.flipX = false;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Player"))
		{
			// nếu chạm player muốn dừng thì gọi SetCanMove(false)
			SetCanMove(false);
		}
	}
}
