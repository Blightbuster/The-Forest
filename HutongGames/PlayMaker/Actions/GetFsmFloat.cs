using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.StateMachine)]
	[Tooltip("Get the value of a Float Variable from another FSM.")]
	public class GetFsmFloat : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.fsmName = string.Empty;
			this.storeValue = null;
		}

		
		public override void OnEnter()
		{
			this.DoGetFsmFloat();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoGetFsmFloat();
		}

		
		private void DoGetFsmFloat()
		{
			if (this.storeValue.IsNone)
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
				this.fsm = ActionHelpers.GetGameObjectFsm(ownerDefaultTarget, this.fsmName.Value);
				this.goLastFrame = ownerDefaultTarget;
			}
			if (this.fsm == null)
			{
				return;
			}
			FsmFloat fsmFloat = this.fsm.FsmVariables.GetFsmFloat(this.variableName.Value);
			if (fsmFloat == null)
			{
				return;
			}
			this.storeValue.Value = fsmFloat.Value;
		}

		
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[UIHint(UIHint.FsmName)]
		[Tooltip("Optional name of FSM on Game Object")]
		public FsmString fsmName;

		
		[RequiredField]
		[UIHint(UIHint.FsmFloat)]
		public FsmString variableName;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmFloat storeValue;

		
		public bool everyFrame;

		
		private GameObject goLastFrame;

		
		private PlayMakerFSM fsm;
	}
}
