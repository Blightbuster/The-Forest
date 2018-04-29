using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Tests if a String contains another String.")]
	[ActionCategory(ActionCategory.Logic)]
	public class StringContains : FsmStateAction
	{
		
		public override void Reset()
		{
			this.stringVariable = null;
			this.containsString = string.Empty;
			this.trueEvent = null;
			this.falseEvent = null;
			this.storeResult = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoStringContains();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoStringContains();
		}

		
		private void DoStringContains()
		{
			if (this.stringVariable.IsNone || this.containsString.IsNone)
			{
				return;
			}
			bool flag = this.stringVariable.Value.Contains(this.containsString.Value);
			if (this.storeResult != null)
			{
				this.storeResult.Value = flag;
			}
			if (flag && this.trueEvent != null)
			{
				base.Fsm.Event(this.trueEvent);
				return;
			}
			if (!flag && this.falseEvent != null)
			{
				base.Fsm.Event(this.falseEvent);
			}
		}

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		[Tooltip("The String variable to test.")]
		public FsmString stringVariable;

		
		[RequiredField]
		[Tooltip("Test if the String variable contains this string.")]
		public FsmString containsString;

		
		[Tooltip("Event to send if true.")]
		public FsmEvent trueEvent;

		
		[Tooltip("Event to send if false.")]
		public FsmEvent falseEvent;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the true/false result in a bool variable.")]
		public FsmBool storeResult;

		
		[Tooltip("Repeat every frame. Useful if any of the strings are changing over time.")]
		public bool everyFrame;
	}
}
