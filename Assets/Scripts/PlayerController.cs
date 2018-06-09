using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public DebugUI m_DebugUI;
	public Transform m_BoundsFront;
	public GameObject m_PlatformPrefab;

	private string THRUST_AXIS = "Thrust";
	private string BRAKE_AXIS = "Brake";
	private string FWD_AXIS = "Vertical";
	private string ORTHO_AXIS = "Horizontal";

	private const float MAX_SPEED = 25;
	private const float FWD_ACC_RATIO = 0.75f;
	private const float MIN_FWD_DECC = 1.5f;
	private const float FWD_DAMPING_RATIO = 0.075f;
	private const float MIN_FWD_DAMPING = 0.75f;

	private const float MAX_ROT_ANG_SPEED = 2.5f;
	private const float ROT_ANG_ACC_RATIO = 4.5f;
	private const float ROT_ANG_DECC_RATIO = 9f;
	private const float ROT_SCALE_DOWN_SPEED = 0.2f * MAX_SPEED;

	private const float MAX_STEER_ANG_SPEED = 1.75f;
	private const float STEER_ANG_ACC_RATIO = 3.5f;
	private const float STEER_LANDED_ANG_ACC_RATIO = 14f;
	private const float STEER_ANG_DECC_RATIO = 9f;
	private const float STEER_SCALE_DOWN_SPEED = 0.2f * MAX_SPEED;

	private float STEER_FORCE_DIFF_MULT;

	private const float GRAVITY_RADIUS = 75.0f;
	private float DRAG_COEFF;
	private float GRAVITY_ACC = 7.5f;

	private const string SPEED_UI_KEY = "Speed";

	// Related to gravity
	private const float SCAN_INTERVAL = 0.2f;
	private int SCAN_FRAME_SKIP;
	private int m_Scan_cur_frame = 0;

	private float m_ThrustInput;
	private float m_BrakeInput;
	private float m_FwdInput;
	private float m_OrthoInput;

	private Rigidbody m_Body;

	private PController m_FwdMotor;
	private PController m_FwdDamping;

	private float BOUNDS_SIZE;
	private float BOUNDS_PUSH_RANGE = 10f;
	private float BOUNDS_PUSH_MAG = 5f;

	private const float PLATFORM_GRAVITY_MAX_HEIGHT = 25f;
	private const float PLATFORM_GRAVITY_MIN_HEIGHT = -10f;
	// Max height from the platform to the ship when the ship is still considered as landed
	private float PLATFORM_MAX_LANDING_HEIGHT;
	private BoxCollider m_PlatformCollider;
	private bool m_LandedOnPlatform = false;

	void Awake() {
		m_Body = GetComponent<Rigidbody> ();
		m_Body.inertiaTensorRotation = Quaternion.identity;
		DRAG_COEFF = m_Body.mass * 0.5f;
		STEER_FORCE_DIFF_MULT = 20f * m_Body.mass;
		SCAN_FRAME_SKIP = (int)Mathf.Round(SCAN_INTERVAL / Time.fixedDeltaTime);

		m_FwdMotor = new PController (FWD_ACC_RATIO);
		m_FwdDamping = new PController (FWD_DAMPING_RATIO);
		m_FwdDamping.setMinOutput(MIN_FWD_DAMPING);

		BOUNDS_SIZE = m_BoundsFront.localScale.x / 2f;

		m_PlatformCollider = m_PlatformPrefab.GetComponent<BoxCollider> ();

		float platformSize = m_PlatformCollider.size.y * m_PlatformPrefab.transform.localScale.y / 2f;
		CapsuleCollider shipCollider = GetComponent<CapsuleCollider> ();
		float shipSize = shipCollider.height * shipCollider.transform.localScale.y / 2f;
		float maxLandedOffset = 0.5f;
		PLATFORM_MAX_LANDING_HEIGHT = platformSize + shipSize + maxLandedOffset;
	}

	void FixedUpdate()
	{
		m_ThrustInput = Input.GetAxis (THRUST_AXIS);
		m_BrakeInput = Input.GetAxis (BRAKE_AXIS);
		m_FwdInput = Input.GetAxis (FWD_AXIS);
		m_OrthoInput = Input.GetAxis (ORTHO_AXIS);

		ApplyFwdForce ();
		ApplyOrthoForce ();
		ApplySteeringForce ();

		m_Scan_cur_frame++;
		if (m_Scan_cur_frame >= SCAN_FRAME_SKIP) {
			UpdateGravityObjects();
			m_Scan_cur_frame = 0;
		}
		gravCnt = sphereGravityObjects.Count;
		ApplyGravity ();
		m_DebugUI.UpdateVar ("Grav", gravCnt.ToString());
		ApplyBoundsForce ();

		ApplyRotation ();
		ApplySteering ();
		ApplyYRotation ();
	}

	void ApplyFwdForce()
	{
		float targetSpeed = GetTargetSpeed ();
		bool engineOn = isEngineOn ();
		Vector3 fwd = m_Body.transform.forward;
		float fwdSpeed = Vector3.Dot (fwd, m_Body.velocity);
		m_DebugUI.UpdateVar (SPEED_UI_KEY, fwdSpeed.ToString ("0.0"));

		PController controller;
		if (engineOn || m_LandedOnPlatform) {
			controller = m_FwdMotor;

			// Braking or (landed and no controls)
			if (targetSpeed < 0.1f) {
				m_FwdMotor.setMinOutput (MIN_FWD_DECC);
			} else { // Throttling
				m_FwdMotor.clearMinOutput ();
			}
		} else {
			controller = m_FwdDamping;
		}

		float fwdSpeedDiff = targetSpeed - fwdSpeed;
		controller.setMaxOutput (fwdSpeedDiff / Time.fixedDeltaTime);
		float fwdAcc = controller.getOutput(fwdSpeed, targetSpeed);
		// F = m * a
		Vector3 fwdForce = m_Body.mass * fwdAcc * fwd;
		m_Body.AddForce (fwdForce);
	}

	private float GetTargetSpeed() {
		if (m_BrakeInput > 0.1f) {
			return 0f;
		} else if (m_ThrustInput > 0.1f) {
			return MAX_SPEED;
		}
		// Engine turned off
		Debug.Assert(isEngineOn() == false);
		return 0f;
	}

	private bool isEngineOn() {
		return (m_ThrustInput > 0.1f) || (m_BrakeInput > 0.1f);
	}

	// Damping only
	void ApplyOrthoForce() {
		Vector3 right = m_Body.transform.right;
		float orthoSpeed = Vector3.Dot (right, m_Body.velocity);

		// a = dv/dt
		float orthoAcc = -orthoSpeed / Time.fixedDeltaTime;
		Vector3 orthoForce = m_Body.mass * orthoAcc * right;
		m_Body.AddForce (orthoForce);
	}

	void ApplyRotation()
	{
		float targetAngSpeed;
		float angAccRatio;
		float shipFwdSpeedAbs = Mathf.Abs(Vector3.Dot(m_Body.transform.forward, m_Body.velocity));
		if (Mathf.Abs (m_OrthoInput) > 0.1f) {
			targetAngSpeed = -Mathf.Sign (m_OrthoInput) * MAX_ROT_ANG_SPEED;
			// Make rotation slower at a slow linear velocity
			float scaleDownRatio = Mathf.Min (shipFwdSpeedAbs / ROT_SCALE_DOWN_SPEED, 1f);
			targetAngSpeed *= scaleDownRatio;
			angAccRatio = ROT_ANG_ACC_RATIO;
		} else {
			targetAngSpeed = 0f;
			angAccRatio = ROT_ANG_DECC_RATIO;
		}

		Vector3 fwd = m_Body.transform.forward;
		float fwdAngSpeed = Vector3.Dot (fwd, m_Body.angularVelocity);

		float fwdAngSpeedDiff = targetAngSpeed - fwdAngSpeed;
		float angAcc = fwdAngSpeedDiff * angAccRatio;
		float torqueMag = m_Body.inertiaTensor.z * angAcc;
		Vector3 relTorque = new Vector3(0, 0, torqueMag);
		m_Body.AddRelativeTorque (relTorque);
	}

	void ApplySteering()
	{
		float targetAngSpeed;
		float angAccRatio;

		float shipFwdSpeedAbs = Mathf.Abs(Vector3.Dot(m_Body.transform.forward, m_Body.velocity));
		if (Mathf.Abs (m_FwdInput) > 0.1f) {
			targetAngSpeed = Mathf.Sign (m_FwdInput) * MAX_STEER_ANG_SPEED;
			// Make steering slower at a slow linear velocity
			float scaleDownRatio = Mathf.Min (shipFwdSpeedAbs / STEER_SCALE_DOWN_SPEED, 1f);
			targetAngSpeed *= scaleDownRatio;

			// Increase steering strength when landed: otherwise the ship
			// is unable to take off from a platform
			angAccRatio = m_LandedOnPlatform ? STEER_LANDED_ANG_ACC_RATIO : STEER_ANG_ACC_RATIO;
		} else {
			targetAngSpeed = 0f;
			angAccRatio = STEER_ANG_DECC_RATIO;
		}

		Vector3 right = m_Body.transform.right;
		float xAngSpeed = Vector3.Dot (right, m_Body.angularVelocity);

		float xAngSpeedDiff = targetAngSpeed - xAngSpeed;
		float angAcc = xAngSpeedDiff * angAccRatio;
		float torqueMag = m_Body.inertiaTensor.x * angAcc;
		Vector3 relTorque = new Vector3(torqueMag, 0, 0);
		m_Body.AddRelativeTorque (relTorque);
	}

	// Damping only
	void ApplyYRotation() {
		Vector3 up = m_Body.transform.up;
		float upAngSpeed = Vector3.Dot (up, m_Body.angularVelocity);

		float angAcc = -upAngSpeed / Time.fixedDeltaTime;
		float torqueMag = m_Body.inertiaTensor.y * angAcc;
		Vector3 relTorque = new Vector3(0, torqueMag, 0);
		m_Body.AddRelativeTorque (relTorque);
	}

	void ApplySteeringForce ()
	{
		// Keep Y velocity zero
		Vector3 up = m_Body.transform.up;
		float upSpeed = Vector3.Dot (up, m_Body.velocity);

		m_DebugUI.UpdateVar ("UpSpeed", upSpeed.ToString ("0.0"));

		// F = m * dv/dt
		float maxForceMag = m_Body.mass * Mathf.Abs(upSpeed)/Time.fixedDeltaTime;

		Vector3 right = m_Body.transform.right;
		float xAngSpeed = Vector3.Dot (right, m_Body.angularVelocity);

		float dragForceMag = Mathf.Abs (upSpeed) * DRAG_COEFF;
		float diffForceMag = Mathf.Abs(STEER_FORCE_DIFF_MULT * xAngSpeed);

		float steeringForceMag = dragForceMag + diffForceMag;

		steeringForceMag = Mathf.Min (steeringForceMag, maxForceMag);
		Vector3 dragForce = -Mathf.Sign(upSpeed) * steeringForceMag * up;
		m_Body.AddForce (dragForce);
	}

	// Gravity towards the center of the sphere
	private List<GameObject> sphereGravityObjects = new List<GameObject> ();
	// Gravity in a single direction
	private List<GameObject> directionalGravityObjects = new List<GameObject> ();

	private int gravCnt = 0;

	void UpdateGravityObjects() {
		// TODO: delay by one frame per player number
		Collider[] hitColliders = Physics.OverlapSphere(m_Body.transform.position, GRAVITY_RADIUS);

		sphereGravityObjects.Clear ();
		// TODO: filter duplicates
		foreach (Collider collider in hitColliders) {
			GameObject obj = collider.gameObject;
			if (obj.tag.Contains ("SPHERE_GRAVITY")) {
				sphereGravityObjects.Add (obj);
			}
		}

		directionalGravityObjects.Clear ();
		foreach (Collider collider in hitColliders) {
			GameObject obj = collider.gameObject;
			if (obj.tag.Contains ("DIR_GRAVITY")) {
				directionalGravityObjects.Add (obj);
			}
		}
	}

	void ApplyGravity() {

		m_LandedOnPlatform = false;

		// TODO: filter duplicates eg. GameObject with two colliders
		foreach(GameObject obj in sphereGravityObjects) {
			Vector3 toObj = obj.transform.position - m_Body.transform.position;
			toObj.Normalize ();

			float forceMag = m_Body.mass * GRAVITY_ACC;
			m_Body.AddForce (forceMag * toObj);
		}

		// Warning: The platform is expected to have Y up rotation
		foreach (GameObject obj in directionalGravityObjects) {

			Vector3 platformGravityArea = new Vector3 (m_PlatformCollider.size.x, PLATFORM_GRAVITY_MAX_HEIGHT*2f, m_PlatformCollider.size.z);
			float rotation = obj.transform.rotation.eulerAngles.y;
			Vector3 rotatedArea = Quaternion.AngleAxis (rotation, Vector3.up) * platformGravityArea;
			platformGravityArea = new Vector3 (Mathf.Abs (rotatedArea.x), rotatedArea.y, Mathf.Abs (rotatedArea.z));

			Bounds platformBounds = new Bounds(obj.transform.position, platformGravityArea);

			if (!platformBounds.Contains (transform.position)) {
				return;
			}

			float height = transform.position.y - obj.transform.position.y;
			if (height < PLATFORM_GRAVITY_MIN_HEIGHT) {
				return;
			}

			float platformToShip = m_Body.position.y - obj.transform.position.y;

			if (platformToShip > 0f && platformToShip < PLATFORM_MAX_LANDING_HEIGHT) {
				m_LandedOnPlatform = true;
			}

			gravCnt++;

			Vector3 gravityDir = Vector3.down;

			float forceMag = m_Body.mass * GRAVITY_ACC;
			m_Body.AddForce (forceMag * gravityDir);
		}
	}

	// Toss away from the game bounds if very close
	// This will prevent getting stuck eg. in a corner
	void ApplyBoundsForce() {
		ApplyBoundsForceAxis (0, m_Body.transform.position.x);
		ApplyBoundsForceAxis (1, m_Body.transform.position.y);
		ApplyBoundsForceAxis (2, m_Body.transform.position.z);
	}

	private void ApplyBoundsForceAxis(int axisIndex, float position) {
		if (BOUNDS_SIZE - Mathf.Abs (position) < BOUNDS_PUSH_RANGE) {
			float sign = -Mathf.Sign (position);
			Vector3 dir;
			if (axisIndex == 0) {
				dir = new Vector3 (sign, 0, 0);
			}
			else if (axisIndex == 1) {
				dir = new Vector3 (0, sign, 0);
			}
			else if (axisIndex == 2) {
				dir = new Vector3 (0, 0, sign);
			} else {
				throw new Exception ("Unhandled state");
			}
			// F = m*a
			m_Body.AddForce (dir*BOUNDS_PUSH_MAG*m_Body.mass);
		}
	}
}
