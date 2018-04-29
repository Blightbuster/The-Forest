﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Returns the culling of this Animator component. Optionnaly sends events.\nIf true ('AlwaysAnimate'): always animate the entire character. Object is animated even when offscreen.\nIf False ('BasedOnRenderers') animation is disabled when renderers are not visible.")]
	[ActionCategory("Animator")]
	public class GetAnimatorCullingMode : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.alwaysAnimate = null;
			this.alwaysAnimateEvent = null;
			this.basedOnRenderersEvent = null;
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
			this.DoCheckCulling();
			base.Finish();
		}

		
		private void DoCheckCulling()
		{
			if (this._animator == null)
			{
				return;
			}
			bool flag = this._animator.cullingMode == AnimatorCullingMode.AlwaysAnimate;
			this.alwaysAnimate.Value = flag;
			if (flag)
			{
				base.Fsm.Event(this.alwaysAnimateEvent);
			}
			else
			{
				base.Fsm.Event(this.basedOnRenderersEvent);
			}
		}

		
		[CheckForComponent(typeof(Animator))]
		[RequiredField]
		[Tooltip("The Target. An Animator component is required")]
		public FsmOwnerDefault gameObject;

		
		[UIHint(UIHint.Variable)]
		[ActionSection("Results")]
		[Tooltip("If true, always animate the entire character, else animation is disabled when renderers are not visible")]
		[RequiredField]
		public FsmBool alwaysAnimate;

		
		[Tooltip("Event send if culling mode is 'AlwaysAnimate'")]
		public FsmEvent alwaysAnimateEvent;

		
		[Tooltip("Event send if culling mode is 'BasedOnRenders'")]
		public FsmEvent basedOnRenderersEvent;

		
		private Animator _animator;
	}
}
