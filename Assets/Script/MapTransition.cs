using UnityEngine;
using UnityEngine.SceneManagement;

public class MapTransition : MonoBehaviour
{
	[SerializeField] private int sceneIndexToLoad; // index của scene

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player")) // kiểm tra va chạm với Player
		{
			SceneManager.LoadScene(sceneIndexToLoad);
		}
	}
}
