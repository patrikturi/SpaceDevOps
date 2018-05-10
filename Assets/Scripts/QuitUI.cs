using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitUI : MonoBehaviour {

	void OnGUI() {
		Event e = Event.current;
		if (e.type == EventType.KeyDown) {
			if (e.keyCode == KeyCode.Escape) {
				gameObject.SetActive (false);
			} else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) {
				Application.Quit ();
			}
		}
	}
}
