using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public GameObject m_Target;

	private string CAMERA_BUTTON = "Camera";
	// Camera will be flipped if view hits a large object
	private string LARGE_OBJECT_TAG = "LARGE";
	private Vector3 POS_OFFSET_DIR = new Vector3 (0, 0.48f, -0.88f);
	private float POS_OFFSET_MAG_DEFAULT = 11.41f;
	private const float POS_SMOOTHING_DURATION = 0.4f;
	private float UP_SMOOTHING_STEP;
	private const float UP_VECTOR_FLIP_COOLDOWN = 2f; // seconds

	private MeshRenderer m_TargetRenderer;
	private PlayerController m_TargetController;
	private Vector3 m_CameraUp;
	private float m_UpVectorSign = 1f;
	private Vector3 m_Velocity = Vector3.zero;
	// false means it's Third person camera
	private bool m_FirstPersonCamera = false;
	private float m_UpVectorFlipCooldown = 0f;

	void Start() {
		m_TargetRenderer = m_Target.GetComponent<MeshRenderer> ();
		UP_SMOOTHING_STEP = 1f * Time.fixedDeltaTime;
		m_CameraUp = m_Target.transform.up;

		m_TargetController = m_Target.GetComponent<PlayerController> ();
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
			m_CameraUp = m_UpVectorSign * targetTr.up; // Needed to transition smoothly into Third Person Camera
		} else {
			updateThirdPersonCamera (targetTr);
		}
	}

	private void updateThirdPersonCamera(Transform targetTr) {
		m_TargetRenderer.enabled = true;

		RaycastHit hit;
		float posOffsetMag;

		// Bring camera closer if there is a raycast hit
		if (Physics.Raycast(targetTr.position, targetTr.rotation * POS_OFFSET_DIR, out hit, POS_OFFSET_MAG_DEFAULT)) {

			string tag = hit.transform.gameObject.tag;
			// Check if we have to flip the Up vector of the camera
			// 1) Cooldown of the flip is elapsed
			// 2) The object hit has a specific tag
			if ((m_UpVectorFlipCooldown < Time.time) && tag.Contains (LARGE_OBJECT_TAG)) {
				RaycastHit hit2;
				Vector3 proposedNewDir = new Vector3 (POS_OFFSET_DIR.x, -POS_OFFSET_DIR.y, POS_OFFSET_DIR.z);
				// 3) Flip Up vector only if the new view will be free
				if (!Physics.Raycast (targetTr.position, targetTr.rotation * proposedNewDir, out hit2, POS_OFFSET_MAG_DEFAULT)
					|| !hit2.transform.gameObject.tag.Contains (LARGE_OBJECT_TAG)) {
					POS_OFFSET_DIR = proposedNewDir;
					m_UpVectorSign *= -1f;
					m_UpVectorFlipCooldown = Time.time + UP_VECTOR_FLIP_COOLDOWN;
					m_TargetController.FlipUpVector ();
				}
			}

			posOffsetMag = hit.distance;
		} else {
			posOffsetMag = POS_OFFSET_MAG_DEFAULT;
		}

		Vector3 desiredPos = targetTr.position + targetTr.rotation * (POS_OFFSET_DIR * posOffsetMag);

		Vector3 newPos = Vector3.SmoothDamp (transform.position, desiredPos, ref m_Velocity, POS_SMOOTHING_DURATION,
			Mathf.Infinity, Time.fixedDeltaTime);

		// Must keep the camera Up vector moving otherwise the camera can 'flip' around
		m_CameraUp = Vector3.Lerp (m_CameraUp, m_UpVectorSign * targetTr.up, UP_SMOOTHING_STEP);

		transform.position = newPos;
		transform.LookAt (targetTr.position, m_CameraUp);
	}
}
