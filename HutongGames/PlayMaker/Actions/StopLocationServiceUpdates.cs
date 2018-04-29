using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Stops location service updates. This could be useful for saving battery life.")]
	[ActionCategory(ActionCategory.Device)]
	public class StopLocationServiceUpdates : FsmStateAction
	{
		
		public override void Reset()
		{
		}

		
		public override void OnEnter()
		{
			base.Finish();
		}
	}
}
