using UnityEngine;
using UnityEngine.EventSystems;

public class Cat5Click : MonoBehaviour, IPointerClickHandler
{
	private CatMovement catMove;
	private bool isIdle = false;

	void Start()
	{
		catMove = GetComponent<CatMovement>();
		if (catMove == null) Debug.LogWarning("Thiếu CatMovement trên object này!");
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		isIdle = !isIdle;
		catMove?.SetCanMove(!isIdle);
		Debug.Log(gameObject.name + " isIdle = " + isIdle);
	}
}
