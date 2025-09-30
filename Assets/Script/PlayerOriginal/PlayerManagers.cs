using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PlayerManagers : MonoBehaviour
{
	public GameObject[] platformPrefabs;
	public GameObject[] topdownPrefabs;
	public CinemachineVirtualCamera VCam;

	private static GameObject playerInstance;      // instance player duy nhất
	private static int playerMode = 0;             // 0 = none, 1 = platform, 2 = topdown
	private static int currentCharacterIndex = -1; // index đã spawn

	// Dùng Start() để chắc chắn mọi GameObject trong scene đã được tạo
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

		// Tìm spawn point trong scene (tag "PlayerSpawn")
		GameObject spawn = GameObject.FindWithTag("PlayerSpawn");
		Vector3 spawnPos = spawn != null ? spawn.transform.position : Vector3.zero;

		// Bảo vệ index nằm trong mảng
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
			Debug.Log("[PlayerManagers] Instantiate new player. Mode=" + requiredMode + " Index=" + characterIndex);
		}
		else
		{
			// Nếu mode khác (ví dụ platform -> topdown), destroy và instantiate mới
			if (playerMode != requiredMode || currentCharacterIndex != characterIndex)
			{
				Debug.Log("[PlayerManagers] Replacing existing player. oldMode=" + playerMode + " newMode=" + requiredMode);
				Destroy(playerInstance);
				playerInstance = null;
				InstantiateNewPlayer(requiredMode, characterIndex, spawnPos);
			}
			else
			{
				// cùng mode -> chỉ di chuyển tới spawn
				playerInstance.transform.position = spawnPos;
				ResetVelocity(playerInstance);
			}
		}

		// Gắn camera follow player
		if (VCam != null && playerInstance != null)
		{
			VCam.Follow = playerInstance.transform;
		}
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
		else // mode == 2
		{
			playerInstance = Instantiate(topdownPrefabs[characterIndex], pos, Quaternion.identity);
			playerMode = 2;
		}

		currentCharacterIndex = characterIndex;
		DontDestroyOnLoad(playerInstance);
		// tuỳ chọn: đặt tag để dễ tìm
		playerInstance.tag = "Player";
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
