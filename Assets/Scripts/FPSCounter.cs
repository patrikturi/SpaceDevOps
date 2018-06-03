using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
	public Text m_fpsText;

	int m_frameCounter = 0;
	float m_timeCounter = 0.0f;
	float m_lastFramerate = 0.0f;
	float m_refreshTime = 0.5f;

	void Update()
	{
		if( m_timeCounter < m_refreshTime )
		{
			m_timeCounter += Time.deltaTime;
			m_frameCounter++;
		}
		else
		{
			m_lastFramerate = Mathf.Round((float)m_frameCounter/m_timeCounter);
			m_frameCounter = 0;
			m_timeCounter = 0.0f;
			m_fpsText.text = m_lastFramerate.ToString ();
		}
	}
}