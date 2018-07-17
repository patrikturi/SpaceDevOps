using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PController
{
	public PController(float gain) {
		this.gain = gain;
	}

	protected float gain;
	protected float min;
	protected float max;
	protected bool hasMin = false;
	protected bool hasMax = false;

	public void SetGain(float gain) {
		this.gain = gain;
	}

	public void SetMinOutput(float value) {
		min = Mathf.Abs(value);
		hasMin = true;
	}

	public void ClearMinOutput() {
		hasMin = false;
	}

	public void SetMaxOutput(float value) {
		max = Mathf.Abs(value);
		hasMax = true;
	}

	public void ClearMaxOutput() {
		hasMax = false;
	}

	public float ApplyLimits(float outValue) {
		if (hasMin) {
			if (outValue < 0f) {
				outValue = Mathf.Min (outValue, -min);
			} else {
				outValue = Mathf.Max (outValue, min);
			}
		}
		if (hasMax) {
			outValue = Mathf.Clamp (outValue, -max, max);
		}
		return outValue;
	}

	public float GetOutput(float current, float target) {
		float outValue = (target - current) * gain;
		return ApplyLimits(outValue);
	}
}
