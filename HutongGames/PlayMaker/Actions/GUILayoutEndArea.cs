using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Close a GUILayout group started with BeginArea.")]
	[ActionCategory(ActionCategory.GUILayout)]
	public class GUILayoutEndArea : FsmStateAction
	{
		
		public override void Reset()
		{
		}

		
		public override void OnGUI()
		{
			GUILayout.EndArea();
		}
	}
}
