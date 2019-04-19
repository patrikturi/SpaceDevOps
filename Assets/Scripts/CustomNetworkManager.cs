using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager {

	public GameManager Game;

	private bool worldSpawned = false;

    // Server callbacks
    public override void OnServerConnect(NetworkConnection conn) {
        Debug.Log("A client connected to the server: " + conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn) {

        NetworkServer.DestroyPlayersForConnection(conn);
        if (conn.lastError != NetworkError.Ok) {
            if (LogFilter.logError) { Debug.LogError("ServerDisconnected due to error: " + conn.lastError); }
        }

		Debug.Log("A client disconnected from the server: " + conn);
    }

    public override void OnServerReady(NetworkConnection conn) {
		Debug.Log("Client is set to the ready state (ready to receive state updates): " + conn);
        NetworkServer.SetClientReady(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
		Debug.Log("Client has requested to get his player added to the game");
		// Unfortunately can't spawn in OnStartServer() because NetworkServer is not initialized yet
		if (!worldSpawned) {
			worldSpawned = true;
			SceneManager.Instance.SpawnWorld ();
		}

        var player = (GameObject)GameObject.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

		Game.SpawnPlayer (player);
    }

    public override void OnServerError(NetworkConnection conn, int errorCode) {
        Debug.Log("Server network error occurred: " + (NetworkError)errorCode);
    }

    public override void OnStartHost() {
        Debug.Log("Host has started");
    }

    public override void OnStartServer() {
        Debug.Log("Server has started");
		UIManager.Instance.LoadServerInGameUI ();
    }

    public override void OnStopServer() {
        Debug.Log("Server has stopped");
    }

    public override void OnStopHost() {
        Debug.Log("Host has stopped");
    }

    // Client callbacks
    public override void OnClientConnect(NetworkConnection conn) {
        Debug.Log("Connected successfully to server, now to set up other stuff for the client...");
		ClientScene.AddPlayer(conn, 0);
		UIManager.Instance.LoadInGameUI ();
    }

    public override void OnClientDisconnect(NetworkConnection conn) {

		foreach(var player in conn.playerControllers) {
			GameObject playerObject = player.gameObject;
			if (playerObject != null) {
				NetworkServer.Destroy (player.gameObject);
			}
		}

        StopClient();

        if (conn.lastError != NetworkError.Ok)
        {
            if (LogFilter.logError) { Debug.LogError("ClientDisconnected due to error: " + conn.lastError); }
        }

        Debug.Log("Client disconnected from server: " + conn);
    }

    public override void OnClientError(NetworkConnection conn, int errorCode) {
        Debug.Log("Client network error occurred: " + (NetworkError)errorCode);
    }

    public override void OnClientNotReady(NetworkConnection conn) {
        Debug.Log("Server has set client to be not-ready (stop getting state updates)");
    }

    public override void OnStartClient(NetworkClient client) {
        Debug.Log("Client has started");
    }

    public override void OnStopClient() {
        Debug.Log("Client has stopped");
    }

    public override void OnClientSceneChanged(NetworkConnection conn) {
        base.OnClientSceneChanged(conn);
        Debug.Log("Server triggered scene change and we've done the same, do any extra work here for the client...");
    }
}
