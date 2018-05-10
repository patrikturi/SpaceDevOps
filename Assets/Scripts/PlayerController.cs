using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public DebugUI m_DebugUI;

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
	private const float STEER_ANG_DECC_RATIO = 9f;
	private const float STEER_SCALE_DOWN_SPEED = 0.2f * MAX_SPEED;

	private float STEER_FORCE_DIFF_MULT;

	private const float GRAVITY_RADIUS = 50.0f;
	private float DRAG_COEFF;
	private float GRAVITY_ACC = 7.5f;

	private const string SPEED_UI_KEY = "Speed";

	// Related to gravity
	private const float SCAN_INTERVAL = 0.2f;
	private int SCAN_FRAME_SKIP;
	private int scan_cur_frame = 0;

	private float m_ThrustInput;
	private float m_BrakeInput;
	private float m_FwdInput;
	private float m_OrthoInput;

	private Rigidbody m_Body;

	private PController m_FwdMotor;
	private PController m_FwdDamping;

	void Awake() {
		m_Body = GetComponent<Rigidbody> ();
		m_Body.inertiaTensorRotation = Quaternion.identity;
		DRAG_COEFF = m_Body.mass * 0.5f;
		STEER_FORCE_DIFF_MULT = 20f * m_Body.mass;
		SCAN_FRAME_SKIP = (int)Mathf.Round(SCAN_INTERVAL / Time.fixedDeltaTime);

		m_FwdMotor = new PController (FWD_ACC_RATIO);
		m_FwdDamping = new PController (FWD_DAMPING_RATIO);
		m_FwdDamping.setMinOutput(MIN_FWD_DAMPING);
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

		scan_cur_frame++;
		if (scan_cur_frame >= SCAN_FRAME_SKIP) {
			UpdateGravityObjects();
			scan_cur_frame = 0;
		}
		ApplyGravity ();

		ApplyRotation ();
		ApplySteering ();
		ApplyYRotation ();
	}

	void ApplyFwdForce()
	{
		float targetSpeed = GetTargetSpeed ();
		bool engineOn = isEngineOn();
		Vector3 fwd = m_Body.transform.forward;
		float fwdSpeed = Vector3.Dot (fwd, m_Body.velocity);
		m_DebugUI.UpdateVar (SPEED_UI_KEY, fwdSpeed.ToString("0.0"));

		PController controller;
		if (engineOn) {
			controller = m_FwdMotor;

			if (targetSpeed < 0.1f) {
				m_FwdMotor.setMinOutput (MIN_FWD_DECC);
			} else {
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
		float shipFwdSpeed = Vector3.Dot(m_Body.transform.forward, m_Body.velocity);
		if (Mathf.Abs (m_OrthoInput) > 0.1f) {
			targetAngSpeed = -Mathf.Sign (m_OrthoInput) * MAX_ROT_ANG_SPEED;
			// Make rotation slower at a slow linear velocity
			float scaleDownRatio = Mathf.Min (shipFwdSpeed / ROT_SCALE_DOWN_SPEED, 1f);
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

		float shipFwdSpeed = Vector3.Dot(m_Body.transform.forward, m_Body.velocity);
		if (Mathf.Abs (m_FwdInput) > 0.1f) {
			targetAngSpeed = Mathf.Sign (m_FwdInput) * MAX_STEER_ANG_SPEED;
			// Make steering slower at a slow linear velocity
			float scaleDownRatio = Mathf.Min (shipFwdSpeed / STEER_SCALE_DOWN_SPEED, 1f);
			targetAngSpeed *= scaleDownRatio;
			angAccRatio = STEER_ANG_ACC_RATIO;
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

	private List<GameObject> gravityObjects = new List<GameObject> ();

	void UpdateGravityObjects() {
		Collider[] hitColliders = Physics.OverlapSphere(m_Body.transform.position, GRAVITY_RADIUS);

		gravityObjects.Clear ();
		// TODO: filter duplicates
		foreach (Collider collider in hitColliders) {
			GameObject obj = collider.gameObject;
			if (obj.tag.Contains ("GRAVITY")) {
				gravityObjects.Add (obj);
			}
		}
		m_DebugUI.UpdateVar ("Grav", gravityObjects.Count.ToString());
	}

	void ApplyGravity() {
		// TODO: filter duplicates eg. GameObject with two colliders
		foreach(GameObject obj in gravityObjects) {
			Vector3 toObj = obj.transform.position - m_Body.transform.position;

			float forceMag = m_Body.mass * GRAVITY_ACC;
			toObj.Normalize ();
			Vector3 force = forceMag * toObj;
			m_Body.AddForce (force);
		}
	}
}
