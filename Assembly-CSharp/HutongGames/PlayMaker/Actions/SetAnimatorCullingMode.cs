﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Animator")]
	[Tooltip("Controls culling of this Animator component.\nIf true, set to 'AlwaysAnimate': always animate the entire character. Object is animated even when offscreen.\nIf False, set to 'BasedOnRenderes' animation is disabled when renderers are not visible.")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W1064")]
	public class SetAnimatorCullingMode : FsmStateAction
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.alwaysAnimate = null;
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
			this.SetCullingMode();
			base.Finish();
		}

		private void SetCullingMode()
		{
			if (this._animator == null)
			{
				return;
			}
			this._animator.cullingMode = ((!this.alwaysAnimate.Value) ? AnimatorCullingMode.CullUpdateTransforms : AnimatorCullingMode.AlwaysAnimate);
		}

		[RequiredField]
		[CheckForComponent(typeof(Animator))]
		[Tooltip("The Target. An Animator component is required")]
		public FsmOwnerDefault gameObject;

		[Tooltip("If true, always animate the entire character, else animation is disabled when renderers are not visible")]
		public FsmBool alwaysAnimate;

		private Animator _animator;
	}
}
