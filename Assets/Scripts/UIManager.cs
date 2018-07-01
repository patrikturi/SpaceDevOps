using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

	public GameObject DebugPanel;
	public GameObject HelpPanel;
	public GameObject QuitPanel;

	void Awake () {
		Cursor.visible = false;
		DebugPanel.SetActive (Debug.isDebugBuild);
	}

	void OnGUI() {
		Event e = Event.current;
		if (e.type == EventType.KeyDown) {
			if (e.keyCode == KeyCode.F1) {
				HelpPanel.SetActive (true);
			} else if (e.keyCode == KeyCode.Escape) {
				QuitPanel.SetActive (true);
			}
		} else if (e.type == EventType.KeyUp) {
			if (e.keyCode == KeyCode.F1) {
				HelpPanel.SetActive (false);
			}
		}
	}
}
