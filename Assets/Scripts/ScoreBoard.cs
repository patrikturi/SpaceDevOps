using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ScoreBoard : MonoBehaviour {

	public GameObject ScoreEntry;

	private Dictionary<NetworkInstanceId, GameObject> entries = new Dictionary<NetworkInstanceId, GameObject>();

	public void Show(Dictionary<NetworkInstanceId, PlayerDetails> playerDetails) {
		foreach (KeyValuePair<NetworkInstanceId, PlayerDetails> entry in playerDetails) {
			PlayerDetails details = entry.Value;
			setScore (entry.Key, details.name, details.color1, details.kills, details.deaths);
		}
	}

	private void setScore(NetworkInstanceId netId, string name, Color color, int kills, int deaths) {
		GameObject entry = null;
		entries.TryGetValue (netId, out entry);
		if (entry == null) {
			entry = Instantiate (ScoreEntry);
			entry.transform.SetParent(transform);
			entries.Add (netId, entry);
		}
		GameObject nameObject = entry.transform.Find ("PlayerName").gameObject;
		GameObject killsObject = entry.transform.Find ("Kills").gameObject;
		GameObject deathsObject = entry.transform.Find ("Deaths").gameObject;

		Text nameText = nameObject.GetComponent<Text> ();
		nameText.text = name;
		nameText.color = color;
		killsObject.GetComponent<Text> ().text = kills.ToString();
		deathsObject.GetComponent<Text> ().text = deaths.ToString();
	}

}
