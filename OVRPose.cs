using System;
using UnityEngine;


[Serializable]
public struct OVRPose
{
	
	
	public static OVRPose identity
	{
		get
		{
			return new OVRPose
			{
				position = Vector3.zero,
				orientation = Quaternion.identity
			};
		}
	}

	
	public override bool Equals(object obj)
	{
		return obj is OVRPose && this == (OVRPose)obj;
	}

	
	public override int GetHashCode()
	{
		return this.position.GetHashCode() ^ this.orientation.GetHashCode();
	}

	
	public static bool operator ==(OVRPose x, OVRPose y)
	{
		return x.position == y.position && x.orientation == y.orientation;
	}

	
	public static bool operator !=(OVRPose x, OVRPose y)
	{
		return !(x == y);
	}

	
	public static OVRPose operator *(OVRPose lhs, OVRPose rhs)
	{
		return new OVRPose
		{
			position = lhs.position + lhs.orientation * rhs.position,
			orientation = lhs.orientation * rhs.orientation
		};
	}

	
	public OVRPose Inverse()
	{
		OVRPose result;
		result.orientation = Quaternion.Inverse(this.orientation);
		result.position = result.orientation * -this.position;
		return result;
	}

	
	internal OVRPose flipZ()
	{
		OVRPose result = this;
		result.position.z = -result.position.z;
		result.orientation.z = -result.orientation.z;
		result.orientation.w = -result.orientation.w;
		return result;
	}

	
	internal OVRPlugin.Posef ToPosef()
	{
		return new OVRPlugin.Posef
		{
			Position = this.position.ToVector3f(),
			Orientation = this.orientation.ToQuatf()
		};
	}

	
	public Vector3 position;

	
	public Quaternion orientation;
}
