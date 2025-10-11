using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyManaBar : MonoBehaviour
{
	public Slider manaSlider;

	private ManaSystem enemyMana;

	private void Awake()
	{
		GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");

		if (enemy == null)
		{
			Debug.LogError("No enemy found in the scene. Make sure it has tag 'Enemy'");
			return;
		}

		enemyMana = enemy.GetComponent<ManaSystem>();

		if (enemyMana == null)
		{
			Debug.LogError("Enemy does not have a ManaSystem component!");
		}
	}

	void Start()
	{
		if (enemyMana != null)
		{
			manaSlider.value = CalculateSliderPercentage(enemyMana.CurrentMana, enemyMana.MaxMana);
		}
	}

	private void OnEnable()
	{
		if (enemyMana != null)
			enemyMana.manaChanged.AddListener(OnManaChanged);
	}

	private void OnDisable()
	{
		if (enemyMana != null)
			enemyMana.manaChanged.RemoveListener(OnManaChanged);
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
