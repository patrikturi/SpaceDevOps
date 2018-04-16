using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	private string m_ThrustAxis = "Thrust";
	private string m_FwdAxis = "Vertical";
	private string m_OrthoAxis = "Horizontal";

	private float m_MaxSpeed = 25;
	private float m_FwdAcc = 0.75f;

	private float m_MaxRotAngSpeed = 2.5f;
	private float m_RotAngAcc = 4.5f;
	private float m_RotAngDecc = 9f;

	private float m_MaxSteerAngSpeed = 1.75f;
	private float m_SteerAngAcc = 3.5f;
	private float m_SteerAngDecc = 9f;

	private float m_ThrustInput;
	private float m_FwdInput;
	private float m_OrthoInput;

	private Rigidbody m_Body;

	// Use this for initialization
	void Start () {
		
	}

	void Awake() {
		m_Body = GetComponent<Rigidbody> ();
		m_Body.inertiaTensorRotation = Quaternion.identity;
	}

	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate()
	{
		m_ThrustInput = Input.GetAxis (m_ThrustAxis);
		m_FwdInput = Input.GetAxis (m_FwdAxis);
		m_OrthoInput = Input.GetAxis (m_OrthoAxis);

		ApplyFwdForce ();
		ApplyRotation ();
		ApplySteering ();
		ApplySteeringForce ();
	}

	void ApplyFwdForce()
	{
		float targetSpeed;
		if (Mathf.Abs (m_ThrustInput) > 0.1f) {
			targetSpeed = Mathf.Sign (m_ThrustInput) * m_MaxSpeed;
		} else {
			targetSpeed = 0f;
		}

		Vector3 fwd = m_Body.transform.forward;
		Vector3 vel = m_Body.velocity;
		float fwdSpeed = Vector3.Dot (fwd, vel);
		float fwdSpeedDiff = targetSpeed - fwdSpeed;

		// F = m * a
		Vector3 fwdForce = m_Body.mass * fwdSpeedDiff * m_FwdAcc * fwd;
		m_Body.AddForce(fwdForce);
	}

	void ApplyRotation()
	{
		float targetAngSpeed;
		float angAcc;
		if (Mathf.Abs (m_OrthoInput) > 0.1f) {
			targetAngSpeed = -Mathf.Sign (m_OrthoInput) * m_MaxRotAngSpeed;
			angAcc = m_RotAngAcc;
		} else {
			targetAngSpeed = 0f;
			angAcc = m_RotAngDecc;
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
		if (Mathf.Abs (m_FwdInput) > 0.1f) {
			targetAngSpeed = Mathf.Sign (m_FwdInput) * m_MaxSteerAngSpeed;
			angAcc = m_SteerAngAcc;
		} else {
			targetAngSpeed = 0f;
			angAcc = m_SteerAngDecc;
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
