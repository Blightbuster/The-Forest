using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Get the value of an Integer Variable from another FSM.")]
	[ActionCategory(ActionCategory.StateMachine)]
	public class GetFsmInt : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.fsmName = string.Empty;
			this.storeValue = null;
		}

		
		public override void OnEnter()
		{
			this.DoGetFsmInt();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoGetFsmInt();
		}

		
		private void DoGetFsmInt()
		{
			if (this.storeValue == null)
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
				return;
			}
			FsmInt fsmInt = this.fsm.FsmVariables.GetFsmInt(this.variableName.Value);
			if (fsmInt == null)
			{
				return;
			}
			this.storeValue.Value = fsmInt.Value;
		}

		
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Optional name of FSM on Game Object")]
		[UIHint(UIHint.FsmName)]
		public FsmString fsmName;

		
		[UIHint(UIHint.FsmInt)]
		[RequiredField]
		public FsmString variableName;

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmInt storeValue;

		
		public bool everyFrame;

		
		private GameObject goLastFrame;

		
		private PlayMakerFSM fsm;
	}
}
