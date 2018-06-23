using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

	public RectTransform m_HealthBar;

	private float INITIAL_WIDTH;
	private const int MAX_HEALTH = 100;
	private int m_CurrentHealth = MAX_HEALTH;

	void Awake() {
		INITIAL_WIDTH = m_HealthBar.sizeDelta.x;
	}

	public void takeDamage(int amount) {
		m_CurrentHealth -= amount;
		if (m_CurrentHealth <= 0) {
			m_CurrentHealth = 0;
			Debug.Log("Dead");
		}
		m_HealthBar.sizeDelta = new Vector2(INITIAL_WIDTH*m_CurrentHealth/MAX_HEALTH, m_HealthBar.sizeDelta.y);
	}
}
