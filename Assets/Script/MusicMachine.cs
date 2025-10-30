using UnityEngine;
using UnityEngine.EventSystems;

public class MusicMachine : MonoBehaviour, IPointerClickHandler
{
	[SerializeField] private GameObject musicPanel; // Panel UI
	[SerializeField] private float maxInteractDistance = 3f;
	[SerializeField] private Transform playerTransform;

	private bool isActive = false;
	private bool isInitializing = false;

	private void Start()
	{
		if (musicPanel != null)
			musicPanel.SetActive(false);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (playerTransform != null)
		{
			float d = Vector2.Distance(playerTransform.position, transform.position);
			if (d > maxInteractDistance)
			{
				Debug.Log("Too far to interact. Distance: " + d);
				return;
			}
		}

		if (!isInitializing)
			ToggleMusicPanel();
	}

	private void ToggleMusicPanel()
	{
		if (musicPanel == null) return;

		isActive = !isActive;
		musicPanel.SetActive(isActive);

		MusicPlayerUI playerUI = musicPanel.GetComponent<MusicPlayerUI>();
		if (playerUI == null) return;

		if (isActive)
		{
			isInitializing = true;

			// 🔹 1. Load lại nhạc đang nghe dở và phát tiếp luôn
			playerUI.LoadMusicState(autoPlay: true);

			// 🔹 2. Nếu chưa có bài nào từng phát, bắt đầu bài đầu tiên
			if (playerUI.GetCurrentClip() == null)
				playerUI.PlaySong(0);

			// 🔹 3. Nếu nhạc đang phát → tắt nhạc nền
			var source = playerUI.GetAudioSource();
			if (source != null && source.isPlaying && AudioManager_Main.instance != null)
				AudioManager_Main.instance.PauseBackgroundMusic(true, 0.25f);

			// ✅ 4. Delay nhỏ để tránh double click toggle gây ngắt nhạc
			Invoke(nameof(FinishInitialization), 0.25f);
		}
		else
		{
			// 🔹 Khi panel tắt, lưu trạng thái nhạc và bật lại nhạc nền
			playerUI.SaveMusicState();

			var source = playerUI.GetAudioSource();
			if (source != null && source.isPlaying)
				source.Pause(); // chỉ pause, không stop để nhớ vị trí

			if (AudioManager_Main.instance != null)
				AudioManager_Main.instance.ResumeBackgroundMusic(true, 0.25f);
		}
	}

	private void FinishInitialization()
	{
		isInitializing = false;
	}
}
