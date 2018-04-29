using System;
using UnityEngine;


public class resetBloodMaterialDamage : MonoBehaviour
{
	
	private void Start()
	{
		this.resetMaterialBlood();
	}

	
	private void resetMaterialBlood()
	{
		Renderer component = base.transform.GetComponent<Renderer>();
		if (component)
		{
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			component.GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetFloat("_Damage1", 0f);
			materialPropertyBlock.SetFloat("_Damage2", 0f);
			materialPropertyBlock.SetFloat("_Damage3", 0f);
			materialPropertyBlock.SetFloat("_Damage4", 0f);
			component.SetPropertyBlock(materialPropertyBlock);
		}
	}
}
