using UnityEngine;

public class CatMovement : MonoBehaviour
{
	public float moveSpeed = 2f; // tốc độ di chuyển
	public Transform player;     // tham chiếu đến nhân vật người chơi

	private Rigidbody2D rb;
	private SpriteRenderer spriteRenderer;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();

		if (player == null)
		{
			// Tìm đối tượng có tag "Player" (nếu chưa gán thủ công)
			GameObject p = GameObject.FindGameObjectWithTag("Player");
			if (p != null) player = p.transform;
		}
	}
	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Player"))
		{
			rb.velocity = Vector2.zero; // đứng yên khi chạm player
		}
	}

	void Update()
	{
		if (player == null) return;

		// hướng từ mèo -> player
		Vector2 direction = (player.position - transform.position).normalized;

		// di chuyển
		rb.velocity = direction * moveSpeed;

		// lật sprite nếu cần
		if (direction.x > 0)
			spriteRenderer.flipX = true; // nhìn sang phải
		else if (direction.x < 0)
			spriteRenderer.flipX = false;  // nhìn sang trái
	}
}
