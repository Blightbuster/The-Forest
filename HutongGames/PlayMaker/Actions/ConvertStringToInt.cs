using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Converts an String value to an Int value.")]
	[ActionCategory(ActionCategory.Convert)]
	public class ConvertStringToInt : FsmStateAction
	{
		
		public override void Reset()
		{
			this.intVariable = null;
			this.stringVariable = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoConvertStringToInt();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoConvertStringToInt();
		}

		
		private void DoConvertStringToInt()
		{
			this.intVariable.Value = int.Parse(this.stringVariable.Value);
		}

		
		[Tooltip("The String variable to convert to an integer.")]
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmString stringVariable;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result in an Int variable.")]
		public FsmInt intVariable;

		
		[Tooltip("Repeat every frame. Useful if the String variable is changing.")]
		public bool everyFrame;
	}
}
