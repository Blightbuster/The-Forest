using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	public class IgnoreHovering : MonoBehaviour
	{
		
		[Tooltip("If Hand is not null, only ignore the specified hand")]
		public Hand onlyIgnoreHand;
	}
}
