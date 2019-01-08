using System;
using System.Collections.Generic;
using Bolt;
using UnityEngine;

public class CoopMutantMaterialSync : EntityBehaviour<IMutantState>
{
	private void Awake()
	{
		if (!BoltNetwork.isRunning)
		{
			base.enabled = false;
		}
		this.bloodPropertyBlock = new MaterialPropertyBlock();
		this.animator = base.GetComponent<Animator>();
		this.dummyType = base.transform.GetComponent<dummyTypeSetup>();
		this.MaterialIndexes = new Dictionary<Material, int>(this.Materials.Length);
		for (int i = 0; i < this.Materials.Length; i++)
		{
			this.MaterialIndexes[this.Materials[i]] = i;
		}
	}

	public void ForceStart()
	{
		this.Awake();
	}

	public override void Attached()
	{
		if (!base.entity.IsOwner())
		{
			base.state.AddCallback("MainMaterialIndex", new PropertyCallback(this.OnMainMaterialIndexChanged));
			this.token = (base.entity.attachToken as CoopMutantDummyToken);
			if (this.token != null)
			{
				if (!this.ignoreScale)
				{
					base.transform.localScale = this.token.Scale;
				}
				this.ApplyMaterial(this.token.MaterialIndex);
			}
		}
		base.Invoke("resetSkinDamage", 0.3f);
		if (this.dummyDamageSync && BoltNetwork.isRunning)
		{
			this.setSkinDamage();
		}
	}

	private void Update()
	{
		if (base.entity.IsOwner())
		{
			if (this.SkinnedRenderer)
			{
				this.SetMaterialName(this.SkinnedRenderer.sharedMaterial);
			}
			else if (this.Renderers != null)
			{
				for (int i = 0; i < this.Renderers.Length; i++)
				{
					if (this.Renderers[i] && this.Renderers[i].enabled && this.Renderers[i].gameObject.activeInHierarchy)
					{
						this.SetMaterialName(this.Renderers[i].sharedMaterial);
					}
				}
			}
			else
			{
				Debug.LogError("no renderers to pull material name from found");
			}
		}
	}

	public void SetMaterialName(Material material)
	{
		int num;
		if (this.MaterialIndexes.TryGetValue(material, out num) && base.state.MainMaterialIndex != num)
		{
			base.state.MainMaterialIndex = num;
		}
	}

	public int GetMaterialIndex()
	{
		if (this.SkinnedRenderer)
		{
			int result;
			if (this.MaterialIndexes.TryGetValue(this.SkinnedRenderer.sharedMaterial, out result))
			{
				return result;
			}
		}
		else if (this.Renderers != null)
		{
			for (int i = 0; i < this.Renderers.Length; i++)
			{
				int result;
				if (this.Renderers[i] && this.MaterialIndexes.TryGetValue(this.Renderers[i].sharedMaterial, out result))
				{
					return result;
				}
			}
		}
		else
		{
			Debug.LogError("no renderers to pull material name from found");
		}
		return 1;
	}

	public void ApplyMaterial(int materialIndex)
	{
		if (this.Disabled)
		{
			return;
		}
		Material material = (materialIndex < 0 || materialIndex >= this.Materials.Length) ? null : this.Materials[materialIndex];
		if (material)
		{
			if (this.SkinnedRenderer)
			{
				this.SkinnedRenderer.sharedMaterial = material;
			}
			else if (this.Renderers != null)
			{
				for (int i = 0; i < this.Renderers.Length; i++)
				{
					if (this.Renderers[i])
					{
						this.Renderers[i].sharedMaterial = material;
					}
				}
			}
		}
		else
		{
			Debug.LogErrorFormat("could not find material index {0}", new object[]
			{
				materialIndex
			});
		}
	}

	private void OnMainMaterialIndexChanged(IState _, string propertyPath, ArrayIndices arrayIndices)
	{
		this.ApplyMaterial(base.state.MainMaterialIndex);
		if (this.dummyDamageSync && BoltNetwork.isRunning)
		{
			this.setSkinDamage();
		}
	}

	private void setSkinDamage()
	{
		if (this.Renderers != null && this.token != null)
		{
			for (int i = 0; i < this.Renderers.Length; i++)
			{
				this.Renderers[i].GetPropertyBlock(this.bloodPropertyBlock);
				this.bloodPropertyBlock.SetFloat("_Damage1", this.token.skinDamage1);
				this.bloodPropertyBlock.SetFloat("_Damage2", this.token.skinDamage2);
				this.bloodPropertyBlock.SetFloat("_Damage3", this.token.skinDamage3);
				this.bloodPropertyBlock.SetFloat("_Damage4", this.token.skinDamage4);
				if (BoltNetwork.isClient)
				{
					this.bloodPropertyBlock.SetColor("_Color", this.storedColor);
				}
				this.Renderers[i].SetPropertyBlock(this.bloodPropertyBlock);
			}
		}
	}

	private void forceSkinColor()
	{
		if (this.SkinnedRenderer)
		{
			this.SkinnedRenderer.GetPropertyBlock(this.bloodPropertyBlock);
			this.bloodPropertyBlock.SetColor("_Color", this.storedColor);
			this.SkinnedRenderer.SetPropertyBlock(this.bloodPropertyBlock);
		}
	}

	private void resetSkinDamage()
	{
		if (this.Disabled)
		{
			return;
		}
		if (!this.SkinnedRenderer)
		{
			return;
		}
		this.SkinnedRenderer.GetPropertyBlock(this.bloodPropertyBlock);
		this.bloodPropertyBlock.SetFloat("_Damage1", 0f);
		this.bloodPropertyBlock.SetFloat("_Damage2", 0f);
		this.bloodPropertyBlock.SetFloat("_Damage3", 0f);
		this.bloodPropertyBlock.SetFloat("_Damage4", 0f);
		this.SkinnedRenderer.SetPropertyBlock(this.bloodPropertyBlock);
	}

	public void setSkinColor(Color set)
	{
		if (this.SkinnedRenderer)
		{
			this.SkinnedRenderer.GetPropertyBlock(this.bloodPropertyBlock);
			this.bloodPropertyBlock.SetColor("_Color", set);
			this.SkinnedRenderer.SetPropertyBlock(this.bloodPropertyBlock);
		}
		if (this.Renderers != null)
		{
			for (int i = 0; i < this.Renderers.Length; i++)
			{
				this.Renderers[i].GetPropertyBlock(this.bloodPropertyBlock);
				this.bloodPropertyBlock.SetColor("_Color", set);
				this.Renderers[i].SetPropertyBlock(this.bloodPropertyBlock);
			}
		}
		this.storedColor = set;
	}

	public bool Disabled;

	public bool dummyDamageSync;

	public bool ignoreScale;

	public Material[] Materials;

	public Renderer[] Renderers;

	public SkinnedMeshRenderer SkinnedRenderer;

	public Color storedColor = new Color(1f, 1f, 1f, 1f);

	private MaterialPropertyBlock bloodPropertyBlock;

	private Animator animator;

	private CoopMutantDummyToken token;

	private dummyTypeSetup dummyType;

	private Dictionary<Material, int> MaterialIndexes;
}
