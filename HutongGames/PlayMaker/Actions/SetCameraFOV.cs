﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Camera)]
	[Tooltip("Sets Field of View used by the Camera.")]
	public class SetCameraFOV : ComponentAction<Camera>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.fieldOfView = 50f;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoSetCameraFOV();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoSetCameraFOV();
		}

		
		private void DoSetCameraFOV()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				base.camera.fieldOfView = this.fieldOfView.Value;
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Camera))]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		public FsmFloat fieldOfView;

		
		public bool everyFrame;
	}
}
