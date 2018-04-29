using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Application)]
	[Tooltip("Quits the player application.")]
	public class ApplicationQuit : FsmStateAction
	{
		
		public override void Reset()
		{
		}

		
		public override void OnEnter()
		{
			Application.Quit();
			base.Finish();
		}
	}
}
