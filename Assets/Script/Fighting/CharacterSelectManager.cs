using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
	[Header("Danh sách Prefab Player")]
	public GameObject[] playerPrefabs;

	[Header("Danh sách Prefab Enemy (AI)")]
	public GameObject[] enemyPrefabs;

	[Header("Scene Index")]
	[Tooltip("Index của scene Battle trong Build Settings")]
	public int battleSceneIndex = 1;
	[Tooltip("Index của scene Main Menu trong Build Settings")]
	public int mainMenuSceneIndex = 0;

	[Header("Mũi tên hiển thị (Prefab)")]
	[Tooltip("Prefab hoặc object của mũi tên (tam giác ngược có SpriteRenderer + TextMesh)")]
	public GameObject arrowPrefab;

	private int currentSelectedCharacterIndex = -1;
	private int selectedPlayerIndex = -1;
	private int selectedEnemyIndex = -1;

	private GameObject currentArrow;        // mũi tên tạm thời (chỉ hiện khi click chọn)
	private GameObject playerArrow;         // mũi tên cố định cho Player
	private GameObject enemyArrow;          // mũi tên cố định cho Enemy

	// 🟢 Khi click vào ảnh hoặc object nhân vật
	public void SelectCharacter(int index)
	{
		currentSelectedCharacterIndex = index;
		Debug.Log("Đang chọn nhân vật số: " + index + " - " + playerPrefabs[index].name);

		// Tìm object trong scene (đặt tên giống prefab)
		GameObject target = GameObject.Find(playerPrefabs[index].name);
		if (target == null)
		{
			Debug.LogWarning("⚠️ Không tìm thấy object nhân vật trong scene!");
			return;
		}

		// Xóa mũi tên tạm cũ (nếu có)
		if (currentArrow != null)
			Destroy(currentArrow);

		// Tạo mũi tên tạm thời (màu xám, không chữ)
		if (arrowPrefab != null)
		{
			Vector3 spawnPos = target.transform.position + Vector3.up * 2f;
			currentArrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
			currentArrow.transform.SetParent(target.transform);
			SetArrow(currentArrow, Color.gray, ""); // mũi tên tạm màu xám
		}
		else
		{
			Debug.LogWarning("⚠️ Chưa gán Arrow Prefab vào CharacterSelectManager!");
		}
	}

	// 🟠 Khi ấn vào nút "Chọn làm Player"
	public void ChooseAsPlayer()
	{
		if (currentSelectedCharacterIndex == -1)
		{
			Debug.LogWarning("⚠️ Bạn chưa chọn nhân vật nào để gán làm Player!");
			return;
		}

		selectedPlayerIndex = currentSelectedCharacterIndex;
		Debug.Log("✅ Đã chọn " + playerPrefabs[selectedPlayerIndex].name + " làm Player!");

		GameObject target = GameObject.Find(playerPrefabs[selectedPlayerIndex].name);
		if (target == null) return;

		// Xóa mũi tên Player cũ (nếu có)
		if (playerArrow != null)
			Destroy(playerArrow);

		// Tạo mũi tên mới (xanh, chữ PLAYER)
		Vector3 spawnPos = target.transform.position + Vector3.up * 2f;
		playerArrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
		playerArrow.transform.SetParent(target.transform);
		SetArrow(playerArrow, Color.green, "PLAYER");

		// Xóa mũi tên tạm nếu trùng
		if (currentArrow != null)
			Destroy(currentArrow);
	}

	// 🔵 Khi ấn vào nút "Chọn làm Enemy"
	public void ChooseAsEnemy()
	{
		if (currentSelectedCharacterIndex == -1)
		{
			Debug.LogWarning("⚠️ Bạn chưa chọn nhân vật nào để gán làm Enemy!");
			return;
		}

		selectedEnemyIndex = currentSelectedCharacterIndex;
		Debug.Log("✅ Đã chọn " + enemyPrefabs[selectedEnemyIndex].name + " làm Enemy!");

		GameObject target = GameObject.Find(playerPrefabs[selectedEnemyIndex].name);
		if (target == null) return;

		// Xóa mũi tên Enemy cũ (nếu có)
		if (enemyArrow != null)
			Destroy(enemyArrow);

		// Tạo mũi tên mới (đỏ, chữ ENEMY)
		Vector3 spawnPos = target.transform.position + Vector3.up * 2f;
		enemyArrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
		enemyArrow.transform.SetParent(target.transform);
		SetArrow(enemyArrow, Color.red, "ENEMY");

		// Xóa mũi tên tạm nếu trùng
		if (currentArrow != null)
			Destroy(currentArrow);
	}

	// ⚙️ Hàm đổi màu + hiển thị chữ trên mũi tên
	private void SetArrow(GameObject arrow, Color color, string labelText)
	{
		if (arrow == null) return;

		SpriteRenderer sr = arrow.GetComponentInChildren<SpriteRenderer>();
		TextMesh textMesh = arrow.GetComponentInChildren<TextMesh>();

		if (sr != null)
			sr.color = color;
		if (textMesh != null)
			textMesh.text = labelText;
	}

	// 🔥 Khi nhấn nút Fight
	public void StartBattle()
	{
		if (selectedPlayerIndex < 0 || selectedEnemyIndex < 0)
		{
			Debug.LogWarning("⚠️ Chưa chọn đủ Player hoặc Enemy!");
			return;
		}

		PlayerPrefs.SetInt("SelectedPlayerIndex", selectedPlayerIndex);
		PlayerPrefs.SetInt("SelectedEnemyIndex", selectedEnemyIndex);
		PlayerPrefs.Save();

		SceneManager.LoadScene(battleSceneIndex);
	}

	// ⏹ Thoát ra menu chính
	public void QuitGame()
	{
		SceneManager.LoadScene(mainMenuSceneIndex);
	}
}
