using System;
using System.Collections.Generic;
using Bolt;
using TheForest.Player.Clothing;
using TheForest.Utils;
using UnityEngine;


public class CoopPlayerVariations : EntityBehaviour<IPlayerState>
{
	
	
	private CoopPlayerVariation ActiveVariation
	{
		get
		{
			return this.Variations[this._activeVariationIndex];
		}
	}

	
	public void SetVariation(int variation, int hair, List<int> clothingItemIdList)
	{
		Material[] tShirtClean = null;
		Material[] tShirtRed = null;
		Mesh hatMesh = null;
		Material[] hatClean = null;
		Material[] hatRed = null;
		ClothingItem.BoneGroup index = ClothingItem.BoneGroup.Tops;
		Mesh topMesh = null;
		Material[] array = null;
		Material[] topRed = null;
		Mesh pantsMesh = null;
		Material[] pantsClean = null;
		Material[] pantsRed = null;
		bool flag = false;
		BodyOptions tshirt = BodyOptions.Default;
		BodyOptions arms = BodyOptions.Default;
		BodyOptions pants = BodyOptions.Default;
		BodyOptions shoes = BodyOptions.Default;
		this._activeVariationIndex = variation;
		for (int i = 0; i < this.Variations.Length; i++)
		{
			if (i != this._activeVariationIndex)
			{
				this.Variations[i].Toggle(false);
			}
		}
		for (int j = clothingItemIdList.Count - 1; j >= 0; j--)
		{
			if (clothingItemIdList[j] <= 0)
			{
				tshirt = BodyOptions.None;
				arms = BodyOptions.JustHands;
				pants = BodyOptions.None;
			}
			else
			{
				ClothingItem clothingItem = ClothingItemDatabase.ClothingItemById(clothingItemIdList[j]);
				if (clothingItem._displayType == ClothingItem.DisplayTypes.FullBody)
				{
					tshirt = BodyOptions.None;
					arms = BodyOptions.JustHands;
					pants = BodyOptions.None;
					shoes = BodyOptions.None;
					index = clothingItem._boneGroup;
					topMesh = clothingItem._meshLods[0];
					array = this.GetMaterials(clothingItem);
					topRed = ((clothingItem._materialsRed == null || clothingItem._materialsRed.Length <= 0) ? array : clothingItem._materialsRed);
					pantsMesh = null;
				}
				else if (clothingItem._displayType == ClothingItem.DisplayTypes.Pants)
				{
					pantsMesh = clothingItem._meshLods[0];
					pantsClean = this.GetMaterials(clothingItem);
					pantsRed = clothingItem._materialsRed;
				}
				else if (clothingItem._displayType == ClothingItem.DisplayTypes.TShirt)
				{
					tShirtClean = this.GetMaterials(clothingItem);
					tShirtRed = clothingItem._materialsRed;
				}
				else if (clothingItem._displayType == ClothingItem.DisplayTypes.Hat)
				{
					if (this.AllowHats())
					{
						hatMesh = clothingItem._meshLods[0];
						hatClean = this.GetMaterials(clothingItem);
						hatRed = clothingItem._materialsRed;
						flag = true;
					}
				}
				else
				{
					index = clothingItem._boneGroup;
					topMesh = clothingItem._meshLods[0];
					array = this.GetMaterials(clothingItem);
					topRed = ((clothingItem._materialsRed == null || clothingItem._materialsRed.Length <= 0) ? array : clothingItem._materialsRed);
					switch (clothingItem._displayType)
					{
					case ClothingItem.DisplayTypes.TopPartial_Hands:
						tshirt = BodyOptions.NoArms;
						arms = BodyOptions.JustHands;
						break;
					case ClothingItem.DisplayTypes.TopPartial_Arms:
						tshirt = BodyOptions.Default;
						arms = BodyOptions.Default;
						break;
					case ClothingItem.DisplayTypes.TopFull_Hands:
						tshirt = BodyOptions.None;
						arms = BodyOptions.JustHands;
						break;
					case ClothingItem.DisplayTypes.TopFull_Arms:
						tshirt = BodyOptions.None;
						arms = BodyOptions.Default;
						break;
					}
				}
				if (clothingItem._hidePants)
				{
					pants = BodyOptions.None;
				}
				if (clothingItem._hideShoes)
				{
					shoes = BodyOptions.None;
				}
			}
		}
		this.ActiveVariation.Toggle(true);
		this.ActiveVariation.SetMeshes(hatMesh, topMesh, this.BoneGroups[(int)index].Bones, pantsMesh);
		this.ActiveVariation.SetMaterials(hatClean, hatRed, array, topRed, tShirtClean, tShirtRed, pantsClean, pantsRed);
		this.ActiveVariation.SetBodyOptions(tshirt, arms, pants, shoes);
		this.ActiveVariation.SetHair((!flag) ? hair : -1);
	}

	
	private bool AllowHats()
	{
		return this._activeVariationIndex == 0;
	}

	
	private Material[] GetMaterials(ClothingItem eachItem)
	{
		if (eachItem._materialVariations.NullOrEmpty())
		{
			return eachItem._materials;
		}
		string value = "skinType_" + this._activeVariationIndex;
		foreach (MaterialSet materialSet in eachItem._materialVariations)
		{
			if (materialSet != null && !materialSet._id.NullOrEmpty())
			{
				if (materialSet._id.Equals(value))
				{
					return materialSet._materials;
				}
			}
		}
		return eachItem._materials;
	}

	
	public void UpdateSkinVariation(bool blood, bool mud, bool red, bool cold)
	{
		this.ActiveVariation.UpdateSkinVariation(blood, mud, red, cold);
	}

	
	public void ResetSkinColor()
	{
		if (this.ActiveVariation.NormalSkin.a > 0f)
		{
			this.ActiveVariation.ResetSkinColor();
		}
	}

	
	public void resetSkinBlood()
	{
		this.ActiveVariation.SetArmSkinBlood(0f);
	}

	
	private void Awake()
	{
		if (ForestVR.Prototype)
		{
			base.enabled = false;
			return;
		}
		if (this.Database)
		{
			this.Database.OnEnable();
		}
		for (int i = 0; i < this.Variations.Length; i++)
		{
			this.Variations[i].Init(this.Hat, this.Top, this.Pants);
		}
	}

	
	private void OnDestroy()
	{
		this.resetSkinBlood();
		this.ResetSkinColor();
	}

	
	
	public int BodyVariationCount
	{
		get
		{
			return this.Variations.Length * 4 * 5;
		}
	}

	
	
	public int ClothingVariationCount
	{
		get
		{
			return 15;
		}
	}

	
	public ClothingItemDatabase Database;

	
	public SkinnedMeshRenderer Hat;

	
	public SkinnedMeshRenderer Top;

	
	public SkinnedMeshRenderer Pants;

	
	public SkinnedMeshRenderer FullBody;

	
	public CoopPlayerVariation[] Variations;

	
	public SkinnedMeshRenderer[] BoneSources;

	
	public bool ImportBonesArrays;

	
	public List<CoopPlayerVariations.BoneGroup> BoneGroups;

	
	private int _activeVariationIndex;

	
	[Serializable]
	public class ClothVariations
	{
		
		public SkinnedMeshRenderer Model;

		
		public Material[] Materials;

		
		public Material MaterialRed;
	}

	
	[Serializable]
	public class BoneGroup
	{
		
		public Transform[] Bones;
	}
}
