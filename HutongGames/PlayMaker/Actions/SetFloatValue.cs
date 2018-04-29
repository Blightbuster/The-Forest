using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sets the value of a Float Variable.")]
	[ActionCategory(ActionCategory.Math)]
	public class SetFloatValue : FsmStateAction
	{
		
		public override void Reset()
		{
			this.floatVariable = null;
			this.floatValue = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.floatVariable.Value = this.floatValue.Value;
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.floatVariable.Value = this.floatValue.Value;
		}

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmFloat floatVariable;

		
		[RequiredField]
		public FsmFloat floatValue;

		
		public bool everyFrame;
	}
}
