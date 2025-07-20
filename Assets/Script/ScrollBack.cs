using System.Collections.Generic;
using UnityEngine;

public class ScrollBack : MonoBehaviour
{
	public GameObject backgroundPrefab; 
	public Transform player;           
	public float backgroundWidth = 10f; 
	private List<GameObject> backgrounds = new List<GameObject>();

	private float spawnX = 0f; 

	void Start()
	{
		
		for (int i = 0; i < 3; i++)
		{
			SpawnBackground();
		}
	}

	void Update()
	{
		
		if (player.position.x + backgroundWidth * 2 > spawnX)
		{
			SpawnBackground();
		}
	}

	void SpawnBackground()
	{
		GameObject bg = Instantiate(backgroundPrefab, new Vector3(spawnX, 0, 0), Quaternion.identity);
		backgrounds.Add(bg);
		spawnX += backgroundWidth;
	}
}
