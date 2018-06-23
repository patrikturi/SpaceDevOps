using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	private const int BULLET_DAMAGE = 20;

	void OnTriggerEnter(Collider other) {

		GameObject hitObject = other.gameObject;
		// Ship hull
		Health health = hitObject.GetComponent<Health>();

		// Ship wings
		// TODO: should hitting the wing take less damage?
		if(health == null && hitObject.name.Contains("Wing")) {
			GameObject parentObject = hitObject.transform.parent.gameObject;
			health = parentObject.GetComponent<Health> ();
		}

		if (health != null) {
			health.takeDamage (BULLET_DAMAGE);
		}

		Destroy (gameObject);
	}
}
