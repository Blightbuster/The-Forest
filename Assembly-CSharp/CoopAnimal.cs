using System;
using System.Collections;
using Bolt;
using TheForest.Utils;
using UnityEngine;

public class CoopAnimal : CoopBase<IAnimalState>
{
	private void Awake()
	{
		if (Terrain.activeTerrain && Terrain.activeTerrain.materialTemplate)
		{
			this.snowStartHeight = Terrain.activeTerrain.materialTemplate.GetFloat("_SnowStartHeight");
			this.snowFadeLength = Terrain.activeTerrain.materialTemplate.GetFloat("_SnowFadeLength");
			this.snowStartHeight += this.snowFadeLength / 4f;
			this.snowFadeLength /= 2f;
		}
		this.tr = base.transform;
		if (this.rotationTransform)
		{
			this.smoothRot = this.rotationTransform.rotation;
		}
		if (this.tr.parent)
		{
			this.smoothPos = this.tr.parent.position;
		}
		this.bloodPropertyBlock = new MaterialPropertyBlock();
	}

	private void OnEnable()
	{
		if (this.rotationTransform)
		{
			this.smoothRot = this.rotationTransform.rotation;
		}
		else if (this.smoothRotations)
		{
			this.smoothRot = base.transform.rotation;
		}
		if (this.tr.parent)
		{
			this.smoothPos = this.tr.parent.position;
		}
		if (!this.birdType)
		{
			base.gameObject.tag = "animalRoot";
		}
		if (this.birdType && BoltNetwork.isClient && !Scene.SceneTracker.clientBirds.Contains(base.gameObject))
		{
			Scene.SceneTracker.clientBirds.Add(base.gameObject);
		}
		if (BoltNetwork.isClient)
		{
			base.StartCoroutine(this.delayedAnimalVisRoutine());
		}
	}

	public void setOnSnow(bool onoff)
	{
		if (onoff)
		{
			if (this.snowMaterial)
			{
				this.skin.sharedMaterial = this.snowMaterial;
			}
			AnimalTypeTrigger component = base.transform.GetComponent<AnimalTypeTrigger>();
			if (component && component._type == AnimalType.EuropeanRabbit)
			{
				component._type = AnimalType.snowRabbit;
			}
			this.isSnow = true;
		}
		else
		{
			this.isSnow = false;
			if (this.normalMat)
			{
				this.skin.sharedMaterial = this.normalMat;
			}
		}
	}

	private void LateUpdate()
	{
		if (this.smoothRotations)
		{
			if (this.rotationTransform)
			{
				this.smoothRot = Quaternion.Lerp(this.smoothRot, this.rotationTransform.rotation, Time.deltaTime * 13f);
				this.rotationTransform.rotation = this.smoothRot;
			}
			else
			{
				this.smoothRot = Quaternion.Lerp(this.smoothRot, this.tr.parent.rotation, Time.deltaTime * 13f);
				this.tr.parent.rotation = this.smoothRot;
			}
		}
		if (this.smoothTranslate && this.tr.parent)
		{
			if ((this.smoothPos - this.tr.parent.position).sqrMagnitude > 100f)
			{
				this.smoothPos = this.tr.parent.position;
			}
			else
			{
				this.smoothPos = Vector3.Lerp(this.smoothPos, this.tr.parent.position, Time.deltaTime * 18f);
			}
			this.tr.parent.position = this.smoothPos;
		}
	}

	private void OnSkinDamage1()
	{
		if (!this.skin)
		{
			return;
		}
		this.skin.GetPropertyBlock(this.bloodPropertyBlock);
		this.bloodPropertyBlock.SetFloat("_Damage1", base.state.skinDamage1);
		this.skin.SetPropertyBlock(this.bloodPropertyBlock);
	}

	private void OnSkinDamage2()
	{
		if (!this.skin)
		{
			return;
		}
		this.skin.GetPropertyBlock(this.bloodPropertyBlock);
		this.bloodPropertyBlock.SetFloat("_Damage2", base.state.skinDamage2);
		this.skin.SetPropertyBlock(this.bloodPropertyBlock);
	}

	private void OnSkinDamage3()
	{
		if (!this.skin)
		{
			return;
		}
		this.skin.GetPropertyBlock(this.bloodPropertyBlock);
		this.bloodPropertyBlock.SetFloat("_Damage3", base.state.skinDamage3);
		this.skin.SetPropertyBlock(this.bloodPropertyBlock);
	}

	private void OnSkinDamage4()
	{
		if (!this.skin)
		{
			return;
		}
		this.skin.GetPropertyBlock(this.bloodPropertyBlock);
		this.bloodPropertyBlock.SetFloat("_Damage4", base.state.skinDamage4);
		this.skin.SetPropertyBlock(this.bloodPropertyBlock);
	}

	public override void Attached()
	{
		if (BoltNetwork.isServer)
		{
			syncAnimalState syncAnimalState = syncAnimalState.Create(GlobalTargets.AllClients);
			syncAnimalState.target = base.entity;
			syncAnimalState.onSnow = this.isSnow;
			syncAnimalState.Send();
		}
		this.bloodPropertyBlock = new MaterialPropertyBlock();
		base.state.AddCallback("skinDamage1", new PropertyCallbackSimple(this.OnSkinDamage1));
		base.state.AddCallback("skinDamage2", new PropertyCallbackSimple(this.OnSkinDamage2));
		base.state.AddCallback("skinDamage3", new PropertyCallbackSimple(this.OnSkinDamage3));
		base.state.AddCallback("skinDamage4", new PropertyCallbackSimple(this.OnSkinDamage4));
	}

	private bool IsOnSnow()
	{
		if (base.transform.position.z < -300f && base.transform.position.y > this.snowStartHeight)
		{
			Terrain activeTerrain = Terrain.activeTerrain;
			if (!activeTerrain || this.snowFadeLength <= 0f)
			{
				return true;
			}
			Vector3 vector = activeTerrain.transform.InverseTransformPoint(base.transform.position);
			TerrainData terrainData = activeTerrain.terrainData;
			Vector2 vector2 = new Vector2(vector.x / terrainData.size.x, vector.z / terrainData.size.z);
			Vector3 interpolatedNormal = terrainData.GetInterpolatedNormal(vector2.x, vector2.y);
			float num = (base.transform.position.y - this.snowStartHeight) / this.snowFadeLength;
			num -= (1f - interpolatedNormal.y * interpolatedNormal.y) * 2f;
			num += 0.5f;
			if (num >= 1f || (num > 0f && UnityEngine.Random.value < num))
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator delayedAnimalVisRoutine()
	{
		float t = 0f;
		while (t < 0.5f)
		{
			t += Time.deltaTime;
			if (this.skin)
			{
				this.skin.enabled = false;
			}
			yield return null;
		}
		if (this.skin)
		{
			this.skin.enabled = true;
		}
		yield break;
	}

	private void OnDisable()
	{
		if (this.birdType && BoltNetwork.isClient && Scene.SceneTracker.clientBirds.Contains(base.gameObject))
		{
			Scene.SceneTracker.clientBirds.Remove(base.gameObject);
		}
	}

	[SerializeField]
	public Transform rotationTransform;

	private Transform tr;

	public bool birdType;

	public bool isSnow;

	public bool smoothRotations;

	public bool smoothTranslate;

	private Quaternion smoothRot;

	private Vector3 smoothPos;

	public Material normalMat;

	public Material snowMaterial;

	public SkinnedMeshRenderer skin;

	private MaterialPropertyBlock bloodPropertyBlock;

	[SerializeField]
	public Animator _animator;

	private bool counted;

	private float snowStartHeight;

	private float snowFadeLength;
}
