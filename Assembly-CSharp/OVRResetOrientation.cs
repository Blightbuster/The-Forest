using System;
using UnityEngine;

public class OVRResetOrientation : MonoBehaviour
{
	private void Update()
	{
		if (OVRInput.GetDown(this.resetButton, OVRInput.Controller.Active))
		{
			OVRManager.display.RecenterPose();
		}
	}

	public OVRInput.RawButton resetButton = OVRInput.RawButton.Y;
}
