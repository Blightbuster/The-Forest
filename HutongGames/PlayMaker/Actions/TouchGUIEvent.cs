using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Device)]
	[Tooltip("Sends events when a GUI Texture or GUI Text is touched. Optionally filter by a fingerID.")]
	public class TouchGUIEvent : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.fingerId = new FsmInt
			{
				UseVariable = true
			};
			this.touchBegan = null;
			this.touchMoved = null;
			this.touchStationary = null;
			this.touchEnded = null;
			this.touchCanceled = null;
			this.storeFingerId = null;
			this.storeHitPoint = null;
			this.normalizeHitPoint = false;
			this.storeOffset = null;
			this.relativeTo = TouchGUIEvent.OffsetOptions.Center;
			this.normalizeOffset = true;
			this.everyFrame = true;
		}

		
		public override void OnEnter()
		{
			this.DoTouchGUIEvent();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoTouchGUIEvent();
		}

		
		private void DoTouchGUIEvent()
		{
			if (Input.touchCount > 0)
			{
				GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
				if (ownerDefaultTarget == null)
				{
					return;
				}
				this.guiElement = (ownerDefaultTarget.GetComponent<GUITexture>() ?? ownerDefaultTarget.GetComponent<GUIText>());
				if (this.guiElement == null)
				{
					return;
				}
				foreach (Touch touch in Input.touches)
				{
					this.DoTouch(touch);
				}
			}
		}

		
		private void DoTouch(Touch touch)
		{
			if (this.fingerId.IsNone || touch.fingerId == this.fingerId.Value)
			{
				Vector3 vector = touch.position;
				if (this.guiElement.HitTest(vector))
				{
					if (touch.phase == TouchPhase.Began)
					{
						this.touchStartPos = vector;
					}
					this.storeFingerId.Value = touch.fingerId;
					if (this.normalizeHitPoint.Value)
					{
						vector.x /= (float)Screen.width;
						vector.y /= (float)Screen.height;
					}
					this.storeHitPoint.Value = vector;
					this.DoTouchOffset(vector);
					switch (touch.phase)
					{
					case TouchPhase.Began:
						base.Fsm.Event(this.touchBegan);
						return;
					case TouchPhase.Moved:
						base.Fsm.Event(this.touchMoved);
						return;
					case TouchPhase.Stationary:
						base.Fsm.Event(this.touchStationary);
						return;
					case TouchPhase.Ended:
						base.Fsm.Event(this.touchEnded);
						return;
					case TouchPhase.Canceled:
						base.Fsm.Event(this.touchCanceled);
						return;
					}
				}
				else
				{
					base.Fsm.Event(this.notTouching);
				}
			}
		}

		
		private void DoTouchOffset(Vector3 touchPos)
		{
			if (this.storeOffset.IsNone)
			{
				return;
			}
			Rect screenRect = this.guiElement.GetScreenRect();
			Vector3 value = default(Vector3);
			TouchGUIEvent.OffsetOptions offsetOptions = this.relativeTo;
			if (offsetOptions != TouchGUIEvent.OffsetOptions.TopLeft)
			{
				if (offsetOptions != TouchGUIEvent.OffsetOptions.Center)
				{
					if (offsetOptions == TouchGUIEvent.OffsetOptions.TouchStart)
					{
						value = touchPos - this.touchStartPos;
					}
				}
				else
				{
					Vector3 b = new Vector3(screenRect.x + screenRect.width * 0.5f, screenRect.y + screenRect.height * 0.5f, 0f);
					value = touchPos - b;
				}
			}
			else
			{
				value.x = touchPos.x - screenRect.x;
				value.y = touchPos.y - screenRect.y;
			}
			if (this.normalizeOffset.Value)
			{
				value.x /= screenRect.width;
				value.y /= screenRect.height;
			}
			this.storeOffset.Value = value;
		}

		
		[RequiredField]
		[CheckForComponent(typeof(GUIElement))]
		[Tooltip("The Game Object that owns the GUI Texture or GUI Text.")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Only detect touches that match this fingerID, or set to None.")]
		public FsmInt fingerId;

		
		[ActionSection("Events")]
		[Tooltip("Event to send on touch began.")]
		public FsmEvent touchBegan;

		
		[Tooltip("Event to send on touch moved.")]
		public FsmEvent touchMoved;

		
		[Tooltip("Event to send on stationary touch.")]
		public FsmEvent touchStationary;

		
		[Tooltip("Event to send on touch ended.")]
		public FsmEvent touchEnded;

		
		[Tooltip("Event to send on touch cancel.")]
		public FsmEvent touchCanceled;

		
		[Tooltip("Event to send if not touching (finger down but not over the GUI element)")]
		public FsmEvent notTouching;

		
		[ActionSection("Store Results")]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the fingerId of the touch.")]
		public FsmInt storeFingerId;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the screen position where the GUI element was touched.")]
		public FsmVector3 storeHitPoint;

		
		[Tooltip("Normalize the hit point screen coordinates (0-1).")]
		public FsmBool normalizeHitPoint;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the offset position of the hit.")]
		public FsmVector3 storeOffset;

		
		[Tooltip("How to measure the offset.")]
		public TouchGUIEvent.OffsetOptions relativeTo;

		
		[Tooltip("Normalize the offset.")]
		public FsmBool normalizeOffset;

		
		[ActionSection("")]
		[Tooltip("Repeate every frame.")]
		public bool everyFrame;

		
		private Vector3 touchStartPos;

		
		private GUIElement guiElement;

		
		public enum OffsetOptions
		{
			
			TopLeft,
			
			Center,
			
			TouchStart
		}
	}
}
