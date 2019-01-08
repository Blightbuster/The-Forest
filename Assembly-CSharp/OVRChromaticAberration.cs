using System;
using UnityEngine;

public class OVRChromaticAberration : MonoBehaviour
{
	private void Start()
	{
		OVRManager.instance.chromatic = this.chromatic;
	}

	private void Update()
	{
		if (OVRInput.GetDown(this.toggleButton, OVRInput.Controller.Active))
		{
			this.chromatic = !this.chromatic;
			OVRManager.instance.chromatic = this.chromatic;
		}
	}

	public OVRInput.RawButton toggleButton = OVRInput.RawButton.X;

	private bool chromatic;
}
