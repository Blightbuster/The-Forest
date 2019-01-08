using System;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB2_UpdateSkinnedMeshBoundsFromBones : MonoBehaviour
{
	private void Start()
	{
		this.smr = base.GetComponent<SkinnedMeshRenderer>();
		if (this.smr == null)
		{
			Debug.LogError("Need to attach MB2_UpdateSkinnedMeshBoundsFromBones script to an object with a SkinnedMeshRenderer component attached.");
			return;
		}
		this.bones = this.smr.bones;
		this.smr.updateWhenOffscreen = true;
		this.smr.updateWhenOffscreen = false;
	}

	private void Update()
	{
		if (this.smr != null)
		{
			MB3_MeshCombiner.UpdateSkinnedMeshApproximateBoundsFromBonesStatic(this.bones, this.smr);
		}
	}

	private SkinnedMeshRenderer smr;

	private Transform[] bones;
}
