using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
	public Transform target;
	public float moveSpeed = 2f;
	public float nextWayPointDistance = 2f;
	public float repeatTimeUpdatePath = 0.5f;
	public SpriteRenderer characterSR;
	public Animator animator;
	public int minDamage;
	public int maxDamage;

	private Path path;
	private Seeker seeker;
	private Rigidbody2D rb;
	private Health PlayerHealth;
	private WeaponManager weaponManager;  // ✅ cache WeaponManager
	private Coroutine moveCoroutine;

	public float freezeDurationTime;
	private float freezeDuration;

	private void Start()
	{
		seeker = GetComponent<Seeker>();
		rb = GetComponent<Rigidbody2D>();
		freezeDuration = 0;
		target = FindObjectOfType<Player>()?.transform;
		weaponManager = FindObjectOfType<WeaponManager>(); // ✅ chỉ gọi 1 lần ở Start()

		InvokeRepeating(nameof(CalculatePath), 0f, repeatTimeUpdatePath);
	}

	void CalculatePath()
	{
		if (target == null || seeker == null) return;
		if (seeker.IsDone())
			seeker.StartPath(rb.position, target.position, OnPathCompleted);
	}

	void OnPathCompleted(Path p)
	{
		if (!p.error)
		{
			path = p;
			MoveToTarget();
		}
	}

	void MoveToTarget()
	{
		if (moveCoroutine != null) StopCoroutine(moveCoroutine);
		moveCoroutine = StartCoroutine(MoveToTargetCoroutine());
	}

	public void FreezeEnemy()
	{
		freezeDuration = freezeDurationTime;
	}

	IEnumerator MoveToTargetCoroutine()
	{
		if (path == null || path.vectorPath == null) yield break;

		int currentWP = 0;

		while (currentWP < path.vectorPath.Count)
		{
			while (freezeDuration > 0)
			{
				freezeDuration -= Time.deltaTime;
				yield return null;
			}

			Vector2 direction = ((Vector2)path.vectorPath[currentWP] - rb.position).normalized;
			Vector2 force = direction * moveSpeed * Time.deltaTime;
			transform.position += (Vector3)force;

			float distance = Vector2.Distance(rb.position, path.vectorPath[currentWP]);
			if (distance < nextWayPointDistance)
				currentWP++;

			// ✅ Fix scale (0 ở trục Z có thể gây mất hiển thị)
			if (force.x != 0)
				characterSR.transform.localScale = new Vector3(force.x < 0 ? -1 : 1, 1, 1);

			yield return null;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			PlayerHealth = collision.GetComponent<Health>();
			if (PlayerHealth != null)
				InvokeRepeating(nameof(DamagePlayer), 0, 1f);
		}

		if (collision.CompareTag("FireRange") && weaponManager != null)
		{
			weaponManager.AddEnemyToFireRange(transform);
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			CancelInvoke(nameof(DamagePlayer));
			PlayerHealth = null;
		}

		if (collision.CompareTag("FireRange") && weaponManager != null)
		{
			weaponManager.RemoveEnemyToFireRange(transform);
		}
	}

	void DamagePlayer()
	{
		if (PlayerHealth == null) return;

		int damage = Random.Range(minDamage, maxDamage);
		PlayerHealth.TakeDam(damage);

		var player = PlayerHealth.GetComponent<Player>();
		if (player != null)
			player.TakeDamageEffect(damage);
	}
}
