using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Set the send rate for all networkViews. Default is 15")]
	public class NetworkSetSendRate : FsmStateAction
	{
		
		public override void Reset()
		{
			this.sendRate = 15f;
		}

		
		public override void OnEnter()
		{
			this.DoSetSendRate();
			base.Finish();
		}

		
		private void DoSetSendRate()
		{
			Network.sendRate = this.sendRate.Value;
		}

		
		[RequiredField]
		[Tooltip("The send rate for all networkViews")]
		public FsmFloat sendRate;
	}
}
