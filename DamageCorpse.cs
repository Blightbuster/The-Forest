using System;
using System.Collections;
using Bolt;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;


public class DamageCorpse : EntityBehaviour
{
	
	private void Start()
	{
		this.bloodPropertyBlock = new MaterialPropertyBlock();
		this.propManager = base.transform.GetComponentInParent<mutantPropManager>();
		this.setupManager = base.transform.GetComponentInParent<setupBodyVariation>();
		this.dummyType = base.transform.GetComponentInParent<dummyTypeSetup>();
	}

	
	public void DoLocalCut(int health)
	{
		if (health >= 20)
		{
			return;
		}
		UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate<GameObject>(this.BloodSplat, base.transform.position, Quaternion.identity), 0.5f);
		this.MyGore.SetActive(true);
		if (this.MyGoreSkinny)
		{
			this.MyGoreSkinny.SetActive(true);
		}
		this.Health = health;
		if (health <= 0)
		{
			this.CutDown();
		}
	}

	
	private void ignoreCutting()
	{
		this.ignoreHit = true;
	}

	
	private void Hit(int damage)
	{
		if (!this.animator)
		{
			this.animator = base.transform.GetComponentInParent<Animator>();
		}
		if (this.animator && this.animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Scene.animHash.nooseTrapDeathHash)
		{
			return;
		}
		if (this.ignoreHit)
		{
			this.ignoreHit = false;
			return;
		}
		if (this.infected)
		{
			LocalPlayer.Stats.BloodInfection.TryGetInfected();
		}
		if (base.entity.IsAttached())
		{
			HitCorpse hitCorpse = HitCorpse.Create(GlobalTargets.OnlyServer);
			hitCorpse.Entity = base.entity;
			hitCorpse.Damage = damage;
			hitCorpse.BodyPartIndex = base.GetComponentInParent<CoopSliceAndDiceMutant>().GetBodyPartIndex(this);
			hitCorpse.Send();
			if (!base.entity.isOwner)
			{
				this.DoLocalCut(this.Health - damage);
			}
		}
		else
		{
			this.DoLocalCut(this.Health - damage);
		}
	}

	
	private void CutDown()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		dummyTypeSetup component = base.transform.root.GetComponent<dummyTypeSetup>();
		if (component && (component._type == EnemyType.skinnyMale || component._type == EnemyType.skinnyFemale || component._type == EnemyType.skinnyMalePale))
		{
			flag = true;
		}
		if (this.femaleHair && this.femaleHair.activeSelf)
		{
			flag2 = true;
		}
		if (this.propManager)
		{
			this.isPoisoned = this.propManager.poisoned;
			if (this.propManager.skinnedHeadMask.activeSelf && this.isHead)
			{
				flag3 = true;
				this.propManager.skinnedHeadMask.SetActive(false);
			}
		}
		if (this.setupManager)
		{
			this.isPoisoned = this.setupManager.poisoned;
		}
		Vector3 position = Vector3.zero;
		GameObject gameObject = null;
		if (this.instantiateCutPrefab)
		{
			Quaternion rotation;
			if (this.instantiatePivot)
			{
				position = this.instantiatePivot.transform.position;
				rotation = this.instantiatePivot.transform.rotation;
			}
			else
			{
				position = this.MyCut.transform.position;
				rotation = this.MyCut.transform.rotation;
			}
			if (!BoltNetwork.isClient)
			{
				if (flag)
				{
					gameObject = UnityEngine.Object.Instantiate<GameObject>(this.spawnSkinny, position, rotation);
				}
				else
				{
					gameObject = UnityEngine.Object.Instantiate<GameObject>(this.spawnRegular, position, rotation);
				}
				if (component && component._type == EnemyType.paleMale)
				{
					gameObject.transform.localScale = gameObject.transform.localScale * 1.15f;
				}
				if (flag2)
				{
					enableGoReceiver componentInChildren = gameObject.GetComponentInChildren<enableGoReceiver>();
					if (componentInChildren)
					{
						componentInChildren.doEnableGo();
					}
				}
				if (flag3)
				{
					coopChoppedPartsReplicator component2 = gameObject.GetComponent<coopChoppedPartsReplicator>();
					if (component2 && component2.faceMask)
					{
						component2.faceMask.SetActive(true);
					}
					else
					{
						flag3 = false;
					}
				}
			}
			base.StartCoroutine(this.setupCutSkin(gameObject, flag));
		}
		else if (!BoltNetwork.isClient)
		{
			this.MyCut.SetActive(true);
			if (flag2)
			{
				enableGoReceiver componentInChildren2 = this.MyCut.GetComponentInChildren<enableGoReceiver>();
				if (componentInChildren2)
				{
					componentInChildren2.doEnableGo();
				}
			}
			base.StartCoroutine(this.setupCutSkin(this.MyCut, flag));
		}
		if (BoltNetwork.isClient && this.storePrefab && this.instantiateCutPrefab)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.storePrefab, position, base.transform.rotation);
			storeLocalMutantInfo2 component3 = gameObject2.GetComponent<storeLocalMutantInfo2>();
			Scene.SceneTracker.storedRagDollPrefabs.Add(gameObject2);
			component3.mat = this.sourceMat;
			CoopMutantMaterialSync component4 = base.transform.root.GetComponent<CoopMutantMaterialSync>();
			if (component4)
			{
				component3.matColor = component4.storedColor;
			}
			else if (component)
			{
				if (this.isPoisoned)
				{
					component3.matColor = Scene.SceneTracker.poisonedColor;
				}
				else if (component._type == EnemyType.paleMale || component._type == EnemyType.skinnyMalePale)
				{
					component3.matColor = Scene.SceneTracker.paleMutantColor;
				}
				else
				{
					component3.matColor = Scene.SceneTracker.regularMutantColor;
				}
			}
			component3.bloodPropertyBlock = this.bloodPropertyBlock;
			component3.showHair = flag2;
			component3.showMask = flag3;
		}
		SkinnedMeshRenderer component5 = this.MyPart.GetComponent<SkinnedMeshRenderer>();
		foreach (Transform transform in component5.bones)
		{
			IEnumerator enumerator = transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform2 = (Transform)obj;
					if (transform2.GetComponent<arrowTrajectory>())
					{
						transform2.parent = null;
						Rigidbody component6 = transform2.GetComponent<Rigidbody>();
						if (component6)
						{
							component6.isKinematic = false;
							component6.useGravity = true;
						}
						Collider component7 = transform2.GetComponent<Collider>();
						if (component7)
						{
							component7.isTrigger = false;
							component7.enabled = true;
						}
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}
		this.MyPart.SetActive(false);
		if (this.MyPartSkinny)
		{
			this.MyPartSkinny.SetActive(false);
		}
		if (this.props.Length > 0)
		{
			for (int j = 0; j < this.props.Length; j++)
			{
				this.props[j].SetActive(false);
			}
		}
		if (LocalPlayer.Transform && Vector3.Distance(LocalPlayer.Transform.position, base.transform.position) < 4f)
		{
			EventRegistry.Enemy.Publish(TfEvent.CutLimb, null);
		}
		this.MyCut.transform.parent = null;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	
	private void Explosion(float dist)
	{
		base.Invoke("CutDown", 1f);
	}

	
	private IEnumerator setupCutSkin(GameObject go, bool skinny)
	{
		GameObject source;
		if (skinny)
		{
			source = this.MyPartSkinny;
		}
		else
		{
			source = this.MyPart;
		}
		if (source == null)
		{
			yield break;
		}
		MeshRenderer r = null;
		if (go)
		{
			getMesh componentInChildren = go.GetComponentInChildren<getMesh>();
			if (componentInChildren)
			{
				r = componentInChildren.transform.GetComponent<MeshRenderer>();
			}
		}
		if (r)
		{
			for (int i = 0; i < r.materials.Length; i++)
			{
				if (!r.materials[i].name.Contains("JustBlood"))
				{
					MeshRenderer mr = source.GetComponent<MeshRenderer>();
					this.sourceMat = r.material;
					if (mr)
					{
						this.sourceMat = mr.material;
					}
					SkinnedMeshRenderer sm = source.GetComponent<SkinnedMeshRenderer>();
					if (sm)
					{
						this.sourceMat = sm.material;
					}
					Material[] newMats = r.materials;
					newMats[i] = this.sourceMat;
					r.materials = newMats;
					yield return YieldPresets.WaitForFixedUpdate;
					MaterialPropertyBlock sourceBlock = new MaterialPropertyBlock();
					Color sourceColor = default(Color);
					if (this.propManager)
					{
						sourceBlock = this.propManager.bloodPropertyBlock;
						if (this.isPoisoned)
						{
							sourceColor = Scene.SceneTracker.poisonedColor;
						}
						else
						{
							sourceColor = this.propManager.sourceColor;
						}
					}
					else if (this.setupManager)
					{
						sourceBlock = this.setupManager.bloodPropertyBlock;
						if (this.isPoisoned)
						{
							sourceColor = Scene.SceneTracker.poisonedColor;
						}
						else
						{
							sourceColor = this.setupManager.sourceColor;
						}
					}
					r.GetPropertyBlock(this.bloodPropertyBlock);
					this.bloodPropertyBlock.SetColor("_Color", sourceColor);
					this.bloodPropertyBlock.SetFloat("_Damage1", sourceBlock.GetFloat("_Damage1"));
					this.bloodPropertyBlock.SetFloat("_Damage2", sourceBlock.GetFloat("_Damage2"));
					this.bloodPropertyBlock.SetFloat("_Damage3", sourceBlock.GetFloat("_Damage3"));
					this.bloodPropertyBlock.SetFloat("_Damage4", sourceBlock.GetFloat("_Damage4"));
					r.SetPropertyBlock(this.bloodPropertyBlock);
				}
			}
		}
		else
		{
			MeshRenderer component = source.GetComponent<MeshRenderer>();
			if (component)
			{
				this.sourceMat = component.material;
			}
			SkinnedMeshRenderer component2 = source.GetComponent<SkinnedMeshRenderer>();
			if (component2)
			{
				this.sourceMat = component2.material;
			}
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			if (this.propManager)
			{
				materialPropertyBlock = this.propManager.bloodPropertyBlock;
			}
			else if (this.setupManager)
			{
				materialPropertyBlock = this.setupManager.bloodPropertyBlock;
			}
		}
		yield return null;
		yield break;
	}

	
	private mutantPropManager propManager;

	
	private setupBodyVariation setupManager;

	
	private dummyTypeSetup dummyType;

	
	private Animator animator;

	
	public int Health;

	
	public GameObject BloodSplat;

	
	public GameObject MyCut;

	
	public GameObject spawnRegular;

	
	public GameObject spawnSkinny;

	
	public GameObject MyGore;

	
	public GameObject MyPart;

	
	public GameObject MyGoreSkinny;

	
	public GameObject MyPartSkinny;

	
	public GameObject[] props;

	
	public GameObject femaleHair;

	
	public GameObject femaleSkirtOnly;

	
	public bool ignoreHit;

	
	public bool infected;

	
	public bool instantiateCutPrefab;

	
	public bool isHead;

	
	private bool isPoisoned;

	
	public Transform instantiatePivot;

	
	public GameObject storePrefab;

	
	private Material sourceMat;

	
	private MaterialPropertyBlock bloodPropertyBlock;
}
