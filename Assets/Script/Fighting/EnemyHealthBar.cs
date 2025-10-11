using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
	public Slider healthSlider;
	private Damageable enemyDamageable;

	private void Awake()
	{
		// Tìm enemy bằng tag
		GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");

		if (enemy == null)
		{
			Debug.LogError("Không tìm thấy Enemy trong scene! Hãy chắc chắn enemy có tag 'Enemy'.");
			enabled = false;
			return;
		}

		enemyDamageable = enemy.GetComponent<Damageable>();
		if (enemyDamageable == null)
		{
			Debug.LogError("Enemy không có component Damageable!");
			enabled = false;
		}
	}

	private void Start()
	{
		UpdateHealthBar(enemyDamageable.Health, enemyDamageable.MaxHealth);
	}

	private void OnEnable()
	{
		enemyDamageable.healthChanged.AddListener(OnEnemyHealthChanged);
	}

	private void OnDisable()
	{
		enemyDamageable.healthChanged.RemoveListener(OnEnemyHealthChanged);
	}

	private void OnEnemyHealthChanged(int newHealth, int maxHealth)
	{
		UpdateHealthBar(newHealth, maxHealth);
	}

	private void UpdateHealthBar(float currentHealth, float maxHealth)
	{
		healthSlider.value = currentHealth / maxHealth;
	}
}
