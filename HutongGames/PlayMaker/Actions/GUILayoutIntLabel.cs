using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.GUILayout)]
	[Tooltip("GUILayout Label for an Int Variable.")]
	public class GUILayoutIntLabel : GUILayoutAction
	{
		
		public override void Reset()
		{
			base.Reset();
			this.prefix = string.Empty;
			this.style = string.Empty;
			this.intVariable = null;
		}

		
		public override void OnGUI()
		{
			if (string.IsNullOrEmpty(this.style.Value))
			{
				GUILayout.Label(new GUIContent(this.prefix.Value + this.intVariable.Value), base.LayoutOptions);
			}
			else
			{
				GUILayout.Label(new GUIContent(this.prefix.Value + this.intVariable.Value), this.style.Value, base.LayoutOptions);
			}
		}

		
		[Tooltip("Text to put before the int variable.")]
		public FsmString prefix;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Int variable to display.")]
		public FsmInt intVariable;

		
		[Tooltip("Optional GUIStyle in the active GUISKin.")]
		public FsmString style;
	}
}
