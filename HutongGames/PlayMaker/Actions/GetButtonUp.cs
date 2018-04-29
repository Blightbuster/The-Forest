using System;
using TheForest.Utils;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sends an Event when a Button is released.")]
	[ActionCategory(ActionCategory.Input)]
	public class GetButtonUp : FsmStateAction
	{
		
		public override void Reset()
		{
			this.buttonName = "Fire1";
			this.sendEvent = null;
			this.storeResult = null;
		}

		
		public override void OnUpdate()
		{
			bool buttonUp = Input.GetButtonUp(this.buttonName.Value);
			if (buttonUp)
			{
				base.Fsm.Event(this.sendEvent);
			}
			this.storeResult.Value = buttonUp;
		}

		
		[Tooltip("The name of the button. Set in the Unity Input Manager.")]
		[RequiredField]
		public FsmString buttonName;

		
		[Tooltip("Event to send if the button is released.")]
		public FsmEvent sendEvent;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Set to True if the button is released.")]
		public FsmBool storeResult;
	}
}
