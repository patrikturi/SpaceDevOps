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

	public void takeDamage(int amount) {
		m_CurrentHealth -= amount;
		if (m_CurrentHealth <= 0) {
			m_CurrentHealth = 0;
			die ();
		}
		m_HealthBar.sizeDelta = new Vector2(INITIAL_WIDTH*m_CurrentHealth/MAX_HEALTH, m_HealthBar.sizeDelta.y);
	}

	GameObject explosion;

	private void die() {
		explosion = (GameObject)Instantiate (m_ExplosionPrefab, transform.position + transform.forward*0.75f, transform.rotation);

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
