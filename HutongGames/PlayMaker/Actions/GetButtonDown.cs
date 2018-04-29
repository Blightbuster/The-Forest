using System;
using TheForest.Utils;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sends an Event when a Button is pressed.")]
	[ActionCategory(ActionCategory.Input)]
	public class GetButtonDown : FsmStateAction
	{
		
		public override void Reset()
		{
			this.buttonName = "Fire1";
			this.sendEvent = null;
			this.storeResult = null;
		}

		
		public override void OnUpdate()
		{
			bool buttonDown = Input.GetButtonDown(this.buttonName.Value);
			if (buttonDown)
			{
				base.Fsm.Event(this.sendEvent);
			}
			this.storeResult.Value = buttonDown;
		}

		
		[Tooltip("The name of the button. Set in the Unity Input Manager.")]
		[RequiredField]
		public FsmString buttonName;

		
		[Tooltip("Event to send if the button is pressed.")]
		public FsmEvent sendEvent;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Set to True if the button is pressed.")]
		public FsmBool storeResult;
	}
}
