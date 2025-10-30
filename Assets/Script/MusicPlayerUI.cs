using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class MusicPlayerUI : MonoBehaviour
{
	[Header("Audio Setup")]
	[SerializeField] private AudioSource audioSource;
	[SerializeField] public AudioClip[] playlist;
	[SerializeField] private AudioMixer audioMixer;
	[SerializeField] private string volumeParameter = "MusicVolume";

	[Header("UI Elements")]
	[SerializeField] private Image playPauseIcon;
	[SerializeField] private Sprite playSprite;
	[SerializeField] private Sprite pauseSprite;
	[SerializeField] private TextMeshProUGUI songNameText;

	[Header("Volume Control")]
	[SerializeField] private Slider volumeSlider;
	private const string MUSIC_VOLUME_KEY = "Music_Volume";
	private float blockAutoNextTimer = 0f;
	[Header("Progress Bar")]
	[SerializeField] private Slider progressSlider;
	[SerializeField] private TextMeshProUGUI timeText;

	private const string MUSIC_INDEX_KEY = "Music_Index";
	private const string MUSIC_TIME_KEY = "Music_Time";
	private const string MUSIC_ISPLAYING_KEY = "Music_IsPlaying";

	private int currentIndex = 0;
	private bool isPlaying = false;
	private bool isDraggingProgress = false;
	private bool hasEnded = false; // ngăn auto-next gọi nhiều lần

	private void Start()
	{
		if (audioSource == null)
			audioSource = GetComponent<AudioSource>();

		audioSource.playOnAwake = false;

		LoadMusicState();
		LoadVolume();

		if (volumeSlider != null)
			volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

		if (progressSlider != null)
			progressSlider.onValueChanged.AddListener(OnProgressChanged);

		UpdatePlayPauseIcon();
		UpdateSongName();

		// 🧩 Thêm dòng này để chặn auto-next trong 1 giây đầu (fix lỗi bài đầu tiên phát 2 lần)
		blockAutoNextTimer = 1f;
	}


	private void Update()
	{
		// 🧩 Chặn auto-next trong vài giây đầu sau khi load game
		if (blockAutoNextTimer > 0f)
		{
			blockAutoNextTimer -= Time.deltaTime;
			UpdateProgressUI(); // vẫn cập nhật UI khi đếm ngược
			return; // ⛔ bỏ qua toàn bộ kiểm tra auto-next
		}

		UpdateProgressUI();

		// Auto-next chỉ 1 lần khi bài kết thúc
		if (audioSource != null && audioSource.clip != null && isPlaying)
		{
			// Chỉ khi đang phát thực sự và chưa hết bài mới được kiểm tra
			if (!hasEnded && audioSource.isPlaying && audioSource.time >= audioSource.clip.length - 0.08f)
			{
				hasEnded = true;
				NextSongAuto();
			}
			else if (audioSource.time < audioSource.clip.length - 0.08f)
			{
				hasEnded = false;
			}
		}
	}

	private void OnApplicationQuit()
	{
		SaveMusicState();
		SaveVolume();
	}

	public void PlaySong(int index)
	{
		if (playlist == null || playlist.Length == 0) return;

		currentIndex = Mathf.Clamp(index, 0, playlist.Length - 1);
		audioSource.clip = playlist[currentIndex];
		audioSource.time = 0f;
		audioSource.Play();
		isPlaying = true;
		hasEnded = false;

		if (AudioManager_Main.instance != null)
			AudioManager_Main.instance.PauseBackgroundMusic(true, 0.25f);

		UpdatePlayPauseIcon();
		UpdateSongName();
		SaveMusicState();
	}

	public void NextSong()
	{
		if (playlist == null || playlist.Length == 0) return;

		currentIndex = (currentIndex + 1) % playlist.Length;
		PlaySong(currentIndex); // PlaySong resets flags
	}

	public void PrevSong()
	{
		if (playlist == null || playlist.Length == 0) return;

		currentIndex--;
		if (currentIndex < 0) currentIndex = playlist.Length - 1;
		PlaySong(currentIndex);
	}

	public void TogglePlay()
	{
		if (audioSource == null) return;

		if (audioSource.isPlaying)
		{
			audioSource.Pause();
			isPlaying = false;

			if (AudioManager_Main.instance != null)
				AudioManager_Main.instance.ResumeBackgroundMusic(true, 0.25f);
		}
		else
		{
			// nếu chưa có clip, bật bài đầu
			if (audioSource.clip == null && playlist != null && playlist.Length > 0)
			{
				PlaySong(currentIndex);
				return;
			}

			audioSource.Play();
			isPlaying = true;

			if (AudioManager_Main.instance != null)
				AudioManager_Main.instance.PauseBackgroundMusic(true, 0.25f);
		}

		UpdatePlayPauseIcon();
		SaveMusicState();
	}

	// Lưu / Load trạng thái
	public void SaveMusicState()
	{
		if (audioSource == null || audioSource.clip == null) return;

		PlayerPrefs.SetInt(MUSIC_INDEX_KEY, currentIndex);
		PlayerPrefs.SetFloat(MUSIC_TIME_KEY, audioSource.time);
		PlayerPrefs.SetInt(MUSIC_ISPLAYING_KEY, isPlaying ? 1 : 0);
		PlayerPrefs.Save();
	}

	public void LoadMusicState(bool autoPlay = true)
	{
		if (playlist == null || playlist.Length == 0) return;
		if (!PlayerPrefs.HasKey(MUSIC_INDEX_KEY)) return;

		currentIndex = PlayerPrefs.GetInt(MUSIC_INDEX_KEY);
		float savedTime = PlayerPrefs.GetFloat(MUSIC_TIME_KEY);
		bool wasPlaying = PlayerPrefs.GetInt(MUSIC_ISPLAYING_KEY) == 1;

		currentIndex = Mathf.Clamp(currentIndex, 0, playlist.Length - 1);
		audioSource.clip = playlist[currentIndex];

		// Clamp savedTime an toàn
		if (audioSource.clip != null)
			audioSource.time = Mathf.Clamp(savedTime, 0f, Mathf.Max(0.01f, audioSource.clip.length - 0.05f));

		if (autoPlay || wasPlaying)
		{
			audioSource.Play();
			isPlaying = true;

			if (AudioManager_Main.instance != null)
				AudioManager_Main.instance.PauseBackgroundMusic(true, 0.25f);
		}
		else
		{
			isPlaying = false;
		}

		hasEnded = false;
		UpdateSongName();
		UpdatePlayPauseIcon();
	}

	// Volume
	public void OnVolumeChanged(float value)
	{
		if (audioMixer == null) return;
		float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
		audioMixer.SetFloat(volumeParameter, dB);

		PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
		PlayerPrefs.Save();
	}

	private void LoadVolume()
	{
		float savedValue = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);

		if (volumeSlider != null)
			volumeSlider.value = savedValue;

		if (audioMixer != null)
		{
			float dB = Mathf.Log10(Mathf.Max(savedValue, 0.0001f)) * 20;
			audioMixer.SetFloat(volumeParameter, dB);
		}
	}

	private void SaveVolume()
	{
		if (volumeSlider != null)
		{
			PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volumeSlider.value);
			PlayerPrefs.Save();
		}
	}

	// Progress UI + Tua
	private void UpdateProgressUI()
	{
		if (audioSource == null || audioSource.clip == null) return;

		if (progressSlider != null && !isDraggingProgress)
		{
			progressSlider.maxValue = audioSource.clip.length;
			// clamp thời gian hiện tại vào [0, clip.length]
			progressSlider.value = Mathf.Clamp(audioSource.time, 0f, audioSource.clip.length);
		}

		if (timeText != null)
			timeText.text = $"{FormatTime(Mathf.Clamp(audioSource.time, 0f, audioSource.clip.length))} / {FormatTime(audioSource.clip.length)}";
	}

	private void OnProgressChanged(float value)
	{
		if (!isDraggingProgress) return;

		if (timeText != null && audioSource.clip != null)
			timeText.text = $"{FormatTime(Mathf.Clamp(value, 0f, audioSource.clip.length))} / {FormatTime(audioSource.clip.length)}";
	}

	public void OnBeginDragProgress()
	{
		isDraggingProgress = true;
	}

	public void OnEndDragProgress()
	{
		if (audioSource != null && audioSource.clip != null && progressSlider != null)
		{
			// clamp chặt để không seek vượt quá độ dài clip (tránh invalid seek)
			float newTime = Mathf.Clamp(progressSlider.value, 0f, Mathf.Max(0.01f, audioSource.clip.length - 0.02f));
			audioSource.time = newTime;

			// Nếu user kéo đến gần cuối và muốn next liền thì để Update() bắt và next 1 lần
		}

		isDraggingProgress = false;
	}

	private string FormatTime(float time)
	{
		int minutes = Mathf.FloorToInt(time / 60f);
		int seconds = Mathf.FloorToInt(time % 60f);
		return $"{minutes:0}:{seconds:00}";
	}

	// Auto next implementation
	private void NextSongAuto()
	{
		if (playlist == null || playlist.Length == 0) return;

		currentIndex = (currentIndex + 1) % playlist.Length;

		audioSource.clip = playlist[currentIndex];
		audioSource.time = 0f;

		isPlaying = true;
		hasEnded = true; // 🔒 khóa để tránh auto-next double
		UpdatePlayPauseIcon();
		UpdateSongName();
		SaveMusicState();

		Debug.Log($"🔁 Tự động chuyển sang bài: {playlist[currentIndex].name}");

		// 🧩 Chờ 0.05s rồi mới play để đảm bảo clip đã gán xong
		Invoke(nameof(PlayNextDelayed), 0.05f);
		Invoke(nameof(ResetEndFlag), 0.3f); // reset lại cờ sau một chút
	}

	// 🎵 Hàm phụ - đảm bảo bài mới thực sự phát
	private void PlayNextDelayed()
	{
		if (audioSource != null && audioSource.clip != null)
			audioSource.Play();
	}
	private void ResetEndFlag()
	{
		hasEnded = false;
	}
	private void UpdatePlayPauseIcon()
	{
		if (playPauseIcon == null) return;
		playPauseIcon.sprite = (audioSource != null && audioSource.isPlaying) ? pauseSprite : playSprite;
	}

	private void UpdateSongName()
	{
		if (songNameText == null) return;

		if (playlist == null || playlist.Length == 0)
		{
			songNameText.text = "Không có bài hát";
			return;
		}

		if (audioSource.clip != null)
			songNameText.text = audioSource.clip.name;
		else
			songNameText.text = "Chưa phát bài nào";
	}

	// Helpers
	public AudioSource GetAudioSource() => audioSource;
	public AudioClip GetCurrentClip() => audioSource.clip;
}
