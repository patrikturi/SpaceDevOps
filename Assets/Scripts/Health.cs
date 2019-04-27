using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Health : NetworkBehaviour {

	public const int MAX_HEALTH = 100;

	[SyncVar (hook = "OnHealthChanged")] public int currentHealth;
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

	public virtual void Start() {
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
		if (!isServer || currentHealth <= 0) {
			return;
		}

		if (amount >= currentHealth && amount != 0) {
			DieServer ();
		}
		setHealth (currentHealth - amount);
	}

	private int prevHealthClient = MAX_HEALTH;

	void OnHealthChanged(int health) {
		OnHealthChangedCallback (health);

		int damage = prevHealthClient - health;
		damage = Mathf.Clamp (damage, 0, MAX_HEALTH);
		prevHealthClient = health;

		if (health > 0) {
			if (fireParticles != null && damage > 0) {
				fireParticles.Emit (damage / 10 + 1);
			}
		} else {
			DieClient ();
		}
	}

	protected virtual void OnHealthChangedCallback(int health) {
		// Override this function to get callback in the superclass
	}

	protected virtual void OnDeathServerCallback() {
		// Override this function to get callback in the superclass
	}

	protected virtual void setHealth(int health) {
		// Managed by the server, clients receive OnHealthChanged event
		if (isServer) {
			currentHealth = Mathf.Clamp(health, 0, MAX_HEALTH);
		}
	}

	private void DieServer() {
		OnDeathServerCallback ();
	}

	private void DieClient() {
		// Detach fire particles from the ship so they don't disappear when the ship is deactivated
		if (fire != null) {
			fire.transform.parent = null;
		}

		// TODO: isClient -> apply effects for client only?
		if (explosion != null) {
			explosion.transform.parent = null;
		}
		if (fire != null) {
			fire.transform.parent = null;
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

		Invoke ("AttachExplosionAgain", 7.5f); // Long enough time for the explosion to be over
	}

	private void SetInactive() {
		gameObject.SetActive (false);
	}

	private void AttachExplosionAgain() {
		if (fireParticles != null) {
			fireParticles.Clear ();
			fire.transform.parent = gameObject.transform;
		}
		if (explosionParticles != null) {
			explosionParticles.Clear ();
			explosion.transform.parent = gameObject.transform;
		}
	}
}
