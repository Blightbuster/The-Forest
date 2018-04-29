using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sets the value of a Vector3 Variable.")]
	[ActionCategory(ActionCategory.Vector3)]
	public class SetVector3Value : FsmStateAction
	{
		
		public override void Reset()
		{
			this.vector3Variable = null;
			this.vector3Value = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.vector3Variable.Value = this.vector3Value.Value;
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.vector3Variable.Value = this.vector3Value.Value;
		}

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmVector3 vector3Variable;

		
		[RequiredField]
		public FsmVector3 vector3Value;

		
		public bool everyFrame;
	}
}
