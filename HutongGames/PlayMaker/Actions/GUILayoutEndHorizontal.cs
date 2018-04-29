using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.GUILayout)]
	[Tooltip("Close a group started with BeginHorizontal.")]
	public class GUILayoutEndHorizontal : FsmStateAction
	{
		
		public override void Reset()
		{
		}

		
		public override void OnGUI()
		{
			GUILayout.EndHorizontal();
		}
	}
}
