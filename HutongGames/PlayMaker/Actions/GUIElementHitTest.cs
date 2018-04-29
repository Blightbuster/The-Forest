﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.GUIElement)]
	[Tooltip("Performs a Hit Test on a Game Object with a GUITexture or GUIText component.")]
	public class GUIElementHitTest : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.camera = null;
			this.screenPoint = new FsmVector3
			{
				UseVariable = true
			};
			this.screenX = new FsmFloat
			{
				UseVariable = true
			};
			this.screenY = new FsmFloat
			{
				UseVariable = true
			};
			this.normalized = true;
			this.hitEvent = null;
			this.everyFrame = true;
		}

		
		public override void OnEnter()
		{
			this.DoHitTest();
			if (!this.everyFrame.Value)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoHitTest();
		}

		
		private void DoHitTest()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				return;
			}
			if (ownerDefaultTarget != this.gameObjectCached)
			{
				this.guiElement = (ownerDefaultTarget.GetComponent<GUITexture>() ?? ownerDefaultTarget.GetComponent<GUIText>());
				this.gameObjectCached = ownerDefaultTarget;
			}
			if (this.guiElement == null)
			{
				base.Finish();
				return;
			}
			Vector3 screenPosition = (!this.screenPoint.IsNone) ? this.screenPoint.Value : new Vector3(0f, 0f);
			if (!this.screenX.IsNone)
			{
				screenPosition.x = this.screenX.Value;
			}
			if (!this.screenY.IsNone)
			{
				screenPosition.y = this.screenY.Value;
			}
			if (this.normalized.Value)
			{
				screenPosition.x *= (float)Screen.width;
				screenPosition.y *= (float)Screen.height;
			}
			if (this.guiElement.HitTest(screenPosition, this.camera))
			{
				this.storeResult.Value = true;
				base.Fsm.Event(this.hitEvent);
			}
			else
			{
				this.storeResult.Value = false;
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(GUIElement))]
		[Tooltip("The GameObject that has a GUITexture or GUIText component.")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Specify camera or use MainCamera as default.")]
		public Camera camera;

		
		[Tooltip("A vector position on screen. Usually stored by actions like GetTouchInfo, or World To Screen Point.")]
		public FsmVector3 screenPoint;

		
		[Tooltip("Specify screen X coordinate.")]
		public FsmFloat screenX;

		
		[Tooltip("Specify screen Y coordinate.")]
		public FsmFloat screenY;

		
		[Tooltip("Whether the specified screen coordinates are normalized (0-1).")]
		public FsmBool normalized;

		
		[Tooltip("Event to send if the Hit Test is true.")]
		public FsmEvent hitEvent;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result of the Hit Test in a bool variable (true/false).")]
		public FsmBool storeResult;

		
		[Tooltip("Repeat every frame. Useful if you want to wait for the hit test to return true.")]
		public FsmBool everyFrame;

		
		private GUIElement guiElement;

		
		private GameObject gameObjectCached;
	}
}
