using UnityEngine;
using System.Collections;

public class CatAutoMeow : MonoBehaviour
{
	[Header("Âm thanh mèo")]
	public AudioClip[] meowClips;   // chứa 2 tiếng meo (hoặc nhiều hơn)
	private AudioSource audioSource;

	[Header("Thời gian meo ngẫu nhiên")]
	public float minDelay = 5f;     // thời gian nhỏ nhất giữa các lần meo
	public float maxDelay = 10f;    // thời gian lớn nhất giữa các lần meo

	void Start()
	{
		// Nếu object chưa có AudioSource thì tự tạo
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
		}

		// Cấu hình cơ bản
		audioSource.playOnAwake = false;
		audioSource.loop = false;
		audioSource.spatialBlend = 0f; // âm thanh 2D (toàn cảnh)

		// ✅ Gán về đúng Mixer Group (SFX)
		if (AudioManager_Main.instance != null && AudioManager_Main.instance.mainMixer != null)
		{
			var sfxGroup = AudioManager_Main.instance.mainMixer.FindMatchingGroups("SFX");
			if (sfxGroup.Length > 0)
				audioSource.outputAudioMixerGroup = sfxGroup[0];
		}

		// Bắt đầu meo tự động
		StartCoroutine(AutoMeowRoutine());
	}

	IEnumerator AutoMeowRoutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
			PlayRandomMeow();
		}
	}

	void PlayRandomMeow()
	{
		if (meowClips == null || meowClips.Length == 0)
			return;

		// Random clip trong danh sách
		int r = Random.Range(0, meowClips.Length);
		AudioClip clip = meowClips[r];

		// Không cho đè tiếng nếu đang kêu
		if (!audioSource.isPlaying)
		{
			// Random pitch để nghe tự nhiên hơn (0.9–1.1)
			audioSource.pitch = Random.Range(0.9f, 1.1f);
			audioSource.PlayOneShot(clip);
		}
	}
}
