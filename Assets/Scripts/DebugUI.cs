using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour {

	public GameObject m_NamePrefab;
	public GameObject m_ValuePrefab;

	private Dictionary<string, Text> values = new Dictionary<string, Text>();

	public void UpdateVar(string name, string value) {
		if (!values.ContainsKey (name)) {
			CreateNewText (name);
		}
		values [name].text = value;
	}

	private void CreateNewText(string name) {
		GameObject nameObject = (GameObject)Instantiate (m_NamePrefab);
		Text nameText = nameObject.GetComponent<Text> ();
		nameText.text = name;
		nameText.transform.SetParent (transform);
		nameText.transform.SetSiblingIndex (0); // Add to the end of the layout

		GameObject valueObject = (GameObject)Instantiate (m_NamePrefab);
		Text valueText = valueObject.GetComponent<Text> ();
		valueText.text = "";
		valueText.transform.SetParent (transform);
		valueText.transform.SetSiblingIndex (1);

		values.Add (name, valueText);
	}

	public Text getText(string name) {
		return values [name];
	}
}
