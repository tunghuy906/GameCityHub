using UnityEngine;
using UnityEngine.EventSystems;

public class CatWUpClick : MonoBehaviour, IPointerClickHandler
{
	private Animator animator;
	private bool isWakeUp = false;

	void Start()
	{
		animator = GetComponent<Animator>();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		isWakeUp = !isWakeUp; // đảo trạng thái
		animator.SetBool("isWakeUp", isWakeUp);

		Debug.Log("Mèo trạng thái: " + (isWakeUp ? "Thức dậy" : "Ngủ"));
	}
}
