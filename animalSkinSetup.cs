using System;
using UnityEngine;


public class animalSkinSetup : MonoBehaviour
{
	
	private void setSkinDamageProperty(MaterialPropertyBlock propertyBlock)
	{
		this.skin.SetPropertyBlock(propertyBlock);
	}

	
	private void setSkin(Material mat)
	{
		this.skin.sharedMaterial = mat;
	}

	
	public SkinnedMeshRenderer skin;
}
