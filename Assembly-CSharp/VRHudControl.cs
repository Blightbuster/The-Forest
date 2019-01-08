using System;
using UnityEngine;

public class VRHudControl : MonoBehaviour
{
	private void Start()
	{
		if (ForestVR.Enabled)
		{
			this.HudCam.enabled = false;
			this.VRHudCam.enabled = true;
		}
		else
		{
			this.HudCam.enabled = true;
			this.VRHudCam.enabled = false;
		}
	}

	public Camera HudCam;

	public Camera VRHudCam;
}
