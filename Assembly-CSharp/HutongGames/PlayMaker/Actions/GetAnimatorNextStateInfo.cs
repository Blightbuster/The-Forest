using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Animator")]
	[Tooltip("Gets the next State information on a specified layer")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W1054")]
	public class GetAnimatorNextStateInfo : FsmStateAction
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.layerIndex = null;
			this.name = null;
			this.nameHash = null;
			this.tagHash = null;
			this.length = null;
			this.normalizedTime = null;
			this.isStateLooping = null;
			this.loopCount = null;
			this.currentLoopProgress = null;
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
			this.GetLayerInfo();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		public void OnAnimatorMoveEvent()
		{
			if (this._animatorProxy != null)
			{
				this.GetLayerInfo();
			}
		}

		public override void OnUpdate()
		{
			if (this._animatorProxy == null)
			{
				this.GetLayerInfo();
			}
		}

		private void GetLayerInfo()
		{
			if (this._animator != null)
			{
				AnimatorStateInfo nextAnimatorStateInfo = this._animator.GetNextAnimatorStateInfo(this.layerIndex.Value);
				this.nameHash.Value = nextAnimatorStateInfo.nameHash;
				if (!this.name.IsNone)
				{
					this.name.Value = this._animator.GetLayerName(this.layerIndex.Value);
				}
				this.tagHash.Value = nextAnimatorStateInfo.tagHash;
				this.length.Value = nextAnimatorStateInfo.length;
				this.isStateLooping.Value = nextAnimatorStateInfo.loop;
				this.normalizedTime.Value = nextAnimatorStateInfo.normalizedTime;
				if (!this.loopCount.IsNone || !this.currentLoopProgress.IsNone)
				{
					this.loopCount.Value = (int)Math.Truncate((double)nextAnimatorStateInfo.normalizedTime);
					this.currentLoopProgress.Value = nextAnimatorStateInfo.normalizedTime - (float)this.loopCount.Value;
				}
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
		[UIHint(UIHint.Variable)]
		[Tooltip("The layer's name.")]
		public FsmString name;

		[UIHint(UIHint.Variable)]
		[Tooltip("The layer's name Hash")]
		public FsmInt nameHash;

		[UIHint(UIHint.Variable)]
		[Tooltip("The layer's tag hash")]
		public FsmInt tagHash;

		[UIHint(UIHint.Variable)]
		[Tooltip("Is the state looping. All animations in the state must be looping")]
		public FsmBool isStateLooping;

		[UIHint(UIHint.Variable)]
		[Tooltip("The Current duration of the state. In seconds, can vary when the State contains a Blend Tree ")]
		public FsmFloat length;

		[UIHint(UIHint.Variable)]
		[Tooltip("The integer part is the number of time a state has been looped. The fractional part is the % (0-1) of progress in the current loop")]
		public FsmFloat normalizedTime;

		[UIHint(UIHint.Variable)]
		[Tooltip("The integer part is the number of time a state has been looped. This is extracted from the normalizedTime")]
		public FsmInt loopCount;

		[UIHint(UIHint.Variable)]
		[Tooltip("The progress in the current loop. This is extracted from the normalizedTime")]
		public FsmFloat currentLoopProgress;

		private PlayMakerAnimatorMoveProxy _animatorProxy;

		private Animator _animator;
	}
}
