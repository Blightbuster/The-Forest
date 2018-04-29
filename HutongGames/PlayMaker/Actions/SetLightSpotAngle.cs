﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Lights)]
	[Tooltip("Sets the Spot Angle of a Light.")]
	public class SetLightSpotAngle : ComponentAction<Light>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.lightSpotAngle = 20f;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoSetLightRange();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoSetLightRange();
		}

		
		private void DoSetLightRange()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				base.light.spotAngle = this.lightSpotAngle.Value;
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Light))]
		public FsmOwnerDefault gameObject;

		
		public FsmFloat lightSpotAngle;

		
		public bool everyFrame;
	}
}
