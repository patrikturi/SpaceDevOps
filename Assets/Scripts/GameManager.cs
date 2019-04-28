using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

	public static float PLAYER_RESPAWN_TIME = 8f;

	public static GameManager Instance;

	public GameObject m_PlayerShip;
	public GameObject m_Camera;

	private Vector3[] spawnPoints;
	private Quaternion[] spawnRotations;
	private int nextSpawnPos = 0;
	private Dictionary<NetworkInstanceId, PlayerDetails> allPlayerDetails = new Dictionary<NetworkInstanceId, PlayerDetails> ();

	public void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if(Instance != this) {
			Destroy (this);
		}
	}

	public Dictionary<NetworkInstanceId, PlayerDetails> getPlayerDetails() {
		return allPlayerDetails;
	}

	public void SetPlayerDetails(NetworkInstanceId instanceId, string name, Color col1, Color col2) {

		// Send details of existing players to a new player joined
		if (!allPlayerDetails.ContainsKey (instanceId)) {
			foreach (var entry in allPlayerDetails) {
				PlayerDetails details = entry.Value;
				RpcSetPlayerDetails (entry.Key, details.name, details.color1, details.color2);
			}
		}

		RpcSetPlayerDetails (instanceId, name, col1, col2);
		allPlayerDetails.Add (instanceId, new PlayerDetails (name, col1, col2));
	}

	[ClientRpc]
	void RpcSetPlayerDetails(NetworkInstanceId instanceId, string name, Color col1, Color col2) {

		if (!allPlayerDetails.ContainsKey (instanceId)) {
			PlayerDetails details = new PlayerDetails(name, col1, col2);
			allPlayerDetails.Add (instanceId, details);
		}

		GameObject player = ClientScene.FindLocalObject (instanceId);
		// TODO: Player might have already left - entries are not removed yet
		if (player != null) {
			PlayerController controller = player.GetComponent<PlayerController> ();
			controller.SetMaterial ("main", col1);
			controller.SetMaterial ("wings", col2);
		}
	}

	// Server only
	public void SpawnPlayer(GameObject player) {
		if (spawnPoints == null) {
			if (Debug.isDebugBuild) {
				int size = 4;
				spawnPoints = new Vector3[size];
				spawnRotations = new Quaternion[size];
				for (int i = 0; i < size; i++) {
					spawnPoints [i] = new Vector3 (0, 0, 5+i*5);
					spawnRotations[i] = Quaternion.identity;
				}
			} else {
				NetworkStartPosition[] networkPositions = FindObjectsOfType<NetworkStartPosition> ();
				spawnPoints = new Vector3[networkPositions.Length];
				spawnRotations = new Quaternion[networkPositions.Length];
				for(int i=0; i<networkPositions.Length; i++) {
					spawnPoints [i] = networkPositions[i].transform.position;
					spawnRotations [i] = networkPositions[i].transform.rotation;
				}
			}
		}

		if (nextSpawnPos >= spawnPoints.Length) {
			nextSpawnPos = 0;
		}
		int n = nextSpawnPos;
		nextSpawnPos++;

		StartCoroutine(SpawnPlayerDelayed(player, spawnPoints[n], spawnRotations[n]));
	}
		
	IEnumerator SpawnPlayerDelayed(GameObject player, Vector3 pos, Quaternion rot)
	{
		// Wait for PlayerController to be assigned on the client as well
		yield return new WaitForSeconds(1f);

		Health health = player.GetComponent<Health> ();
		health.Reset ();
		player.SetActive (true);

		var instanceId = player.GetComponent<NetworkIdentity> ().netId;
		RpcSpawnPlayer (instanceId, pos, rot);
	}

	// Player position is managed by the Client
	[ClientRpc]
	void RpcSpawnPlayer(NetworkInstanceId playerId, Vector3 pos, Quaternion rot) {
		GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
		NetworkIdentity playerNetworkIdentity = player.GetComponent<NetworkIdentity> ();

		if (playerNetworkIdentity.netId == playerId) {
			
			player.transform.position = pos;
			player.transform.rotation = rot;

			Rigidbody playerBody = player.GetComponent<Rigidbody> ();
			playerBody.velocity = new Vector3(0, 0, 0);

			player.SetActive (true);
		}
	}

	public void PlayerDied(GameObject player) {
		NetworkInstanceId netId = player.GetComponent<NetworkIdentity> ().netId;
		PlayerDetails details = allPlayerDetails [netId];
		details.deaths += 1;
		RpcUpdateDeaths (netId, details.deaths);

		StartCoroutine (RespawnPlayerDelayed (player));
	}

	public void AddKill(GameObject player) {
		NetworkInstanceId netId = player.GetComponent<NetworkIdentity> ().netId;
		PlayerDetails details = allPlayerDetails [netId];
		details.kills += 1;
		RpcUpdateKills (netId, details.kills);
	}

	IEnumerator RespawnPlayerDelayed(GameObject player) {
		yield return new WaitForSeconds(PLAYER_RESPAWN_TIME);
		SpawnPlayer (player);
	}

	[ClientRpc]
	void RpcUpdateKills(NetworkInstanceId netId, int kills) {
		// TODO: skip if host
		allPlayerDetails [netId].kills = kills;
	}

	[ClientRpc]
	void RpcUpdateDeaths(NetworkInstanceId netId, int deaths) {
		// TODO: skip if host
		allPlayerDetails [netId].deaths = deaths;
	}

	void OnGUI() {
		Event e = Event.current;

		if (Debug.isDebugBuild && isServer && e.type == EventType.KeyUp && e.control && e.keyCode == KeyCode.R) {
			m_PlayerShip.SetActive(true);
			// TODO: Create Player script -> reset()
			m_PlayerShip.GetComponent<Health> ().Reset ();
			Rigidbody body = m_PlayerShip.GetComponent<Rigidbody>();
			body.transform.position = Vector3.zero;
			body.transform.rotation = Quaternion.identity;
			body.velocity = Vector3.zero;

			m_Camera.GetComponent<CameraController> ().Reset ();
		}
	}
}
