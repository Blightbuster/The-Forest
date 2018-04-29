﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Application)]
	[Tooltip("Sets if the Application should play in the background. Useful for servers or testing network games on one machine.")]
	public class ApplicationRunInBackground : FsmStateAction
	{
		
		public override void Reset()
		{
			this.runInBackground = true;
		}

		
		public override void OnEnter()
		{
			Application.runInBackground = this.runInBackground.Value;
			base.Finish();
		}

		
		public FsmBool runInBackground;
	}
}
