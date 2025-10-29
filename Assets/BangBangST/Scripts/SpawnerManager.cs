using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
	public float startTimeBtwSpawn;
	private float timeBtwSpawn;

	public GameObject[] enemies;
	public WeaponManager weaponManager;
	public List<Spawner> spawners;

	private Player player;
	int maxEnemy = 5;
	int roundCount = 0;

	private void Start()
	{
		player = FindObjectOfType<Player>();
	}

	public List<int> GetRandomIndices(int n, int k)
	{
		List<int> allIndices = new List<int>();
		for (int i = 0; i < n; i++)
		{
			allIndices.Add(i);
		}

		List<int> randomIndices = new List<int>();

		int remainingItems = n;
		for (int i = 0; i < Mathf.Min(k, n); i++)
		{
			int randomIndex = UnityEngine.Random.Range(0, remainingItems);
			randomIndices.Add(allIndices[randomIndex]);
			allIndices[randomIndex] = allIndices[remainingItems - 1];
			remainingItems--;
		}

		return randomIndices;
	}

	private void Update()
	{
		if (timeBtwSpawn <= 0)
		{
			if (spawners == null || spawners.Count == 0)
			{
				Debug.LogWarning("SpawnerManager: No spawners assigned!");
				return;
			}

			int randEnemyCount = UnityEngine.Random.Range(2, maxEnemy);

			if (weaponManager != null && weaponManager.Enemies.Count <= 5)
				randEnemyCount = UnityEngine.Random.Range(maxEnemy - 2, maxEnemy);

			// 🔒 Giới hạn số spawn hợp lệ
			int validSpawnCount = Mathf.Min(maxEnemy, spawners.Count);

			// Lấy random index trong phạm vi spawners.Count
			List<int> randomIndex = GetRandomIndices(validSpawnCount, randEnemyCount);

			foreach (int index in randomIndex)
			{
				if (index < 0 || index >= spawners.Count)
				{
					Debug.LogWarning($"Invalid spawner index: {index}");
					continue;
				}

				int randEnemy = UnityEngine.Random.Range(0, enemies.Length);
				if (enemies.Length > 0 && spawners[index] != null)
					spawners[index].spawnEnemy(enemies[randEnemy]);
			}

			timeBtwSpawn = startTimeBtwSpawn;

			roundCount++;
			if (roundCount > 10)
			{
				roundCount = 0;
				maxEnemy = Mathf.Max(spawners.Count, maxEnemy + 1);
			}
		}
		else
		{
			timeBtwSpawn -= Time.deltaTime;
		}
	}
}
