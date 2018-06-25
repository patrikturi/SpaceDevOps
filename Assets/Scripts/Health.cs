using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

	public RectTransform m_HealthBar;
	public GameObject m_ExplosionPrefab;

	private float INITIAL_WIDTH;
	private const int MAX_HEALTH = 100;
	private const float EXPLOSION_LIFETIME = 4.5f;
	private const float EXPLOSION_BURST_TIME = 0.5f;

	private int m_CurrentHealth = MAX_HEALTH;

	void Awake() {
		INITIAL_WIDTH = m_HealthBar.sizeDelta.x;
	}

	public void Reset() {
		setHealth (MAX_HEALTH);
	}

	void OnGUI() {
		Event e = Event.current;

		if (Debug.isDebugBuild && e.type == EventType.KeyUp && e.control && e.keyCode == KeyCode.D) {
			takeDamage (MAX_HEALTH);
		}
	}

	public void takeDamage(int amount) {
		if (m_CurrentHealth <= 0) {
			return;
		}

		if (m_CurrentHealth > amount) {
			setHealth (m_CurrentHealth - amount);
		} else {
			die ();
		}
	}

	private void setHealth(int health) {
		m_CurrentHealth = health;
		m_HealthBar.sizeDelta = new Vector2(INITIAL_WIDTH*m_CurrentHealth/MAX_HEALTH, m_HealthBar.sizeDelta.y);
	}

	private void die() {
		setHealth (0);
		GameObject explosion = (GameObject)Instantiate (m_ExplosionPrefab, transform.position + transform.forward*0.75f, transform.rotation);

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
