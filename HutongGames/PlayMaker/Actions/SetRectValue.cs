using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sets the value of a Rect Variable.")]
	[ActionCategory(ActionCategory.Rect)]
	public class SetRectValue : FsmStateAction
	{
		
		public override void Reset()
		{
			this.rectVariable = null;
			this.rectValue = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.rectVariable.Value = this.rectValue.Value;
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.rectVariable.Value = this.rectValue.Value;
		}

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmRect rectVariable;

		
		[RequiredField]
		public FsmRect rectValue;

		
		public bool everyFrame;
	}
}
