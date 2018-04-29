﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Transform)]
	[Tooltip("Transforms a Direction from world space to a Game Object's local space. The opposite of TransformDirection.")]
	public class InverseTransformDirection : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.worldDirection = null;
			this.storeResult = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoInverseTransformDirection();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoInverseTransformDirection();
		}

		
		private void DoInverseTransformDirection()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				return;
			}
			this.storeResult.Value = ownerDefaultTarget.transform.InverseTransformDirection(this.worldDirection.Value);
		}

		
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		public FsmVector3 worldDirection;

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmVector3 storeResult;

		
		public bool everyFrame;
	}
}
