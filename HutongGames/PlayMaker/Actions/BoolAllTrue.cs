using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Tests if all the given Bool Variables are True.")]
	[ActionCategory(ActionCategory.Logic)]
	public class BoolAllTrue : FsmStateAction
	{
		
		public override void Reset()
		{
			this.boolVariables = null;
			this.sendEvent = null;
			this.storeResult = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoAllTrue();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoAllTrue();
		}

		
		private void DoAllTrue()
		{
			if (this.boolVariables.Length == 0)
			{
				return;
			}
			bool flag = true;
			for (int i = 0; i < this.boolVariables.Length; i++)
			{
				if (!this.boolVariables[i].Value)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				base.Fsm.Event(this.sendEvent);
			}
			this.storeResult.Value = flag;
		}

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		[Tooltip("The Bool variables to check.")]
		public FsmBool[] boolVariables;

		
		[Tooltip("Event to send if all the Bool variables are True.")]
		public FsmEvent sendEvent;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result in a Bool variable.")]
		public FsmBool storeResult;

		
		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;
	}
}
