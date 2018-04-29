using System;
using TheForest.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Input)]
	[Tooltip("Gets a world direction Vector from 2 Input Axis. Typically used for a third person controller with Relative To set to the camera.")]
	public class GetAxisVector : FsmStateAction
	{
		
		public override void Reset()
		{
			this.horizontalAxis = "Horizontal";
			this.verticalAxis = "Vertical";
			this.multiplier = 1f;
			this.mapToPlane = GetAxisVector.AxisPlane.XZ;
			this.storeVector = null;
			this.storeMagnitude = null;
		}

		
		public override void OnUpdate()
		{
			Vector3 a = default(Vector3);
			Vector3 a2 = default(Vector3);
			if (this.relativeTo.Value == null)
			{
				GetAxisVector.AxisPlane axisPlane = this.mapToPlane;
				if (axisPlane != GetAxisVector.AxisPlane.XZ)
				{
					if (axisPlane != GetAxisVector.AxisPlane.XY)
					{
						if (axisPlane == GetAxisVector.AxisPlane.YZ)
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
				GetAxisVector.AxisPlane axisPlane2 = this.mapToPlane;
				if (axisPlane2 != GetAxisVector.AxisPlane.XZ)
				{
					if (axisPlane2 == GetAxisVector.AxisPlane.XY || axisPlane2 == GetAxisVector.AxisPlane.YZ)
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
			float d = (!this.horizontalAxis.IsNone && !string.IsNullOrEmpty(this.horizontalAxis.Value)) ? TheForest.Utils.Input.GetAxis(this.horizontalAxis.Value) : 0f;
			float d2 = (!this.verticalAxis.IsNone && !string.IsNullOrEmpty(this.verticalAxis.Value)) ? TheForest.Utils.Input.GetAxis(this.verticalAxis.Value) : 0f;
			Vector3 vector = d * a2 + d2 * a;
			vector *= this.multiplier.Value;
			this.storeVector.Value = vector;
			if (!this.storeMagnitude.IsNone)
			{
				this.storeMagnitude.Value = vector.magnitude;
			}
		}

		
		[Tooltip("The name of the horizontal input axis. See Unity Input Manager.")]
		public FsmString horizontalAxis;

		
		[Tooltip("The name of the vertical input axis. See Unity Input Manager.")]
		public FsmString verticalAxis;

		
		[Tooltip("Input axis are reported in the range -1 to 1, this multiplier lets you set a new range.")]
		public FsmFloat multiplier;

		
		[RequiredField]
		[Tooltip("The world plane to map the 2d input onto.")]
		public GetAxisVector.AxisPlane mapToPlane;

		
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
