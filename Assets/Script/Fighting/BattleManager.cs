using UnityEngine;

public class BattleManager : MonoBehaviour
{
	[Header("Spawn Points")]
	public Transform playerSpawn;
	public Transform enemySpawn;

	[Header("Prefab Player/Enemy")]
	public GameObject[] playerPrefabs;
	public GameObject[] enemyPrefabs;

	[Header("Match Timer")]
	public MatchTimer matchTimer; // gán từ Inspector

	void Start()
	{
		int playerIndex = PlayerPrefs.GetInt("SelectedPlayerIndex", 0);
		int enemyIndex = PlayerPrefs.GetInt("SelectedEnemyIndex", 0);

		GameObject player = Instantiate(playerPrefabs[playerIndex], playerSpawn.position, Quaternion.identity);
		GameObject enemy = Instantiate(enemyPrefabs[enemyIndex], enemySpawn.position, Quaternion.identity);

		Debug.Log($"🟢 Spawned Player: {player.name} | 🔴 Enemy: {enemy.name}");

		// 🔹 Gán Damageable cho MatchTimer
		matchTimer.player = player.GetComponent<Damageable>();
		matchTimer.enemy = enemy.GetComponent<Damageable>();
	}
}
