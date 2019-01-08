using System;
using UnityEngine;

public class VRCutSceneSetup : MonoBehaviour
{
	private void Start()
	{
		if (!ForestVR.Enabled)
		{
			if (this.mainCam)
			{
				this.mainCam.forceIntoRenderTexture = false;
				this.mainCam.targetTexture = null;
			}
		}
	}

	public Camera mainCam;

	public Camera VRCam;
}
