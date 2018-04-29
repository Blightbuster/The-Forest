using System;
using System.Collections.Generic;
using Bolt;
using TheForest.Utils;
using UnityEngine;


public class setupBodyVariation : EntityBehaviour<IMutantState>
{
	
	private void Awake()
	{
		this.Hair.SetActive(false);
		this.Clothing.SetActive(false);
		this.bloodPropertyBlock = new MaterialPropertyBlock();
		if (this.skinnyParts.Length > 0)
		{
			this.skinnyPartsParent = this.skinnyParts[0].transform.parent.gameObject;
		}
		if (this.regularParts.Length > 0)
		{
			this.regularPartsParent = this.regularParts[0].transform.parent.gameObject;
		}
		this.materialLookup.Add(this.BaldMat.name, this.BaldMat);
		this.materialLookup.Add(this.FireMat.name, this.FireMat);
		this.materialLookup.Add(this.paintedMat.name, this.FireMat);
		this.materialLookup.Add(this.SkinnyMat.name, this.SkinnyMat);
		this.materialLookup.Add(this.femaleMat.name, this.femaleMat);
		this.materialLookup.Add(this.femaleBurntMat.name, this.femaleBurntMat);
	}

	
	private void OnDespawned()
	{
		this.SetBlendShapeWeight(0, 0);
		this.SetBlendShapeWeight(1, 0);
		this.burnt = false;
	}

	
	public void SetBlendShapeWeight(int index, int weight)
	{
		if (!this.dummySetup)
		{
			this.skinRenderer.SetBlendShapeWeight(index, (float)weight);
		}
	}

	
	public void SetMaterial(Material mat)
	{
		if (this.skinRenderer)
		{
			this.skinRenderer.material = mat;
		}
		if (this.lowSkinnySkin)
		{
			this.lowSkinnySkin.material = mat;
		}
		if (this.lowSkin)
		{
			this.lowSkin.material = mat;
		}
	}

	
	public void SetCutMaterial(Material mat)
	{
		foreach (GameObject gameObject in this.regularParts)
		{
			gameObject.GetComponent<Renderer>().material = mat;
		}
		foreach (GameObject gameObject2 in this.skinnyParts)
		{
			gameObject2.GetComponent<Renderer>().material = mat;
		}
	}

	
	public void SetSkinnyHair()
	{
		this.Hair.SetActive(true);
		this.Hair.GetComponent<Renderer>().material = this.SkinnyHair;
	}

	
	public void SetRegularHair(int index)
	{
		this.Hair.SetActive(true);
		this.Hair.GetComponent<Renderer>().material = this.HairMats[index];
	}

	
	public void EnableClothing()
	{
		this.Clothing.SetActive(true);
	}

	
	public void enableFemaleSkinny()
	{
		if (this.dummySetup)
		{
			this.SetCutMaterial(this.SkinnyMat);
		}
		else
		{
			this.SetMaterial(this.SkinnyMat);
		}
		this.SetSkinnyHair();
		this.Clothing.SetActive(false);
		if (this.dummySetup)
		{
			this.skinnyPartsParent.SetActive(true);
			this.regularPartsParent.SetActive(false);
		}
		else
		{
			this.SetBlendShapeWeight(0, 0);
			this.SetBlendShapeWeight(1, 100);
		}
		base.Invoke("setSkinColor", 0.2f);
	}

	
	public void enableMaleSkinny()
	{
		this.SetMaterial(this.SkinnyMat);
		this.SetSkinnyHair();
		this.Clothing.SetActive(false);
		this.SetBlendShapeWeight(0, 0);
		this.SetBlendShapeWeight(1, 100);
	}

	
	public void setFemaleRegular(int dice)
	{
		this.femaleDice = UnityEngine.Random.Range(0, 5);
		if (this.dummySetup)
		{
			this.femaleDice = dice;
		}
		if (this.dummySetup)
		{
			this.skinnyPartsParent.SetActive(false);
			this.regularPartsParent.SetActive(true);
		}
		else
		{
			this.SetBlendShapeWeight(1, 0);
		}
		switch (this.femaleDice)
		{
		case 0:
			this.Hair.SetActive(true);
			this.Clothing.SetActive(true);
			if (this.dummySetup)
			{
				this.SetCutMaterial(this.femaleMat);
			}
			else
			{
				this.SetMaterial(this.femaleMat);
			}
			this.SetBlendShapeWeight(0, 25);
			break;
		case 1:
			this.Hair.SetActive(false);
			this.Clothing.SetActive(true);
			if (this.dummySetup)
			{
				this.SetCutMaterial(this.BaldMat);
			}
			else
			{
				this.SetMaterial(this.BaldMat);
			}
			this.SetBlendShapeWeight(0, 50);
			break;
		case 2:
			this.Hair.SetActive(false);
			this.Clothing.SetActive(false);
			if (this.dummySetup)
			{
				this.SetCutMaterial(this.BaldMat);
			}
			else
			{
				this.SetMaterial(this.femaleMat);
			}
			this.SetBlendShapeWeight(0, 75);
			break;
		case 3:
			this.Hair.SetActive(true);
			this.Clothing.SetActive(true);
			if (this.dummySetup)
			{
				this.SetCutMaterial(this.femaleMat);
			}
			else
			{
				this.SetMaterial(this.femaleMat);
			}
			this.SetBlendShapeWeight(0, 100);
			break;
		case 4:
			this.Hair.SetActive(true);
			this.Clothing.SetActive(true);
			if (this.dummySetup)
			{
				this.SetCutMaterial(this.femaleMat);
			}
			else
			{
				this.SetMaterial(this.femaleMat);
			}
			this.SetBlendShapeWeight(0, 0);
			break;
		}
		base.Invoke("setSkinColor", 0.2f);
	}

	
	public void setFemalePainted(int dice)
	{
		this.femaleDice = UnityEngine.Random.Range(0, 2);
		if (this.dummySetup)
		{
			this.femaleDice = dice;
		}
		if (this.dummySetup)
		{
			this.skinnyPartsParent.SetActive(false);
			this.regularPartsParent.SetActive(true);
		}
		else
		{
			this.SetBlendShapeWeight(1, 0);
		}
		int num = this.femaleDice;
		if (num != 0)
		{
			if (num == 1)
			{
				this.Hair.SetActive(false);
				this.Clothing.SetActive(true);
				if (this.dummySetup)
				{
					this.SetCutMaterial(this.paintedMat);
				}
				else
				{
					this.SetMaterial(this.paintedMat);
				}
				this.SetBlendShapeWeight(0, 75);
			}
		}
		else
		{
			this.Hair.SetActive(false);
			this.Clothing.SetActive(false);
			if (this.dummySetup)
			{
				this.SetCutMaterial(this.paintedMat);
			}
			else
			{
				this.SetMaterial(this.paintedMat);
			}
			this.SetBlendShapeWeight(0, 50);
		}
		base.Invoke("setSkinColor", 0.2f);
	}

	
	private void setRegularFemale()
	{
		this.skinnyPartsParent.SetActive(false);
		this.regularPartsParent.SetActive(true);
	}

	
	private void setFemaleClothes()
	{
		this.Clothing.SetActive(true);
	}

	
	private void setFemaleHair()
	{
		this.Hair.SetActive(true);
	}

	
	private void setFemaleFire()
	{
		if (this.burnt)
		{
			return;
		}
		this.SetCutMaterial(this.FireMat);
	}

	
	private void enableLeaderProps(int dice)
	{
	}

	
	private void setSkinColor()
	{
		Color value = Scene.SceneTracker.regularMutantColor;
		if (this.pale && !this.skinned)
		{
			value = Scene.SceneTracker.paleMutantColor;
		}
		if (this.poisoned)
		{
			value = Scene.SceneTracker.poisonedColor;
		}
		this.sourceColor = value;
		if (this.regularParts.Length > 0)
		{
			foreach (GameObject gameObject in this.regularParts)
			{
				Renderer component = gameObject.GetComponent<Renderer>();
				component.GetPropertyBlock(this.bloodPropertyBlock);
				this.bloodPropertyBlock.SetColor("_Color", value);
				component.SetPropertyBlock(this.bloodPropertyBlock);
			}
		}
		if (this.skinnyParts.Length > 0)
		{
			foreach (GameObject gameObject2 in this.skinnyParts)
			{
				Renderer component2 = gameObject2.GetComponent<Renderer>();
				component2.GetPropertyBlock(this.bloodPropertyBlock);
				this.bloodPropertyBlock.SetColor("_Color", value);
				component2.SetPropertyBlock(this.bloodPropertyBlock);
			}
		}
		if (this.skinRenderer)
		{
			this.skinRenderer.GetPropertyBlock(this.bloodPropertyBlock);
			this.bloodPropertyBlock.SetColor("_Color", value);
			this.skinRenderer.SetPropertyBlock(this.bloodPropertyBlock);
		}
	}

	
	public void setBurntSkin()
	{
		this.burnt = true;
		if (this.femaleBurntMat)
		{
			this.SetCutMaterial(this.femaleBurntMat);
			this.SetMaterial(this.femaleBurntMat);
		}
	}

	
	private void setSkinDamageProperty(MaterialPropertyBlock block)
	{
		this.bloodPropertyBlock = block;
		if (this.regularParts.Length > 0)
		{
			foreach (GameObject gameObject in this.regularParts)
			{
				Renderer component = gameObject.GetComponent<Renderer>();
				component.SetPropertyBlock(block);
			}
		}
		if (this.skinnyParts.Length > 0)
		{
			foreach (GameObject gameObject2 in this.skinnyParts)
			{
				Renderer component2 = gameObject2.GetComponent<Renderer>();
				component2.SetPropertyBlock(block);
			}
		}
	}

	
	private void setSkinDamage1(float val)
	{
		this.applySkinDamage("_Damage1", val);
	}

	
	private void setSkinDamage2(float val)
	{
		this.applySkinDamage("_Damage2", val);
	}

	
	private void setSkinDamage3(float val)
	{
		this.applySkinDamage("_Damage3", val);
	}

	
	private void setSkinDamage4(float val)
	{
		this.applySkinDamage("_Damage4", val);
	}

	
	private void applySkinDamage(string t, float s)
	{
		if (this.regularParts.Length > 0)
		{
			foreach (GameObject gameObject in this.regularParts)
			{
				gameObject.GetComponent<Renderer>().material.SetFloat(t, s);
			}
		}
		if (this.skinnyParts.Length > 0)
		{
			foreach (GameObject gameObject2 in this.skinnyParts)
			{
				gameObject2.GetComponent<Renderer>().material.SetFloat(t, s);
			}
		}
	}

	
	private void setPoisoned(bool onoff)
	{
		this.poisoned = onoff;
	}

	
	public GameObject Clothing;

	
	public GameObject Hair;

	
	public Material BaldMat;

	
	public Material FireMat;

	
	public Material paintedMat;

	
	public Material SkinnyMat;

	
	public Material femaleMat;

	
	public Material femaleBurntMat;

	
	public Dictionary<string, Material> materialLookup = new Dictionary<string, Material>();

	
	public SkinnedMeshRenderer skinRenderer;

	
	public SkinnedMeshRenderer lowSkin;

	
	public SkinnedMeshRenderer lowSkinnySkin;

	
	public Material SkinnyHair;

	
	public Material[] HairMats;

	
	public GameObject[] skinnyParts;

	
	public GameObject[] regularParts;

	
	public GameObject props1Go;

	
	public Color sourceColor;

	
	public bool skipSetup;

	
	public bool dummySetup;

	
	public bool pale;

	
	public bool skinned;

	
	private GameObject skinnyPartsParent;

	
	private GameObject regularPartsParent;

	
	public int femaleDice;

	
	private int BaldDice;

	
	public int FireDice;

	
	public bool burnt = true;

	
	public bool poisoned;

	
	public MaterialPropertyBlock bloodPropertyBlock;
}
