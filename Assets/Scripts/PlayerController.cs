using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	private float m_MaxSpeed = 25;
	private float m_FwdAcc = 0.75f;
	private string m_FwdAxis = "Vertical";
	private string m_OrthoAxis = "Horizontal";
	private Rigidbody m_Body;
	private float m_FwdInput;
	private float m_OrthoInput;
	private float m_TargetSpeed = 0f;

	// Use this for initialization
	void Start () {
		
	}

	void Awake() {
		m_Body = GetComponent<Rigidbody> ();
	}

	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate()
	{
		m_FwdInput = Input.GetAxis (m_FwdAxis);
		m_OrthoInput = Input.GetAxis (m_OrthoAxis);

		if (Mathf.Abs (m_FwdInput) > 0.1f) {
			m_TargetSpeed = Mathf.Sign (m_FwdInput) * m_MaxSpeed;
		} else {
			m_TargetSpeed = 0f;
		}



		Vector3 fwd = m_Body.transform.forward;
		Vector3 vel = m_Body.velocity;
		float fwdSpeed = Vector3.Dot (fwd, vel);
		float fwdSpeedDiff = m_TargetSpeed - fwdSpeed;

		Vector3 fwdForce = fwdSpeedDiff * fwd * m_Body.mass * m_FwdAcc;
		m_Body.AddForce(fwdForce);
	}
}
