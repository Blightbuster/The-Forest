﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Math)]
	[Tooltip("Clamp the value of an Integer Variable to a Min/Max range.")]
	public class IntClamp : FsmStateAction
	{
		
		public override void Reset()
		{
			this.intVariable = null;
			this.minValue = null;
			this.maxValue = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoClamp();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoClamp();
		}

		
		private void DoClamp()
		{
			this.intVariable.Value = Mathf.Clamp(this.intVariable.Value, this.minValue.Value, this.maxValue.Value);
		}

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmInt intVariable;

		
		[RequiredField]
		public FsmInt minValue;

		
		[RequiredField]
		public FsmInt maxValue;

		
		public bool everyFrame;
	}
}
