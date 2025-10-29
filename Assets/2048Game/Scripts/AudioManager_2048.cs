using UnityEngine;
using UnityEngine.Audio;

public class AudioManager_2048 : MonoBehaviour
{
	public static AudioManager_2048 instance;

	[Header("Mixer & Sources")]
	public AudioMixer mixer2048;
	public AudioSource musicSource;
	public AudioSource sfxSource;

	[Header("Clips")]
	public AudioClip bgmClip;
	public AudioClip moveClip;

	void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		// Gán output Mixer group
		if (mixer2048 != null)
		{
			var musicGroup = mixer2048.FindMatchingGroups("Music")[0];
			var sfxGroup = mixer2048.FindMatchingGroups("SFX")[0];

			musicSource.outputAudioMixerGroup = musicGroup;
			sfxSource.outputAudioMixerGroup = sfxGroup;
		}
	}

	void Start()
	{
		//PlayBGM();
	}
	 
	public void PlayBGM()
	{
		if (bgmClip == null || musicSource == null) return;
		musicSource.clip = bgmClip;
		musicSource.loop = true;
		musicSource.Play();
	}

	public void PlayMoveSound()
	{
		if (moveClip != null && sfxSource != null)
			sfxSource.PlayOneShot(moveClip);
	}
	public void StopMusic()
	{
		if (musicSource != null && musicSource.isPlaying)
			musicSource.Stop();
	}

}
