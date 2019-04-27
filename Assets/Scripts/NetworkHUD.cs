using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class NetworkHUD : MonoBehaviour {

	public static NetworkHUD Instance;

	public Button ConnectButton, HostButton, ServerButton;

	public InputField NameInput, AddressInput, PortInput;

	public Dropdown SelectColor1, SelectColor2;

	public Text ErrorLabel, ErrorDetails;

	private NetworkManager networkManager;

	public string PlayerName { get { return playerName; } }
	public Color PlayerColor1 { get { return playerColor1; } }
	public Color PlayerColor2 { get { return playerColor2; } }
	private string playerName;
	private Color playerColor1;
	private Color playerColor2;

	private Dictionary<string, Color> colors = new Dictionary<string, Color>()
	{
		{"blue", Color.blue},
		{"cyan", Color.cyan},
		{"green", Color.green},
		{"magenta", Color.magenta},
		{"red", Color.red},
		{"white", Color.white},
		{"yellow", Color.yellow},
		{"brown", new Color(0.42f, 0.18f, 0f, 1f)},
		{"orange", new Color(0.99f, 0.4f, 0f, 1f)}
	};

	void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if(Instance != this) {
			Destroy (this);
		}
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

		if (name.Length == 0) {
			error ("Must enter a player name first!");
		} else if(port > 0 && setupPlayer ()) {
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
		string address = AddressInput.text;
		int port = getPort ();

		if (address.Length == 0) {
			error ("Must enter an IP address first!");
		} else if(port > 0 && setupPlayer ()) {
			networkManager.networkPort = port;
			networkManager.networkAddress = address;
			networkManager.StartClient ();
		}
	}

	// Returns true if succesful
	private bool setupPlayer() {
		string name = NameInput.text;
		if (name.Length == 0) {
			error ("Must enter a player name first!");
			return false;
		}
		playerName = name;
		playerColor1 = colors[SelectColor1.captionText.text];
		playerColor2 = colors[SelectColor2.captionText.text];
		return true;
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
