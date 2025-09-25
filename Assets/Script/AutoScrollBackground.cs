using UnityEngine;

public class AutoScrollBackground : MonoBehaviour
{
	public float scrollSpeed = 2f; // tốc độ cuộn
	private float backgroundWidth;

	void Start()
	{
		// Lấy chiều rộng sprite từ SpriteRenderer
		backgroundWidth = GetComponent<SpriteRenderer>().bounds.size.x;
	}

	void Update()
	{
		// Di chuyển sang trái
		transform.Translate(Vector2.left * scrollSpeed * Time.deltaTime);

		// Nếu tấm này đi ra ngoài màn hình → reset về cuối
		if (transform.position.x <= -backgroundWidth)
		{
			Vector3 newPos = transform.position;
			newPos.x += backgroundWidth * 3; // vì bạn có 3 tấm ghép liền nhau
			transform.position = newPos;
		}
	}
}
