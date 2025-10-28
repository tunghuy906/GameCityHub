using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemCleaner : MonoBehaviour
{
	void Awake()
	{
		var systems = FindObjectsOfType<EventSystem>();
		if (systems.Length > 1)
		{
			Debug.Log($"[EventSystemCleaner] Found {systems.Length} EventSystems, removing extras...");
			// Giữ lại cái đầu tiên
			for (int i = 1; i < systems.Length; i++)
				Destroy(systems[i].gameObject);
		}
	}
}
