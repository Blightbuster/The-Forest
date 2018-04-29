﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.GUILayout)]
	[Tooltip("GUILayout Text Field to edit a Float Variable. Optionally send an event if the text has been edited.")]
	public class GUILayoutFloatField : GUILayoutAction
	{
		
		public override void Reset()
		{
			base.Reset();
			this.floatVariable = null;
			this.style = string.Empty;
			this.changedEvent = null;
		}

		
		public override void OnGUI()
		{
			bool changed = GUI.changed;
			GUI.changed = false;
			if (!string.IsNullOrEmpty(this.style.Value))
			{
				this.floatVariable.Value = float.Parse(GUILayout.TextField(this.floatVariable.Value.ToString(), this.style.Value, base.LayoutOptions));
			}
			else
			{
				this.floatVariable.Value = float.Parse(GUILayout.TextField(this.floatVariable.Value.ToString(), base.LayoutOptions));
			}
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
		[Tooltip("Float Variable to show in the edit field.")]
		public FsmFloat floatVariable;

		
		[Tooltip("Optional GUIStyle in the active GUISKin.")]
		public FsmString style;

		
		[Tooltip("Optional event to send when the value changes.")]
		public FsmEvent changedEvent;
	}
}
