using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Store the current send rate for all NetworkViews")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkGetSendRate : FsmStateAction
	{
		
		public override void Reset()
		{
			this.sendRate = null;
		}

		
		public override void OnEnter()
		{
			this.DoGetSendRate();
			base.Finish();
		}

		
		private void DoGetSendRate()
		{
			this.sendRate.Value = Network.sendRate;
		}

		
		[Tooltip("Store the current send rate for NetworkViews")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmFloat sendRate;
	}
}
