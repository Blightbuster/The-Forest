using System;
using UnityEngine;
using UnityEngine.VR;

public class VRSettingsHelper : MonoBehaviour
{
	private void OnValidate()
	{
		if (Application.isPlaying && this.AllowEdits)
		{
			VRSettings.renderScale = this.RenderScale;
			VRSettings.renderViewportScale = this.RenderViewportScale;
			VRSettings.showDeviceView = this.ShowDeviceView;
		}
	}

	private void Update()
	{
		this.RefreshValues();
	}

	private void RefreshValues()
	{
	}

	public bool AllowEdits;

	public bool VREnabled;

	public int EyeTextureHeight;

	public int EyeTextureWidth;

	public bool IsDeviceActive;

	public string LoadedDeviceName;

	public float RenderScale;

	public float RenderViewportScale;

	public bool ShowDeviceView;

	public string[] SupportedDevices;
}
