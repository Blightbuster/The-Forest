using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("A Vertical Slider linked to a Float Variable.")]
	[ActionCategory(ActionCategory.GUILayout)]
	public class GUILayoutVerticalSlider : GUILayoutAction
	{
		
		public override void Reset()
		{
			base.Reset();
			this.floatVariable = null;
			this.topValue = 100f;
			this.bottomValue = 0f;
			this.changedEvent = null;
		}

		
		public override void OnGUI()
		{
			bool changed = GUI.changed;
			GUI.changed = false;
			if (this.floatVariable != null)
			{
				this.floatVariable.Value = GUILayout.VerticalSlider(this.floatVariable.Value, this.topValue.Value, this.bottomValue.Value, base.LayoutOptions);
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

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmFloat floatVariable;

		
		[RequiredField]
		public FsmFloat topValue;

		
		[RequiredField]
		public FsmFloat bottomValue;

		
		public FsmEvent changedEvent;
	}
}
