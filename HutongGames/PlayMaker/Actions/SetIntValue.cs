using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sets the value of an Integer Variable.")]
	[ActionCategory(ActionCategory.Math)]
	public class SetIntValue : FsmStateAction
	{
		
		public override void Reset()
		{
			this.intVariable = null;
			this.intValue = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.intVariable.Value = this.intValue.Value;
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.intVariable.Value = this.intValue.Value;
		}

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmInt intVariable;

		
		[RequiredField]
		public FsmInt intValue;

		
		public bool everyFrame;
	}
}
