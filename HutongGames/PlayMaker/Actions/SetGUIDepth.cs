using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.GUI)]
	[Tooltip("Sets the sorting depth of subsequent GUI elements.")]
	public class SetGUIDepth : FsmStateAction
	{
		
		public override void Reset()
		{
			this.depth = 0;
		}

		
		public override void Awake()
		{
			base.Fsm.HandleOnGUI = true;
		}

		
		public override void OnGUI()
		{
			GUI.depth = this.depth.Value;
		}

		
		[RequiredField]
		public FsmInt depth;
	}
}
