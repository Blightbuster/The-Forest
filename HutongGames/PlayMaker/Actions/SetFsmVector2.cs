using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Set the value of a Vector2 Variable in another FSM.")]
	[ActionCategory(ActionCategory.StateMachine)]
	public class SetFsmVector2 : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.fsmName = string.Empty;
			this.setValue = null;
		}

		
		public override void OnEnter()
		{
			this.DoSetFsmVector2();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		private void DoSetFsmVector2()
		{
			if (this.setValue == null)
			{
				return;
			}
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				return;
			}
			if (ownerDefaultTarget != this.goLastFrame)
			{
				this.goLastFrame = ownerDefaultTarget;
				this.fsm = ActionHelpers.GetGameObjectFsm(ownerDefaultTarget, this.fsmName.Value);
			}
			if (this.fsm == null)
			{
				this.LogWarning("Could not find FSM: " + this.fsmName.Value);
				return;
			}
			FsmVector2 fsmVector = this.fsm.FsmVariables.GetFsmVector2(this.variableName.Value);
			if (fsmVector != null)
			{
				fsmVector.Value = this.setValue.Value;
			}
			else
			{
				this.LogWarning("Could not find variable: " + this.variableName.Value);
			}
		}

		
		public override void OnUpdate()
		{
			this.DoSetFsmVector2();
		}

		
		[RequiredField]
		[Tooltip("The GameObject that owns the FSM.")]
		public FsmOwnerDefault gameObject;

		
		[UIHint(UIHint.FsmName)]
		[Tooltip("Optional name of FSM on Game Object")]
		public FsmString fsmName;

		
		[RequiredField]
		[Tooltip("The name of the FSM variable.")]
		[UIHint(UIHint.FsmVector2)]
		public FsmString variableName;

		
		[Tooltip("Set the value of the variable.")]
		[RequiredField]
		public FsmVector2 setValue;

		
		[Tooltip("Repeat every frame. Useful if the value is changing.")]
		public bool everyFrame;

		
		private GameObject goLastFrame;

		
		private PlayMakerFSM fsm;
	}
}
