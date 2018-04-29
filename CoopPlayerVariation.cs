using System;
using TheForest.Utils;
using UnityEngine;


[Serializable]
public class CoopPlayerVariation
{
	
	public void Init(SkinnedMeshRenderer hat, SkinnedMeshRenderer top, SkinnedMeshRenderer pants)
	{
		this._hat = hat;
		this._top = top;
		this._pants = pants;
		this._armsColorBlock = new MaterialPropertyBlock();
		this._headColorBlock = new MaterialPropertyBlock();
		this._armsBloodBlock = new MaterialPropertyBlock();
		this._colorPropertyId = Shader.PropertyToID("_Color");
		this._armBloodVal = 0f;
		this.SetArmSkinBlood(this._armBloodVal);
		this._lerpSkinColor = this.NormalSkin;
	}

	
	public void Toggle(bool on)
	{
		if (this.Head)
		{
			this.Head.gameObject.SetActive(on);
		}
		if (!on)
		{
			this.SetHair(-1);
			this.ResetSkinColor();
		}
	}

	
	public void SetMeshes(Mesh hatMesh, Mesh topMesh, Transform[] topBones, Mesh pantsMesh)
	{
		if (hatMesh)
		{
			this._hat.sharedMesh = hatMesh;
		}
		this._hat.enabled = hatMesh;
		if (topMesh)
		{
			this._top.sharedMesh = topMesh;
			this._top.bones = topBones;
		}
		this._top.enabled = topMesh;
		if (pantsMesh)
		{
			this._pants.sharedMesh = pantsMesh;
		}
		this._pants.enabled = pantsMesh;
	}

	
	public void SetMaterials(Material[] hatClean, Material[] hatRed, Material[] topClean, Material[] topRed, Material[] tShirtClean, Material[] tShirtRed, Material[] pantsClean, Material[] pantsRed)
	{
		this._hatMaterialClean = hatClean;
		this._hatMaterialRed = hatRed;
		this._topMaterialClean = topClean;
		this._topMaterialRed = topRed;
		this._tShirtMaterialClean = tShirtClean;
		this._tShirtMaterialRed = tShirtRed;
		this._pantsMaterialClean = pantsClean;
		this._pantsMaterialRed = pantsRed;
		this.SetBodyMaterial(this._hatMaterialClean, this._topMaterialClean, this._tShirtMaterialClean, this._pantsMaterialClean);
	}

	
	public void SetBodyOptions(BodyOptions tshirt, BodyOptions arms, BodyOptions pants, BodyOptions shoes)
	{
		this.TShirt.SetActiveSelfSafe(tshirt == BodyOptions.Default);
		this.TShirtNoArms.SetActiveSelfSafe(tshirt == BodyOptions.NoArms);
		bool flag = arms == BodyOptions.JustHands;
		this.Arms.SetActiveSelfSafe(!flag);
		this.Hands.SetActiveSelfSafe(flag);
		this._pants.SetActiveSelfSafe(pants == BodyOptions.Default);
		this.Shoes.SetActiveSelfSafe(shoes == BodyOptions.Default);
	}

	
	public void SetHair(int hair)
	{
		this._hair = hair;
		for (int i = 0; i < this.Hair.Length; i++)
		{
			bool flag = i == hair;
			if (this.Hair[i] && this.Hair[i].activeSelf != flag)
			{
				this.Hair[i].SetActive(flag);
			}
		}
	}

	
	public void UpdateSkinVariation(bool blood, bool mud, bool red, bool cold)
	{
		this.ResetSkinColor();
		if (red)
		{
			this.ApplyRedSkin(cold);
		}
		else if (blood)
		{
			this.ApplyBloodSkin(cold);
		}
		else if (mud)
		{
			this.ApplyMudSkin(cold);
		}
		else
		{
			this.ApplyCleanSkin(cold);
		}
		this.UpdateSkinColor(cold);
	}

	
	private void ApplyCleanSkin(bool cold)
	{
		this._armBloodVal = Mathf.Lerp(this._armBloodVal, 0f, Time.deltaTime / 2f);
		if (this._armBloodVal < 0.05f)
		{
			this._armBloodVal = 0f;
		}
		if (this.Arms)
		{
			this.Arms.sharedMaterial = this.MaterialArmsClean;
			this.SetArmSkinBlood(this._armBloodVal);
		}
		if (this.Hands)
		{
			this.Hands.sharedMaterial = this.MaterialArmsClean;
			this.SetArmSkinBlood(this._armBloodVal);
		}
		if (this.IsValid(this._tShirtMaterialClean))
		{
			this.SetBodyMaterial(this._hatMaterialClean, this._topMaterialClean, this._tShirtMaterialClean, this._pantsMaterialClean);
		}
		this.SetHeadMaterial(this.MaterialHeadClean, cold);
	}

	
	private void ApplyMudSkin(bool cold)
	{
		if (this.Arms)
		{
			this.Arms.sharedMaterial = this.MaterialArmsMuddy;
		}
		if (this.Hands)
		{
			this.Hands.sharedMaterial = this.MaterialArmsMuddy;
		}
		if (this.IsValid(this._tShirtMaterialClean))
		{
			this.SetBodyMaterial(this._hatMaterialClean, this._topMaterialClean, this._tShirtMaterialClean, this._pantsMaterialClean);
		}
		this.SetHeadMaterial(this.MaterialHeadMuddy, cold);
	}

	
	private void ApplyBloodSkin(bool cold)
	{
		if (this.Arms)
		{
			this.Arms.sharedMaterial = this.MaterialArmsBloody;
			this._armBloodVal = 0.5f;
			this.SetArmSkinBlood(this._armBloodVal);
		}
		if (this.Hands)
		{
			this.Hands.sharedMaterial = this.MaterialArmsBloody;
			this._armBloodVal = 0.5f;
			this.SetArmSkinBlood(this._armBloodVal);
		}
		this.SetHeadMaterial(this.MaterialHeadBloody, cold);
	}

	
	private void ApplyRedSkin(bool cold)
	{
		if (this.Arms)
		{
			this.Arms.sharedMaterial = this.MaterialArmsRed;
		}
		if (this.Hands)
		{
			this.Hands.sharedMaterial = this.MaterialArmsRed;
		}
		this.SetBodyMaterial(this._hatMaterialRed, this._topMaterialRed, this._tShirtMaterialRed, this._pantsMaterialRed);
		this.SetHeadMaterial(this.MaterialHeadRed, cold);
	}

	
	private void UpdateSkinColor(bool cold)
	{
		this._lerpSkinColor = Color.Lerp(this._lerpSkinColor, (!cold) ? this.NormalSkin : this.ColdSkin, Time.deltaTime / 2f);
		if (this.Arms)
		{
			this.Arms.GetPropertyBlock(this._armsColorBlock);
			this._armsColorBlock.SetColor(this._colorPropertyId, this._lerpSkinColor);
			this.Arms.SetPropertyBlock(this._armsColorBlock);
		}
		if (this.Hands)
		{
			this.Hands.GetPropertyBlock(this._armsColorBlock);
			this._armsColorBlock.SetColor(this._colorPropertyId, this._lerpSkinColor);
			this.Hands.SetPropertyBlock(this._armsColorBlock);
		}
	}

	
	private void SetHeadMaterial(Material mat, bool cold)
	{
		if (this.Head == null)
		{
			return;
		}
		this.Head.sharedMaterial = ((!mat) ? this.MaterialHeadClean : mat);
		this.Head.GetPropertyBlock(this._headColorBlock);
		this._headColorBlock.SetColor(this._colorPropertyId, this._lerpSkinColor);
		this.Head.SetPropertyBlock(this._headColorBlock);
	}

	
	private void SetBodyMaterial(Material[] hatMat, Material[] topMat, Material[] tshirtMat, Material[] pantsMat)
	{
		if (this._hat && this.IsValid(hatMat))
		{
			this._hat.sharedMaterials = hatMat;
		}
		if (this._top && this.IsValid(topMat))
		{
			this._top.sharedMaterials = topMat;
		}
		if (this.TShirt && this.IsValid(tshirtMat))
		{
			this.TShirt.sharedMaterials = tshirtMat;
		}
		if (this.TShirtNoArms && this.IsValid(tshirtMat))
		{
			this.TShirtNoArms.sharedMaterials = tshirtMat;
		}
		if (this._pants && this.IsValid(pantsMat))
		{
			this._pants.sharedMaterials = pantsMat;
		}
	}

	
	private bool IsValid(Material[] matArray)
	{
		return matArray != null && matArray.Length > 0;
	}

	
	public void ResetSkinColor()
	{
		if (ForestVR.Prototype)
		{
			return;
		}
		if (this.Arms)
		{
			this.Arms.GetPropertyBlock(this._armsColorBlock);
			this._armsColorBlock.SetColor(this._colorPropertyId, this.NormalSkin);
			this.Arms.SetPropertyBlock(this._armsColorBlock);
		}
		if (this.Hands)
		{
			this.Hands.GetPropertyBlock(this._armsColorBlock);
			this._armsColorBlock.SetColor(this._colorPropertyId, this.NormalSkin);
			this.Hands.SetPropertyBlock(this._armsColorBlock);
		}
		if (this.Head)
		{
			this.Head.GetPropertyBlock(this._headColorBlock);
			this._headColorBlock.SetColor(this._colorPropertyId, this.NormalSkin);
			this.Head.SetPropertyBlock(this._headColorBlock);
		}
	}

	
	public void SetArmSkinBlood(float amount)
	{
		if (this.Arms != null)
		{
			this.Arms.GetPropertyBlock(this._armsBloodBlock);
			this._armsBloodBlock.SetFloat("_Damage1", amount);
			this._armsBloodBlock.SetFloat("_Damage2", amount);
			this._armsBloodBlock.SetFloat("_Damage3", amount);
			this._armsBloodBlock.SetFloat("_Damage4", amount);
			this.Arms.SetPropertyBlock(this._armsBloodBlock);
		}
		if (this.Hands != null)
		{
			this.Hands.GetPropertyBlock(this._armsBloodBlock);
			this._armsBloodBlock.SetFloat("_Damage1", amount);
			this._armsBloodBlock.SetFloat("_Damage2", amount);
			this._armsBloodBlock.SetFloat("_Damage3", amount);
			this._armsBloodBlock.SetFloat("_Damage4", amount);
			this.Hands.SetPropertyBlock(this._armsBloodBlock);
		}
	}

	
	private void tranferArmBlood()
	{
	}

	
	public Color NormalSkin;

	
	public Color ColdSkin;

	
	public SkinnedMeshRenderer Arms;

	
	public SkinnedMeshRenderer Hands;

	
	public SkinnedMeshRenderer Head;

	
	public GameObject[] Hair;

	
	public Material MaterialArmsClean;

	
	public Material MaterialArmsBloody;

	
	public Material MaterialArmsMuddy;

	
	public Material MaterialArmsRed;

	
	public Material MaterialHeadClean;

	
	public Material MaterialHeadBloody;

	
	public Material MaterialHeadMuddy;

	
	public Material MaterialHeadRed;

	
	public Material BodyMaterialRed;

	
	public SkinnedMeshRenderer Shoes;

	
	public SkinnedMeshRenderer TShirtNoArms;

	
	public SkinnedMeshRenderer TShirt;

	
	private Material[] _tShirtMaterialClean;

	
	private Material[] _tShirtMaterialRed;

	
	private SkinnedMeshRenderer _hat;

	
	private Material[] _hatMaterialClean;

	
	private Material[] _hatMaterialRed;

	
	private SkinnedMeshRenderer _top;

	
	private Material[] _topMaterialClean;

	
	private Material[] _topMaterialRed;

	
	private SkinnedMeshRenderer _pants;

	
	private Material[] _pantsMaterialClean;

	
	private Material[] _pantsMaterialRed;

	
	private int _hair;

	
	private int _colorPropertyId;

	
	private MaterialPropertyBlock _armsColorBlock;

	
	private MaterialPropertyBlock _headColorBlock;

	
	private MaterialPropertyBlock _armsBloodBlock;

	
	private float _armBloodVal;

	
	private Color _lerpSkinColor;
}
