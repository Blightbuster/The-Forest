using System;
using UnityEngine;


[Serializable]
public class CoopPlayerVariation
{
	
	public void Init()
	{
		this._armColorBlock = new MaterialPropertyBlock();
		this._headColorBlock = new MaterialPropertyBlock();
		this._colorPropertyId = Shader.PropertyToID("_Color");
	}

	
	public void Toggle(bool on)
	{
		if (this.Head)
		{
			this.Head.gameObject.SetActive(on);
		}
		if (!on)
		{
			this.ResetSkinColor();
			this.SetHair(-1);
		}
	}

	
	public void SetMeshes(SkinnedMeshRenderer tShirtMesh, SkinnedMeshRenderer pantsMesh)
	{
		this._tShirtMesh = tShirtMesh;
		this._pantsMesh = pantsMesh;
	}

	
	public void SetMaterials(Material tShirtClean, Material tShirtRed, Material pantsClean, Material pantsRed)
	{
		this._tShirtMaterialClean = tShirtClean;
		this._tShirtMaterialRed = tShirtRed;
		this._pantsMaterialClean = pantsClean;
		this._pantsMaterialRed = pantsRed;
		this.SetBodyMaterial(this._tShirtMaterialClean, this._pantsMaterialClean);
	}

	
	public void SetBodyOptions(BodyOptions pants, BodyOptions tshirt, BodyOptions arms)
	{
		if (this._pantsMesh && this._pantsMesh.gameObject.activeSelf != (pants == BodyOptions.Default))
		{
			this._pantsMesh.gameObject.SetActive(pants == BodyOptions.Default);
		}
		if (this._tShirtMesh && this._tShirtMesh.gameObject.activeSelf != (tshirt == BodyOptions.Default))
		{
			this._tShirtMesh.gameObject.SetActive(tshirt == BodyOptions.Default);
		}
		if (this.TShirtNoArms && this.TShirtNoArms.gameObject.activeSelf != (tshirt == BodyOptions.NoArms))
		{
			this.TShirtNoArms.gameObject.SetActive(tshirt == BodyOptions.NoArms);
		}
		if (this.Hands)
		{
			if (this.Arms.gameObject.activeSelf != (arms != BodyOptions.JustHands))
			{
				this.Arms.gameObject.SetActive(arms != BodyOptions.JustHands);
			}
			if (this.Hands.gameObject.activeSelf != (arms == BodyOptions.JustHands))
			{
				this.Hands.gameObject.SetActive(arms == BodyOptions.JustHands);
			}
		}
	}

	
	public void SetHair(int hair)
	{
		this._hair = hair;
		for (int i = 0; i < this.Hair.Length; i++)
		{
			bool flag = i == hair;
			if (this.Hair[i].activeSelf != flag)
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
			if (this.Arms)
			{
				this.Arms.sharedMaterial = this.MaterialArmsRed;
			}
			if (this.Hands)
			{
				this.Hands.sharedMaterial = this.MaterialArmsRed;
			}
			this.SetBodyMaterial(this._tShirtMaterialRed, this._pantsMaterialRed);
			this.SetHeadMaterial(this.MaterialHeadRed, cold);
		}
		else if (blood)
		{
			if (!red)
			{
				if (this.Arms)
				{
					this.Arms.sharedMaterial = this.MaterialArmsBloody;
				}
				if (this.Hands)
				{
					this.Hands.sharedMaterial = this.MaterialArmsBloody;
				}
				this.SetHeadMaterial(this.MaterialHeadBloody, cold);
			}
		}
		else if (mud)
		{
			if (!red)
			{
				if (this.Arms)
				{
					this.Arms.sharedMaterial = this.MaterialArmsMuddy;
				}
				if (this.Hands)
				{
					this.Hands.sharedMaterial = this.MaterialArmsMuddy;
				}
				if (this._tShirtMaterialClean)
				{
					this.SetBodyMaterial(this._tShirtMaterialClean, this._pantsMaterialClean);
				}
				this.SetHeadMaterial(this.MaterialHeadMuddy, cold);
			}
		}
		else
		{
			if (this.Arms)
			{
				this.Arms.sharedMaterial = this.MaterialArmsClean;
			}
			if (this.Hands)
			{
				this.Hands.sharedMaterial = this.MaterialArmsClean;
			}
			if (this._tShirtMaterialClean)
			{
				this.SetBodyMaterial(this._tShirtMaterialClean, this._pantsMaterialClean);
			}
			this.SetHeadMaterial(this.MaterialHeadClean, cold);
		}
		if (this.Arms)
		{
			this.Arms.GetPropertyBlock(this._armColorBlock);
			this._armColorBlock.SetColor(this._colorPropertyId, (!cold) ? this.NormalSkin : this.ColdSkin);
			this.Arms.SetPropertyBlock(this._armColorBlock);
		}
		if (this.Hands)
		{
			this.Hands.GetPropertyBlock(this._armColorBlock);
			this._armColorBlock.SetColor(this._colorPropertyId, (!cold) ? this.NormalSkin : this.ColdSkin);
			this.Hands.SetPropertyBlock(this._armColorBlock);
		}
	}

	
	private void SetHeadMaterial(Material mat, bool cold)
	{
		if (this.Head)
		{
			this.Head.sharedMaterial = ((!mat) ? this.MaterialHeadClean : mat);
			this.Head.GetPropertyBlock(this._headColorBlock);
			this._headColorBlock.SetColor(this._colorPropertyId, (!cold) ? this.NormalSkin : this.ColdSkin);
			this.Head.SetPropertyBlock(this._headColorBlock);
		}
	}

	
	private void SetBodyMaterial(Material bodyMat, Material pantsMat)
	{
		if (this._pantsMesh && this._pantsMesh.gameObject.activeSelf)
		{
			this._pantsMesh.sharedMaterial = pantsMat;
		}
		if (this._tShirtMesh && this._tShirtMesh.gameObject.activeSelf)
		{
			this._tShirtMesh.sharedMaterial = bodyMat;
		}
		if (this.TShirtNoArms && this.TShirtNoArms.gameObject.activeSelf)
		{
			this.TShirtNoArms.sharedMaterial = bodyMat;
		}
	}

	
	public void ResetSkinColor()
	{
		if (this.Arms)
		{
			this.Arms.GetPropertyBlock(this._armColorBlock);
			this._armColorBlock.SetColor(this._colorPropertyId, this.NormalSkin);
			this.Arms.SetPropertyBlock(this._armColorBlock);
		}
		if (this.Hands)
		{
			this.Hands.GetPropertyBlock(this._armColorBlock);
			this._armColorBlock.SetColor(this._colorPropertyId, this.NormalSkin);
			this.Hands.SetPropertyBlock(this._armColorBlock);
		}
		if (this.Head)
		{
			this.Head.GetPropertyBlock(this._headColorBlock);
			this._headColorBlock.SetColor(this._colorPropertyId, this.NormalSkin);
			this.Head.SetPropertyBlock(this._headColorBlock);
		}
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

	
	[Header("deprecated")]
	public SkinnedMeshRenderer TShirtDefault;

	
	public SkinnedMeshRenderer TShirtNoArms;

	
	public SkinnedMeshRenderer Pants;

	
	public SkinnedMeshRenderer Body;

	
	public Material[] BodyMaterialsClean;

	
	private SkinnedMeshRenderer _tShirtMesh;

	
	private Material _tShirtMaterialClean;

	
	private Material _tShirtMaterialRed;

	
	private SkinnedMeshRenderer _pantsMesh;

	
	private Material _pantsMaterialClean;

	
	private Material _pantsMaterialRed;

	
	private int _hair;

	
	private int _colorPropertyId;

	
	private MaterialPropertyBlock _armColorBlock;

	
	private MaterialPropertyBlock _headColorBlock;
}
