using System.Collections;
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

	// internal
	private readonly HashSet<string> allowedScenes = new HashSet<string>
	{
		"Menu",
		"PlatFormGame",
		"TopDownMap",
		"CatRoom"
	};

	private Coroutine fadeCoroutine;
	private int pauseRequests = 0; // nếu >0 nghĩa là có ít nhất 1 yêu cầu tắt nhạc
	private float defaultVolume = 1f; // lưu volume gốc của AudioSource (0..1)

	private void Awake()
	{
		string currentScene = SceneManager.GetActiveScene().name;

		if (!allowedScenes.Contains(currentScene))
		{
			Destroy(gameObject);
			return;
		}

		if (instance == null)
		{
			instance = this;
			//DontDestroyOnLoad(gameObject);
			SceneManager.sceneLoaded += OnSceneLoaded;

			if (musicSource != null)
				defaultVolume = musicSource.volume;
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

		StartCoroutine(PlaySceneMusicWithDelay(sceneName));
	}

	private System.Collections.IEnumerator PlaySceneMusicWithDelay(string sceneName)
	{
		yield return new WaitUntil(() => musicSource != null);
		yield return new WaitForSeconds(0.05f);

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
		musicSource.volume = defaultVolume;
		musicSource.Play();
	}

	public void StopMusic()
	{
		if (musicSource != null && musicSource.isPlaying)
			musicSource.Stop();
	}

	// ----- New API -----
	/// <summary>
	/// Yêu cầu tạm tắt nhạc nền. Có thể fade mượt.
	/// Nếu có nhiều yêu cầu, pauseRequests sẽ tăng và chỉ resume khi tất cả release.
	/// </summary>
	public void PauseBackgroundMusic(bool fade = true, float fadeTime = 0.3f)
	{
		if (musicSource == null) return;

		pauseRequests++;
		// Nếu đã có yêu cầu trước đó, music đã đang giảm/stop — chỉ tăng counter
		if (pauseRequests > 1) return;

		// fade volume xuống 0 (sau đó Pause)
		if (fade)
		{
			if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
			fadeCoroutine = StartCoroutine(FadeAndPause(fadeTime));
		}
		else
		{
			musicSource.Pause();
		}
	}

	/// <summary>
	/// Giải phóng 1 yêu cầu tắt nhạc. Khi tất cả yêu cầu đã release (pauseRequests == 0) thì resume nhạc.
	/// </summary>
	public void ResumeBackgroundMusic(bool fade = true, float fadeTime = 0.3f)
	{
		if (musicSource == null) return;

		pauseRequests = Mathf.Max(0, pauseRequests - 1);
		if (pauseRequests > 0) return; // vẫn còn 1 yêu cầu khác, không resume

		// resume phát và fade volume lên
		if (fade)
		{
			if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
			fadeCoroutine = StartCoroutine(ResumeAndFade(fadeTime));
		}
		else
		{
			musicSource.UnPause();
		}
	}

	private IEnumerator FadeAndPause(float time)
	{
		float startVol = musicSource.volume;
		float elapsed = 0f;
		while (elapsed < time)
		{
			elapsed += Time.deltaTime;
			musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / time);
			yield return null;
		}
		musicSource.volume = 0f;
		musicSource.Pause();
		fadeCoroutine = null;
	}

	private IEnumerator ResumeAndFade(float time)
	{
		// nếu trước đó audio đang pause (ví dụ Pause() gọi), unpause trước để fade in
		if (!musicSource.isPlaying)
			musicSource.UnPause();

		float elapsed = 0f;
		float start = musicSource.volume;
		float target = defaultVolume;
		// nếu start gần 0, set start = 0 để fade from zero
		if (start < 0.001f) start = 0f;

		while (elapsed < time)
		{
			elapsed += Time.deltaTime;
			musicSource.volume = Mathf.Lerp(start, target, elapsed / time);
			yield return null;
		}
		musicSource.volume = target;
		fadeCoroutine = null;
	}

	// Cleanup
	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
}
