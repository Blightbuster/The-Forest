using System;
using UnityEngine;

public class OVRProfile : UnityEngine.Object
{
	[Obsolete]
	public string id
	{
		get
		{
			return "000abc123def";
		}
	}

	[Obsolete]
	public string userName
	{
		get
		{
			return "Oculus User";
		}
	}

	[Obsolete]
	public string locale
	{
		get
		{
			return "en_US";
		}
	}

	public float ipd
	{
		get
		{
			return Vector3.Distance(OVRPlugin.GetNodePose(OVRPlugin.Node.EyeLeft, OVRPlugin.Step.Render).ToOVRPose().position, OVRPlugin.GetNodePose(OVRPlugin.Node.EyeRight, OVRPlugin.Step.Render).ToOVRPose().position);
		}
	}

	public float eyeHeight
	{
		get
		{
			return OVRPlugin.eyeHeight;
		}
	}

	public float eyeDepth
	{
		get
		{
			return OVRPlugin.eyeDepth;
		}
	}

	public float neckHeight
	{
		get
		{
			return this.eyeHeight - 0.075f;
		}
	}

	[Obsolete]
	public OVRProfile.State state
	{
		get
		{
			return OVRProfile.State.READY;
		}
	}

	[Obsolete]
	public enum State
	{
		NOT_TRIGGERED,
		LOADING,
		READY,
		ERROR
	}
}
