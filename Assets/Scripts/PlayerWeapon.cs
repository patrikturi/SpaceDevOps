using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerWeapon : NetworkBehaviour {

	// TODO: bullet pooling

	public GameObject BulletPrefab;

	private const float BULLET_SPEED = 50f;
	private const float BULLET_DURATION = 5.5f;
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
		if (!isLocalPlayer) {
			return;
		}

		float fireInput = Input.GetAxis ("Fire");

		if (fireInput > 0.1f && fireEnabledTimeStamp <= Time.time) {

			CmdFireAll(bulletSpawnLeft.position, bulletSpawnRight.position, bulletSpawnLeft.rotation, bulletSpawnRight.rotation, shipBody.velocity);
			fireEnabledTimeStamp = Time.time + COOLDOWN_DURATION;
		}
	}

	[Command]
	void CmdFireAll( Vector3 spawnPosLeft, Vector3 spawnPosRight, Quaternion spawnRotLeft, Quaternion spawnRotRight, Vector3 shipVelocity) {
		FireSingle (spawnPosLeft, spawnRotLeft, shipVelocity);
		FireSingle (spawnPosRight, spawnRotLeft, shipVelocity);
	}

	private void FireSingle(Vector3 spawnPos, Quaternion spawnRot, Vector3 shipVelocity) {
		var bulletObject = (GameObject)Instantiate (BulletPrefab, spawnPos, spawnRot * bulletRotationOffset);

		Rigidbody body = bulletObject.GetComponent<Rigidbody> ();
		body.velocity = shipVelocity + spawnRot * Vector3.forward * BULLET_SPEED;
		bulletObject.layer = BULLET_LAYER;
		NetworkServer.Spawn (bulletObject);

		var bulletScript = bulletObject.GetComponent<Bullet> ();
		bulletScript.Player = gameObject;

		// TODO: use NetworkDestroy?
		Destroy (bulletObject, BULLET_DURATION);
	}
}
