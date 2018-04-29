using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sets the value of an Object Variable.")]
	[ActionCategory(ActionCategory.UnityObject)]
	public class SetObjectValue : FsmStateAction
	{
		
		public override void Reset()
		{
			this.objectVariable = null;
			this.objectValue = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.objectVariable.Value = this.objectValue.Value;
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.objectVariable.Value = this.objectValue.Value;
		}

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmObject objectVariable;

		
		[RequiredField]
		public FsmObject objectValue;

		
		public bool everyFrame;
	}
}
