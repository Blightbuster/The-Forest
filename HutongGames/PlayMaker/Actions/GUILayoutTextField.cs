using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("GUILayout Text Field. Optionally send an event if the text has been edited.")]
	[ActionCategory(ActionCategory.GUILayout)]
	public class GUILayoutTextField : GUILayoutAction
	{
		
		public override void Reset()
		{
			base.Reset();
			this.text = null;
			this.maxLength = 25;
			this.style = "TextField";
			this.changedEvent = null;
		}

		
		public override void OnGUI()
		{
			bool changed = GUI.changed;
			GUI.changed = false;
			this.text.Value = GUILayout.TextField(this.text.Value, this.maxLength.Value, this.style.Value, base.LayoutOptions);
			if (GUI.changed)
			{
				base.Fsm.Event(this.changedEvent);
				GUIUtility.ExitGUI();
			}
			else
			{
				GUI.changed = changed;
			}
		}

		
		[UIHint(UIHint.Variable)]
		public FsmString text;

		
		public FsmInt maxLength;

		
		public FsmString style;

		
		public FsmEvent changedEvent;
	}
}
