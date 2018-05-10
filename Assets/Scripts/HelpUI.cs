using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpUI : MonoBehaviour {

	public GameObject m_HelpPanel;

	void OnGUI() {
		Event e = Event.current;
		if (e.keyCode == KeyCode.F1) {
			if (e.type == EventType.KeyDown) {
				m_HelpPanel.SetActive (true);
			} else if (e.type == EventType.KeyUp) {
				m_HelpPanel.SetActive (false);
			}
		}
	}
}
