using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.StateMachine)]
	[Tooltip("Set the value of a Color Variable in another FSM.")]
	public class SetFsmColor : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.fsmName = string.Empty;
			this.setValue = null;
		}

		
		public override void OnEnter()
		{
			this.DoSetFsmColor();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		private void DoSetFsmColor()
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
			FsmColor fsmColor = this.fsm.FsmVariables.GetFsmColor(this.variableName.Value);
			if (fsmColor != null)
			{
				fsmColor.Value = this.setValue.Value;
			}
			else
			{
				this.LogWarning("Could not find variable: " + this.variableName.Value);
			}
		}

		
		public override void OnUpdate()
		{
			this.DoSetFsmColor();
		}

		
		[Tooltip("The GameObject that owns the FSM.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Optional name of FSM on Game Object")]
		[UIHint(UIHint.FsmName)]
		public FsmString fsmName;

		
		[RequiredField]
		[Tooltip("The name of the FSM variable.")]
		[UIHint(UIHint.FsmColor)]
		public FsmString variableName;

		
		[RequiredField]
		[Tooltip("Set the value of the variable.")]
		public FsmColor setValue;

		
		[Tooltip("Repeat every frame. Useful if the value is changing.")]
		public bool everyFrame;

		
		private GameObject goLastFrame;

		
		private PlayMakerFSM fsm;
	}
}
