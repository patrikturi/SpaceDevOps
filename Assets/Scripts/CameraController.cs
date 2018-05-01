using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public GameObject m_Target;

	private string CAMERA_BUTTON = "Camera";
	private Vector3 POS_OFFSET = new Vector3(0, 5.5f, -10f);
	private const float POS_SMOOTHING_DURATION = 0.4f;
	private float UP_SMOOTHING_STEP;

	private MeshRenderer m_TargetRenderer;
	private Vector3 m_CameraUp;
	private Vector3 m_Velocity = Vector3.zero;
	// false means it's Third person camera
	private bool m_FirstPersonCamera = false;

	void Start() {
		m_TargetRenderer = m_Target.GetComponent<MeshRenderer> ();
		UP_SMOOTHING_STEP = 1f * Time.fixedDeltaTime;
		m_CameraUp = m_Target.transform.up;
	}

	void FixedUpdate () {
		Transform targetTr = m_Target.transform;
		if (Input.GetButtonDown (CAMERA_BUTTON)) {
			m_FirstPersonCamera = !m_FirstPersonCamera;
		}

		if (m_FirstPersonCamera) {
			// TODO: will need a better solution for Networked Multiplayer, see:
			// https://answers.unity.com/questions/63261/network-restrictive-rendering.html
			m_TargetRenderer.enabled = false;
			transform.position = targetTr.position;
			transform.rotation = targetTr.rotation;
			m_CameraUp = targetTr.up; // Needed to transition smoothly into Third Person Camera
		} else { // Third Person Camera
			m_TargetRenderer.enabled = true;
			Vector3 desiredPos = targetTr.position + targetTr.rotation * POS_OFFSET;
			Vector3 newPos = Vector3.SmoothDamp (transform.position, desiredPos, ref m_Velocity, POS_SMOOTHING_DURATION,
				                Mathf.Infinity, Time.fixedDeltaTime);

			// Must keep the camera Up vector moving otherwise the camera can 'flip' around
			m_CameraUp = Vector3.Lerp (m_CameraUp, targetTr.up, UP_SMOOTHING_STEP);

			transform.position = newPos;
			transform.LookAt (targetTr.position, m_CameraUp);
		}
	}
}
