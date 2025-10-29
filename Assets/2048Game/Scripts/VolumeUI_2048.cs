using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeUI_2048 : MonoBehaviour
{
	[Header("Mixer")]
	public AudioMixer mixer2048;

	[Header("Sliders")]
	public Slider musicSlider;
	public Slider sfxSlider;

	[Header("UI Panel")]
	public GameObject volumePanel;

	private bool isPanelVisible = false;

	private void Start()
	{
		LoadVolume();

		musicSlider.onValueChanged.AddListener(delegate { SetMusicVolume(); });
		sfxSlider.onValueChanged.AddListener(delegate { SetSFXVolume(); });

		volumePanel.SetActive(false); // UI ẩn khi bắt đầu
	}

	public void ToggleVolumePanel()
	{
		isPanelVisible = !isPanelVisible;
		volumePanel.SetActive(isPanelVisible);
	}

	public void SetMusicVolume()
	{
		float v = Mathf.Clamp(musicSlider.value, 0.0001f, 1f);
		mixer2048.SetFloat("BGM2048Volume", Mathf.Log10(v) * 20f);
		PlayerPrefs.SetFloat("BGM2048Value", v);
	}

	public void SetSFXVolume()
	{
		float v = Mathf.Clamp(sfxSlider.value, 0.0001f, 1f);
		mixer2048.SetFloat("SFX2048Volume", Mathf.Log10(v) * 20f);
		PlayerPrefs.SetFloat("SFX2048Value", v);
	}

	void LoadVolume()
	{
		float musicVol = PlayerPrefs.GetFloat("BGM2048Value", 0.75f);
		float sfxVol = PlayerPrefs.GetFloat("SFX2048Value", 0.75f);

		musicSlider.value = musicVol;
		sfxSlider.value = sfxVol;

		SetMusicVolume();
		SetSFXVolume();
	}
}
