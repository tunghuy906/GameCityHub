using UnityEngine;
using UnityEditor;
using TMPro;

public class TMP_CanvasRendererCleaner : EditorWindow
{
	[MenuItem("Tools/Fix TMP CanvasRenderer Warning")]
	public static void CleanAllTMP()
	{
		int count = 0;

		// Lấy tất cả TextMeshPro trong project (kể cả trong prefab và scene)
		var allTMPs = Resources.FindObjectsOfTypeAll<TextMeshPro>();
		foreach (var tmp in allTMPs)
		{
			// Kiểm tra nếu có CanvasRenderer thì xoá
			var cr = tmp.GetComponent<CanvasRenderer>();
			if (cr != null)
			{
				Debug.Log($"[TMP Fix] Removed CanvasRenderer from {tmp.name}", tmp);
				DestroyImmediate(cr, true);
				EditorUtility.SetDirty(tmp.gameObject);
				count++;
			}
		}

		Debug.Log($"✅ TMP Fix complete! Removed {count} unnecessary CanvasRenderer components.");
	}
}
