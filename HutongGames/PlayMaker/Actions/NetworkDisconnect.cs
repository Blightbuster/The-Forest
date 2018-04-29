using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Disconnect from the server.")]
	public class NetworkDisconnect : FsmStateAction
	{
		
		public override void OnEnter()
		{
			Network.Disconnect();
			base.Finish();
		}
	}
}
