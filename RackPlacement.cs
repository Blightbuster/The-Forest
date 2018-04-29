using System;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
public class RackPlacement
{
	
	public void ApplyTo(ref Vector3 itemLocalPos, ref Quaternion itemLocalRot, ref Vector3 itemScale)
	{
		itemLocalPos = this.Position;
		itemLocalRot = Quaternion.Euler(this.Rotation);
		itemScale = this.Scale;
	}

	
	[FormerlySerializedAs("Type")]
	public RackPlacementTypes PlacementType;

	
	public Vector3 Position;

	
	public Vector3 Scale;

	
	public Vector3 Rotation;
}
