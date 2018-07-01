using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {

	public static SceneManager Instance;

	public RectTransform HealthBar;
	public Transform BoundsFront;

	void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if(Instance != this){
			Destroy (this);
		}
	}
}
