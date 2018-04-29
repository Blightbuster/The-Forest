using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Input)]
	[Tooltip("Rotates a GameObject based on mouse movement. Minimum and Maximum values can be used to constrain the rotation.")]
	public class MouseLook2 : ComponentAction<Rigidbody>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.axes = MouseLook2.RotationAxes.MouseXAndY;
			this.sensitivityX = 15f;
			this.sensitivityY = 15f;
			this.minimumX = -360f;
			this.maximumX = 360f;
			this.minimumY = -60f;
			this.maximumY = 60f;
			this.everyFrame = true;
		}

		
		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				base.Finish();
				return;
			}
			if (!base.UpdateCache(ownerDefaultTarget) && base.rigidbody)
			{
				base.rigidbody.freezeRotation = true;
			}
			this.DoMouseLook();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoMouseLook();
		}

		
		private void DoMouseLook()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				return;
			}
			Transform transform = ownerDefaultTarget.transform;
			MouseLook2.RotationAxes rotationAxes = this.axes;
			if (rotationAxes != MouseLook2.RotationAxes.MouseXAndY)
			{
				if (rotationAxes != MouseLook2.RotationAxes.MouseX)
				{
					if (rotationAxes == MouseLook2.RotationAxes.MouseY)
					{
						transform.localEulerAngles = new Vector3(-this.GetYRotation(), transform.localEulerAngles.y, 0f);
					}
				}
				else
				{
					transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, this.GetXRotation(), 0f);
				}
			}
			else
			{
				transform.localEulerAngles = new Vector3(this.GetYRotation(), this.GetXRotation(), 0f);
			}
		}

		
		private float GetXRotation()
		{
			this.rotationX += Input.GetAxis("Mouse X") * this.sensitivityX.Value;
			this.rotationX = MouseLook2.ClampAngle(this.rotationX, this.minimumX, this.maximumX);
			return this.rotationX;
		}

		
		private float GetYRotation()
		{
			this.rotationY += Input.GetAxis("Mouse Y") * this.sensitivityY.Value;
			this.rotationY = MouseLook2.ClampAngle(this.rotationY, this.minimumY, this.maximumY);
			return this.rotationY;
		}

		
		private static float ClampAngle(float angle, FsmFloat min, FsmFloat max)
		{
			if (!min.IsNone && angle < min.Value)
			{
				angle = min.Value;
			}
			if (!max.IsNone && angle > max.Value)
			{
				angle = max.Value;
			}
			return angle;
		}

		
		[RequiredField]
		[Tooltip("The GameObject to rotate.")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("The axes to rotate around.")]
		public MouseLook2.RotationAxes axes;

		
		[RequiredField]
		public FsmFloat sensitivityX;

		
		[RequiredField]
		public FsmFloat sensitivityY;

		
		[RequiredField]
		[HasFloatSlider(-360f, 360f)]
		public FsmFloat minimumX;

		
		[RequiredField]
		[HasFloatSlider(-360f, 360f)]
		public FsmFloat maximumX;

		
		[RequiredField]
		[HasFloatSlider(-360f, 360f)]
		public FsmFloat minimumY;

		
		[RequiredField]
		[HasFloatSlider(-360f, 360f)]
		public FsmFloat maximumY;

		
		[Tooltip("Repeat every frame.")]
		public bool everyFrame;

		
		private float rotationX;

		
		private float rotationY;

		
		public enum RotationAxes
		{
			
			MouseXAndY,
			
			MouseX,
			
			MouseY
		}
	}
}
