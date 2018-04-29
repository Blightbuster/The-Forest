using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Inserts a flexible space element.")]
	[ActionCategory(ActionCategory.GUILayout)]
	public class GUILayoutFlexibleSpace : FsmStateAction
	{
		
		public override void Reset()
		{
		}

		
		public override void OnGUI()
		{
			GUILayout.FlexibleSpace();
		}
	}
}
