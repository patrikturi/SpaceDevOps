using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

	public static UIManager Instance;

	public GameObject LobbyPanel;
	public GameObject DebugPanel;
	public GameObject HelpPanel;
	public GameObject QuitPanel;
	public GameObject IntroPanel;
	public GameObject PlayerHUD;
	public GameObject FPSText;
	public GameObject ScorePanel;

	void Awake () {
		if (Instance == null) {
			Instance = this;
		} else if(Instance != this) {
			Destroy (this);
		}
	}

	public void LoadInGameUI() {
		LobbyPanel.SetActive (false);
		DebugPanel.SetActive (Debug.isDebugBuild);
		IntroPanel.SetActive (true);
		PlayerHUD.SetActive (true);
		FPSText.SetActive (true);
		Cursor.visible = false;
	}

	public void LoadServerInGameUI() {
		LobbyPanel.SetActive (false);
	}

	void OnGUI() {
		Event e = Event.current;
		if (e.type == EventType.KeyDown) {
			if (e.keyCode == KeyCode.F1) {
				HelpPanel.SetActive (true);
			} else if (e.keyCode == KeyCode.Escape) {
				QuitPanel.SetActive (true);
			} else if (e.keyCode == KeyCode.Tab) {
				ScoreBoard board = ScorePanel.GetComponent<ScoreBoard> ();
				board.Show (GameManager.Instance.getPlayerDetails ());
				ScorePanel.SetActive (true);
			}
		} else if (e.type == EventType.KeyUp) {
			if (e.keyCode == KeyCode.F1) {
				HelpPanel.SetActive (false);
			} else if (e.keyCode == KeyCode.Tab) {
				ScorePanel.SetActive (false);
			}
		}
	}
}
