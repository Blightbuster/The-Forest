using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Get the IP address, port, update rate and dedicated server flag of the master server and store in variables.")]
	[ActionCategory(ActionCategory.Network)]
	public class MasterServerGetProperties : FsmStateAction
	{
		
		public override void Reset()
		{
			this.ipAddress = null;
			this.port = null;
			this.updateRate = null;
			this.dedicatedServer = null;
			this.isDedicatedServerEvent = null;
			this.isNotDedicatedServerEvent = null;
		}

		
		public override void OnEnter()
		{
			this.GetMasterServerProperties();
			base.Finish();
		}

		
		private void GetMasterServerProperties()
		{
			this.ipAddress.Value = MasterServer.ipAddress;
			this.port.Value = MasterServer.port;
			this.updateRate.Value = MasterServer.updateRate;
			bool flag = MasterServer.dedicatedServer;
			this.dedicatedServer.Value = flag;
			if (flag && this.isDedicatedServerEvent != null)
			{
				base.Fsm.Event(this.isDedicatedServerEvent);
			}
			if (!flag && this.isNotDedicatedServerEvent != null)
			{
				base.Fsm.Event(this.isNotDedicatedServerEvent);
			}
		}

		
		[Tooltip("The IP address of the master server.")]
		[UIHint(UIHint.Variable)]
		public FsmString ipAddress;

		
		[Tooltip("The connection port of the master server.")]
		[UIHint(UIHint.Variable)]
		public FsmInt port;

		
		[Tooltip("The minimum update rate for master server host information update. Default is 60 seconds")]
		[UIHint(UIHint.Variable)]
		public FsmInt updateRate;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Flag to report if this machine is a dedicated server.")]
		public FsmBool dedicatedServer;

		
		[Tooltip("Event sent if this machine is a dedicated server")]
		public FsmEvent isDedicatedServerEvent;

		
		[Tooltip("Event sent if this machine is not a dedicated server")]
		public FsmEvent isNotDedicatedServerEvent;
	}
}
