using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

	public static GameManager Instance;

	public GameObject m_PlayerShip;
	public GameObject m_Camera;

	private Vector3[] spawnPoints;
	private Quaternion[] spawnRotations;
	private int nextSpawnPos = 0;

	public void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if(Instance != this) {
			Destroy (this);
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
		StartCoroutine (RespawnPlayerDelayed (player));
	}

	IEnumerator RespawnPlayerDelayed(GameObject player) {
		yield return new WaitForSeconds(5f);
		SpawnPlayer (player);
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
