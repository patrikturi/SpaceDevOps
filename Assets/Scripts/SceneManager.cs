using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {

	public static SceneManager Instance;

	public RectTransform HealthBar;
	public Transform BoundsFront;
	private Transform platformsParent;
	private Transform planetsParent;
	private Transform doodadsParent;
	public GameObject PlatformPrefab;
	public GameObject PlanetPrefab;
	public List<GameObject> DoodadPrefabs;
	private int DOODAD_COUNT = 200;
	private const float DOODAD_RADIUS_MAX = 1.5f;
	private const float DOODAD_SIZE_MIN = 0.6f;
	private const float DOODAD_SIZE_MAX = 5f;

	public float BOUNDS_SIZE {
		get {
			return boundsSize;
		}
	}
	public float PLANET_COLLIDER_SIZE {
		get {
			return planetColliderSize;
		}
	}
	public float PLANET_MAX_SIZE {
		get {
			return planetColliderSize * PLANET_SIZE_VAR_MAX;
		}
	}
	private float boundsSize = -1f;
	private float planetColliderSize = -1f;
	private const int PLATFORM_CNT = 16;
	private const int PLANET_CNT = 16;
	private const float PLANET_SIZE_VAR_MIN = 0.7f;
	public const float PLANET_SIZE_VAR_MAX = 1.5f;

	void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if(Instance != this) {
			Destroy (this);
		}

		boundsSize = BoundsFront.localScale.x / 2f;

		SphereCollider planetCollider = PlanetPrefab.GetComponent<SphereCollider> ();
		planetColliderSize = planetCollider.radius;

		GameObject platforms = new GameObject ();
		platforms.name = "Platforms";
		platformsParent = platforms.transform;
		GameObject planets = new GameObject ();
		planets.name = "Planets";
		planetsParent = planets.transform;

		GameObject doodads = new GameObject ();
		doodads.name = "Doodads";
		doodadsParent = doodads.transform;

		SpawnStaticScene ();

		SpawnDoodads(DOODAD_COUNT);
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
		float variationRatio = 0.6f;
		offset = new Vector3 (0, planetsSpace, 0);
		SpawnPlanets (planetDiv, 0f, offset, planetsSpace*variationRatio);
		offset = new Vector3 (0, 3*planetsSpace, 0);
		SpawnPlanets (planetDiv, 0f, offset, planetsSpace*variationRatio);
		offset = new Vector3 (0, 5*planetsSpace, 0);
		SpawnPlanets (planetDiv, 0f, offset, planetsSpace*variationRatio);
		offset = new Vector3 (0, -planetsSpace, 0);
		SpawnPlanets (planetDiv, 0f, offset, planetsSpace*variationRatio);
		offset = new Vector3 (0, -3*planetsSpace, 0);
		SpawnPlanets (planetDiv, 0f, offset, planetsSpace*variationRatio);
		offset = new Vector3 (0, -5*planetsSpace, 0);
		SpawnPlanets (planetDiv, 0f, offset, planetsSpace*variationRatio);
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

			Quaternion planetRot = Quaternion.Euler(Random.insideUnitSphere * 360f);
			GameObject planet = Instantiate (PlanetPrefab, pos, planetRot, planetsParent);
			float size = Random.Range (PLANET_SIZE_VAR_MIN, PLANET_SIZE_VAR_MAX);
			planet.transform.localScale = new Vector3 (size, size, size);
		}
	}

	// Spawn destructible things
	private void SpawnDoodads(int spawnCount) {

		int count = 0;
		Collider[] hitResults = new Collider[1];
		int layer = LayerMask.GetMask ("Default");
		float spawnRange = BOUNDS_SIZE*1.2f - DOODAD_RADIUS_MAX;
		if (DoodadPrefabs.Count == 0) {
			return;
		}
		while (count < spawnCount) {
			Vector3 pos = Random.insideUnitSphere * spawnRange;
			float size = Random.Range (DOODAD_SIZE_MIN, DOODAD_SIZE_MAX);

			int hits = Physics.OverlapSphereNonAlloc (pos, DOODAD_RADIUS_MAX*size, hitResults, layer, QueryTriggerInteraction.Ignore);

			if (hits <= 0) {
				Vector3 rotVector = Random.insideUnitCircle * 360f;
				Quaternion rot = Quaternion.Euler (rotVector);
				int typeIndex = Random.Range (0, DoodadPrefabs.Count);
				GameObject prefab = DoodadPrefabs [typeIndex];
				GameObject doodad = Instantiate (prefab, pos, rot, doodadsParent);
				doodad.transform.localScale = new Vector3 (size, size, size);
				Rigidbody body = doodad.GetComponent<Rigidbody> ();
				if (body != null) {
					body.mass = body.mass * size;
				}
				count++;
			}
		}
	}
}
