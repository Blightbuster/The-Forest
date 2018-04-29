using System;
using UniLinq;
using UnityEngine;


public class CoopPlayerVariations : MonoBehaviour
{
	
	public void SetVariation(int variation, int tShirtType, int tShirtMat, int pantsType, int pantsMat, int hair, PlayerCloting clothing, int clothingVariation)
	{
		SkinnedMeshRenderer tShirtMesh = null;
		Material tShirtClean = null;
		Material tShirtRed = null;
		bool flag = false;
		BodyOptions pants = BodyOptions.Default;
		BodyOptions tshirt = BodyOptions.Default;
		BodyOptions arms = BodyOptions.Default;
		this._variation = variation;
		this._clothing = clothing;
		this._clothingVariation = clothingVariation;
		for (int i = 0; i < this.Variations.Length; i++)
		{
			if (i != this._variation)
			{
				this.Variations[i].Toggle(false);
			}
		}
		this.Variations[this._variation].Toggle(true);
		if (tShirtType >= this.TShirts.Length)
		{
			tShirtType = 0;
		}
		if (tShirtMat >= this.TShirts[tShirtType].Materials.Length)
		{
			tShirtMat = 0;
		}
		if (tShirtType < 0)
		{
			this._clothing = PlayerCloting.Blacksuit;
		}
		else
		{
			tShirtMesh = this.TShirts[tShirtType].Model;
			tShirtClean = this.TShirts[tShirtType].Materials[tShirtMat];
			tShirtRed = this.TShirts[tShirtType].MaterialRed;
		}
		for (int j = 0; j < this.TShirts.Length; j++)
		{
			if (this.TShirts[j].Model.gameObject.activeSelf != (j == tShirtType))
			{
				this.TShirts[j].Model.gameObject.SetActive(j == tShirtType);
			}
		}
		for (int k = 0; k < this.Pants.Length; k++)
		{
			if (this.Pants[k].Model.gameObject.activeSelf != (k == pantsType))
			{
				this.Pants[k].Model.gameObject.SetActive(k == pantsType);
			}
		}
		if (pantsType >= this.Pants.Length)
		{
			pantsType = 0;
		}
		if (pantsMat >= this.Pants[pantsType].Materials.Length)
		{
			pantsMat = 0;
		}
		SkinnedMeshRenderer model = this.Pants[pantsType].Model;
		Material pantsClean = this.Pants[pantsType].Materials[pantsMat];
		Material materialRed = this.Pants[pantsType].MaterialRed;
		if (this.BlackSuit)
		{
			bool flag2 = (this._clothing & PlayerCloting.Blacksuit) != PlayerCloting.Default;
			this.BlackSuit.SetActive(flag2);
			if (flag2)
			{
				pants = BodyOptions.None;
				tshirt = BodyOptions.None;
				arms = BodyOptions.JustHands;
			}
		}
		if (this.Jacket)
		{
			bool flag3 = (this._clothing & PlayerCloting.Jacket) != PlayerCloting.Default;
			this.Jacket.SetActive(flag3);
			if (flag3)
			{
				arms = BodyOptions.JustHands;
			}
		}
		if (this.Vest)
		{
			bool flag4 = (this._clothing & PlayerCloting.Vest) != PlayerCloting.Default;
			this.Vest.gameObject.SetActive(flag4);
			if (flag4)
			{
				this.Vest.sharedMaterial = this.VestVariations[this._clothingVariation];
			}
		}
		if (this.Hoodie)
		{
			bool flag5 = (this._clothing & PlayerCloting.Hoodie) != PlayerCloting.Default;
			this.Hoodie.gameObject.SetActive(flag5);
			if (flag5)
			{
				this.Hoodie.sharedMaterial = this.HoodieVariations[this._clothingVariation];
				tshirt = BodyOptions.None;
				arms = BodyOptions.JustHands;
			}
		}
		if (this.ShirtOpen)
		{
			bool flag6 = (this._clothing & PlayerCloting.ShirtOpen) != PlayerCloting.Default;
			this.ShirtOpen.gameObject.SetActive(flag6);
			if (flag6)
			{
				tshirt = BodyOptions.NoArms;
				arms = BodyOptions.JustHands;
			}
		}
		if (this.ShirtClosed)
		{
			bool flag7 = (this._clothing & PlayerCloting.ShirtClosed) != PlayerCloting.Default;
			this.ShirtClosed.gameObject.SetActive(flag7);
			if (flag7)
			{
				tshirt = BodyOptions.None;
				arms = BodyOptions.JustHands;
			}
		}
		if (this.JacketLow)
		{
			bool flag8 = (this._clothing & PlayerCloting.JacketLow) != PlayerCloting.Default;
			this.JacketLow.gameObject.SetActive(flag8);
			if (flag8)
			{
				tshirt = BodyOptions.NoArms;
				arms = BodyOptions.JustHands;
			}
		}
		if (this.HoodieUp)
		{
			bool flag9 = (this._clothing & PlayerCloting.HoodieUp) != PlayerCloting.Default;
			this.HoodieUp.gameObject.SetActive(flag9);
			if (flag9)
			{
				flag = true;
				tshirt = BodyOptions.None;
				arms = BodyOptions.JustHands;
			}
		}
		if (this.Beanies != null)
		{
			for (int l = 0; l < this.Beanies.Length; l++)
			{
				if (this.Beanies[l])
				{
					if (this.Beanies[l].gameObject.activeSelf != ((this._clothing & (PlayerCloting)(16 << l)) != PlayerCloting.Default))
					{
						bool active = !this.Beanies[l].gameObject.activeSelf;
						this.Beanies[l].gameObject.SetActive(active);
					}
					if (this.Beanies[l].gameObject.activeSelf)
					{
						flag = true;
					}
				}
			}
		}
		this.Variations[this._variation].SetMeshes(tShirtMesh, model);
		this.Variations[this._variation].SetMaterials(tShirtClean, tShirtRed, pantsClean, materialRed);
		this.Variations[this._variation].SetBodyOptions(pants, tshirt, arms);
		this.Variations[this._variation].SetHair((!flag) ? hair : -1);
	}

	
	public void UpdateSkinVariation(bool blood, bool mud, bool red, bool cold)
	{
		this.Variations[this._variation].UpdateSkinVariation(blood, mud, red, cold);
	}

	
	public void ResetSkinColor()
	{
		if (this.Variations[this._variation].NormalSkin.a > 0f)
		{
			this.Variations[this._variation].ResetSkinColor();
		}
	}

	
	private void Awake()
	{
		for (int i = 0; i < this.Variations.Length; i++)
		{
			this.Variations[i].Init();
		}
	}

	
	private void OnDestroy()
	{
		this.ResetSkinColor();
	}

	
	
	public int BodyVariationCount
	{
		get
		{
			return this.Variations.Length * this.TShirts.Sum((CoopPlayerVariations.ClothVariations ts) => ts.Materials.Length) * this.Pants.Sum((CoopPlayerVariations.ClothVariations ts) => ts.Materials.Length);
		}
	}

	
	
	public int ClothingVariationCount
	{
		get
		{
			return 2 + this.VestVariations.Length + this.HoodieVariations.Length + 1 + 1 + 1 + 1 + 3;
		}
	}

	
	public CoopPlayerVariation[] Variations;

	
	public CoopPlayerVariations.ClothVariations[] TShirts;

	
	public CoopPlayerVariations.ClothVariations[] Pants;

	
	public GameObject BlackSuit;

	
	public GameObject Jacket;

	
	public SkinnedMeshRenderer Vest;

	
	public Material[] VestVariations;

	
	public SkinnedMeshRenderer Hoodie;

	
	public Material[] HoodieVariations;

	
	public SkinnedMeshRenderer ShirtOpen;

	
	public SkinnedMeshRenderer ShirtClosed;

	
	public SkinnedMeshRenderer JacketLow;

	
	public SkinnedMeshRenderer HoodieUp;

	
	public SkinnedMeshRenderer[] Beanies;

	
	private int _variation;

	
	private PlayerCloting _clothing;

	
	private int _clothingVariation;

	
	[Serializable]
	public class ClothVariations
	{
		
		public SkinnedMeshRenderer Model;

		
		public Material[] Materials;

		
		public Material MaterialRed;
	}
}
