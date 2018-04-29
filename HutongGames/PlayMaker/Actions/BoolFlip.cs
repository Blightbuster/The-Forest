using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Flips the value of a Bool Variable.")]
	[ActionCategory(ActionCategory.Math)]
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

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		[Tooltip("Bool variable to flip.")]
		public FsmBool boolVariable;
	}
}
