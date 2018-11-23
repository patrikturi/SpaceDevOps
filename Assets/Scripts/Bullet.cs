using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	// The firing player
	public GameObject Player;

	private const int BULLET_DAMAGE = 20;

	void OnTriggerEnter(Collider other) {

		GameObject hitObject = other.gameObject;
		// Ship hull
		Health health = hitObject.GetComponent<Health>();

		// Ship wings
		// TODO: should hitting the wing take less damage?
		if(health == null && hitObject.name.Contains("Wing")) {
			Transform parent = hitObject.transform.parent;
			if (parent != null) {
				hitObject = parent.gameObject;
				health = hitObject.GetComponent<Health> ();
			}
		}

		// Damageable and not self
		if (health != null && hitObject != Player) {
			health.TakeDamage (BULLET_DAMAGE);
		}

		if (hitObject != Player) {
			Destroy (gameObject);
		}
	}
}
