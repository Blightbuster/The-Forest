using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Connect to a server.")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkConnect : FsmStateAction
	{
		
		public override void Reset()
		{
			this.remoteIP = "127.0.0.1";
			this.remotePort = 25001;
			this.password = string.Empty;
			this.errorEvent = null;
			this.errorString = null;
		}

		
		public override void OnEnter()
		{
			NetworkConnectionError networkConnectionError = Network.Connect(this.remoteIP.Value, this.remotePort.Value, this.password.Value);
			if (networkConnectionError != NetworkConnectionError.NoError)
			{
				this.errorString.Value = networkConnectionError.ToString();
				this.LogError(this.errorString.Value);
				base.Fsm.Event(this.errorEvent);
			}
			base.Finish();
		}

		
		[RequiredField]
		[Tooltip("IP address of the host. Either a dotted IP address or a domain name.")]
		public FsmString remoteIP;

		
		[Tooltip("The port on the remote machine to connect to.")]
		[RequiredField]
		public FsmInt remotePort;

		
		[Tooltip("Optional password for the server.")]
		public FsmString password;

		
		[ActionSection("Errors")]
		[Tooltip("Event to send in case of an error connecting to the server.")]
		public FsmEvent errorEvent;

		
		[Tooltip("Store the error string in a variable.")]
		[UIHint(UIHint.Variable)]
		public FsmString errorString;
	}
}
