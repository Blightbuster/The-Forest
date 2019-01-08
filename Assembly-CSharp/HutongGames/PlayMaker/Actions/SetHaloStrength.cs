using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.RenderSettings)]
	[Tooltip("Sets the size of light halos.")]
	public class SetHaloStrength : FsmStateAction
	{
		public override void Reset()
		{
			this.haloStrength = 0.5f;
			this.everyFrame = false;
		}

		public override void OnEnter()
		{
			this.DoSetHaloStrength();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		public override void OnUpdate()
		{
			this.DoSetHaloStrength();
		}

		private void DoSetHaloStrength()
		{
			RenderSettings.haloStrength = this.haloStrength.Value;
		}

		[RequiredField]
		public FsmFloat haloStrength;

		public bool everyFrame;
	}
}
