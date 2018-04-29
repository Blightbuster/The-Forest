﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("Animator")]
	[Tooltip("Synchronize a NavMesh Agent velocity and rotation with the animator process.")]
	public class NavMeshAgentAnimatorSynchronizer : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
		}

		
		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				base.Finish();
				return;
			}
			this._agent = ownerDefaultTarget.GetComponent<NavMeshAgent>();
			this._animator = ownerDefaultTarget.GetComponent<Animator>();
			if (this._animator == null)
			{
				base.Finish();
				return;
			}
			this._trans = ownerDefaultTarget.transform;
			this._animatorProxy = ownerDefaultTarget.GetComponent<PlayMakerAnimatorMoveProxy>();
			if (this._animatorProxy != null)
			{
				this._animatorProxy.OnAnimatorMoveEvent += this.OnAnimatorMoveEvent;
			}
		}

		
		public void OnAnimatorMoveEvent()
		{
			this._agent.velocity = this._animator.deltaPosition / Time.deltaTime;
			this._trans.rotation = this._animator.rootRotation;
		}

		
		public override void OnExit()
		{
			if (this._animatorProxy != null)
			{
				this._animatorProxy.OnAnimatorMoveEvent -= this.OnAnimatorMoveEvent;
			}
		}

		
		[CheckForComponent(typeof(Animator))]
		[Tooltip("The Agent target. An Animator component and a PlayMakerAnimatorMoveProxy component are required")]
		[CheckForComponent(typeof(PlayMakerAnimatorMoveProxy))]
		[RequiredField]
		[CheckForComponent(typeof(NavMeshAgent))]
		public FsmOwnerDefault gameObject;

		
		private PlayMakerAnimatorMoveProxy _animatorProxy;

		
		private Animator _animator;

		
		private NavMeshAgent _agent;

		
		private Transform _trans;
	}
}
