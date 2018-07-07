using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {

	public static SceneManager Instance;

	public RectTransform HealthBar;
	public Transform BoundsFront;
	private Transform platformsParent;
	private Transform planetsParent;
	public GameObject PlatformPrefab;
	public GameObject PlanetPrefab;

	public float BOUNDS_SIZE {
		get {
			return boundsSize;
		}
	}
	private float boundsSize;
	private const int PLATFORM_CNT = 16;
	private const int PLANET_CNT = 48;

	void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if(Instance != this){
			Destroy (this);
		}

		boundsSize = BoundsFront.localScale.x / 2f;
		GameObject platforms = new GameObject ();
		platforms.name = "Platforms";
		platformsParent = platforms.transform;
		GameObject planets = new GameObject ();
		planets.name = "Planets";
		planetsParent = planets.transform;

		SpawnStaticScene ();
	}

	private void SpawnStaticScene() {
		Vector3 offset;
		int platformDiv = 4;
		offset = new Vector3 (0, BOUNDS_SIZE / 3f, 0);
		SpawnPlatforms (platformDiv, 0f, offset);
		offset = new Vector3 (0, 2* BOUNDS_SIZE / 3f, 0);
		SpawnPlatforms (platformDiv, 0.5f, offset);
		offset = new Vector3 (0, -BOUNDS_SIZE / 3f, 0);
		SpawnPlatforms (platformDiv, 0f, offset);
		offset = new Vector3 (0, -2* BOUNDS_SIZE / 3f, 0);
		SpawnPlatforms (platformDiv, 0.5f, offset);

		int planetDiv = 6;
		float planetsSpace = BOUNDS_SIZE / 6f;
		offset = new Vector3 (0, planetsSpace, 0);
		SpawnPlanets (planetDiv, 0f, offset, planetsSpace*0.75f);
		offset = new Vector3 (0, 3*planetsSpace, 0);
		SpawnPlanets (planetDiv, 0f, offset, planetsSpace*0.75f);
		offset = new Vector3 (0, 5*planetsSpace, 0);
		SpawnPlanets (planetDiv, 0f, offset, planetsSpace*0.75f);
		offset = new Vector3 (0, -planetsSpace, 0);
		SpawnPlanets (planetDiv, 0f, offset, planetsSpace*0.75f);
		offset = new Vector3 (0, -3*planetsSpace, 0);
		SpawnPlanets (planetDiv, 0f, offset, planetsSpace*0.75f);
		offset = new Vector3 (0, -5*planetsSpace, 0);
		SpawnPlanets (planetDiv, 0f, offset, planetsSpace*0.75f);
	}

	private void SpawnPlatforms(int div, float startPhase, Vector3 offset) {
		Vector3 vector = new Vector3(BOUNDS_SIZE/2f, 0, 0);
		Quaternion rot;
		int cnt = PLATFORM_CNT / div;
		for (int i = 0; i < cnt; i++) {
			rot = Quaternion.AngleAxis (360f/ cnt * (i+startPhase), Vector3.up);
			Vector3 pos = rot * vector + offset;
			Instantiate (PlatformPrefab, pos, Quaternion.identity, platformsParent);
		}
	}

	private void SpawnPlanets(int div, float startPhase, Vector3 offset, float offsetVarMax) {
		Vector3 vector = new Vector3(BOUNDS_SIZE/2f, 0, 0);
		Quaternion rot;
		int cnt = PLANET_CNT / div;
		for (int i = 0; i < cnt; i++) {
			float angleVar = Random.Range (0.2f, 0.8f);
			rot = Quaternion.AngleAxis (360f/ cnt * (i+startPhase+angleVar), Vector3.up);
			Vector3 pos = rot * vector + offset;
			float offsetVarX = Random.Range (-offsetVarMax*4, offsetVarMax*4);
			float offsetVarY = Random.Range (-offsetVarMax, offsetVarMax);
			float offsetVarZ = Random.Range (-offsetVarMax*4, offsetVarMax*4);
			pos += new Vector3 (offsetVarX, offsetVarY, offsetVarZ);
			Instantiate (PlanetPrefab, pos, Quaternion.identity, planetsParent);
		}
	}
}
