using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class NetworkHUD : MonoBehaviour {

	public Button ConnectButton, HostButton, ServerButton;

	public InputField NameInput, AddressInput, PortInput;

	public Dropdown SelectColor1, SelectColor2;

	public Text ErrorLabel, ErrorDetails;

	private NetworkManager networkManager;

	void Awake() {
		networkManager = GetComponent<NetworkManager> ();
	}

	// Use this for initialization
	void Start () {
		HostButton.onClick.AddListener (StartHost);
		ServerButton.onClick.AddListener (StartServer);
		ConnectButton.onClick.AddListener (StartClient);
	}
	
	// Update is called once per frame
	void Update () {
		if (!NetworkClient.active && !NetworkServer.active && networkManager.matchMaker == null) {
			if (!isUIFocused ()) {
				if (Input.GetKeyDown (KeyCode.H)) {
					StartHost ();
				}
				if (Input.GetKeyDown (KeyCode.S)) {
					StartServer ();
				}
				if (Input.GetKeyDown (KeyCode.C)) {
					StartClient ();
				}
			}
		}
	}

	void StartHost() {
		int port = getPort ();
		string name = NameInput.text;
		if (name.Length == 0) {
			error ("Must enter a player name first!");
		} else if (port <= 0) {
			// message already shown
		} else {
			networkManager.networkPort = port;
			networkManager.StartHost ();
		}
	}

	void StartServer() {
		int port = getPort ();
		if (port > 0) {
			networkManager.networkPort = port;
			networkManager.StartServer ();
		}
	}

	void StartClient() {
		string name = NameInput.text;
		string address = AddressInput.text;
		int port = getPort ();
		if (name.Length == 0) {
			error ("Must enter a player name first!");
		} else if (address.Length == 0) {
			error ("Must enter an IP address first!");
		} else if (port <= 0) {
			// message already shown
		} else { // All ok
			networkManager.networkPort = port;
			networkManager.networkAddress = address;
			networkManager.StartClient ();
		}
	}

	private bool isUIFocused() {
		return NameInput.isFocused || PortInput.isFocused || AddressInput.isFocused;
	}

	private int getPort() {
		int port;
		try {
			port = Int32.Parse (PortInput.text);
		} catch(FormatException) {
			error ("Specified Port must be an integer!");
			return -1;
		}
		if (port <= 0) {
			error ("Specified port must be greater than zero!");
		}
		return port;
	}

	private void error(string message) {
		ErrorLabel.text = "Error:";
		ErrorDetails.text = message;
	}
}
