﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Math)]
	[Tooltip("Sets an Integer Variable to a random value between Min/Max.")]
	public class RandomInt : FsmStateAction
	{
		public override void Reset()
		{
			this.min = 0;
			this.max = 100;
			this.storeResult = null;
			this.inclusiveMax = false;
		}

		public override void OnEnter()
		{
			this.storeResult.Value = ((!this.inclusiveMax) ? UnityEngine.Random.Range(this.min.Value, this.max.Value) : UnityEngine.Random.Range(this.min.Value, this.max.Value + 1));
			base.Finish();
		}

		[RequiredField]
		public FsmInt min;

		[RequiredField]
		public FsmInt max;

		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmInt storeResult;

		[Tooltip("Should the Max value be included in the possible results?")]
		public bool inclusiveMax;
	}
}
