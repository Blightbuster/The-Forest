using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Immediately return to the previously active state.")]
	[ActionCategory(ActionCategory.StateMachine)]
	public class GotoPreviousState : FsmStateAction
	{
		
		public override void Reset()
		{
		}

		
		public override void OnEnter()
		{
			if (base.Fsm.PreviousActiveState != null)
			{
				this.Log("Goto Previous State: " + base.Fsm.PreviousActiveState.Name);
				base.Fsm.GotoPreviousState();
			}
			base.Finish();
		}
	}
}
