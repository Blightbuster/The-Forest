using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("Animator")]
	[Tooltip("Gets the layer's current weight")]
	[HelpUrl("https:
	public class GetAnimatorLayerWeight : FsmStateAction
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
			this._animatorProxy = ownerDefaultTarget.GetComponent<PlayMakerAnimatorMoveProxy>();
			if (this._animatorProxy != null)
			{
				this._animatorProxy.OnAnimatorMoveEvent += this.OnAnimatorMoveEvent;
			}
			this.GetLayerWeight();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public void OnAnimatorMoveEvent()
		{
			if (this._animatorProxy != null)
			{
				this.GetLayerWeight();
			}
		}

		
		public override void OnUpdate()
		{
			if (this._animatorProxy == null)
			{
				this.GetLayerWeight();
			}
		}

		
		private void GetLayerWeight()
		{
			if (this._animator != null)
			{
				this.layerWeight.Value = this._animator.GetLayerWeight(this.layerIndex.Value);
			}
		}

		
		public override void OnExit()
		{
			if (this._animatorProxy != null)
			{
				this._animatorProxy.OnAnimatorMoveEvent -= this.OnAnimatorMoveEvent;
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Animator))]
		[Tooltip("The target. An Animator component and a PlayMakerAnimatorProxy component are required")]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		[Tooltip("The layer's index")]
		public FsmInt layerIndex;

		
		[Tooltip("Repeat every frame. Useful when value is subject to change over time.")]
		public bool everyFrame;

		
		[ActionSection("Results")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The layer's current weight")]
		public FsmFloat layerWeight;

		
		private PlayMakerAnimatorMoveProxy _animatorProxy;

		
		private Animator _animator;
	}
}
