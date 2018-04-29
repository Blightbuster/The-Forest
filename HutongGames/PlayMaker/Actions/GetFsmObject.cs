using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Get the value of an Object Variable from another FSM.")]
	[ActionCategory(ActionCategory.StateMachine)]
	public class GetFsmObject : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.fsmName = string.Empty;
			this.variableName = string.Empty;
			this.storeValue = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoGetFsmVariable();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoGetFsmVariable();
		}

		
		private void DoGetFsmVariable()
		{
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
			if (this.fsm == null || this.storeValue == null)
			{
				return;
			}
			FsmObject fsmObject = this.fsm.FsmVariables.GetFsmObject(this.variableName.Value);
			if (fsmObject != null)
			{
				this.storeValue.Value = fsmObject.Value;
			}
		}

		
		[Tooltip("The GameObject that owns the FSM.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Optional name of FSM on Game Object")]
		[UIHint(UIHint.FsmName)]
		public FsmString fsmName;

		
		[RequiredField]
		[UIHint(UIHint.FsmObject)]
		public FsmString variableName;

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmObject storeValue;

		
		[Tooltip("Repeat every frame.")]
		public bool everyFrame;

		
		private GameObject goLastFrame;

		
		protected PlayMakerFSM fsm;
	}
}
