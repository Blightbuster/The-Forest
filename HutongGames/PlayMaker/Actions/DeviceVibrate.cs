using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Causes the device to vibrate for half a second.")]
	[ActionCategory(ActionCategory.Device)]
	public class DeviceVibrate : FsmStateAction
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
