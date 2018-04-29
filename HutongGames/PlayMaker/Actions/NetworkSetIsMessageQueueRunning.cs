using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Enable or disable the processing of network messages.\n\nIf this is disabled no RPC call execution or network view synchronization takes place.")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkSetIsMessageQueueRunning : FsmStateAction
	{
		
		public override void Reset()
		{
			this.isMessageQueueRunning = null;
		}

		
		public override void OnEnter()
		{
			Network.isMessageQueueRunning = this.isMessageQueueRunning.Value;
			base.Finish();
		}

		
		[Tooltip("Is Message Queue Running. If this is disabled no RPC call execution or network view synchronization takes place")]
		public FsmBool isMessageQueueRunning;
	}
}
