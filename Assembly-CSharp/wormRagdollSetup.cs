using System;
using System.Collections;
using Bolt;
using UnityEngine;

public class wormRagdollSetup : EntityBehaviour<IStaticPickup>
{
	private void Awake()
	{
		this._colorPropertyId = Shader.PropertyToID("_Color");
		this.bloodPropertyBlock = new MaterialPropertyBlock();
	}

	public override void Attached()
	{
		if (base.entity.isOwner)
		{
			base.state.burning = this.burning;
		}
		if (!base.entity.isOwner)
		{
			base.state.AddCallback("burning", new PropertyCallbackSimple(this.burningChanged));
		}
	}

	private void burningChanged()
	{
		this.burning = base.state.burning;
		if (this.burning)
		{
			this.setOnFire();
		}
	}

	private void Start()
	{
		this._lerpColor = this.normalSkin;
		ConstantForce componentInChildren = base.transform.GetComponentInChildren<ConstantForce>();
		componentInChildren.torque = Vector3.up * 1E+07f + base.transform.right * 100000f;
		componentInChildren.force = new Vector3((float)UnityEngine.Random.Range(-100, 100), (float)UnityEngine.Random.Range(-100, 100), (float)UnityEngine.Random.Range(-100, 100));
		if (BoltNetwork.isClient)
		{
			int bodyVariation = 0;
			if (UnityEngine.Random.value > 0.5f)
			{
				bodyVariation = 1;
			}
			this.setBodyVariation(bodyVariation);
		}
	}

	private void setOnFire()
	{
		if (this.fireGo)
		{
			this.fireGo.SetActive(true);
			if (BoltNetwork.isServer)
			{
				this.burning = true;
			}
		}
	}

	private void setSkinDamage()
	{
		this.baseSkin.GetPropertyBlock(this.bloodPropertyBlock);
		this.bloodPropertyBlock.SetFloat("_Damage1", 1f);
		this.bloodPropertyBlock.SetFloat("_Damage3", 1f);
		this.bloodPropertyBlock.SetFloat("_Damage2", 1f);
		this.bloodPropertyBlock.SetFloat("_Damage4", 1f);
		this.baseSkin.SetPropertyBlock(this.bloodPropertyBlock);
	}

	private IEnumerator deadColorRoutine()
	{
		float t = 0f;
		yield return null;
		yield return null;
		while (t < 1f)
		{
			this._lerpColor = Color.Lerp(this._lerpColor, this.deadSkin, t);
			if (this.baseSkin)
			{
				this.baseSkin.GetPropertyBlock(this.bloodPropertyBlock);
				this.bloodPropertyBlock.SetColor(this._colorPropertyId, this._lerpColor);
				this.baseSkin.SetPropertyBlock(this.bloodPropertyBlock);
			}
			t += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	private void setBodyVariation(int val)
	{
		if (val == 1)
		{
			this.baseSkin.sharedMesh = this.bodyVar1.sharedMesh;
			this.baseSkin.sharedMaterial = this.bodyVar1Mat;
		}
		else if (val == 2)
		{
			this.baseSkin.sharedMesh = this.bodyVar2.sharedMesh;
			this.baseSkin.sharedMaterial = this.bodyVar2Mat;
		}
		this.setSkinDamage();
		base.StartCoroutine(this.deadColorRoutine());
	}

	public Color deadSkin;

	public Color normalSkin;

	private Color _lerpColor;

	private int _colorPropertyId;

	public SkinnedMeshRenderer baseSkin;

	public SkinnedMeshRenderer bodyVar1;

	public SkinnedMeshRenderer bodyVar2;

	public Material bodyVar1Mat;

	public Material bodyVar2Mat;

	public GameObject fireGo;

	public bool burning;

	private MaterialPropertyBlock bloodPropertyBlock;
}
