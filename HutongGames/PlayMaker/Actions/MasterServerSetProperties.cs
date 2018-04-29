using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Set the IP address, port, update rate and dedicated server flag of the master server.")]
	public class MasterServerSetProperties : FsmStateAction
	{
		
		public override void Reset()
		{
			this.ipAddress = "127.0.0.1";
			this.port = 10002;
			this.updateRate = 60;
			this.dedicatedServer = false;
		}

		
		public override void OnEnter()
		{
			this.SetMasterServerProperties();
			base.Finish();
		}

		
		private void SetMasterServerProperties()
		{
			MasterServer.ipAddress = this.ipAddress.Value;
			MasterServer.port = this.port.Value;
			MasterServer.updateRate = this.updateRate.Value;
			MasterServer.dedicatedServer = this.dedicatedServer.Value;
		}

		
		[Tooltip("Set the IP address of the master server.")]
		public FsmString ipAddress;

		
		[Tooltip("Set the connection port of the master server.")]
		public FsmInt port;

		
		[Tooltip("Set the minimum update rate for master server host information update. Default is 60 seconds.")]
		public FsmInt updateRate;

		
		[Tooltip("Set if this machine is a dedicated server.")]
		public FsmBool dedicatedServer;
	}
}
