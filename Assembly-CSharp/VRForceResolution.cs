using System;
using UnityEngine;
using UnityEngine.VR;

public class VRForceResolution : MonoBehaviour
{
	private void Awake()
	{
		if (base.enabled && ForestVR.Enabled)
		{
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
	}

	private void Update()
	{
		this.Resolution = VRSettings.renderScale;
		if (!this.Resolution.Equals(this.TargetResolution))
		{
			VRSettings.renderScale = this.TargetResolution;
		}
	}

	public float Resolution;

	public float TargetResolution = 1f;
}
