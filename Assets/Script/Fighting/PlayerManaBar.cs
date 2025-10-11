using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
	public Slider manaSlider;

	private ManaSystem playerMana;

	private void Awake()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");

		if (player == null)
		{
			Debug.LogError("No player found in the scene. Make sure it has tag 'Player'");
			return;
		}

		playerMana = player.GetComponent<ManaSystem>();

		if (playerMana == null)
		{
			Debug.LogError("Player does not have a ManaSystem component!");
		}
	}

	void Start()
	{
		if (playerMana != null)
		{
			manaSlider.value = CalculateSliderPercentage(playerMana.CurrentMana, playerMana.MaxMana);
		}
	}

	private void OnEnable()
	{
		if (playerMana != null)
			playerMana.manaChanged.AddListener(OnManaChanged);
	}

	private void OnDisable()
	{
		if (playerMana != null)
			playerMana.manaChanged.RemoveListener(OnManaChanged);
	}

	private float CalculateSliderPercentage(float currentMana, float maxMana)
	{
		return currentMana / maxMana;
	}

	private void OnManaChanged(int newMana, int maxMana)
	{
		manaSlider.value = CalculateSliderPercentage(newMana, maxMana);
	}
}
