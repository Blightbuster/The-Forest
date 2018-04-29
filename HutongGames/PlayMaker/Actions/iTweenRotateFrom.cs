﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("iTween")]
	[Tooltip("Instantly changes a GameObject's Euler angles in degrees then returns it to it's starting rotation over time.")]
	public class iTweenRotateFrom : iTweenFsmAction
	{
		
		public override void Reset()
		{
			base.Reset();
			this.id = new FsmString
			{
				UseVariable = true
			};
			this.transformRotation = new FsmGameObject
			{
				UseVariable = true
			};
			this.vectorRotation = new FsmVector3
			{
				UseVariable = true
			};
			this.time = 1f;
			this.delay = 0f;
			this.loopType = iTween.LoopType.none;
			this.speed = new FsmFloat
			{
				UseVariable = true
			};
			this.space = Space.World;
		}

		
		public override void OnEnter()
		{
			base.OnEnteriTween(this.gameObject);
			if (this.loopType != iTween.LoopType.none)
			{
				base.IsLoop(true);
			}
			this.DoiTween();
		}

		
		public override void OnExit()
		{
			base.OnExitiTween(this.gameObject);
		}

		
		private void DoiTween()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				return;
			}
			Vector3 vector = (!this.vectorRotation.IsNone) ? this.vectorRotation.Value : Vector3.zero;
			if (!this.transformRotation.IsNone && this.transformRotation.Value)
			{
				vector = ((this.space != Space.World) ? (this.transformRotation.Value.transform.localEulerAngles + vector) : (this.transformRotation.Value.transform.eulerAngles + vector));
			}
			this.itweenType = "rotate";
			iTween.RotateFrom(ownerDefaultTarget, iTween.Hash(new object[]
			{
				"rotation",
				vector,
				"name",
				(!this.id.IsNone) ? this.id.Value : string.Empty,
				(!this.speed.IsNone) ? "speed" : "time",
				(!this.speed.IsNone) ? this.speed.Value : ((!this.time.IsNone) ? this.time.Value : 1f),
				"delay",
				(!this.delay.IsNone) ? this.delay.Value : 0f,
				"easetype",
				this.easeType,
				"looptype",
				this.loopType,
				"oncomplete",
				"iTweenOnComplete",
				"oncompleteparams",
				this.itweenID,
				"onstart",
				"iTweenOnStart",
				"onstartparams",
				this.itweenID,
				"ignoretimescale",
				!this.realTime.IsNone && this.realTime.Value,
				"islocal",
				this.space == Space.Self
			}));
		}

		
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("iTween ID. If set you can use iTween Stop action to stop it by its id.")]
		public FsmString id;

		
		[Tooltip("A rotation from a GameObject.")]
		public FsmGameObject transformRotation;

		
		[Tooltip("A rotation vector the GameObject will animate from.")]
		public FsmVector3 vectorRotation;

		
		[Tooltip("The time in seconds the animation will take to complete.")]
		public FsmFloat time;

		
		[Tooltip("The time in seconds the animation will wait before beginning.")]
		public FsmFloat delay;

		
		[Tooltip("Can be used instead of time to allow animation based on speed. When you define speed the time variable is ignored.")]
		public FsmFloat speed;

		
		[Tooltip("The shape of the easing curve applied to the animation.")]
		public iTween.EaseType easeType = iTween.EaseType.linear;

		
		[Tooltip("The type of loop to apply once the animation has completed.")]
		public iTween.LoopType loopType;

		
		[Tooltip("Whether to animate in local or world space.")]
		public Space space;
	}
}
