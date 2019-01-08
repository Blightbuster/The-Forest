using System;
using UnityEngine;

public abstract class OVRComposition
{
	public abstract OVRManager.CompositionMethod CompositionMethod();

	public abstract void Update(Camera mainCamera);

	public abstract void Cleanup();

	public virtual void RecenterPose()
	{
	}

	internal OVRPose ComputeCameraWorldSpacePose(OVRPlugin.CameraExtrinsics extrinsics)
	{
		OVRPose ovrpose = default(OVRPose);
		OVRPose ovrpose2 = default(OVRPose);
		OVRPose ovrpose3 = extrinsics.RelativePose.ToOVRPose();
		ovrpose2 = ovrpose3;
		if (extrinsics.AttachedToNode != OVRPlugin.Node.None && OVRPlugin.GetNodePresent(extrinsics.AttachedToNode))
		{
			if (this.usingLastAttachedNodePose)
			{
				Debug.Log("The camera attached node get tracked");
				this.usingLastAttachedNodePose = false;
			}
			OVRPose lhs = OVRPlugin.GetNodePose(extrinsics.AttachedToNode, OVRPlugin.Step.Render).ToOVRPose();
			this.lastAttachedNodePose = lhs;
			ovrpose2 = lhs * ovrpose2;
		}
		else if (extrinsics.AttachedToNode != OVRPlugin.Node.None)
		{
			if (!this.usingLastAttachedNodePose)
			{
				Debug.LogWarning("The camera attached node could not be tracked, using the last pose");
				this.usingLastAttachedNodePose = true;
			}
			ovrpose2 = this.lastAttachedNodePose * ovrpose2;
		}
		return OVRExtensions.ToWorldSpacePose(ovrpose2);
	}

	protected bool usingLastAttachedNodePose;

	protected OVRPose lastAttachedNodePose = default(OVRPose);
}
