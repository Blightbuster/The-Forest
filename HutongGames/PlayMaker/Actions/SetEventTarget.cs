using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sets the target FSM for all subsequent events sent by this state. The default 'Self' sends events to this FSM.")]
	[ActionCategory(ActionCategory.StateMachine)]
	public class SetEventTarget : FsmStateAction
	{
		
		public override void Reset()
		{
		}

		
		public override void OnEnter()
		{
			base.Fsm.EventTarget = this.eventTarget;
			base.Finish();
		}

		
		public FsmEventTarget eventTarget;
	}
}
