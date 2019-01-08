using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VR;

public class VRSwitcher : MonoBehaviour
{
	private void Awake()
	{
		Debug.LogWarning("VR SWITCHER AWAKE");
		if (this.RunOnAwake)
		{
			Debug.LogWarning("VR SWITCHER RUNNING ON DEFAULT SETTINGS");
			this.SwitchVRRig(VRSettings.loadedDeviceName != string.Empty);
		}
	}

	public void ActivateVRDevice(VRSystemTarget target)
	{
		if (target != VRSystemTarget.None)
		{
			if (target != VRSystemTarget.Oculus)
			{
				if (target == VRSystemTarget.OpenVR)
				{
					base.StartCoroutine(this.LoadAndEnable("openvr", true));
				}
			}
			else
			{
				base.StartCoroutine(this.LoadAndEnable("oculus", true));
			}
		}
		else
		{
			base.StartCoroutine(this.LoadAndEnable(string.Empty, false));
		}
	}

	private void SwitchVRRig(bool enable)
	{
		Debug.Log("Switching vr rig enable = " + enable);
		ForestVR.Enabled = enable;
	}

	private IEnumerator LoadAndEnable(string deviceName, bool enable)
	{
		Debug.Log(string.Concat(new object[]
		{
			"Loading and enabling ",
			deviceName,
			" enable = ",
			enable
		}));
		if (VRSettings.loadedDeviceName == deviceName)
		{
			yield break;
		}
		VRSettings.LoadDeviceByName(deviceName);
		yield return null;
		VRSettings.enabled = enable;
		if (enable)
		{
			this.SwitchVRRig(true);
			this.OnEnabled.Invoke();
		}
		else
		{
			this.SwitchVRRig(false);
			this.OnDisabled.Invoke();
		}
		yield break;
	}

	private void OnGUI()
	{
	}

	[Header("Input")]
	public bool RunOnAwake;

	[Header("Output")]
	public UnityEvent OnEnabled;

	public UnityEvent OnDisabled;

	public bool ShowGUI;
}
