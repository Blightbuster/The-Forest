using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Gets the event that caused the transition to the current state, and stores it in a String Variable.")]
	[ActionCategory(ActionCategory.StateMachine)]
	public class GetLastEvent : FsmStateAction
	{
		
		public override void Reset()
		{
			this.storeEvent = null;
		}

		
		public override void OnEnter()
		{
			this.storeEvent.Value = ((base.Fsm.LastTransition != null) ? base.Fsm.LastTransition.EventName : "START");
			base.Finish();
		}

		
		[UIHint(UIHint.Variable)]
		public FsmString storeEvent;
	}
}
