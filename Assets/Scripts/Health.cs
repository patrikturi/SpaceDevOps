using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

	public int MAX_HEALTH = 100;

	protected int currentHealth;
	protected float destroyDelayTime = 0f;
	private GameObject fire;
	private GameObject explosion;
	private ParticleSystem fireParticles;
	private ParticleSystem explosionParticles;

	void Awake() {
		fire = findOptionalChild("Fire");
		explosion = findOptionalChild("Explosion");
		if (fire != null) {
			fireParticles = fire.GetComponent<ParticleSystem> ();
		}
		if (explosion != null) {
			explosionParticles = explosion.GetComponent<ParticleSystem> ();
		}
	}

	GameObject findOptionalChild(string objectName) {
		Transform tr = transform.Find (objectName);
		if (tr == null) {
			return null;
		}
		return tr.gameObject;
	}

	protected virtual void Start() {
		currentHealth = MAX_HEALTH;
	}

	void OnEnable() {
		if (fire != null) {
			fire.transform.parent = transform;
			fire.transform.localPosition = Vector3.zero;
			fire.transform.localRotation = Quaternion.identity;
		}
		if (explosion != null) {
			explosion.transform.parent = transform;
			explosion.transform.localPosition = Vector3.zero;
			explosion.transform.localRotation = Quaternion.identity;
		}
	}

	public void Reset() {
		setHealth (MAX_HEALTH);
	}

	public void TakeDamage(int amount) {
		if (currentHealth <= 0) {
			return;
		}

		if (currentHealth > amount) {
			setHealth (currentHealth - amount);
			if (fireParticles != null) {
				fireParticles.Emit (amount / 10 + 1);
			}
		} else {
			Die ();
		}
	}

	protected virtual void setHealth(int health) {
		currentHealth = health;
	}

	private void Die() {
		setHealth (0);
		// Detach fire particles from the ship so they don't disappear when the ship is deactivated
		if (fire != null) {
			fire.transform.parent = null;
		}
		if (explosion != null) {
			explosion.transform.parent = null;
		}

		if (explosionParticles != null) {
			explosionParticles.transform.position = transform.position + transform.forward * 0.75f;
		}

		if (explosionParticles != null) {
			Rigidbody body = GetComponent<Rigidbody> ();
			Rigidbody expBody = explosionParticles.GetComponent<Rigidbody> ();
			expBody.velocity = body.velocity;
			explosionParticles.Play ();
		}

		if (destroyDelayTime > float.Epsilon) {
			Invoke ("SetInactive", destroyDelayTime);
		} else {
			SetInactive();
		}
	}

	private void SetInactive() {
		gameObject.SetActive (false);
	}
}
