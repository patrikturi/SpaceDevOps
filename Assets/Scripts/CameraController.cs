using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public Transform m_Target;

	private Vector3 POS_OFFSET = new Vector3(0, 5.5f, -10f);
	private float POS_SMOOTHING_DURATION = 0.4f;
	private float UP_SMOOTHING_STEP;

	private Vector3 m_CameraUp;
	private Vector3 m_Velocity = Vector3.zero;

	void Start() {
		UP_SMOOTHING_STEP = 1f * Time.fixedDeltaTime;
		m_CameraUp = m_Target.transform.up;
	}

	void FixedUpdate () {
		Vector3 desiredPos = m_Target.position + m_Target.transform.rotation * POS_OFFSET;
		Vector3 newPos = Vector3.SmoothDamp (transform.position, desiredPos, ref m_Velocity, POS_SMOOTHING_DURATION,
			Mathf.Infinity, Time.fixedDeltaTime);

		// Must keep the camera Up vector moving otherwise the camera can 'flip' around
		m_CameraUp = Vector3.Lerp (m_CameraUp, m_Target.transform.up, UP_SMOOTHING_STEP);

		transform.position = newPos;
		transform.LookAt (m_Target.position, m_CameraUp);
	}
}
