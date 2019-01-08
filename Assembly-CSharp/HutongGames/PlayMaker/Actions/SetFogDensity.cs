using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.RenderSettings)]
	[Tooltip("Sets the density of the Fog in the scene.")]
	public class SetFogDensity : FsmStateAction
	{
		public override void Reset()
		{
			this.fogDensity = 0.5f;
			this.everyFrame = false;
		}

		public override void OnEnter()
		{
			this.DoSetFogDensity();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		public override void OnUpdate()
		{
			this.DoSetFogDensity();
		}

		private void DoSetFogDensity()
		{
			RenderSettings.fogDensity = this.fogDensity.Value;
		}

		[RequiredField]
		public FsmFloat fogDensity;

		public bool everyFrame;
	}
}
