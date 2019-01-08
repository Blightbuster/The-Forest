using System;
using UnityEngine;

public class OVRTracker
{
	public bool isPresent
	{
		get
		{
			return OVRManager.isHmdPresent && OVRPlugin.positionSupported;
		}
	}

	public bool isPositionTracked
	{
		get
		{
			return OVRPlugin.positionTracked;
		}
	}

	public bool isEnabled
	{
		get
		{
			return OVRManager.isHmdPresent && OVRPlugin.position;
		}
		set
		{
			if (!OVRManager.isHmdPresent)
			{
				return;
			}
			OVRPlugin.position = value;
		}
	}

	public int count
	{
		get
		{
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				if (this.GetPresent(i))
				{
					num++;
				}
			}
			return num;
		}
	}

	public OVRTracker.Frustum GetFrustum(int tracker = 0)
	{
		if (!OVRManager.isHmdPresent)
		{
			return default(OVRTracker.Frustum);
		}
		return OVRPlugin.GetTrackerFrustum((OVRPlugin.Tracker)tracker).ToFrustum();
	}

	public OVRPose GetPose(int tracker = 0)
	{
		if (!OVRManager.isHmdPresent)
		{
			return OVRPose.identity;
		}
		OVRPose ovrpose;
		switch (tracker)
		{
		case 0:
			ovrpose = OVRPlugin.GetNodePose(OVRPlugin.Node.TrackerZero, OVRPlugin.Step.Render).ToOVRPose();
			break;
		case 1:
			ovrpose = OVRPlugin.GetNodePose(OVRPlugin.Node.TrackerOne, OVRPlugin.Step.Render).ToOVRPose();
			break;
		case 2:
			ovrpose = OVRPlugin.GetNodePose(OVRPlugin.Node.TrackerTwo, OVRPlugin.Step.Render).ToOVRPose();
			break;
		case 3:
			ovrpose = OVRPlugin.GetNodePose(OVRPlugin.Node.TrackerThree, OVRPlugin.Step.Render).ToOVRPose();
			break;
		default:
			return OVRPose.identity;
		}
		return new OVRPose
		{
			position = ovrpose.position,
			orientation = ovrpose.orientation * Quaternion.Euler(0f, 180f, 0f)
		};
	}

	public bool GetPoseValid(int tracker = 0)
	{
		if (!OVRManager.isHmdPresent)
		{
			return false;
		}
		switch (tracker)
		{
		case 0:
			return OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.TrackerZero);
		case 1:
			return OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.TrackerOne);
		case 2:
			return OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.TrackerTwo);
		case 3:
			return OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.TrackerThree);
		default:
			return false;
		}
	}

	public bool GetPresent(int tracker = 0)
	{
		if (!OVRManager.isHmdPresent)
		{
			return false;
		}
		switch (tracker)
		{
		case 0:
			return OVRPlugin.GetNodePresent(OVRPlugin.Node.TrackerZero);
		case 1:
			return OVRPlugin.GetNodePresent(OVRPlugin.Node.TrackerOne);
		case 2:
			return OVRPlugin.GetNodePresent(OVRPlugin.Node.TrackerTwo);
		case 3:
			return OVRPlugin.GetNodePresent(OVRPlugin.Node.TrackerThree);
		default:
			return false;
		}
	}

	public struct Frustum
	{
		public float nearZ;

		public float farZ;

		public Vector2 fov;
	}
}
