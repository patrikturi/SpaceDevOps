using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerDetails {

	public string name;
	public Color color1;
	public Color color2;
	public int kills = 0;
	public int deaths = 0;

	public PlayerDetails(string n, Color col1, Color col2) {
		name = n;
		color1 = col1;
		color2 = col2;
	}
}
