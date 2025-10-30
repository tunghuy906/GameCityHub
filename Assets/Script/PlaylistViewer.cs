using UnityEngine;
using TMPro;

public class PlaylistViewer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MusicPlayerUI musicPlayer; // liên kết MusicPlayerUI
    [SerializeField] private Transform contentParent;   // Content trong ScrollView
    [SerializeField] private TextMeshProUGUI songTextPrefab; // Prefab text bài hát
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color playingColor = Color.yellow;

    private TextMeshProUGUI[] songTexts;

    private void Start()
    {
        GenerateList();
    }

    private void Update()
    {
        UpdateHighlight();
    }

    // 🔹 Tạo danh sách bài hát từ MusicPlayerUI
    private void GenerateList()
    {
        if (musicPlayer == null || musicPlayer.playlist == null) return;

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        songTexts = new TextMeshProUGUI[musicPlayer.playlist.Length];

        for (int i = 0; i < musicPlayer.playlist.Length; i++)
        {
            TextMeshProUGUI txt = Instantiate(songTextPrefab, contentParent);
			if (musicPlayer.playlist[i] != null)
				txt.text = $"{i + 1}. {musicPlayer.playlist[i].name}";
			else
				txt.text = $"{i + 1}. Bài {i + 1}";

			txt.color = normalColor;
            songTexts[i] = txt;
        }
    }

    // 🔹 Cập nhật highlight bài đang phát
    private void UpdateHighlight()
    {
        if (songTexts == null || musicPlayer == null || musicPlayer.playlist == null) return;

        for (int i = 0; i < songTexts.Length; i++)
        {
            var clip = musicPlayer.playlist[i];
            if (clip != null && musicPlayer.GetCurrentClip() == clip)
                songTexts[i].color = playingColor;
            else
                songTexts[i].color = normalColor;
        }
    }
}
