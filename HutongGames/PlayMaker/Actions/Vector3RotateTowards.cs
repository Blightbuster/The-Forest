using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Rotates a Vector3 direction from Current towards Target.")]
	[ActionCategory(ActionCategory.Vector3)]
	public class Vector3RotateTowards : FsmStateAction
	{
		
		public override void Reset()
		{
			this.currentDirection = new FsmVector3
			{
				UseVariable = true
			};
			this.targetDirection = new FsmVector3
			{
				UseVariable = true
			};
			this.rotateSpeed = 360f;
			this.maxMagnitude = 1f;
		}

		
		public override void OnUpdate()
		{
			this.currentDirection.Value = Vector3.RotateTowards(this.currentDirection.Value, this.targetDirection.Value, this.rotateSpeed.Value * 0.0174532924f * Time.deltaTime, this.maxMagnitude.Value);
		}

		
		[RequiredField]
		public FsmVector3 currentDirection;

		
		[RequiredField]
		public FsmVector3 targetDirection;

		
		[RequiredField]
		[Tooltip("Rotation speed in degrees per second")]
		public FsmFloat rotateSpeed;

		
		[RequiredField]
		[Tooltip("Max Magnitude per second")]
		public FsmFloat maxMagnitude;
	}
}
