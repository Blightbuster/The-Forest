using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sets a Float variable to its absolute value.")]
	[ActionCategory(ActionCategory.Math)]
	public class FloatAbs : FsmStateAction
	{
		
		public override void Reset()
		{
			this.floatVariable = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoFloatAbs();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoFloatAbs();
		}

		
		private void DoFloatAbs()
		{
			this.floatVariable.Value = Mathf.Abs(this.floatVariable.Value);
		}

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		[Tooltip("The Float variable.")]
		public FsmFloat floatVariable;

		
		[Tooltip("Repeat every frame. Useful if the Float variable is changing.")]
		public bool everyFrame;
	}
}
