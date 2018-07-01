using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour {

	// TODO: bullet pooling

	public GameObject BulletPrefab;

	private const float BULLET_SPEED = 30f;
	private const float BULLET_DURATION = 3.5f;
	private const float COOLDOWN_DURATION = 0.25f;
	private const int BULLET_LAYER = 8;

	private Transform bulletSpawnRight;
	private Transform bulletSpawnLeft;
	private float fireEnabledTimeStamp = -1f;

	void Awake() {
		bulletSpawnRight = transform.Find ("BulletSpawnRight").transform;
		bulletSpawnLeft = transform.Find ("BulletSpawnLeft").transform;
	}

	// Update is called once per frame
	void Update () {
		float fireInput = Input.GetAxis ("Fire");

		if (fireInput > 0.1f && fireEnabledTimeStamp <= Time.time) {
			FireAll();
			fireEnabledTimeStamp = Time.time + COOLDOWN_DURATION;
		}
	}

	private void FireAll() {
		FireSingle (bulletSpawnRight);
		FireSingle (bulletSpawnLeft);
	}

	private void FireSingle(Transform spawn) {
		var bullet = (GameObject)Instantiate (BulletPrefab, spawn.position, spawn.rotation);
		bullet.GetComponent<Rigidbody> ().velocity = transform.forward * BULLET_SPEED;
		bullet.layer = BULLET_LAYER;

		Destroy (bullet, BULLET_DURATION);
	}
}
