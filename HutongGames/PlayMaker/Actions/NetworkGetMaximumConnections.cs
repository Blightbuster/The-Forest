using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Get the maximum amount of connections/players allowed.")]
	public class NetworkGetMaximumConnections : FsmStateAction
	{
		
		public override void Reset()
		{
			this.result = null;
		}

		
		public override void OnEnter()
		{
			this.result.Value = Network.maxConnections;
			base.Finish();
		}

		
		[Tooltip("Get the maximum amount of connections/players allowed.")]
		[UIHint(UIHint.Variable)]
		public FsmInt result;
	}
}
