using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

	public GameObject m_DebugPanel;
	public GameObject m_HelpPanel;
	public GameObject m_QuitPanel;

	void Awake () {
		Cursor.visible = false;
		m_DebugPanel.SetActive (Debug.isDebugBuild);
	}

	void OnGUI() {
		Event e = Event.current;
		if (e.type == EventType.KeyDown) {
			if (e.keyCode == KeyCode.F1) {
				m_HelpPanel.SetActive (true);
			} else if (e.keyCode == KeyCode.Escape) {
				m_QuitPanel.SetActive (true);
			}
		} else if (e.type == EventType.KeyUp) {
			if (e.keyCode == KeyCode.F1) {
				m_HelpPanel.SetActive (false);
			}
		}
	}
}
