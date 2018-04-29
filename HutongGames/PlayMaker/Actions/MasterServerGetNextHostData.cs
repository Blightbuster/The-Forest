using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Get the next host data from the master server. \nEach time this action is called it gets the next connected host.This lets you quickly loop through all the connected hosts to get information on each one.")]
	public class MasterServerGetNextHostData : FsmStateAction
	{
		
		public override void Reset()
		{
			this.finishedEvent = null;
			this.loopEvent = null;
			this.index = null;
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
			this.DoGetNextHostData();
			base.Finish();
		}

		
		private void DoGetNextHostData()
		{
			if (this.nextItemIndex >= MasterServer.PollHostList().Length)
			{
				this.nextItemIndex = 0;
				base.Fsm.Event(this.finishedEvent);
				return;
			}
			HostData hostData = MasterServer.PollHostList()[this.nextItemIndex];
			this.index.Value = this.nextItemIndex;
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
			if (this.nextItemIndex >= MasterServer.PollHostList().Length)
			{
				base.Fsm.Event(this.finishedEvent);
				this.nextItemIndex = 0;
				return;
			}
			this.nextItemIndex++;
			if (this.loopEvent != null)
			{
				base.Fsm.Event(this.loopEvent);
			}
		}

		
		[Tooltip("Event to send for looping.")]
		[ActionSection("Set up")]
		public FsmEvent loopEvent;

		
		[Tooltip("Event to send when there are no more hosts.")]
		public FsmEvent finishedEvent;

		
		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The index into the MasterServer Host List")]
		public FsmInt index;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Does this server require NAT punchthrough?")]
		public FsmBool useNat;

		
		[Tooltip("The type of the game (e.g., 'MyUniqueGameType')")]
		[UIHint(UIHint.Variable)]
		public FsmString gameType;

		
		[Tooltip("The name of the game (e.g., 'John Does's Game')")]
		[UIHint(UIHint.Variable)]
		public FsmString gameName;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Currently connected players")]
		public FsmInt connectedPlayers;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Maximum players limit")]
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

		
		[UIHint(UIHint.Variable)]
		[Tooltip("A miscellaneous comment (can hold data)")]
		public FsmString comment;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("The GUID of the host, needed when connecting with NAT punchthrough.")]
		public FsmString guid;

		
		private int nextItemIndex;

		
		private bool noMoreItems;
	}
}
