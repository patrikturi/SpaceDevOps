using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public GameObject m_PlayerShip;
	public GameObject m_Camera;

	void OnGUI() {
		Event e = Event.current;

		if (Debug.isDebugBuild && e.type == EventType.KeyUp && e.control && e.keyCode == KeyCode.R) {
			m_PlayerShip.SetActive(true);
			// TODO: Create Player script -> reset()
			m_PlayerShip.GetComponent<Health> ().Reset ();
			Rigidbody body = m_PlayerShip.GetComponent<Rigidbody>();
			body.transform.position = Vector3.zero;
			body.transform.rotation = Quaternion.identity;
			body.velocity = Vector3.zero;

			m_Camera.GetComponent<CameraController> ().Reset ();
		}
	}
}
