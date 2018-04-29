using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Close a group started with BeginVertical.")]
	[ActionCategory(ActionCategory.GUILayout)]
	public class GUILayoutEndVertical : FsmStateAction
	{
		
		public override void Reset()
		{
		}

		
		public override void OnGUI()
		{
			GUILayout.EndVertical();
		}
	}
}
