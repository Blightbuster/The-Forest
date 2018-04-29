﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Camera)]
	[Tooltip("Sets the Culling Mask used by the Camera.")]
	public class SetCameraCullingMask : ComponentAction<Camera>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.cullingMask = new FsmInt[0];
			this.invertMask = false;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoSetCameraCullingMask();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoSetCameraCullingMask();
		}

		
		private void DoSetCameraCullingMask()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				base.camera.cullingMask = ActionHelpers.LayerArrayToLayerMask(this.cullingMask, this.invertMask.Value);
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Camera))]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Cull these layers.")]
		[UIHint(UIHint.Layer)]
		public FsmInt[] cullingMask;

		
		[Tooltip("Invert the mask, so you cull all layers except those defined above.")]
		public FsmBool invertMask;

		
		public bool everyFrame;
	}
}
