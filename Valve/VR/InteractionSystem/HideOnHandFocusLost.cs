using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	public class HideOnHandFocusLost : MonoBehaviour
	{
		
		private void OnHandFocusLost(Hand hand)
		{
			base.gameObject.SetActive(false);
		}
	}
}
