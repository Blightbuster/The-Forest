using System;
using UnityEngine;

public class VRComponentSwitcher : MonoBehaviour
{
	private void OnEnable()
	{
		this.SwitchTargets();
		if (this.AutoDisableSelf)
		{
			base.enabled = false;
		}
	}

	private void SwitchTargets()
	{
		this.ToggleTargets(this.DefaultTargets, !ForestVR.Enabled);
		this.ToggleTargets(this.VRtargets, ForestVR.Enabled);
	}

	private void ToggleTargets(Component[] targets, bool onoff)
	{
		foreach (Component component in targets)
		{
			if (!(component == null))
			{
				Transform transform = component as Transform;
				if (transform != null)
				{
					transform.gameObject.SetActive(onoff);
				}
				else
				{
					Behaviour behaviour = component as Behaviour;
					if (behaviour != null)
					{
						behaviour.enabled = onoff;
					}
					else
					{
						Collider collider = component as Collider;
						if (collider != null)
						{
							collider.enabled = onoff;
						}
					}
				}
			}
		}
	}

	public Component[] DefaultTargets;

	public Component[] VRtargets;

	public bool AutoDisableSelf = true;
}
