﻿using System;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.GUI)]
	[Tooltip("Enables/Disables the PlayMakerGUI component in the scene. Note, you need a PlayMakerGUI component in the scene to see OnGUI actions. However, OnGUI can be very expensive on mobile devices. This action lets you turn OnGUI on/off (e.g., turn it on for a menu, and off during gameplay).")]
	public class EnableGUI : FsmStateAction
	{
		public override void Reset()
		{
			this.enableGUI = true;
		}

		public override void OnEnter()
		{
			PlayMakerGUI.Instance.enabled = this.enableGUI.Value;
			base.Finish();
		}

		[Tooltip("Set to True to enable, False to disable.")]
		public FsmBool enableGUI;
	}
}
