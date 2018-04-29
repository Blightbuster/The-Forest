using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Get if network messages are enabled or disabled.\n\nIf disabled no RPC call execution or network view synchronization takes place")]
	public class NetworkGetIsMessageQueueRunning : FsmStateAction
	{
		
		public override void Reset()
		{
			this.result = null;
		}

		
		public override void OnEnter()
		{
			this.result.Value = Network.isMessageQueueRunning;
			base.Finish();
		}

		
		[Tooltip("Is Message Queue Running. If this is disabled no RPC call execution or network view synchronization takes place")]
		[UIHint(UIHint.Variable)]
		public FsmBool result;
	}
}
