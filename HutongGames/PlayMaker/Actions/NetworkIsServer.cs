using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Test if your peer type is server.")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkIsServer : FsmStateAction
	{
		
		public override void Reset()
		{
			this.isServer = null;
		}

		
		public override void OnEnter()
		{
			this.DoCheckIsServer();
			base.Finish();
		}

		
		private void DoCheckIsServer()
		{
			this.isServer.Value = Network.isServer;
			if (Network.isServer && this.isServerEvent != null)
			{
				base.Fsm.Event(this.isServerEvent);
			}
			else if (!Network.isServer && this.isNotServerEvent != null)
			{
				base.Fsm.Event(this.isNotServerEvent);
			}
		}

		
		[Tooltip("True if running as server.")]
		[UIHint(UIHint.Variable)]
		public FsmBool isServer;

		
		[Tooltip("Event to send if running as server.")]
		public FsmEvent isServerEvent;

		
		[Tooltip("Event to send if not running as server.")]
		public FsmEvent isNotServerEvent;
	}
}
