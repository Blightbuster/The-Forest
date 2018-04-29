using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Get the number of connected players.\n\nOn a client this returns 1 (the server).")]
	public class NetworkGetConnectionsCount : FsmStateAction
	{
		
		public override void Reset()
		{
			this.connectionsCount = null;
			this.everyFrame = true;
		}

		
		public override void OnEnter()
		{
			this.connectionsCount.Value = Network.connections.Length;
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.connectionsCount.Value = Network.connections.Length;
		}

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Number of connected players.")]
		public FsmInt connectionsCount;

		
		[Tooltip("Repeat every frame.")]
		public bool everyFrame;
	}
}
