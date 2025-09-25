using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MapButton : MonoBehaviour, IPointerClickHandler
{
	public int sceneIndex;

	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log("Clicked: " + gameObject.name);
		SceneManager.LoadScene(sceneIndex);
	}
}
