using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerHealth : Health {

	private static float INITIAL_WIDTH;

	private const float EXPLOSION_BURST_TIME = 0.5f;

	private RectTransform healthBar;

	public override void Start() {
		base.Start ();
		if (isLocalPlayer) {
			healthBar = SceneManager.Instance.HealthBar;
			INITIAL_WIDTH = healthBar.sizeDelta.x;
		}
		base.destroyDelayTime = EXPLOSION_BURST_TIME - 0.2f;
	}

	protected override void OnHealthChangedCallback(int health) {
		if (healthBar != null) {
			healthBar.sizeDelta = new Vector2 (INITIAL_WIDTH * health / MAX_HEALTH, healthBar.sizeDelta.y);
		}
	}

	protected override void OnDeathServerCallback() {
		GameManager.Instance.PlayerDied (gameObject);
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
