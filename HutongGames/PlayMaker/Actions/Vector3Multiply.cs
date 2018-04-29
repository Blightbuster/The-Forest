using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Multiplies a Vector3 variable by a Float.")]
	[ActionCategory(ActionCategory.Vector3)]
	public class Vector3Multiply : FsmStateAction
	{
		
		public override void Reset()
		{
			this.vector3Variable = null;
			this.multiplyBy = 1f;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.vector3Variable.Value = this.vector3Variable.Value * this.multiplyBy.Value;
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.vector3Variable.Value = this.vector3Variable.Value * this.multiplyBy.Value;
		}

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmVector3 vector3Variable;

		
		[RequiredField]
		public FsmFloat multiplyBy;

		
		public bool everyFrame;
	}
}
