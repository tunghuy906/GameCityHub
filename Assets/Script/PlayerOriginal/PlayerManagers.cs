using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PlayerManagers : MonoBehaviour
{
	public GameObject[] platformPrefabs;
	public GameObject[] topdownPrefabs;
	public CinemachineVirtualCamera VCam;

	private static GameObject playerInstance;
	private static int playerMode = 0;             // 0 = none, 1 = platform, 2 = topdown
	private static int currentCharacterIndex = -1;

	// ✅ Chỉ hiện Player ở map 1, 2, 3
	private int[] showInSceneIndexes = { 1, 2, 3 };
	private static bool isSwitchingScene = false;

	private void Awake()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.activeSceneChanged -= OnActiveSceneChanged;
	}

	// 🔹 Ẩn Player NGAY khi scene bắt đầu đổi (trước khi frame mới render)
	private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
	{
		isSwitchingScene = true;
		int index = newScene.buildIndex;

		if (!System.Array.Exists(showInSceneIndexes, s => s == index))
		{
			if (playerInstance != null)
			{
				playerInstance.SetActive(false);
				Debug.Log($"[PlayerManagers] 🔕 Ẩn Player NGAY khi đổi scene sang index: {index}");
			}
		}
	}

	// 🔹 Khi scene load xong hoàn toàn
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		int index = scene.buildIndex;

		if (!System.Array.Exists(showInSceneIndexes, s => s == index))
		{
			// ❌ Không phải map 1,2,3 => XÓA Player hoàn toàn
			if (playerInstance != null)
			{
				Destroy(playerInstance);
				playerInstance = null;
				Debug.Log($"[PlayerManagers] 🧹 Destroy Player trong scene index: {index}");
			}
		}
		else
		{
			// ✅ Là map 1,2,3 => Spawn lại Player nếu chưa có
			if (playerInstance == null)
			{
				Debug.Log($"[PlayerManagers] 🔁 Recreate Player tại scene index: {index}");
				SpawnOrMovePlayer();
			}
			else
			{
				playerInstance.SetActive(true);
				Debug.Log($"[PlayerManagers] ✅ Giữ Player active trong scene index: {index}");
			}
		}

		isSwitchingScene = false;
	}

	// ---------------------- SPAWN LOGIC ----------------------
	void Start()
	{
		SpawnOrMovePlayer();
	}

	void SpawnOrMovePlayer()
	{
		int characterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);
		string sceneName = SceneManager.GetActiveScene().name;

		// Xác định mode mong muốn theo scene
		int requiredMode = 0;
		if (sceneName.Contains("PlatFormGame")) requiredMode = 1;
		else if (sceneName.Contains("TopDownMap") || sceneName.Contains("CatRoom")) requiredMode = 2;
		else
		{
			Debug.LogWarning("[PlayerManagers] Scene không xác định để spawn player: " + sceneName);
			return;
		}

		// Tìm spawn point
		GameObject spawn = GameObject.FindWithTag("PlayerSpawn");
		Vector3 spawnPos = spawn != null ? spawn.transform.position : Vector3.zero;

		// Kiểm tra prefab hợp lệ
		if (characterIndex < 0) characterIndex = 0;
		if (requiredMode == 1 && (platformPrefabs == null || platformPrefabs.Length == 0))
		{
			Debug.LogError("[PlayerManagers] platformPrefabs rỗng!");
			return;
		}
		if (requiredMode == 2 && (topdownPrefabs == null || topdownPrefabs.Length == 0))
		{
			Debug.LogError("[PlayerManagers] topdownPrefabs rỗng!");
			return;
		}
		if (requiredMode == 1 && characterIndex >= platformPrefabs.Length) characterIndex = 0;
		if (requiredMode == 2 && characterIndex >= topdownPrefabs.Length) characterIndex = 0;

		// Nếu chưa có player -> tạo mới
		if (playerInstance == null)
		{
			InstantiateNewPlayer(requiredMode, characterIndex, spawnPos);
			Debug.Log("[PlayerManagers] 🆕 Instantiate new player. Mode=" + requiredMode + " Index=" + characterIndex);
		}
		else
		{
			// Nếu mode khác (platform -> topdown), thay mới
			if (playerMode != requiredMode || currentCharacterIndex != characterIndex)
			{
				Debug.Log("[PlayerManagers] ♻️ Replacing existing player. oldMode=" + playerMode + " newMode=" + requiredMode);
				Destroy(playerInstance);
				playerInstance = null;
				InstantiateNewPlayer(requiredMode, characterIndex, spawnPos);
			}
			else
			{
				// Cùng mode => chỉ di chuyển về spawn
				playerInstance.transform.position = spawnPos;
				ResetVelocity(playerInstance);
			}
		}

		// Gắn camera follow
		if (VCam != null && playerInstance != null)
		{
			VCam.Follow = playerInstance.transform;
		}

		// Gắn AI (nếu có)
		CatMovement[] cats = FindObjectsOfType<CatMovement>();
		foreach (var cat in cats)
		{
			cat.SetTarget(playerInstance.transform);
		}
	}

	void InstantiateNewPlayer(int mode, int characterIndex, Vector3 pos)
	{
		if (mode == 1)
		{
			playerInstance = Instantiate(platformPrefabs[characterIndex], pos, Quaternion.identity);
			playerMode = 1;
		}
		else
		{
			playerInstance = Instantiate(topdownPrefabs[characterIndex], pos, Quaternion.identity);
			playerMode = 2;
		}

		currentCharacterIndex = characterIndex;
		SceneManager.sceneLoaded += (scene, mode) =>
		{
			int index = scene.buildIndex;
			if (System.Array.Exists(showInSceneIndexes, s => s == index))
			{
				// Nếu scene được phép hiển thị player -> spawn
				if (playerInstance == null)
					SpawnOrMovePlayer();
			}
			else
			{
				// Nếu scene KHÔNG cho phép hiển thị player -> xoá player trước khi render
				if (playerInstance != null)
				{
					Destroy(playerInstance);
					playerInstance = null;
				}
			}
		};
		playerInstance.tag = "PlayerPF";
	}

	void ResetVelocity(GameObject go)
	{
		var rb = go.GetComponent<Rigidbody2D>();
		if (rb != null)
		{
			rb.velocity = Vector2.zero;
			rb.angularVelocity = 0f;
		}
	}
}
