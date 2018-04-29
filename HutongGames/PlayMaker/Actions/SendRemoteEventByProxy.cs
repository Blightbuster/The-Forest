﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Send an Fsm Event on a remote machine. Uses Unity RPC functions. Use this instead of SendRemoteEvent if you have multiple PlayMakerFSM components on the GameObject that you want to recieve the event.")]
	public class SendRemoteEventByProxy : ComponentAction<NetworkView>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.remoteEvent = null;
			this.mode = RPCMode.All;
			this.stringData = null;
			this.mode = RPCMode.All;
		}

		
		public override void OnEnter()
		{
			this.DoRPC();
			base.Finish();
		}

		
		private void DoRPC()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (!base.UpdateCache(ownerDefaultTarget))
			{
				return;
			}
			if (!this.stringData.IsNone && this.stringData.Value != string.Empty)
			{
				base.networkView.RPC("ForwardEvent", this.mode, new object[]
				{
					this.remoteEvent.Name,
					this.stringData.Value
				});
			}
			else
			{
				base.networkView.RPC("ForwardEvent", this.mode, new object[]
				{
					this.remoteEvent.Name
				});
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(NetworkView), typeof(PlayMakerRPCProxy))]
		[Tooltip("The game object that sends the event.")]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		[Tooltip("The event you want to send.")]
		public FsmEvent remoteEvent;

		
		[Tooltip("Optional string data. Use 'Get Event Info' action to retrieve it.")]
		public FsmString stringData;

		
		[Tooltip("Option for who will receive an RPC.")]
		public RPCMode mode;
	}
}
