using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public Transform m_Target;

	private Vector3 offset = new Vector3(0, 5.5f, -10f);
	private float m_SmoothDuration = 0.4f;

	private Vector3 m_Velocity = Vector3.zero;

	void FixedUpdate () {
		Vector3 desiredPos = m_Target.position + m_Target.transform.rotation * offset;
		Vector3 newPos = Vector3.SmoothDamp (transform.position, desiredPos, ref m_Velocity, m_SmoothDuration);

		transform.position = newPos;
		transform.LookAt (m_Target.position);
	}
}
