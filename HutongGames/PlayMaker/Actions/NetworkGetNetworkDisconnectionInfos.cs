using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Get the network OnDisconnectedFromServer.")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkGetNetworkDisconnectionInfos : FsmStateAction
	{
		
		public override void Reset()
		{
			this.disconnectionLabel = null;
			this.lostConnectionEvent = null;
			this.disConnectedEvent = null;
		}

		
		public override void OnEnter()
		{
			this.doGetNetworkDisconnectionInfo();
			base.Finish();
		}

		
		private void doGetNetworkDisconnectionInfo()
		{
			NetworkDisconnection disconnectionInfo = Fsm.EventData.DisconnectionInfo;
			this.disconnectionLabel.Value = disconnectionInfo.ToString();
			NetworkDisconnection networkDisconnection = disconnectionInfo;
			if (networkDisconnection != NetworkDisconnection.Disconnected)
			{
				if (networkDisconnection == NetworkDisconnection.LostConnection)
				{
					if (this.lostConnectionEvent != null)
					{
						base.Fsm.Event(this.lostConnectionEvent);
					}
				}
			}
			else if (this.disConnectedEvent != null)
			{
				base.Fsm.Event(this.disConnectedEvent);
			}
		}

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Disconnection label")]
		public FsmString disconnectionLabel;

		
		[Tooltip("The connection to the system has been lost, no reliable packets could be delivered.")]
		public FsmEvent lostConnectionEvent;

		
		[Tooltip("The connection to the system has been closed.")]
		public FsmEvent disConnectedEvent;
	}
}
