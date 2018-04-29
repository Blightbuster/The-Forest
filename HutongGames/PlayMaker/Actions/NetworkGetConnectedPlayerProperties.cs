using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Get connected player properties.")]
	public class NetworkGetConnectedPlayerProperties : FsmStateAction
	{
		
		public override void Reset()
		{
			this.index = null;
			this.IpAddress = null;
			this.port = null;
			this.guid = null;
			this.externalIPAddress = null;
			this.externalPort = null;
		}

		
		public override void OnEnter()
		{
			this.getPlayerProperties();
			base.Finish();
		}

		
		private void getPlayerProperties()
		{
			int value = this.index.Value;
			if (value < 0 || value >= Network.connections.Length)
			{
				base.LogError("Player index out of range");
				return;
			}
			NetworkPlayer networkPlayer = Network.connections[value];
			this.IpAddress.Value = networkPlayer.ipAddress;
			this.port.Value = networkPlayer.port;
			this.guid.Value = networkPlayer.guid;
			this.externalIPAddress.Value = networkPlayer.externalIP;
			this.externalPort.Value = networkPlayer.externalPort;
		}

		
		[RequiredField]
		[Tooltip("The player connection index.")]
		public FsmInt index;

		
		[ActionSection("Result")]
		[Tooltip("Get the IP address of this player.")]
		[UIHint(UIHint.Variable)]
		public FsmString IpAddress;

		
		[Tooltip("Get the port of this player.")]
		[UIHint(UIHint.Variable)]
		public FsmInt port;

		
		[Tooltip("Get the GUID for this player, used when connecting with NAT punchthrough.")]
		[UIHint(UIHint.Variable)]
		public FsmString guid;

		
		[Tooltip("Get the external IP address of the network interface. This will only be populated after some external connection has been made.")]
		[UIHint(UIHint.Variable)]
		public FsmString externalIPAddress;

		
		[Tooltip("Get the external port of the network interface. This will only be populated after some external connection has been made.")]
		[UIHint(UIHint.Variable)]
		public FsmInt externalPort;
	}
}
