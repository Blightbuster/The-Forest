using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sets the value of a Game Object Variable.")]
	[ActionCategory(ActionCategory.GameObject)]
	public class SetGameObject : FsmStateAction
	{
		
		public override void Reset()
		{
			this.variable = null;
			this.gameObject = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.variable.Value = this.gameObject.Value;
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.variable.Value = this.gameObject.Value;
		}

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmGameObject variable;

		
		public FsmGameObject gameObject;

		
		public bool everyFrame;
	}
}
