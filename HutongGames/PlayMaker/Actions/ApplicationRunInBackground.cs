using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sets if the Application should play in the background. Useful for servers or testing network games on one machine.")]
	[ActionCategory(ActionCategory.Application)]
	public class ApplicationRunInBackground : FsmStateAction
	{
		
		public override void Reset()
		{
			this.runInBackground = true;
		}

		
		public override void OnEnter()
		{
			Application.runInBackground = this.runInBackground.Value;
			base.Finish();
		}

		
		public FsmBool runInBackground;
	}
}
