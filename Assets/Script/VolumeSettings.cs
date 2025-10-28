using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
	[SerializeField] private AudioMixer myMixer;
	[SerializeField] private Slider musicSlider;
	[SerializeField] private Slider SFXSlider;

	private bool hasLoaded = false;

	private void Start()
	{
		if (!hasLoaded)
		{
			LoadVolume();
			hasLoaded = true;
		}

		// Khi thay đổi slider thì gọi SetVolume tương ứng
		musicSlider.onValueChanged.AddListener(delegate { SetMusicVolume(); });
		SFXSlider.onValueChanged.AddListener(delegate { SetSFXVolume(); });
	}

	public void SetMusicVolume()
	{
		float volume = musicSlider.value;
		if (volume <= 0) volume = 0.0001f; // tránh lỗi log10(0)
		myMixer.SetFloat("MainMusic", Mathf.Log10(volume) * 20);
		PlayerPrefs.SetFloat("MainMusicVolume", volume);
	}

	public void SetSFXVolume()
	{
		float volume = SFXSlider.value;
		if (volume <= 0) volume = 0.0001f;
		myMixer.SetFloat("MainSFX", Mathf.Log10(volume) * 20);
		PlayerPrefs.SetFloat("MainSFXVolume", volume);
	}

	private void LoadVolume()
	{
		// Lấy giá trị lưu, nếu chưa có thì dùng mặc định 0.75
		float musicVolume = PlayerPrefs.GetFloat("MainMusicVolume", 0.75f);
		float sfxVolume = PlayerPrefs.GetFloat("MainSFXVolume", 0.75f);

		musicSlider.value = musicVolume;
		SFXSlider.value = sfxVolume;

		SetMusicVolume();
		SetSFXVolume();
	}
}
