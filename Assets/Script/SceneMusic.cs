using UnityEngine;

public class SceneMusic : MonoBehaviour
{
	public AudioClip sceneMusic;

	void Start()
	{
		if (AudioManager_Main.instance != null)
		{
			AudioManager_Main.instance.PlayMusic(sceneMusic);
		}
	}
}
