﻿using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sets the value of a Color Variable.")]
	[ActionCategory(ActionCategory.Color)]
	public class SetColorValue : FsmStateAction
	{
		
		public override void Reset()
		{
			this.colorVariable = null;
			this.color = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoSetColorValue();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoSetColorValue();
		}

		
		private void DoSetColorValue()
		{
			if (this.colorVariable != null)
			{
				this.colorVariable.Value = this.color.Value;
			}
		}

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmColor colorVariable;

		
		[RequiredField]
		public FsmColor color;

		
		public bool everyFrame;
	}
}
