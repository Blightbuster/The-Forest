using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Forward an event recieved by this FSM to another target.")]
	[ActionCategory(ActionCategory.StateMachine)]
	public class ForwardEvent : FsmStateAction
	{
		
		public override void Reset()
		{
			this.forwardTo = new FsmEventTarget
			{
				target = FsmEventTarget.EventTarget.FSMComponent
			};
			this.eventsToForward = null;
			this.eatEvents = true;
		}

		
		public override bool Event(FsmEvent fsmEvent)
		{
			if (this.eventsToForward != null)
			{
				foreach (FsmEvent fsmEvent2 in this.eventsToForward)
				{
					if (fsmEvent2 == fsmEvent)
					{
						base.Fsm.Event(this.forwardTo, fsmEvent);
						return this.eatEvents;
					}
				}
			}
			return false;
		}

		
		[Tooltip("Forward to this target.")]
		public FsmEventTarget forwardTo;

		
		[Tooltip("The events to forward.")]
		public FsmEvent[] eventsToForward;

		
		[Tooltip("Should this action eat the events or pass them on.")]
		public bool eatEvents;
	}
}
