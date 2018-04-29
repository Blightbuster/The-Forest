using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Transforms a Position from a Game Object's local space to world space.")]
	[ActionCategory(ActionCategory.Transform)]
	public class TransformPoint : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.localPosition = null;
			this.storeResult = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoTransformPoint();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoTransformPoint();
		}

		
		private void DoTransformPoint()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				return;
			}
			this.storeResult.Value = ownerDefaultTarget.transform.TransformPoint(this.localPosition.Value);
		}

		
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		public FsmVector3 localPosition;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmVector3 storeResult;

		
		public bool everyFrame;
	}
}
