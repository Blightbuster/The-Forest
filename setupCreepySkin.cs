using System;
using System.Collections.Generic;
using UnityEngine;


public class setupCreepySkin : MonoBehaviour
{
	
	private void setSkin(Material mat)
	{
		if (this.skin)
		{
			this.skin.sharedMaterial = mat;
		}
		if (this.explodedPrefab)
		{
			for (int i = 0; i < this.skinParts.Count; i++)
			{
				if (this.skinParts[i])
				{
					this.skinParts[i].material = mat;
				}
			}
		}
	}

	
	private void setRagdollBloody()
	{
		this.bloodPropertyBlock = new MaterialPropertyBlock();
		this.skin.GetPropertyBlock(this.bloodPropertyBlock);
		this.bloodPropertyBlock.SetFloat("_Damage1", 1f);
		this.bloodPropertyBlock.SetFloat("_Damage2", 1f);
		this.bloodPropertyBlock.SetFloat("_Damage3", 1f);
		this.bloodPropertyBlock.SetFloat("_Damage4", 1f);
		this.skin.SetPropertyBlock(this.bloodPropertyBlock);
		if (this.cutBodySkin)
		{
			this.cutBodySkin.SetPropertyBlock(this.bloodPropertyBlock);
		}
		if (this.cutHeadSkin)
		{
			this.cutHeadSkin.SetPropertyBlock(this.bloodPropertyBlock);
		}
	}

	
	private void setSkinDamageProperty(MaterialPropertyBlock block)
	{
		if (this.skin)
		{
			this.skin.SetPropertyBlock(block);
		}
	}

	
	private MaterialPropertyBlock bloodPropertyBlock;

	
	public SkinnedMeshRenderer skin;

	
	public SkinnedMeshRenderer cutBodySkin;

	
	public SkinnedMeshRenderer cutHeadSkin;

	
	public bool explodedPrefab;

	
	public List<MeshRenderer> skinParts = new List<MeshRenderer>();
}
