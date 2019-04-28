using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Version : MonoBehaviour {

	void Start () {
		GetComponent<Text> ().text = "v" + Application.version;
	}
}
