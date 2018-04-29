using System;
using UnityEngine;


public class VRCameraUISwitcher : MonoBehaviour
{
	
	private void Update()
	{
		if (ForestVR.Enabled)
		{
			this.VRCam.targetTexture = this.TheatreMode;
		}
	}

	
	public Camera VRCam;

	
	public RenderTexture TheatreMode;
}
