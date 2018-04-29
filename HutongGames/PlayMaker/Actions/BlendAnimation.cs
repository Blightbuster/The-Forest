﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Blends an Animation towards a Target Weight over a specified Time.\nOptionally sends an Event when finished.")]
	[ActionCategory(ActionCategory.Animation)]
	public class BlendAnimation : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.animName = null;
			this.targetWeight = 1f;
			this.time = 0.3f;
			this.finishEvent = null;
		}

		
		public override void OnEnter()
		{
			this.DoBlendAnimation((this.gameObject.OwnerOption != OwnerDefaultOption.UseOwner) ? this.gameObject.GameObject.Value : base.Owner);
		}

		
		public override void OnUpdate()
		{
			if (DelayedEvent.WasSent(this.delayedFinishEvent))
			{
				base.Finish();
			}
		}

		
		private void DoBlendAnimation(GameObject go)
		{
			if (go == null)
			{
				return;
			}
			Animation component = go.GetComponent<Animation>();
			if (component == null)
			{
				this.LogWarning("Missing Animation component on GameObject: " + go.name);
				base.Finish();
				return;
			}
			AnimationState animationState = component[this.animName.Value];
			if (animationState == null)
			{
				this.LogWarning("Missing animation: " + this.animName.Value);
				base.Finish();
				return;
			}
			float value = this.time.Value;
			component.Blend(this.animName.Value, this.targetWeight.Value, value);
			if (this.finishEvent != null)
			{
				this.delayedFinishEvent = base.Fsm.DelayedEvent(this.finishEvent, animationState.length);
			}
			else
			{
				base.Finish();
			}
		}

		
		[CheckForComponent(typeof(Animation))]
		[Tooltip("The GameObject to animate.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[UIHint(UIHint.Animation)]
		[Tooltip("The name of the animation to blend.")]
		[RequiredField]
		public FsmString animName;

		
		[HasFloatSlider(0f, 1f)]
		[Tooltip("Target weight to blend to.")]
		[RequiredField]
		public FsmFloat targetWeight;

		
		[Tooltip("How long should the blend take.")]
		[RequiredField]
		[HasFloatSlider(0f, 5f)]
		public FsmFloat time;

		
		[Tooltip("Event to send when the blend has finished.")]
		public FsmEvent finishEvent;

		
		private DelayedEvent delayedFinishEvent;
	}
}
