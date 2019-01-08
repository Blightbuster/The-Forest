using System;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;

public class VRGameObjectSwitcher : MonoBehaviour
{
	private void Start()
	{
		if (this.RunOnStart)
		{
			this.SwitchTargets();
		}
	}

	private void OnEnable()
	{
		if (!this.RunOnStart)
		{
			this.SwitchTargets();
		}
	}

	private void OnDisable()
	{
		if (!this.DoDisable)
		{
			return;
		}
		this.DefaultTargets.SetActiveSafe(false);
		this.VRTargets.SetActiveSafe(false);
	}

	private void SwitchTargets()
	{
		this.DefaultTargets.SetActiveSafe(!ForestVR.Enabled);
		this.VRTargets.SetActiveSafe(ForestVR.Enabled);
	}

	public List<GameObject> DefaultTargets;

	public List<GameObject> VRTargets;

	public bool DoDisable;

	public bool RunOnStart;
}
