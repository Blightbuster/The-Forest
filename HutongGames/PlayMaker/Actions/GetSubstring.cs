using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Gets a sub-string from a String Variable.")]
	[ActionCategory(ActionCategory.String)]
	public class GetSubstring : FsmStateAction
	{
		
		public override void Reset()
		{
			this.stringVariable = null;
			this.startIndex = 0;
			this.length = 1;
			this.storeResult = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoGetSubstring();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoGetSubstring();
		}

		
		private void DoGetSubstring()
		{
			if (this.stringVariable == null)
			{
				return;
			}
			if (this.storeResult == null)
			{
				return;
			}
			this.storeResult.Value = this.stringVariable.Value.Substring(this.startIndex.Value, this.length.Value);
		}

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmString stringVariable;

		
		[RequiredField]
		public FsmInt startIndex;

		
		[RequiredField]
		public FsmInt length;

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmString storeResult;

		
		public bool everyFrame;
	}
}
