using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Unregister this server from the master server.\n\nDoes nothing if the server is not registered or has already unregistered.")]
	[ActionCategory(ActionCategory.Network)]
	public class MasterServerUnregisterHost : FsmStateAction
	{
		
		public override void OnEnter()
		{
			MasterServer.UnregisterHost();
			base.Finish();
		}
	}
}
