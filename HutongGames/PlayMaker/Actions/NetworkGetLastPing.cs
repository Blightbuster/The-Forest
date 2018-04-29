﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Get the last ping time to the given player in milliseconds. \nIf the player can't be found -1 will be returned. Pings are automatically sent out every couple of seconds.")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkGetLastPing : FsmStateAction
	{
		
		public override void Reset()
		{
			this.playerIndex = null;
			this.lastPing = null;
			this.PlayerNotFoundEvent = null;
			this.PlayerFoundEvent = null;
			this.cachePlayerReference = true;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			if (this.cachePlayerReference)
			{
				this._player = Network.connections[this.playerIndex.Value];
			}
			this.GetLastPing();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.GetLastPing();
		}

		
		private void GetLastPing()
		{
			if (!this.cachePlayerReference)
			{
				this._player = Network.connections[this.playerIndex.Value];
			}
			int num = Network.GetLastPing(this._player);
			this.lastPing.Value = num;
			if (num == -1 && this.PlayerNotFoundEvent != null)
			{
				base.Fsm.Event(this.PlayerNotFoundEvent);
			}
			if (num != -1 && this.PlayerFoundEvent != null)
			{
				base.Fsm.Event(this.PlayerFoundEvent);
			}
		}

		
		[ActionSection("Setup")]
		[UIHint(UIHint.Variable)]
		[RequiredField]
		[Tooltip("The Index of the player in the network connections list.")]
		public FsmInt playerIndex;

		
		[Tooltip("The player reference is cached, that is if the connections list changes, the player reference remains.")]
		public bool cachePlayerReference = true;

		
		public bool everyFrame;

		
		[ActionSection("Result")]
		[RequiredField]
		[Tooltip("Get the last ping time to the given player in milliseconds.")]
		[UIHint(UIHint.Variable)]
		public FsmInt lastPing;

		
		[Tooltip("Event to send if the player can't be found. Average Ping is set to -1.")]
		public FsmEvent PlayerNotFoundEvent;

		
		[Tooltip("Event to send if the player is found (pings back).")]
		public FsmEvent PlayerFoundEvent;

		
		private NetworkPlayer _player;
	}
}
