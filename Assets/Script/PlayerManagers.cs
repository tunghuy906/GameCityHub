using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class PlayerManagers : MonoBehaviour
{
    public GameObject[] playerPrefabs;
	public CinemachineVirtualCamera VCam;
	int characterIndex;
	private void Awake()
	{
		characterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);
		Debug.Log("Loaded Character Index: " + characterIndex);
		GameObject player = Instantiate(playerPrefabs[characterIndex]);
		VCam.Follow = player.transform;
	}
}
