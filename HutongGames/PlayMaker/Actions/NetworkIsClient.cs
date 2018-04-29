using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Test if your peer type is client.")]
	public class NetworkIsClient : FsmStateAction
	{
		
		public override void Reset()
		{
			this.isClient = null;
		}

		
		public override void OnEnter()
		{
			this.DoCheckIsClient();
			base.Finish();
		}

		
		private void DoCheckIsClient()
		{
			this.isClient.Value = Network.isClient;
			if (Network.isClient && this.isClientEvent != null)
			{
				base.Fsm.Event(this.isClientEvent);
			}
			else if (!Network.isClient && this.isNotClientEvent != null)
			{
				base.Fsm.Event(this.isNotClientEvent);
			}
		}

		
		[UIHint(UIHint.Variable)]
		[Tooltip("True if running as client.")]
		public FsmBool isClient;

		
		[Tooltip("Event to send if running as client.")]
		public FsmEvent isClientEvent;

		
		[Tooltip("Event to send if not running as client.")]
		public FsmEvent isNotClientEvent;
	}
}
