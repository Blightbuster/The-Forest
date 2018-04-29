using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Reverses the direction of a Vector3 Variable. Same as multiplying by -1.")]
	[ActionCategory(ActionCategory.Vector3)]
	public class Vector3Invert : FsmStateAction
	{
		
		public override void Reset()
		{
			this.vector3Variable = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.vector3Variable.Value = this.vector3Variable.Value * -1f;
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.vector3Variable.Value = this.vector3Variable.Value * -1f;
		}

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmVector3 vector3Variable;

		
		public bool everyFrame;
	}
}
