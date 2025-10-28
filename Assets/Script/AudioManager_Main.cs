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

	// ✅ Danh sách scene được phép giữ nhạc
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

		// Nếu scene hiện tại không nằm trong danh sách => hủy ngay
		if (!allowedScenes.Contains(currentScene))
		{
			Destroy(gameObject);
			return;
		}

		// Kiểm tra instance
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);

			// 🔹 Đăng ký event khi load scene
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

		// Nếu scene KHÔNG thuộc allowedScenes => hủy AudioManager
		if (!allowedScenes.Contains(sceneName))
		{
			// ⚠️ Gỡ đăng ký event TRƯỚC khi destroy
			SceneManager.sceneLoaded -= OnSceneLoaded;

			Destroy(gameObject);
			return;
		}

		// 🔹 Kiểm tra xem object có bị hủy chưa
		if (this == null || musicSource == null)
			return;

		// 🔹 Phát nhạc tương ứng với scene
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
		// Gỡ event khi object bị phá hủy (phòng ngừa leak)
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
}
