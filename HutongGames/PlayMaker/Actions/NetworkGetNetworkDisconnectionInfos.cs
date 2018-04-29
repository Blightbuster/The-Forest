using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Get the network OnDisconnectedFromServer.")]
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
			if (disconnectionInfo != NetworkDisconnection.Disconnected)
			{
				if (disconnectionInfo == NetworkDisconnection.LostConnection)
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

		
		[Tooltip("Disconnection label")]
		[UIHint(UIHint.Variable)]
		public FsmString disconnectionLabel;

		
		[Tooltip("The connection to the system has been lost, no reliable packets could be delivered.")]
		public FsmEvent lostConnectionEvent;

		
		[Tooltip("The connection to the system has been closed.")]
		public FsmEvent disConnectedEvent;
	}
}
