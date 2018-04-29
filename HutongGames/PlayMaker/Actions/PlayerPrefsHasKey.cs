using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("PlayerPrefs")]
	[Tooltip("Returns true if key exists in the preferences.")]
	public class PlayerPrefsHasKey : FsmStateAction
	{
		
		public override void Reset()
		{
			this.key = string.Empty;
		}

		
		public override void OnEnter()
		{
			base.Finish();
			if (!this.key.IsNone && !this.key.Value.Equals(string.Empty))
			{
				this.variable.Value = PlayerPrefs.HasKey(this.key.Value);
			}
			base.Fsm.Event((!this.variable.Value) ? this.falseEvent : this.trueEvent);
		}

		
		[RequiredField]
		public FsmString key;

		
		[UIHint(UIHint.Variable)]
		[Title("Store Result")]
		public FsmBool variable;

		
		[Tooltip("Event to send if key exists.")]
		public FsmEvent trueEvent;

		
		[Tooltip("Event to send if key does not exist.")]
		public FsmEvent falseEvent;
	}
}
