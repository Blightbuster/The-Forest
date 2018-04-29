﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Set the value of a variable in another FSM.")]
	[ActionCategory(ActionCategory.StateMachine)]
	public class SetFsmVariable : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.fsmName = string.Empty;
			this.setValue = new FsmVar();
		}

		
		public override void OnEnter()
		{
			this.InitFsmVar();
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

		
		private void InitFsmVar()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				return;
			}
			if (ownerDefaultTarget != this.cachedGO)
			{
				this.sourceFsm = ActionHelpers.GetGameObjectFsm(ownerDefaultTarget, this.fsmName.Value);
				this.sourceVariable = this.sourceFsm.FsmVariables.GetVariable(this.setValue.variableName);
				this.targetVariable = base.Fsm.Variables.GetVariable(this.setValue.variableName);
				this.setValue.Type = FsmUtility.GetVariableType(this.targetVariable);
				if (!string.IsNullOrEmpty(this.setValue.variableName) && this.sourceVariable == null)
				{
					this.LogWarning("Missing Variable: " + this.setValue.variableName);
				}
				this.cachedGO = ownerDefaultTarget;
			}
		}

		
		private void DoGetFsmVariable()
		{
			if (this.setValue.IsNone)
			{
				return;
			}
			this.InitFsmVar();
			this.setValue.GetValueFrom(this.sourceVariable);
			this.setValue.ApplyValueTo(this.targetVariable);
		}

		
		[RequiredField]
		[Tooltip("The GameObject that owns the FSM")]
		public FsmOwnerDefault gameObject;

		
		[UIHint(UIHint.FsmName)]
		[Tooltip("Optional name of FSM on Game Object")]
		public FsmString fsmName;

		
		public FsmString variableName;

		
		[HideTypeFilter]
		[RequiredField]
		public FsmVar setValue;

		
		[Tooltip("Repeat every frame.")]
		public bool everyFrame;

		
		private GameObject cachedGO;

		
		private PlayMakerFSM sourceFsm;

		
		private INamedVariable sourceVariable;

		
		private NamedVariable targetVariable;
	}
}
