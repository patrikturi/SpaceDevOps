using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour {

	public GameObject NamePrefab;
	public GameObject ValuePrefab;

	private Dictionary<string, Text> values = new Dictionary<string, Text>();

	public void UpdateVar(string name, string value) {
		if (!Debug.isDebugBuild) {
			return;
		}
		if (!values.ContainsKey (name)) {
			CreateNewText (name);
		}
		values [name].text = value;
	}

	private void CreateNewText(string name) {
		if (!Debug.isDebugBuild) {
			return;
		}
		GameObject nameObject = (GameObject)Instantiate (NamePrefab);
		Text nameText = nameObject.GetComponent<Text> ();
		nameText.text = name;
		nameText.transform.SetParent (transform);
		nameText.transform.SetSiblingIndex (0); // Add to the end of the layout

		GameObject valueObject = (GameObject)Instantiate (NamePrefab);
		Text valueText = valueObject.GetComponent<Text> ();
		valueText.text = "";
		valueText.transform.SetParent (transform);
		valueText.transform.SetSiblingIndex (1);

		values.Add (name, valueText);
	}

	public Text GetText(string name) {
		if (!Debug.isDebugBuild) {
			return null;
		}
		return values [name];
	}
}
