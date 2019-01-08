using System;
using UnityEngine;

public class OVRModeParms : MonoBehaviour
{
	private void Start()
	{
		if (!OVRManager.isHmdPresent)
		{
			base.enabled = false;
			return;
		}
		base.InvokeRepeating("TestPowerStateMode", 10f, 10f);
	}

	private void Update()
	{
		if (OVRInput.GetDown(this.resetButton, OVRInput.Controller.Active))
		{
			OVRPlugin.cpuLevel = 0;
			OVRPlugin.gpuLevel = 1;
		}
	}

	private void TestPowerStateMode()
	{
		if (OVRPlugin.powerSaving)
		{
			Debug.Log("POWER SAVE MODE ACTIVATED");
		}
	}

	public OVRInput.RawButton resetButton = OVRInput.RawButton.X;
}
