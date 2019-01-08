using System;
using UnityEngine;

public class copyBloodFromTarget : MonoBehaviour
{
	private void OnEnable()
	{
		this.skin = base.transform.GetComponent<SkinnedMeshRenderer>();
		this.copyBloodAndSkin();
	}

	private void copyBloodAndSkin()
	{
		this.skin.sharedMaterial = this.targetSkin.sharedMaterial;
		this.block = new MaterialPropertyBlock();
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		this.targetSkin.GetPropertyBlock(materialPropertyBlock);
		this.skin.GetPropertyBlock(this.block);
		this.block.SetFloat("_Damage1", materialPropertyBlock.GetFloat("_Damage1"));
		this.block.SetFloat("_Damage2", materialPropertyBlock.GetFloat("_Damage2"));
		this.block.SetFloat("_Damage3", materialPropertyBlock.GetFloat("_Damage3"));
		this.block.SetFloat("_Damage4", materialPropertyBlock.GetFloat("_Damage4"));
		this.skin.SetPropertyBlock(this.block);
	}

	private MaterialPropertyBlock block;

	public SkinnedMeshRenderer targetSkin;

	private SkinnedMeshRenderer skin;
}
