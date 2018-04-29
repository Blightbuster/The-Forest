using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sends an Event in the next frame. Useful if you want to loop states every frame.")]
	[ActionCategory(ActionCategory.StateMachine)]
	public class NextFrameEvent : FsmStateAction
	{
		
		public override void Reset()
		{
			this.sendEvent = null;
		}

		
		public override void OnEnter()
		{
		}

		
		public override void OnUpdate()
		{
			base.Finish();
			base.Fsm.Event(this.sendEvent);
		}

		
		[RequiredField]
		public FsmEvent sendEvent;
	}
}
