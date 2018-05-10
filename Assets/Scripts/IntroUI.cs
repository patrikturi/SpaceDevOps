using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroUI : MonoBehaviour {

	private const float INTRO_HELP_TIME = 20f;

	void Start()
	{
		StartCoroutine(DelayedTask());
	}

	IEnumerator DelayedTask()
	{
		yield return new WaitForSeconds(INTRO_HELP_TIME);
		gameObject.SetActive (false);
	}

	void OnGUI() {
		Event e = Event.current;

		if (e.type == EventType.KeyDown && e.keyCode == KeyCode.F1) {
			gameObject.SetActive (false);
		}
	}
}
