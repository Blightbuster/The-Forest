﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("iTween")]
	[Tooltip("Randomly shakes a GameObject's scale by a diminishing amount over time.")]
	public class iTweenShakeScale : iTweenFsmAction
	{
		
		public override void Reset()
		{
			base.Reset();
			this.id = new FsmString
			{
				UseVariable = true
			};
			this.time = 1f;
			this.delay = 0f;
			this.loopType = iTween.LoopType.none;
			this.vector = new FsmVector3
			{
				UseVariable = true
			};
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
			Vector3 vector = Vector3.zero;
			if (!this.vector.IsNone)
			{
				vector = this.vector.Value;
			}
			this.itweenType = "shake";
			iTween.ShakeScale(ownerDefaultTarget, iTween.Hash(new object[]
			{
				"amount",
				vector,
				"name",
				(!this.id.IsNone) ? this.id.Value : string.Empty,
				"time",
				(!this.time.IsNone) ? this.time.Value : 1f,
				"delay",
				(!this.delay.IsNone) ? this.delay.Value : 0f,
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
				!this.realTime.IsNone && this.realTime.Value
			}));
		}

		
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("iTween ID. If set you can use iTween Stop action to stop it by its id.")]
		public FsmString id;

		
		[RequiredField]
		[Tooltip("A vector shake range.")]
		public FsmVector3 vector;

		
		[Tooltip("The time in seconds the animation will take to complete.")]
		public FsmFloat time;

		
		[Tooltip("The time in seconds the animation will wait before beginning.")]
		public FsmFloat delay;

		
		[Tooltip("The type of loop to apply once the animation has completed.")]
		public iTween.LoopType loopType;
	}
}
