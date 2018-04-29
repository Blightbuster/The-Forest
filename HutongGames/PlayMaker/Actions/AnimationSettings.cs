using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Set the Wrap Mode, Blend Mode, Layer and Speed of an Animation.\nNOTE: Settings are applied once, on entering the state, NOT continuously. To dynamically control an animation's settings, use Set Animation Speede etc.")]
	[ActionCategory(ActionCategory.Animation)]
	public class AnimationSettings : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.animName = null;
			this.wrapMode = WrapMode.Loop;
			this.blendMode = AnimationBlendMode.Blend;
			this.speed = 1f;
			this.layer = 0;
		}

		
		public override void OnEnter()
		{
			this.DoAnimationSettings();
			base.Finish();
		}

		
		private void DoAnimationSettings()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null || string.IsNullOrEmpty(this.animName.Value))
			{
				return;
			}
			Animation component = ownerDefaultTarget.GetComponent<Animation>();
			if (component == null)
			{
				this.LogWarning("Missing animation component: " + ownerDefaultTarget.name);
				return;
			}
			AnimationState animationState = component[this.animName.Value];
			if (animationState == null)
			{
				this.LogWarning("Missing animation: " + this.animName.Value);
				return;
			}
			animationState.wrapMode = this.wrapMode;
			animationState.blendMode = this.blendMode;
			if (!this.layer.IsNone)
			{
				animationState.layer = this.layer.Value;
			}
			if (!this.speed.IsNone)
			{
				animationState.speed = this.speed.Value;
			}
		}

		
		[Tooltip("A GameObject with an Animation Component.")]
		[RequiredField]
		[CheckForComponent(typeof(Animation))]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		[Tooltip("The name of the animation.")]
		[UIHint(UIHint.Animation)]
		public FsmString animName;

		
		[Tooltip("The behavior of the animation when it wraps.")]
		public WrapMode wrapMode;

		
		[Tooltip("How the animation is blended with other animations on the Game Object.")]
		public AnimationBlendMode blendMode;

		
		[Tooltip("The speed of the animation. 1 = normal; 2 = double speed...")]
		[HasFloatSlider(0f, 5f)]
		public FsmFloat speed;

		
		[Tooltip("The animation layer")]
		public FsmInt layer;
	}
}
