using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MatchTimer : MonoBehaviour
{
	[Header("Timer Settings")]
	public float matchTime = 120f; // tổng thời gian trận
	private float timeRemaining;
	private bool matchEnded = false;

	[Header("Start Countdown Settings")]
	public TextMeshProUGUI startCountdownText; // Text hiển thị 3...2...1...Start
	public float startCountdownDuration = 3f; // thời gian đếm ngược

	[Header("References")]
	public TextMeshProUGUI timerText;
	public TextMeshProUGUI resultText;
	public Image resultPanel;
	public Damageable player;
	public Damageable enemy;

	private void Start()
	{
		timeRemaining = matchTime;
		UpdateTimerText();

		// Ẩn UI kết quả khi bắt đầu
		resultPanel.gameObject.SetActive(false);
		resultText.gameObject.SetActive(false);

		// Tạm dừng game để đếm ngược
		Time.timeScale = 0f;

		// Bắt đầu coroutine đếm ngược khởi động
		StartCoroutine(StartCountdown());
	}

	private IEnumerator StartCountdown()
	{
		float countdown = startCountdownDuration;

		while (countdown > 0)
		{
			startCountdownText.text = Mathf.Ceil(countdown).ToString();
			yield return new WaitForSecondsRealtime(1f); // dùng Realtime vì Time.timeScale = 0
			countdown--;
		}

		startCountdownText.text = "START!";
		yield return new WaitForSecondsRealtime(1f);

		// Ẩn text đếm ngược
		startCountdownText.gameObject.SetActive(false);

		// Cho game chạy bình thường
		Time.timeScale = 1f;
	}

	private void Update()
	{
		if (matchEnded) return;
		if (Time.timeScale == 0f) return; // khi đang countdown thì ko chạy timer

		// Đếm ngược thời gian
		timeRemaining -= Time.deltaTime;
		UpdateTimerText();

		if (timeRemaining <= 0f)
		{
			timeRemaining = 0f;
			EndMatch();
		}
	}

	private void UpdateTimerText()
	{
		int minutes = Mathf.FloorToInt(timeRemaining / 60);
		int seconds = Mathf.FloorToInt(timeRemaining % 60);
		timerText.text = $"{minutes:00}:{seconds:00}";
	}

	private void EndMatch()
	{
		matchEnded = true;

		int playerHealth = player.Health;
		int enemyHealth = enemy.Health;

		string result = "";
		if (playerHealth > enemyHealth)
			result = "VICTORY";
		else if (enemyHealth > playerHealth)
			result = "LOSE";
		else
			result = "DRAW";

		// Hiện kết quả
		resultPanel.gameObject.SetActive(true);
		resultText.gameObject.SetActive(true);
		resultText.text = result;

		Debug.Log("Match ended!");

		// Dừng game
		Time.timeScale = 0f;
	}
}
