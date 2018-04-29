using System;
using UnityEngine;


public class AeroplaneControlSurfaceAnimator : MonoBehaviour
{
	
	private void Start()
	{
		this.plane = base.GetComponent<AeroplaneController>();
		foreach (AeroplaneControlSurfaceAnimator.ControlSurface controlSurface in this.controlSurfaces)
		{
			controlSurface.originalLocalRotation = controlSurface.transform.localRotation;
		}
	}

	
	private void Update()
	{
		foreach (AeroplaneControlSurfaceAnimator.ControlSurface controlSurface in this.controlSurfaces)
		{
			switch (controlSurface.type)
			{
			case AeroplaneControlSurfaceAnimator.ControlSurface.Type.Aileron:
			{
				Quaternion rotation = Quaternion.Euler(controlSurface.amount * this.plane.RollInput, 0f, 0f);
				this.RotateSurface(controlSurface, rotation);
				break;
			}
			case AeroplaneControlSurfaceAnimator.ControlSurface.Type.Elevator:
			{
				Quaternion rotation2 = Quaternion.Euler(controlSurface.amount * -this.plane.PitchInput, 0f, 0f);
				this.RotateSurface(controlSurface, rotation2);
				break;
			}
			case AeroplaneControlSurfaceAnimator.ControlSurface.Type.Rudder:
			{
				Quaternion rotation3 = Quaternion.Euler(0f, controlSurface.amount * this.plane.YawInput, 0f);
				this.RotateSurface(controlSurface, rotation3);
				break;
			}
			case AeroplaneControlSurfaceAnimator.ControlSurface.Type.RuddervatorNegative:
			{
				float num = this.plane.YawInput - this.plane.PitchInput;
				Quaternion rotation4 = Quaternion.Euler(0f, 0f, controlSurface.amount * num);
				this.RotateSurface(controlSurface, rotation4);
				break;
			}
			case AeroplaneControlSurfaceAnimator.ControlSurface.Type.RuddervatorPositive:
			{
				float num2 = this.plane.YawInput + this.plane.PitchInput;
				Quaternion rotation5 = Quaternion.Euler(0f, 0f, controlSurface.amount * num2);
				this.RotateSurface(controlSurface, rotation5);
				break;
			}
			}
		}
	}

	
	private void RotateSurface(AeroplaneControlSurfaceAnimator.ControlSurface surface, Quaternion rotation)
	{
		Quaternion b = surface.originalLocalRotation * rotation;
		surface.transform.localRotation = Quaternion.Slerp(surface.transform.localRotation, b, this.smoothing * Time.deltaTime);
	}

	
	[SerializeField]
	private float smoothing = 5f;

	
	[SerializeField]
	private AeroplaneControlSurfaceAnimator.ControlSurface[] controlSurfaces;

	
	private AeroplaneController plane;

	
	[Serializable]
	public class ControlSurface
	{
		
		public Transform transform;

		
		public float amount;

		
		public AeroplaneControlSurfaceAnimator.ControlSurface.Type type;

		
		[HideInInspector]
		public Quaternion originalLocalRotation;

		
		public enum Type
		{
			
			Aileron,
			
			Elevator,
			
			Rudder,
			
			RuddervatorNegative,
			
			RuddervatorPositive
		}
	}
}
