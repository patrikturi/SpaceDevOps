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
	private Quaternion bulletRotationRight;
	private Quaternion bulletRotationLeft;
	private float fireEnabledTimeStamp = -1f;

	void Awake() {
		bulletSpawnRight = transform.Find ("BulletSpawnRight").transform;
		bulletSpawnLeft = transform.Find ("BulletSpawnLeft").transform;
		float rotYRight = bulletSpawnRight.transform.localRotation.eulerAngles.y;
		float rotYLeft = bulletSpawnLeft.transform.localRotation.eulerAngles.y;
		bulletRotationRight = Quaternion.AngleAxis (rotYRight, Vector3.up);
		bulletRotationLeft = Quaternion.AngleAxis (rotYLeft, Vector3.up);
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
		Vector3 angles = bulletSpawnRight.transform.localRotation.eulerAngles;
		FireSingle (bulletSpawnRight, bulletRotationRight);
		FireSingle (bulletSpawnLeft,  bulletRotationLeft);
	}

	private void FireSingle(Transform spawn, Quaternion velocityRotation) {
		var bulletObject = (GameObject)Instantiate (BulletPrefab, spawn.position, spawn.rotation);
		bulletObject.GetComponent<Rigidbody> ().velocity = velocityRotation * transform.forward * BULLET_SPEED;
		bulletObject.layer = BULLET_LAYER;
		var bulletScript = bulletObject.GetComponent<Bullet> ();
		bulletScript.Player = gameObject;

		Destroy (bulletObject, BULLET_DURATION);
	}
}
