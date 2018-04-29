using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Math)]
	[Tooltip("Flips the value of a Bool Variable.")]
	public class BoolFlip : FsmStateAction
	{
		
		public override void Reset()
		{
			this.boolVariable = null;
		}

		
		public override void OnEnter()
		{
			this.boolVariable.Value = !this.boolVariable.Value;
			base.Finish();
		}

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Bool variable to flip.")]
		public FsmBool boolVariable;
	}
}
