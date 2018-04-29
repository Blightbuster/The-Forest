using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Subtracts a Vector3 value from a Vector3 variable.")]
	[ActionCategory(ActionCategory.Vector3)]
	public class Vector3Subtract : FsmStateAction
	{
		
		public override void Reset()
		{
			this.vector3Variable = null;
			this.subtractVector = new FsmVector3
			{
				UseVariable = true
			};
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.vector3Variable.Value = this.vector3Variable.Value - this.subtractVector.Value;
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.vector3Variable.Value = this.vector3Variable.Value - this.subtractVector.Value;
		}

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmVector3 vector3Variable;

		
		[RequiredField]
		public FsmVector3 subtractVector;

		
		public bool everyFrame;
	}
}
