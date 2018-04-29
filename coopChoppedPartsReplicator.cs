using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;


public class coopChoppedPartsReplicator : CoopBase<IDynamicPickup>
{
	
	public override void Attached()
	{
		if (!base.entity.isOwner)
		{
			base.StartCoroutine(this.syncPartsRoutine());
		}
	}

	
	private IEnumerator syncPartsRoutine()
	{
		yield return new WaitForFixedUpdate();
		float closestDist = float.PositiveInfinity;
		GameObject closestStoredToken = null;
		for (int i = 0; i < Scene.SceneTracker.storedRagDollPrefabs.Count; i++)
		{
			if (Scene.SceneTracker.storedRagDollPrefabs[i] != null)
			{
				float dist = (base.transform.position - Scene.SceneTracker.storedRagDollPrefabs[i].transform.position).sqrMagnitude;
				if (dist < closestDist)
				{
					closestDist = dist;
					closestStoredToken = Scene.SceneTracker.storedRagDollPrefabs[i];
				}
			}
		}
		if (closestStoredToken && closestDist < 9f)
		{
			this.slmi = closestStoredToken.transform.GetComponent<storeLocalMutantInfo2>();
			if (this.slmi)
			{
				if (this.slmi.showHair)
				{
					enableGoReceiver componentInChildren = base.transform.GetComponentInChildren<enableGoReceiver>();
					if (componentInChildren)
					{
						componentInChildren.doEnableGo();
					}
				}
				if (this.slmi.showMask && this.faceMask)
				{
					this.faceMask.SetActive(true);
				}
				this.setupCutSkin();
			}
		}
		yield return null;
		if (closestStoredToken)
		{
			UnityEngine.Object.Destroy(closestStoredToken);
		}
		Scene.SceneTracker.storedRagDollPrefabs.RemoveAll((GameObject o) => o == null);
		Scene.SceneTracker.storedRagDollPrefabs.TrimExcess();
		yield break;
	}

	
	private void setupCutSkin()
	{
		MeshRenderer meshRenderer = null;
		getMesh componentInChildren = base.transform.GetComponentInChildren<getMesh>();
		if (componentInChildren)
		{
			meshRenderer = componentInChildren.transform.GetComponent<MeshRenderer>();
		}
		if (meshRenderer)
		{
			for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
			{
				if (!meshRenderer.sharedMaterials[i].name.Contains("JustBlood"))
				{
					if (this.slmi.mat != null)
					{
						Material[] materials = meshRenderer.materials;
						materials[i] = this.slmi.mat;
						meshRenderer.materials = materials;
					}
					MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
					materialPropertyBlock.SetColor("_Color", this.slmi.matColor);
					if (this.slmi.bloodPropertyBlock != null)
					{
						materialPropertyBlock.SetFloat("_Damage1", this.slmi.bloodPropertyBlock.GetFloat("_Damage1"));
						materialPropertyBlock.SetFloat("_Damage2", this.slmi.bloodPropertyBlock.GetFloat("_Damage2"));
						materialPropertyBlock.SetFloat("_Damage3", this.slmi.bloodPropertyBlock.GetFloat("_Damage3"));
						materialPropertyBlock.SetFloat("_Damage4", this.slmi.bloodPropertyBlock.GetFloat("_Damage4"));
					}
					meshRenderer.SetPropertyBlock(materialPropertyBlock);
				}
			}
		}
	}

	
	public bool syncBodyParts;

	
	private storeLocalMutantInfo2 slmi;

	
	public GameObject faceMask;
}
