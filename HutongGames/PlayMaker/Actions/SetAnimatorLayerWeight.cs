using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[HelpUrl("https:
	[ActionCategory("Animator")]
	[Tooltip("Sets the layer's current weight")]
	public class SetAnimatorLayerWeight : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.layerIndex = null;
			this.layerWeight = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				base.Finish();
				return;
			}
			this._animator = ownerDefaultTarget.GetComponent<Animator>();
			if (this._animator == null)
			{
				base.Finish();
				return;
			}
			this.DoLayerWeight();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoLayerWeight();
		}

		
		private void DoLayerWeight()
		{
			if (this._animator == null)
			{
				return;
			}
			this._animator.SetLayerWeightReflected(this.layerIndex.Value, this.layerWeight.Value);
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Animator))]
		[Tooltip("The Target. An Animator component is required")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("The layer's index")]
		[RequiredField]
		public FsmInt layerIndex;

		
		[Tooltip("Sets the layer's current weight")]
		[RequiredField]
		public FsmFloat layerWeight;

		
		[Tooltip("Repeat every frame. Useful for changing over time.")]
		public bool everyFrame;

		
		private Animator _animator;
	}
}
