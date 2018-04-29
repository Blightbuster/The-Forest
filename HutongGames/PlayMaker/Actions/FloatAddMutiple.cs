using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Adds multipe float variables to float variable.")]
	[ActionCategory(ActionCategory.Math)]
	public class FloatAddMutiple : FsmStateAction
	{
		
		public override void Reset()
		{
			this.floatVariables = null;
			this.addTo = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoFloatAdd();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoFloatAdd();
		}

		
		private void DoFloatAdd()
		{
			for (int i = 0; i < this.floatVariables.Length; i++)
			{
				this.addTo.Value += this.floatVariables[i].Value;
			}
		}

		
		[Tooltip("The float variables to add.")]
		[UIHint(UIHint.Variable)]
		public FsmFloat[] floatVariables;

		
		[Tooltip("Add to this variable.")]
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmFloat addTo;

		
		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;
	}
}
