using System;
using UnityEngine;

[Obsolete("Use VRGameObjectSwitcher")]
public class VRGameobjectSwitcherOLD : MonoBehaviour
{
	private void Start()
	{
		if (this.RunOnStart)
		{
			this.switchTargets();
		}
	}

	private void OnEnable()
	{
		this.switchTargets();
	}

	private void OnDisable()
	{
		if (!this.DoDisable)
		{
			return;
		}
		if (this.target != null)
		{
			this.target.SetActive(false);
		}
		if (this.VRtarget != null)
		{
			this.VRtarget.SetActive(false);
		}
	}

	private void switchTargets()
	{
		if (ForestVR.Enabled)
		{
			if (this.VRtarget)
			{
				this.VRtarget.SetActive(true);
			}
			if (this.target)
			{
				this.target.SetActive(false);
			}
		}
		else
		{
			if (this.VRtarget)
			{
				this.VRtarget.SetActive(false);
			}
			if (this.target)
			{
				this.target.SetActive(true);
			}
		}
	}

	public GameObject target;

	public GameObject VRtarget;

	public bool DoDisable;

	public bool RunOnStart;
}
