using System;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public class SteamVR_TrackedObject : MonoBehaviour
{
	private SteamVR_TrackedObject()
	{
		this.newPosesAction = SteamVR_Events.NewPosesAction(new UnityAction<TrackedDevicePose_t[]>(this.OnNewPoses));
	}

	public bool isValid { get; private set; }

	private void OnNewPoses(TrackedDevicePose_t[] poses)
	{
		if (this.index == SteamVR_TrackedObject.EIndex.None)
		{
			return;
		}
		int num = (int)this.index;
		this.isValid = false;
		if (poses.Length <= num)
		{
			return;
		}
		if (!poses[num].bDeviceIsConnected)
		{
			return;
		}
		if (!poses[num].bPoseIsValid)
		{
			return;
		}
		this.isValid = true;
		SteamVR_Utils.RigidTransform rigidTransform = new SteamVR_Utils.RigidTransform(poses[num].mDeviceToAbsoluteTracking);
		if (this.origin != null)
		{
			base.transform.position = this.origin.transform.TransformPoint(rigidTransform.pos);
			base.transform.rotation = this.origin.rotation * rigidTransform.rot;
		}
		else
		{
			base.transform.localPosition = rigidTransform.pos;
			base.transform.localRotation = rigidTransform.rot;
		}
	}

	private void OnEnable()
	{
		SteamVR_Render instance = SteamVR_Render.instance;
		if (instance == null)
		{
			base.enabled = false;
			return;
		}
		this.newPosesAction.enabled = true;
	}

	private void OnDisable()
	{
		this.newPosesAction.enabled = false;
		this.isValid = false;
	}

	public void SetDeviceIndex(int index)
	{
		if (Enum.IsDefined(typeof(SteamVR_TrackedObject.EIndex), index))
		{
			this.index = (SteamVR_TrackedObject.EIndex)index;
		}
	}

	public SteamVR_TrackedObject.EIndex index;

	[Tooltip("If not set, relative to parent")]
	public Transform origin;

	private SteamVR_Events.Action newPosesAction;

	public enum EIndex
	{
		None = -1,
		Hmd,
		Device1,
		Device2,
		Device3,
		Device4,
		Device5,
		Device6,
		Device7,
		Device8,
		Device9,
		Device10,
		Device11,
		Device12,
		Device13,
		Device14,
		Device15
	}
}
