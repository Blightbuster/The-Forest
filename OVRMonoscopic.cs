using System;
using UnityEngine;


public class OVRMonoscopic : MonoBehaviour
{
	
	private void Update()
	{
		if (OVRInput.GetDown(this.toggleButton, OVRInput.Controller.Active))
		{
			this.monoscopic = !this.monoscopic;
			OVRManager.instance.monoscopic = this.monoscopic;
		}
	}

	
	public OVRInput.RawButton toggleButton = OVRInput.RawButton.B;

	
	private bool monoscopic;
}
