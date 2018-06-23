using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour {

	// TODO: bullet pooling

	public GameObject m_BulletPrefab;
	public Transform m_BulletSpawnRight;
	public Transform m_BulletSpawnLeft;

	private const float BULLET_SPEED = 30f;
	private const float BULLET_DURATION = 3.5f;
	private const float COOLDOWN_DURATION = 0.25f;
	private const int BULLET_LAYER = 8;

	private float fireEnabledTimeStamp = -1f;

	// Update is called once per frame
	void Update () {
		float fireInput = Input.GetAxis ("Fire");

		if (fireInput > 0.1f && fireEnabledTimeStamp <= Time.time) {
			fireAll();
			fireEnabledTimeStamp = Time.time + COOLDOWN_DURATION;
		}
	}

	private void fireAll() {
		fireSingle (m_BulletSpawnRight);
		fireSingle (m_BulletSpawnLeft);
	}

	private void fireSingle(Transform spawn) {
		var bullet = (GameObject)Instantiate (m_BulletPrefab, spawn.position, spawn.rotation);
		bullet.GetComponent<Rigidbody> ().velocity = transform.forward * BULLET_SPEED;
		bullet.layer = BULLET_LAYER;

		Destroy (bullet, BULLET_DURATION);
	}
}
