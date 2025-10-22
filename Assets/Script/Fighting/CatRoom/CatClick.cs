using UnityEngine;
using UnityEngine.EventSystems;

public class CatClick : MonoBehaviour, IPointerClickHandler
{
	private Animator animator;
	private bool isSmiling = false;

	void Start()
	{
		animator = GetComponent<Animator>();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		isSmiling = !isSmiling; // đảo trạng thái
		animator.SetBool("isSmile", isSmiling);

		Debug.Log("Mèo trạng thái: " + (isSmiling ? "Cười" : "Bình thường"));
	}
}
