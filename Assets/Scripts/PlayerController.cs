using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	private string THRUST_AXIS = "Thrust";
	private string FWD_AXIS = "Vertical";
	private string ORTHO_AXIS = "Horizontal";

	private const float MAX_SPEED = 25;
	private const float FWD_ACC = 0.75f;

	private const float MAX_ROT_ANG_SPEED = 2.5f;
	private const float ROT_ANG_ACC = 4.5f;
	private const float ROT_ANG_DECC = 9f;

	private const float MAX_STEER_ANG_SPEED = 1.75f;
	private const float STEER_ANG_ACC = 3.5f;
	private const float STEER_ANG_DECC = 9f;

	private const float STEERING_SCALE_DOWN_SPEED = 0.2f * MAX_SPEED;

	private float m_ThrustInput;
	private float m_FwdInput;
	private float m_OrthoInput;

	private Rigidbody m_Body;

	void Awake() {
		m_Body = GetComponent<Rigidbody> ();
		m_Body.inertiaTensorRotation = Quaternion.identity;
	}

	void FixedUpdate()
	{
		m_ThrustInput = Input.GetAxis (THRUST_AXIS);
		m_FwdInput = Input.GetAxis (FWD_AXIS);
		m_OrthoInput = Input.GetAxis (ORTHO_AXIS);

		ApplyFwdForce ();
		ApplyRotation ();
		ApplySteering ();
		ApplySteeringForce ();
	}

	void ApplyFwdForce()
	{
		float targetSpeed;
		if (Mathf.Abs (m_ThrustInput) > 0.1f) {
			targetSpeed = Mathf.Sign (m_ThrustInput) * MAX_SPEED;
		} else {
			targetSpeed = 0f;
		}

		Vector3 fwd = m_Body.transform.forward;
		Vector3 vel = m_Body.velocity;
		float fwdSpeed = Vector3.Dot (fwd, vel);
		float fwdSpeedDiff = targetSpeed - fwdSpeed;

		// F = m * a
		Vector3 fwdForce = m_Body.mass * fwdSpeedDiff * FWD_ACC * fwd;
		m_Body.AddForce(fwdForce);
	}

	void ApplyRotation()
	{
		float targetAngSpeed;
		float angAcc;
		if (Mathf.Abs (m_OrthoInput) > 0.1f) {
			targetAngSpeed = -Mathf.Sign (m_OrthoInput) * MAX_ROT_ANG_SPEED;
			angAcc = ROT_ANG_ACC;
		} else {
			targetAngSpeed = 0f;
			angAcc = ROT_ANG_DECC;
		}

		Vector3 fwd = m_Body.transform.forward;
		float fwdAngSpeed = Vector3.Dot (fwd, m_Body.angularVelocity);
		float fwdAngSpeedDiff = targetAngSpeed - fwdAngSpeed;

		// T = I alpha
		float torqueMag = m_Body.inertiaTensor.z * fwdAngSpeedDiff * angAcc;
		Vector3 relTorque = new Vector3(0, 0, torqueMag);
		m_Body.AddRelativeTorque (relTorque);
	}

	void ApplySteering()
	{
		float targetAngSpeed;
		float angAcc;

		float shipFwdSpeed = Vector3.Dot(m_Body.transform.forward, m_Body.velocity);
		if (Mathf.Abs (m_FwdInput) > 0.1f) {
			targetAngSpeed = Mathf.Sign (m_FwdInput) * MAX_STEER_ANG_SPEED;
			// Make steering slower at a slow linear velocity
			float scaleDownRatio = Mathf.Min (shipFwdSpeed / STEERING_SCALE_DOWN_SPEED, 1f);
			targetAngSpeed *= scaleDownRatio;
			angAcc = STEER_ANG_ACC;
		} else {
			targetAngSpeed = 0f;
			angAcc = STEER_ANG_DECC;
		}

		Vector3 right = m_Body.transform.right;
		float fwdAngSpeed = Vector3.Dot (right, m_Body.angularVelocity);
		float fwdAngSpeedDiff = targetAngSpeed - fwdAngSpeed;

		// T = I alpha
		float torqueMag = m_Body.inertiaTensor.x * fwdAngSpeedDiff * angAcc;
		
		Vector3 relTorque = new Vector3(torqueMag, 0, 0);
		m_Body.AddRelativeTorque (relTorque);
	}

	void ApplySteeringForce ()
	{
		// Keep Y velocity zero
		Vector3 up = m_Body.transform.up;
		float upSpeed = Vector3.Dot (up, m_Body.velocity);

		// F = m * a = m * dv/dt
		Vector3 force = m_Body.mass * -upSpeed/Time.fixedDeltaTime * up;
		m_Body.AddForce (force);
	}
}
