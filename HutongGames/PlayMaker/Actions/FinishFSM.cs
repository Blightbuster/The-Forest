using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Stop this FSM. If this FSM was launched by a Run FSM action, it will trigger a Finish event in that state.")]
	[Note("Stop this FSM. If this FSM was launched by a Run FSM action, it will trigger a Finish event in that state.")]
	[ActionCategory(ActionCategory.StateMachine)]
	public class FinishFSM : FsmStateAction
	{
		
		public override void OnEnter()
		{
			base.Fsm.Stop();
		}
	}
}
