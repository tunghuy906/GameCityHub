using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapTransition : MonoBehaviour
{
	[SerializeField] private int sceneIndexToLoad; // index của scene
	[SerializeField] private float delay = 1f;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			StartCoroutine(LoadSceneAfterDelay());
		}
	}
	private IEnumerator LoadSceneAfterDelay()
	{
		yield return new WaitForSeconds(delay);
		SceneManager.LoadScene(sceneIndexToLoad);
	}
}
