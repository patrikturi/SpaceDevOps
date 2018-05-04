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

	private const float MAX_STEER_ANG_SPEED = 1.75f;
	private const float STEER_ANG_ACC_RATIO = 3.5f;
	private const float STEER_ANG_DECC_RATIO = 9f;

	private const float STEERING_SCALE_DOWN_SPEED = 0.2f * MAX_SPEED;

	private const string SPEED_UI_KEY = "Speed";

	private float m_ThrustInput;
	private float m_BrakeInput;
	private float m_FwdInput;
	private float m_OrthoInput;

	private Rigidbody m_Body;

	private PController m_FwdMotor = new PController(FWD_ACC_RATIO);
	private PController m_FwdDamping = new PController(FWD_DAMPING_RATIO).setMinOutput(MIN_FWD_DAMPING);

	void Awake() {
		m_Body = GetComponent<Rigidbody> ();
		m_Body.inertiaTensorRotation = Quaternion.identity;
	}

	void FixedUpdate()
	{
		m_ThrustInput = Input.GetAxis (THRUST_AXIS);
		m_BrakeInput = Input.GetAxis (BRAKE_AXIS);
		m_FwdInput = Input.GetAxis (FWD_AXIS);
		m_OrthoInput = Input.GetAxis (ORTHO_AXIS);

		ApplyFwdForce ();
		ApplyOrthoForce ();
		ApplyRotation ();
		ApplySteering ();
		ApplyYRotation ();
		ApplySteeringForce ();
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
		float fwdAcc = controller.setMaxOutput(fwdSpeedDiff/Time.fixedDeltaTime).getOutput(fwdSpeed, targetSpeed);
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
		Vector3 fwdForce = m_Body.mass * orthoAcc * right;
		m_Body.AddForce (fwdForce);
	}

	void ApplyRotation()
	{
		float targetAngSpeed;
		float angAccRatio;
		if (Mathf.Abs (m_OrthoInput) > 0.1f) {
			targetAngSpeed = -Mathf.Sign (m_OrthoInput) * MAX_ROT_ANG_SPEED;
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
			float scaleDownRatio = Mathf.Min (shipFwdSpeed / STEERING_SCALE_DOWN_SPEED, 1f);
			targetAngSpeed *= scaleDownRatio;
			angAccRatio = STEER_ANG_ACC_RATIO;
		} else {
			targetAngSpeed = 0f;
			angAccRatio = STEER_ANG_DECC_RATIO;
		}

		Vector3 right = m_Body.transform.right;
		float fwdAngSpeed = Vector3.Dot (right, m_Body.angularVelocity);

		float fwdAngSpeedDiff = targetAngSpeed - fwdAngSpeed;
		float angAcc = fwdAngSpeedDiff * angAccRatio;
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

		// F = m * a = m * dv/dt
		float acc = -upSpeed/Time.fixedDeltaTime;
		Vector3 force = m_Body.mass * acc * up;
		m_Body.AddForce (force);
	}
}

class PController
{
	private float gain;
	private float min;
	private float max;
	private bool hasMin = false;
	private bool hasMax = false;

	public PController(float gain) {
		this.gain = gain;
	}

	public PController setGain(float gain) {
		this.gain = gain;
		return this;
	}

	public PController setMinOutput(float value) {
		min = Mathf.Abs(value);
		hasMin = true;
		return this;
	}

	public PController clearMinOutput() {
		hasMin = false;
		return this;
	}

	public PController setMaxOutput(float value) {
		max = Mathf.Abs(value);
		hasMax = true;
		return this;
	}

	public PController clearMaxOutput() {
		hasMax = false;
		return this;
	}

	public float getOutput(float current, float target) {
		float outValue = (target - current) * gain;
		if (hasMin) {
			if (outValue < 0f) {
				outValue = Mathf.Min (outValue, -min);
			} else {
				outValue = Mathf.Max (outValue, min);
			}
		}
		if (hasMax) {
			outValue = Mathf.Clamp (outValue, -max, max);
		}
		return outValue;
	}
}
