﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sets the GUISkin used by GUI elements.")]
	[ActionCategory(ActionCategory.GUI)]
	public class SetGUISkin : FsmStateAction
	{
		
		public override void Reset()
		{
			this.skin = null;
			this.applyGlobally = true;
		}

		
		public override void OnGUI()
		{
			if (this.skin != null)
			{
				GUI.skin = this.skin;
			}
			if (this.applyGlobally.Value)
			{
				PlayMakerGUI.GUISkin = this.skin;
				base.Finish();
			}
		}

		
		[RequiredField]
		public GUISkin skin;

		
		public FsmBool applyGlobally;
	}
}
