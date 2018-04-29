using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Gets the Length of a String.")]
	[ActionCategory(ActionCategory.String)]
	public class GetStringLength : FsmStateAction
	{
		
		public override void Reset()
		{
			this.stringVariable = null;
			this.storeResult = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoGetStringLength();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoGetStringLength();
		}

		
		private void DoGetStringLength()
		{
			if (this.stringVariable == null)
			{
				return;
			}
			if (this.storeResult == null)
			{
				return;
			}
			this.storeResult.Value = this.stringVariable.Value.Length;
		}

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmString stringVariable;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmInt storeResult;

		
		public bool everyFrame;
	}
}
