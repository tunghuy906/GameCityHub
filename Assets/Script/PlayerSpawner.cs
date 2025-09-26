using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PlayerSpawner : MonoBehaviour
{
	public GameObject[] platformPrefabs;
	public GameObject[] topdownPrefabs;

	void Start()
	{
		SpawnPlayerForScene();
	}

	void SpawnPlayerForScene()
	{
		int idx = PlayerPrefs.GetInt("SelectedCharacter", 0);
		string sceneName = SceneManager.GetActiveScene().name;

		GameObject prefab = sceneName.Contains("Platform") ? platformPrefabs[idx] : topdownPrefabs[idx];

		GameObject spawn = GameObject.FindWithTag("PlayerSpawn"); // hoặc GameObject.Find("PlayerSpawn")
		Vector3 spawnPos = (spawn != null) ? spawn.transform.position : Vector3.zero;

		GameObject player = Instantiate(prefab, spawnPos, Quaternion.identity);

		// Gán camera follow
		var vcam = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
		if (vcam != null) vcam.Follow = player.transform;
	}
}
