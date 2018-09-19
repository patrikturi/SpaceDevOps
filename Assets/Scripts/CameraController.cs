using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public GameObject Target;

	private const string CAMERA_BUTTON = "Camera";
	// Camera will be flipped if view hits a large object
	private const string LARGE_OBJECT_TAG = "LARGE";
	private static Vector3 POS_OFFSET_DIR = new Vector3 (0, 0.25f, -0.88f);
	private const float POS_OFFSET_MAG_DEFAULT = 11.41f;
	private const float POS_SMOOTHING_DURATION = 0.4f;
	private static float UP_SMOOTHING_STEP;
	private const float LOOK_AHEAD_DISTANCE = 4f;
	private const float LOOK_AHEAD_MULTIPLIER = 5f;

	private MeshRenderer targetRenderer;
	private Rigidbody targetBody;
	private Vector3 cameraUp;
	// Shift Y axis to negative value if current view collides with a large object
	private int offsetYAxisSign = 1;
	private Vector3 velocity = Vector3.zero;
	// false means it's Third person camera
	private bool firstPersonCamera = false;

	void Start() {
		targetRenderer = Target.GetComponent<MeshRenderer> ();
		targetBody = Target.GetComponent<Rigidbody> ();
		UP_SMOOTHING_STEP = 2f * Time.fixedDeltaTime;
		cameraUp = Target.transform.up;
	}

	public void Reset() {
		Transform targetTr = Target.transform;

		cameraUp = targetTr.up;
		if (firstPersonCamera) {
			transform.position = targetTr.position;
			transform.rotation = targetTr.rotation;
		} else {
			Vector3 desiredPos = targetTr.position + targetTr.rotation * (POS_OFFSET_DIR * POS_OFFSET_MAG_DEFAULT);
			transform.position = desiredPos;
			transform.LookAt (targetTr.position, cameraUp);
		}
	}

	void FixedUpdate () {
		Transform targetTr = Target.transform;
		if (Input.GetButtonDown (CAMERA_BUTTON)) {
			firstPersonCamera = !firstPersonCamera;
		}

		if (firstPersonCamera) {
			// TODO: will need a better solution for Networked Multiplayer, see:
			// https://answers.unity.com/questions/63261/network-restrictive-rendering.html
			targetRenderer.enabled = false;
			transform.position = targetTr.position;
			transform.rotation = targetTr.rotation;
			cameraUp = targetTr.up; // Needed to transition smoothly into Third Person Camera
		} else {
			UpdateThirdPersonCamera (targetTr);
		}
	}

	private void UpdateThirdPersonCamera(Transform targetTr) {
		targetRenderer.enabled = true;

		RaycastHit hit;
		float posOffsetMag;

		posOffsetMag = POS_OFFSET_MAG_DEFAULT;
		offsetYAxisSign = 1;

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
					
					offsetYAxisSign = -1;
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
		offsetDir.y *= offsetYAxisSign;
		Vector3 desiredPos = targetTr.position + targetTr.rotation * (offsetDir * posOffsetMag);

		float xAngSpeed = Vector3.Dot (targetTr.right, targetBody.angularVelocity);
		float desiredPosOffset = LOOK_AHEAD_MULTIPLIER * xAngSpeed;

		desiredPos += targetTr.rotation * new Vector3 (0, desiredPosOffset, 0);

		Vector3 newPos = Vector3.SmoothDamp (transform.position, desiredPos, ref velocity, POS_SMOOTHING_DURATION,
			Mathf.Infinity, Time.fixedDeltaTime);

		// Must keep the camera Up vector moving otherwise the camera can 'flip' around
		cameraUp = Vector3.Lerp (cameraUp, targetTr.up, UP_SMOOTHING_STEP);

		transform.position = newPos;
		Vector3 lookAhead = targetTr.rotation * new Vector3 (0, 0, LOOK_AHEAD_DISTANCE);
		transform.LookAt (targetTr.position + lookAhead, cameraUp);
	}
}
