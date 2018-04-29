using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("Animator")]
	[Tooltip("Automatically adjust the gameobject position and rotation so that the AvatarTarget reaches the matchPosition when the current state is at the specified progress")]
	[HelpUrl("https:
	public class AnimatorMatchTarget : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.bodyPart = AvatarTarget.Root;
			this.target = null;
			this.targetPosition = new FsmVector3
			{
				UseVariable = true
			};
			this.targetRotation = new FsmQuaternion
			{
				UseVariable = true
			};
			this.positionWeight = Vector3.one;
			this.rotationWeight = 0f;
			this.startNormalizedTime = null;
			this.targetNormalizedTime = null;
			this.everyFrame = true;
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
			GameObject value = this.target.Value;
			if (value != null)
			{
				this._transform = value.transform;
			}
			this.DoMatchTarget();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoMatchTarget();
		}

		
		private void DoMatchTarget()
		{
			if (this._animator == null || this._animator.IsInTransition(0))
			{
				return;
			}
			Vector3 vector = Vector3.zero;
			Quaternion quaternion = Quaternion.identity;
			if (this._transform != null)
			{
				vector = this._transform.position;
				quaternion = this._transform.rotation;
			}
			if (!this.targetPosition.IsNone)
			{
				vector += this.targetPosition.Value;
			}
			if (!this.targetRotation.IsNone)
			{
				quaternion *= this.targetRotation.Value;
			}
			MatchTargetWeightMask weightMask = new MatchTargetWeightMask(this.positionWeight.Value, this.rotationWeight.Value);
			this._animator.MatchTarget(vector, quaternion, this.bodyPart, weightMask, this.startNormalizedTime.Value, this.targetNormalizedTime.Value);
		}

		
		[CheckForComponent(typeof(Animator))]
		[Tooltip("The Target. An Animator component is required")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("The body part that is involved in the match")]
		public AvatarTarget bodyPart;

		
		[Tooltip("The gameObject target to match")]
		public FsmGameObject target;

		
		[Tooltip("The position of the ik goal. If Goal GameObject set, position is used as an offset from Goal")]
		public FsmVector3 targetPosition;

		
		[Tooltip("The rotation of the ik goal.If Goal GameObject set, rotation is used as an offset from Goal")]
		public FsmQuaternion targetRotation;

		
		[Tooltip("The MatchTargetWeightMask Position XYZ weight")]
		public FsmVector3 positionWeight;

		
		[Tooltip("The MatchTargetWeightMask Rotation weight")]
		public FsmFloat rotationWeight;

		
		[Tooltip("Start time within the animation clip (0 - beginning of clip, 1 - end of clip)")]
		public FsmFloat startNormalizedTime;

		
		[Tooltip("End time within the animation clip (0 - beginning of clip, 1 - end of clip), values greater than 1 can be set to trigger a match after a certain number of loops. Ex: 2.3 means at 30% of 2nd loop")]
		public FsmFloat targetNormalizedTime;

		
		[Tooltip("Should always be true")]
		public bool everyFrame;

		
		private Animator _animator;

		
		private Transform _transform;
	}
}
