﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Animation)]
	[Tooltip("Removes a mixing transform previously added with Add Mixing Transform. If transform has been added as recursive, then it will be removed as recursive. Once you remove all mixing transforms added to animation state all curves become animated again.")]
	public class RemoveMixingTransform : ComponentAction<Animation>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.animationName = string.Empty;
		}

		
		public override void OnEnter()
		{
			this.DoRemoveMixingTransform();
			base.Finish();
		}

		
		private void DoRemoveMixingTransform()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (!base.UpdateCache(ownerDefaultTarget))
			{
				return;
			}
			AnimationState animationState = base.animation[this.animationName.Value];
			if (animationState == null)
			{
				return;
			}
			Transform mix = ownerDefaultTarget.transform.Find(this.transfrom.Value);
			animationState.AddMixingTransform(mix);
		}

		
		[CheckForComponent(typeof(Animation))]
		[Tooltip("The GameObject playing the animation.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("The name of the animation.")]
		[RequiredField]
		public FsmString animationName;

		
		[Tooltip("The mixing transform to remove. E.g., root/upper_body/left_shoulder")]
		[RequiredField]
		public FsmString transfrom;
	}
}
