using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour {

	// TODO: bullet pooling

	public GameObject BulletPrefab;

	private const float BULLET_SPEED = 50f;
	private const float BULLET_DURATION = 3.5f;
	private const float COOLDOWN_DURATION = 0.25f;
	private const int BULLET_LAYER = 8;

	Rigidbody shipBody;
	private Transform bulletSpawnRight;
	private Transform bulletSpawnLeft;
	private Quaternion bulletRotationOffset;
	private float fireEnabledTimeStamp = -1f;

	void Awake() {
		shipBody = GetComponent<Rigidbody> ();
		bulletSpawnRight = transform.Find ("BulletSpawnRight").transform;
		bulletSpawnLeft = transform.Find ("BulletSpawnLeft").transform;

		bulletRotationOffset = Quaternion.AngleAxis (-90f, Vector3.left);
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
		var bulletObject = (GameObject)Instantiate (BulletPrefab, spawn.position, transform.rotation * spawn.localRotation * bulletRotationOffset);

		Rigidbody body = bulletObject.GetComponent<Rigidbody> ();
		body.velocity = shipBody.velocity + transform.rotation * spawn.localRotation * Vector3.forward * BULLET_SPEED;
		bulletObject.layer = BULLET_LAYER;
		var bulletScript = bulletObject.GetComponent<Bullet> ();
		bulletScript.Player = gameObject;

		Destroy (bulletObject, BULLET_DURATION);
	}
}
