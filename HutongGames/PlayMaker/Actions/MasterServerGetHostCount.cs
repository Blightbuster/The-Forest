using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Get the number of hosts on the master server.\n\nUse MasterServer Get Host Data to get host data at a specific index.")]
	[ActionCategory(ActionCategory.Network)]
	public class MasterServerGetHostCount : FsmStateAction
	{
		
		public override void OnEnter()
		{
			this.count.Value = MasterServer.PollHostList().Length;
			base.Finish();
		}

		
		[RequiredField]
		[Tooltip("The number of hosts on the MasterServer.")]
		[UIHint(UIHint.Variable)]
		public FsmInt count;
	}
}
