using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.GUI)]
	[Tooltip("Sets the Tinting Color for all background elements rendered by the GUI. By default only effects GUI rendered by this FSM, check Apply Globally to effect all GUI controls.")]
	public class SetGUIBackgroundColor : FsmStateAction
	{
		
		public override void Reset()
		{
			this.backgroundColor = Color.white;
		}

		
		public override void OnGUI()
		{
			GUI.backgroundColor = this.backgroundColor.Value;
			if (this.applyGlobally.Value)
			{
				PlayMakerGUI.GUIBackgroundColor = GUI.backgroundColor;
			}
		}

		
		[RequiredField]
		public FsmColor backgroundColor;

		
		public FsmBool applyGlobally;
	}
}
