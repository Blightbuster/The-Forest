using System;
using UnityEngine;


public class VRGameobjectSwitcher : MonoBehaviour
{
	
	private void OnEnable()
	{
		this.switchTargets();
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
}
