using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Input)]
	[Tooltip("Transforms 2d input into a 3d world space vector. E.g., can be used to transform input from a touch joystick to a movement vector.")]
	public class TransformInputToWorldSpace : FsmStateAction
	{
		public override void Reset()
		{
			this.horizontalInput = null;
			this.verticalInput = null;
			this.multiplier = 1f;
			this.mapToPlane = TransformInputToWorldSpace.AxisPlane.XZ;
			this.storeVector = null;
			this.storeMagnitude = null;
		}

		public override void OnUpdate()
		{
			Vector3 a = default(Vector3);
			Vector3 a2 = default(Vector3);
			if (this.relativeTo.Value == null)
			{
				TransformInputToWorldSpace.AxisPlane axisPlane = this.mapToPlane;
				if (axisPlane != TransformInputToWorldSpace.AxisPlane.XZ)
				{
					if (axisPlane != TransformInputToWorldSpace.AxisPlane.XY)
					{
						if (axisPlane == TransformInputToWorldSpace.AxisPlane.YZ)
						{
							a = Vector3.up;
							a2 = Vector3.forward;
						}
					}
					else
					{
						a = Vector3.up;
						a2 = Vector3.right;
					}
				}
				else
				{
					a = Vector3.forward;
					a2 = Vector3.right;
				}
			}
			else
			{
				Transform transform = this.relativeTo.Value.transform;
				TransformInputToWorldSpace.AxisPlane axisPlane2 = this.mapToPlane;
				if (axisPlane2 != TransformInputToWorldSpace.AxisPlane.XZ)
				{
					if (axisPlane2 == TransformInputToWorldSpace.AxisPlane.XY || axisPlane2 == TransformInputToWorldSpace.AxisPlane.YZ)
					{
						a = Vector3.up;
						a.z = 0f;
						a = a.normalized;
						a2 = transform.TransformDirection(Vector3.right);
					}
				}
				else
				{
					a = transform.TransformDirection(Vector3.forward);
					a.y = 0f;
					a = a.normalized;
					a2 = new Vector3(a.z, 0f, -a.x);
				}
			}
			float d = (!this.horizontalInput.IsNone) ? this.horizontalInput.Value : 0f;
			float d2 = (!this.verticalInput.IsNone) ? this.verticalInput.Value : 0f;
			Vector3 vector = d * a2 + d2 * a;
			vector *= this.multiplier.Value;
			this.storeVector.Value = vector;
			if (!this.storeMagnitude.IsNone)
			{
				this.storeMagnitude.Value = vector.magnitude;
			}
		}

		[UIHint(UIHint.Variable)]
		[Tooltip("The horizontal input.")]
		public FsmFloat horizontalInput;

		[UIHint(UIHint.Variable)]
		[Tooltip("The vertical input.")]
		public FsmFloat verticalInput;

		[Tooltip("Input axis are reported in the range -1 to 1, this multiplier lets you set a new range.")]
		public FsmFloat multiplier;

		[RequiredField]
		[Tooltip("The world plane to map the 2d input onto.")]
		public TransformInputToWorldSpace.AxisPlane mapToPlane;

		[Tooltip("Make the result relative to a GameObject, typically the main camera.")]
		public FsmGameObject relativeTo;

		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the direction vector.")]
		public FsmVector3 storeVector;

		[UIHint(UIHint.Variable)]
		[Tooltip("Store the length of the direction vector.")]
		public FsmFloat storeMagnitude;

		public enum AxisPlane
		{
			XZ,
			XY,
			YZ
		}
	}
}
