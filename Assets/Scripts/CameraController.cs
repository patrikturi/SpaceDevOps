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

	private MeshRenderer m_TargetRenderer;
	private PlayerController m_TargetController;
	private Vector3 m_CameraUp;
	// Shift Y axis to negative value if current view collides with a large object
	private int m_OffsetYAxisSign = 1;
	private Vector3 m_Velocity = Vector3.zero;
	// false means it's Third person camera
	private bool m_FirstPersonCamera = false;

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
			m_CameraUp = targetTr.up; // Needed to transition smoothly into Third Person Camera
		} else {
			updateThirdPersonCamera (targetTr);
		}
	}

	private void updateThirdPersonCamera(Transform targetTr) {
		m_TargetRenderer.enabled = true;

		RaycastHit hit;
		float posOffsetMag;

		posOffsetMag = POS_OFFSET_MAG_DEFAULT;
		m_OffsetYAxisSign = 1;

		// Bring camera closer if there is a raycast hit
		if (Physics.Raycast(targetTr.position, targetTr.rotation * POS_OFFSET_DIR, out hit, POS_OFFSET_MAG_DEFAULT)) {

			string tag = hit.transform.gameObject.tag;

			posOffsetMag = hit.distance;

			// The current view is blocked -> check if the Y axis of the camera offset can be shifted to negative to provide better sight
			// 1) The object hit must have a specific tag
			if (tag.Contains (LARGE_OBJECT_TAG)) {
				
				Vector3 proposedNewDir = new Vector3 (POS_OFFSET_DIR.x, -POS_OFFSET_DIR.y, POS_OFFSET_DIR.z);
				Vector3 proposedDirAbs = targetTr.rotation * proposedNewDir;
				// 2) The new view has to be free
				bool raycast2Result = Physics.Raycast (targetTr.position, proposedDirAbs, out hit, POS_OFFSET_MAG_DEFAULT);
				if (!raycast2Result || !hit.transform.gameObject.tag.Contains (LARGE_OBJECT_TAG)) {
					
					m_OffsetYAxisSign = -1;
					// Check max camera distance at the new view
					if (raycast2Result) {
						posOffsetMag = hit.distance;
					} else {
						posOffsetMag = POS_OFFSET_MAG_DEFAULT;
					}
				}
			}

		}

		Vector3 offsetDir = new Vector3 (POS_OFFSET_DIR.x, POS_OFFSET_DIR.y, POS_OFFSET_DIR.z);
		offsetDir.y *= m_OffsetYAxisSign;
		Vector3 desiredPos = targetTr.position + targetTr.rotation * (offsetDir * posOffsetMag);

		Vector3 newPos = Vector3.SmoothDamp (transform.position, desiredPos, ref m_Velocity, POS_SMOOTHING_DURATION,
			Mathf.Infinity, Time.fixedDeltaTime);

		// Must keep the camera Up vector moving otherwise the camera can 'flip' around
		m_CameraUp = Vector3.Lerp (m_CameraUp, targetTr.up, UP_SMOOTHING_STEP);

		transform.position = newPos;
		transform.LookAt (targetTr.position, m_CameraUp);
	}
}
