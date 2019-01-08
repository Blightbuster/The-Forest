using System;
using System.Collections.Generic;
using UnityEngine;

public class mutantTransferFire : MonoBehaviour
{
	public void gatherBones()
	{
		if (this.skin == null)
		{
			Debug.Log("no skin mesh found");
			return;
		}
		this.allBones.Clear();
		Transform[] bones = this.skin.bones;
		foreach (Transform transform in bones)
		{
			if (!this.allBones.Contains(transform) && !transform.GetComponent<MeshFilter>() && !transform.GetComponent<weaponInfo>())
			{
				this.allBones.Add(transform);
			}
		}
	}

	public void gatherBonesFromNestedFire()
	{
		this.allBones.Clear();
		spawnParticleController[] componentsInChildren = base.transform.GetComponentsInChildren<spawnParticleController>(true);
		if (componentsInChildren.Length == 0)
		{
			Transform[] componentsInChildren2 = base.transform.GetComponentsInChildren<Transform>(true);
			foreach (Transform transform in componentsInChildren2)
			{
				if (transform.name.Contains("Fire"))
				{
					this.allBones.Add(transform.parent);
				}
			}
		}
		else if (componentsInChildren.Length > 0)
		{
			foreach (spawnParticleController spawnParticleController in componentsInChildren)
			{
				this.allBones.Add(spawnParticleController.transform.parent);
			}
		}
	}

	public void transferFireToTarget(GameObject fire, GameObject target)
	{
		spawnParticleController component = fire.GetComponent<spawnParticleController>();
		mutantTransferFire component2 = target.GetComponent<mutantTransferFire>();
		if (component && component.spawnedPrefab && component2)
		{
			component.spawnedPrefab.SendMessage("disableFollowTarget");
			int index = this.allBones.IndexOf(fire.transform.parent);
			component.spawnedPrefab.transform.parent = component2.allBones[index];
			component.spawnedPrefab.transform.localPosition = fire.transform.localPosition;
			component.spawnedPrefab.transform.localRotation = fire.transform.localRotation;
		}
	}

	public List<Transform> allBones = new List<Transform>();

	public List<GameObject> clientFireGo = new List<GameObject>();

	public SkinnedMeshRenderer skin;
}
