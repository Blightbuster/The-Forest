using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Get the maximum amount of connections/players allowed.")]
	[ActionCategory(ActionCategory.Network)]
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

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Get the maximum amount of connections/players allowed.")]
		public FsmInt result;
	}
}
