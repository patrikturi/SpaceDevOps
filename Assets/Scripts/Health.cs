using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

	public RectTransform HealthBar;
	public GameObject ExplosionPrefab;

	private static float INITIAL_WIDTH;
	private const int MAX_HEALTH = 100;
	private const float EXPLOSION_LIFETIME = 4.5f;
	private const float EXPLOSION_BURST_TIME = 0.5f;

	private int currentHealth = MAX_HEALTH;
	private new ParticleSystem particleSystem;
	private GameObject hitParticlesHost;

	void Awake() {
		INITIAL_WIDTH = HealthBar.sizeDelta.x;
		hitParticlesHost = transform.Find ("HitParticles").gameObject;
		particleSystem = hitParticlesHost.GetComponent<ParticleSystem> ();
	}

	void OnEnable() {
		hitParticlesHost.transform.parent = transform;
	}

	public void Reset() {
		setHealth (MAX_HEALTH);
	}

	void OnGUI() {
		Event e = Event.current;

		if (Debug.isDebugBuild && e.type == EventType.KeyUp && e.control && e.keyCode == KeyCode.D) {
			TakeDamage (MAX_HEALTH);
		} else if (Debug.isDebugBuild && e.type == EventType.KeyUp && e.control && e.keyCode == KeyCode.X) {
			TakeDamage (20);
		}
	}

	public void TakeDamage(int amount) {
		if (currentHealth <= 0) {
			return;
		}

		if (currentHealth > amount) {
			setHealth (currentHealth - amount);
			particleSystem.Emit (amount/10+1);
		} else {
			Die ();
		}
	}

	private void setHealth(int health) {
		currentHealth = health;
		HealthBar.sizeDelta = new Vector2(INITIAL_WIDTH*currentHealth/MAX_HEALTH, HealthBar.sizeDelta.y);
	}

	private void Die() {
		setHealth (0);
		// Detach fire particles from the ship so they don't disappear when the ship is deactivated
		hitParticlesHost.transform.parent = null;
		GameObject explosion = (GameObject)Instantiate (ExplosionPrefab, transform.position + transform.forward*0.75f, transform.rotation);

		Rigidbody body = GetComponent<Rigidbody> ();
		Rigidbody expBody = explosion.GetComponent<Rigidbody> ();
		expBody.velocity = body.velocity;

		Destroy (explosion, EXPLOSION_LIFETIME);

		Invoke ("SetInactive", EXPLOSION_BURST_TIME-0.2f);
	}

	private void SetInactive() {
		gameObject.SetActive (false);
	}
}
