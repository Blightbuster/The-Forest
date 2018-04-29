using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Converts a Bool value to a Color.")]
	[ActionCategory(ActionCategory.Convert)]
	public class ConvertBoolToColor : FsmStateAction
	{
		
		public override void Reset()
		{
			this.boolVariable = null;
			this.colorVariable = null;
			this.falseColor = Color.black;
			this.trueColor = Color.white;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoConvertBoolToColor();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoConvertBoolToColor();
		}

		
		private void DoConvertBoolToColor()
		{
			this.colorVariable.Value = ((!this.boolVariable.Value) ? this.falseColor.Value : this.trueColor.Value);
		}

		
		[UIHint(UIHint.Variable)]
		[Tooltip("The Bool variable to test.")]
		[RequiredField]
		public FsmBool boolVariable;

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		[Tooltip("The Color variable to set based on the bool variable value.")]
		public FsmColor colorVariable;

		
		[Tooltip("Color if Bool variable is false.")]
		public FsmColor falseColor;

		
		[Tooltip("Color if Bool variable is true.")]
		public FsmColor trueColor;

		
		[Tooltip("Repeat every frame. Useful if the Bool variable is changing.")]
		public bool everyFrame;
	}
}
