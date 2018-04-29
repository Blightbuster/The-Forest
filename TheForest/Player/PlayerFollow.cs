using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player
{
	
	public class PlayerFollow : MonoBehaviour
	{
		
		private void Awake()
		{
			if (CoopPeerStarter.DedicatedHost)
			{
				base.enabled = false;
			}
		}

		
		private void LateUpdate()
		{
			if (LocalPlayer.Transform)
			{
				base.transform.position = LocalPlayer.Transform.position;
			}
		}
	}
}
