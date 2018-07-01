using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
	private Text textComponent;
	private int frameCounter = 0;
	private float timeCounter = 0.0f;
	private float lastFramerate = 0.0f;
	private float refreshTime = 0.5f;

	void Awake() {
		textComponent = GetComponent<Text> ();
	}

	void Update()
	{
		if( timeCounter < refreshTime )
		{
			timeCounter += Time.deltaTime;
			frameCounter++;
		}
		else
		{
			lastFramerate = Mathf.Round((float)frameCounter/timeCounter);
			frameCounter = 0;
			timeCounter = 0.0f;
			textComponent.text = lastFramerate.ToString ();
		}
	}
}