using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Animator")]
	[Tooltip("Sets the position, rotation and weights of an IK goal. A GameObject can be set to control the position and rotation, or it can be manually expressed.")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W1067")]
	public class SetAnimatorIKGoal : FsmStateAction
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.goal = null;
			this.position = new FsmVector3
			{
				UseVariable = true
			};
			this.rotation = new FsmQuaternion
			{
				UseVariable = true
			};
			this.positionWeight = 1f;
			this.rotationWeight = 1f;
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
			this._animatorProxy = ownerDefaultTarget.GetComponent<PlayMakerAnimatorIKProxy>();
			if (this._animatorProxy != null)
			{
				this._animatorProxy.OnAnimatorIKEvent += this.OnAnimatorIKEvent;
			}
			else
			{
				Debug.LogWarning("This action requires a PlayMakerAnimatorIKProxy. It may not perform properly if not present.");
			}
			GameObject value = this.goal.Value;
			if (value != null)
			{
				this._transform = value.transform;
			}
			this.DoSetIKGoal();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		public void OnAnimatorIKEvent(int layer)
		{
			if (this._animatorProxy != null)
			{
				this.DoSetIKGoal();
			}
		}

		private void DoSetIKGoal()
		{
			if (this._animator == null)
			{
				return;
			}
			if (this._transform != null)
			{
				if (this.position.IsNone)
				{
					this._animator.SetIKPosition(this.iKGoal, this._transform.position);
				}
				else
				{
					this._animator.SetIKPosition(this.iKGoal, this._transform.position + this.position.Value);
				}
				if (this.rotation.IsNone)
				{
					this._animator.SetIKRotation(this.iKGoal, this._transform.rotation);
				}
				else
				{
					this._animator.SetIKRotation(this.iKGoal, this._transform.rotation * this.rotation.Value);
				}
			}
			else
			{
				if (!this.position.IsNone)
				{
					this._animator.SetIKPosition(this.iKGoal, this.position.Value);
				}
				if (!this.rotation.IsNone)
				{
					this._animator.SetIKRotation(this.iKGoal, this.rotation.Value);
				}
			}
			if (!this.positionWeight.IsNone)
			{
				this._animator.SetIKPositionWeight(this.iKGoal, this.positionWeight.Value);
			}
			if (!this.rotationWeight.IsNone)
			{
				this._animator.SetIKRotationWeight(this.iKGoal, this.rotationWeight.Value);
			}
		}

		public override void OnExit()
		{
			if (this._animatorProxy != null)
			{
				this._animatorProxy.OnAnimatorIKEvent -= this.OnAnimatorIKEvent;
			}
		}

		[RequiredField]
		[CheckForComponent(typeof(Animator))]
		[CheckForComponent(typeof(PlayMakerAnimatorIKProxy))]
		[Tooltip("The target. An Animator component and a PlayMakerAnimatorIKProxy component are required")]
		public FsmOwnerDefault gameObject;

		[Tooltip("The IK goal")]
		public AvatarIKGoal iKGoal;

		[Tooltip("The gameObject target of the ik goal")]
		public FsmGameObject goal;

		[Tooltip("The position of the ik goal. If Goal GameObject set, position is used as an offset from Goal")]
		public FsmVector3 position;

		[Tooltip("The rotation of the ik goal.If Goal GameObject set, rotation is used as an offset from Goal")]
		public FsmQuaternion rotation;

		[HasFloatSlider(0f, 1f)]
		[Tooltip("The translative weight of an IK goal (0 = at the original animation before IK, 1 = at the goal)")]
		public FsmFloat positionWeight;

		[HasFloatSlider(0f, 1f)]
		[Tooltip("Sets the rotational weight of an IK goal (0 = rotation before IK, 1 = rotation at the IK goal)")]
		public FsmFloat rotationWeight;

		[Tooltip("Repeat every frame. Useful when changing over time.")]
		public bool everyFrame;

		private PlayMakerAnimatorIKProxy _animatorProxy;

		private Animator _animator;

		private Transform _transform;
	}
}
