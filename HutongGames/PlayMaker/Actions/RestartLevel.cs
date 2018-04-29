﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Level)]
	[Tooltip("Restarts current level.")]
	public class RestartLevel : FsmStateAction
	{
		
		public override void OnEnter()
		{
			Application.LoadLevel(Application.loadedLevelName);
			base.Finish();
		}
	}
}
