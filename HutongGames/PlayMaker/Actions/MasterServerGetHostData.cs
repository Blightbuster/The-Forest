using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Get host data from the master server.")]
	public class MasterServerGetHostData : FsmStateAction
	{
		
		public override void Reset()
		{
			this.hostIndex = null;
			this.useNat = null;
			this.gameType = null;
			this.gameName = null;
			this.connectedPlayers = null;
			this.playerLimit = null;
			this.ipAddress = null;
			this.port = null;
			this.passwordProtected = null;
			this.comment = null;
			this.guid = null;
		}

		
		public override void OnEnter()
		{
			this.GetHostData();
			base.Finish();
		}

		
		private void GetHostData()
		{
			int num = MasterServer.PollHostList().Length;
			int value = this.hostIndex.Value;
			if (value < 0 || value >= num)
			{
				base.LogError("MasterServer Host index out of range!");
				return;
			}
			HostData hostData = MasterServer.PollHostList()[value];
			if (hostData == null)
			{
				base.LogError("MasterServer HostData could not found at index " + value);
				return;
			}
			this.useNat.Value = hostData.useNat;
			this.gameType.Value = hostData.gameType;
			this.gameName.Value = hostData.gameName;
			this.connectedPlayers.Value = hostData.connectedPlayers;
			this.playerLimit.Value = hostData.playerLimit;
			this.ipAddress.Value = hostData.ip[0];
			this.port.Value = hostData.port;
			this.passwordProtected.Value = hostData.passwordProtected;
			this.comment.Value = hostData.comment;
			this.guid.Value = hostData.guid;
		}

		
		[RequiredField]
		[Tooltip("The index into the MasterServer Host List")]
		public FsmInt hostIndex;

		
		[Tooltip("Does this server require NAT punchthrough?")]
		[UIHint(UIHint.Variable)]
		public FsmBool useNat;

		
		[Tooltip("The type of the game (e.g., 'MyUniqueGameType')")]
		[UIHint(UIHint.Variable)]
		public FsmString gameType;

		
		[Tooltip("The name of the game (e.g., 'John Does's Game')")]
		[UIHint(UIHint.Variable)]
		public FsmString gameName;

		
		[Tooltip("Currently connected players")]
		[UIHint(UIHint.Variable)]
		public FsmInt connectedPlayers;

		
		[Tooltip("Maximum players limit")]
		[UIHint(UIHint.Variable)]
		public FsmInt playerLimit;

		
		[Tooltip("Server IP address.")]
		[UIHint(UIHint.Variable)]
		public FsmString ipAddress;

		
		[Tooltip("Server port")]
		[UIHint(UIHint.Variable)]
		public FsmInt port;

		
		[Tooltip("Does the server require a password?")]
		[UIHint(UIHint.Variable)]
		public FsmBool passwordProtected;

		
		[Tooltip("A miscellaneous comment (can hold data)")]
		[UIHint(UIHint.Variable)]
		public FsmString comment;

		
		[Tooltip("The GUID of the host, needed when connecting with NAT punchthrough.")]
		[UIHint(UIHint.Variable)]
		public FsmString guid;
	}
}
