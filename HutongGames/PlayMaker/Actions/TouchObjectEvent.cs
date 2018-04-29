using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Device)]
	[Tooltip("Sends events when an object is touched. Optionally filter by a fingerID. NOTE: Uses the MainCamera!")]
	public class TouchObjectEvent : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.pickDistance = 100f;
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
			this.storeHitNormal = null;
		}

		
		public override void OnUpdate()
		{
			if (Camera.main == null)
			{
				base.LogError("No MainCamera defined!");
				base.Finish();
				return;
			}
			if (Input.touchCount > 0)
			{
				GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
				if (ownerDefaultTarget == null)
				{
					return;
				}
				foreach (Touch touch in Input.touches)
				{
					if (this.fingerId.IsNone || touch.fingerId == this.fingerId.Value)
					{
						Vector2 position = touch.position;
						RaycastHit raycastHitInfo;
						Physics.Raycast(Camera.main.ScreenPointToRay(position), out raycastHitInfo, this.pickDistance.Value);
						base.Fsm.RaycastHitInfo = raycastHitInfo;
						if (raycastHitInfo.transform != null && raycastHitInfo.transform.gameObject == ownerDefaultTarget)
						{
							this.storeFingerId.Value = touch.fingerId;
							this.storeHitPoint.Value = raycastHitInfo.point;
							this.storeHitNormal.Value = raycastHitInfo.normal;
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
					}
				}
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Collider))]
		[Tooltip("The Game Object to detect touches on.")]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		[Tooltip("How far from the camera is the Game Object pickable.")]
		public FsmFloat pickDistance;

		
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

		
		[ActionSection("Store Results")]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the fingerId of the touch.")]
		public FsmInt storeFingerId;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the world position where the object was touched.")]
		public FsmVector3 storeHitPoint;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the surface normal vector where the object was touched.")]
		public FsmVector3 storeHitNormal;
	}
}
