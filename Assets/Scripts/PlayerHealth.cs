using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Health {

	// Note: replace by network api later
	private bool LOCAL_PLAYER = true;
	private static float INITIAL_WIDTH;

	private const float EXPLOSION_BURST_TIME = 0.5f;

	private RectTransform healthBar;

	void Start() {
		base.Start ();
		if (LOCAL_PLAYER) {
			healthBar = SceneManager.Instance.HealthBar;
			INITIAL_WIDTH = healthBar.sizeDelta.x;
		}
		base.destroyDelayTime = EXPLOSION_BURST_TIME - 0.2f;
	}

	protected override void setHealth(int health) {
		base.setHealth (health);
		if (healthBar != null) {
			healthBar.sizeDelta = new Vector2 (INITIAL_WIDTH * currentHealth / MAX_HEALTH, healthBar.sizeDelta.y);
		}
	}

	void OnGUI() {
		Event e = Event.current;

		if (Debug.isDebugBuild && e.type == EventType.KeyUp && e.control && e.keyCode == KeyCode.D) {
			TakeDamage (MAX_HEALTH);
		} else if (Debug.isDebugBuild && e.type == EventType.KeyUp && e.control && e.keyCode == KeyCode.X) {
			TakeDamage (20);
		}
	}
}
