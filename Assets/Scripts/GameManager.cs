using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

	public GameObject m_PlayerShip;
	public GameObject m_Camera;

	private Vector3[] spawnPoints;
	private Quaternion[] spawnRotations;
	private int nextSpawnPos = 0;

	// Server only
	public void SpawnPlayer(GameObject player) {
		if (spawnPoints == null) {
			if (Debug.isDebugBuild) {
				int size = 4;
				spawnPoints = new Vector3[size];
				spawnRotations = new Quaternion[size];
				for (int i = 0; i < size; i++) {
					spawnPoints [i] = new Vector3 (i*5, 0, 0);
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

		NetworkIdentity playerNetworkIdentity = player.GetComponent<NetworkIdentity> ();

		StartCoroutine(SpawnPlayerDelayed(playerNetworkIdentity.netId, spawnPoints[n], spawnRotations[n]));
	}
		
	IEnumerator SpawnPlayerDelayed(NetworkInstanceId playerId, Vector3 pos, Quaternion rot)
	{
		// Wait for PlayerController to be assigned on the client as well
		yield return new WaitForSeconds(1f);

		RpcSpawnPlayer (playerId, pos, rot);
	}

	// Player position is managed by the Client
	[ClientRpc]
	void RpcSpawnPlayer(NetworkInstanceId playerId, Vector3 pos, Quaternion rot) {
		GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
		NetworkIdentity playerNetworkIdentity = player.GetComponent<NetworkIdentity> ();

		if (playerNetworkIdentity.netId == playerId) {
			player.transform.position = pos;
			player.transform.rotation = rot;
			player.SetActive (true);
		}
	}

	void OnGUI() {
		Event e = Event.current;

		if (Debug.isDebugBuild && e.type == EventType.KeyUp && e.control && e.keyCode == KeyCode.R) {
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
