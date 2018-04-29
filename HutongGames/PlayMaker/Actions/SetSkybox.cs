using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.RenderSettings)]
	[Tooltip("Sets the global Skybox.")]
	public class SetSkybox : FsmStateAction
	{
		
		public override void Reset()
		{
			this.skybox = null;
		}

		
		public override void OnEnter()
		{
			RenderSettings.skybox = this.skybox.Value;
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			RenderSettings.skybox = this.skybox.Value;
		}

		
		public FsmMaterial skybox;

		
		[Tooltip("Repeat every frame. Useful if the Skybox is changing.")]
		public bool everyFrame;
	}
}
