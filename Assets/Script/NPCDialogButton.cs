using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro; // nếu bạn dùng TextMeshPro
using UnityEngine.UI; // để dùng Image

public class NPCDialogButton : MonoBehaviour, IPointerClickHandler
{
	[Header("Dialog Setup")]
	public GameObject dialogPanel;
	public TextMeshProUGUI nameText;
	public TextMeshProUGUI dialogText;
	public Image npcImage; // thêm ảnh NPC
	public Sprite npcSprite; // ảnh muốn hiển thị

	[Header("NPC Info")]
	public string npcName = "NPC";
	[TextArea(3, 10)]
	public string[] dialogLines;
	public float delay = 0.3f;

	private int index = 0;
	private bool isTalking = false;

	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log("Clicked NPC: " + gameObject.name);
		StartCoroutine(StartDialogAfterDelay());
	}

	private IEnumerator StartDialogAfterDelay()
	{
		yield return new WaitForSeconds(delay);
		if (!isTalking)
		{
			StartDialog();
		}
	}

	private void StartDialog()
	{
		dialogPanel.SetActive(true);
		nameText.text = npcName;
		npcImage.sprite = npcSprite; // hiển thị ảnh NPC
		index = 0;
		dialogText.text = dialogLines[index];
		isTalking = true;
	}

	private void Update()
	{
		if (isTalking && Input.GetKeyDown(KeyCode.Space))
		{
			index++;
			if (index < dialogLines.Length)
			{
				dialogText.text = dialogLines[index];
			}
			else
			{
				EndDialog();
			}
		}
	}

	private void EndDialog()
	{
		dialogPanel.SetActive(false);
		isTalking = false;
	}
}