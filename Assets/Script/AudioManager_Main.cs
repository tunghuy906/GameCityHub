using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager_Main : MonoBehaviour
{
	public static AudioManager_Main instance;

	[Header("Audio")]
	public AudioMixer mainMixer;
	public AudioSource musicSource;

	[Header("Music Clips")]
	public AudioClip menuMusic;
	public AudioClip platformMusic;
	public AudioClip topDownMusic;
	public AudioClip catRoomMusic;

	private readonly HashSet<string> allowedScenes = new HashSet<string>
	{
		"Menu",
		"PlatFormGame",
		"TopDownMap",
		"CatRoom"
	};

	private void Awake()
	{
		string currentScene = SceneManager.GetActiveScene().name;

		// Nếu scene hiện tại không nằm trong 4 scene chính => tự huỷ
		if (!allowedScenes.Contains(currentScene))
		{
			Destroy(gameObject);
			return;
		}

		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
			SceneManager.sceneLoaded += OnSceneLoaded;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		string sceneName = scene.name;

		if (!allowedScenes.Contains(sceneName))
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
			Destroy(gameObject);
			return;
		}

		// đảm bảo musicSource và mixer đã sẵn sàng trước khi Play
		StartCoroutine(PlaySceneMusicWithDelay(sceneName));
	}

	private System.Collections.IEnumerator PlaySceneMusicWithDelay(string sceneName)
	{
		// chờ AudioSource và mixer group sẵn sàng
		yield return new WaitUntil(() => musicSource != null && mainMixer != null && mainMixer.FindMatchingGroups("Music").Length > 0);
		yield return new WaitForSeconds(0.05f); // nhỏ, đủ để ổn định

		switch (sceneName)
		{
			case "Menu":
				PlayMusic(menuMusic);
				break;
			case "PlatFormGame":
				PlayMusic(platformMusic);
				break;
			case "TopDownMap":
				PlayMusic(topDownMusic);
				break;
			case "CatRoom":
				PlayMusic(catRoomMusic);
				break;
		}
	}

	public void PlayMusic(AudioClip clip)
	{
		if (clip == null || musicSource == null) return;
		if (musicSource.clip == clip && musicSource.isPlaying) return;

		musicSource.clip = clip;
		musicSource.loop = true;
		musicSource.Play();
	}

	public void StopMusic()
	{
		if (musicSource != null && musicSource.isPlaying)
			musicSource.Stop();
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
}
