using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Get the local network player properties")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkGetLocalPlayerProperties : FsmStateAction
	{
		
		public override void Reset()
		{
			this.IpAddress = null;
			this.port = null;
			this.guid = null;
			this.externalIPAddress = null;
			this.externalPort = null;
		}

		
		public override void OnEnter()
		{
			this.IpAddress.Value = Network.player.ipAddress;
			this.port.Value = Network.player.port;
			this.guid.Value = Network.player.guid;
			this.externalIPAddress.Value = Network.player.externalIP;
			this.externalPort.Value = Network.player.externalPort;
			base.Finish();
		}

		
		[UIHint(UIHint.Variable)]
		[Tooltip("The IP address of this player.")]
		public FsmString IpAddress;

		
		[Tooltip("The port of this player.")]
		[UIHint(UIHint.Variable)]
		public FsmInt port;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("The GUID for this player, used when connecting with NAT punchthrough.")]
		public FsmString guid;

		
		[Tooltip("The external IP address of the network interface. This will only be populated after some external connection has been made.")]
		[UIHint(UIHint.Variable)]
		public FsmString externalIPAddress;

		
		[Tooltip("Returns the external port of the network interface. This will only be populated after some external connection has been made.")]
		[UIHint(UIHint.Variable)]
		public FsmInt externalPort;
	}
}
