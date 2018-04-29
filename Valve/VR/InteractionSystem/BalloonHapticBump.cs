using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	public class BalloonHapticBump : MonoBehaviour
	{
		
		private void OnCollisionEnter(Collision other)
		{
			Balloon componentInParent = other.collider.GetComponentInParent<Balloon>();
			if (componentInParent != null)
			{
				Hand componentInParent2 = this.physParent.GetComponentInParent<Hand>();
				if (componentInParent2 != null)
				{
					componentInParent2.controller.TriggerHapticPulse(500, EVRButtonId.k_EButton_Axis0);
				}
			}
		}

		
		public GameObject physParent;
	}
}
