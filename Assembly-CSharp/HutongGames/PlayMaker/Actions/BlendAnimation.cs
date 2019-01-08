using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Animation)]
	[Tooltip("Blends an Animation towards a Target Weight over a specified Time.\nOptionally sends an Event when finished.")]
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
				base.LogWarning("Missing Animation component on GameObject: " + go.name);
				base.Finish();
				return;
			}
			AnimationState animationState = component[this.animName.Value];
			if (animationState == null)
			{
				base.LogWarning("Missing animation: " + this.animName.Value);
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

		[RequiredField]
		[CheckForComponent(typeof(Animation))]
		[Tooltip("The GameObject to animate.")]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[UIHint(UIHint.Animation)]
		[Tooltip("The name of the animation to blend.")]
		public FsmString animName;

		[RequiredField]
		[HasFloatSlider(0f, 1f)]
		[Tooltip("Target weight to blend to.")]
		public FsmFloat targetWeight;

		[RequiredField]
		[HasFloatSlider(0f, 5f)]
		[Tooltip("How long should the blend take.")]
		public FsmFloat time;

		[Tooltip("Event to send when the blend has finished.")]
		public FsmEvent finishEvent;

		private DelayedEvent delayedFinishEvent;
	}
}
