using System;
using UnityEngine;

public class wormSkinDamage : MonoBehaviour
{
	private void Start()
	{
		this.skinPropertyBlock = new MaterialPropertyBlock();
		this.resetSkinDamage();
	}

	public void setSkinDamage(float val)
	{
		this.skinDamageAmount += val;
		if (this.skinDamageAmount > 1f)
		{
			this.skinDamageAmount = 1f;
		}
		this.baseSkin.GetPropertyBlock(this.skinPropertyBlock);
		this.skinPropertyBlock.SetFloat("_Damage1", this.skinDamageAmount);
		this.skinPropertyBlock.SetFloat("_Damage3", this.skinDamageAmount);
		this.skinPropertyBlock.SetFloat("_Damage2", this.skinDamageAmount);
		this.skinPropertyBlock.SetFloat("_Damage4", this.skinDamageAmount);
		this.baseSkin.SetPropertyBlock(this.skinPropertyBlock);
	}

	public void resetSkinDamage()
	{
		this.baseSkin.GetPropertyBlock(this.skinPropertyBlock);
		this.skinPropertyBlock.SetFloat("_Damage1", 0f);
		this.skinPropertyBlock.SetFloat("_Damage3", 0f);
		this.skinPropertyBlock.SetFloat("_Damage2", 0f);
		this.skinPropertyBlock.SetFloat("_Damage4", 0f);
		this.baseSkin.SetPropertyBlock(this.skinPropertyBlock);
	}

	public SkinnedMeshRenderer baseSkin;

	private MaterialPropertyBlock skinPropertyBlock;

	private float skinDamageAmount;
}
