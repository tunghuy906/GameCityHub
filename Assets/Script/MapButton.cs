using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections; // cần để dùng Coroutine

public class MapButton : MonoBehaviour, IPointerClickHandler
{
	public int sceneIndex;
	public float delay = 0.5f; // thời gian chờ (giây)

	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log("Clicked: " + gameObject.name);
		StartCoroutine(LoadSceneAfterDelay());
	}

	private IEnumerator LoadSceneAfterDelay()
	{
		yield return new WaitForSeconds(delay);
		SceneManager.LoadScene(sceneIndex);
	}
}
