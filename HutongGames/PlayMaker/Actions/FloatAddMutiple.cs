using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Math)]
	[Tooltip("Adds multipe float variables to float variable.")]
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

		
		[UIHint(UIHint.Variable)]
		[Tooltip("The float variables to add.")]
		public FsmFloat[] floatVariables;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Add to this variable.")]
		public FsmFloat addTo;

		
		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;
	}
}
