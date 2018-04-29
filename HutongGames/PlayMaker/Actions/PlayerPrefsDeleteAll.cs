using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Removes all keys and values from the preferences. Use with caution.")]
	[ActionCategory("PlayerPrefs")]
	public class PlayerPrefsDeleteAll : FsmStateAction
	{
		
		public override void Reset()
		{
		}

		
		public override void OnEnter()
		{
			PlayerPrefs.DeleteAll();
			base.Finish();
		}
	}
}
