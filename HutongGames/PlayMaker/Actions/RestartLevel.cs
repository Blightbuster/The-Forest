using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Restarts current level.")]
	[ActionCategory(ActionCategory.Level)]
	public class RestartLevel : FsmStateAction
	{
		
		public override void OnEnter()
		{
			Application.LoadLevel(Application.loadedLevelName);
			base.Finish();
		}
	}
}
