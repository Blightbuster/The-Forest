using System;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
using UnityEngine;


public class getAnimatorParams : MonoBehaviour
{
	
	
	
	public Vector3 DeadPosition
	{
		get
		{
			if (this.useSpikes)
			{
				return this.deadPosition;
			}
			return base.transform.position;
		}
		set
		{
			this.deadPosition = value;
		}
	}

	
	private void Awake()
	{
		this.bloodPropertyBlock = new MaterialPropertyBlock();
		this.cms = base.transform.GetComponent<CoopMutantSetup>();
		this.mrs = base.transform.GetComponentInChildren<mutantRagdollSetup>();
		this.animator = base.transform.GetComponentInChildren<Animator>();
		this.health = base.transform.GetComponentInChildren<EnemyHealth>();
		this.setup = base.transform.GetComponentInChildren<mutantScriptSetup>();
		this.bodyVar = base.transform.GetComponentInChildren<setupBodyVariation>();
	}

	
	private void LateUpdate()
	{
		if (this.useSpikes)
		{
			base.transform.root.position = this.DeadPosition;
		}
	}

	
	public void SpawnDummy(GameObject trap, Quaternion angle, bool isDummyLoad, bool isDiedLayingDown)
	{
		getAnimatorParams.DummyParams p = new getAnimatorParams.DummyParams
		{
			Angle = angle,
			IsDummyLoad = isDummyLoad,
			DiedLayingDown = isDiedLayingDown
		};
		this.health = base.transform.GetComponentInChildren<EnemyHealth>();
		this.health.trapGo = trap;
		base.StartCoroutine(this.spawnDummy(p));
	}

	
	public IEnumerator spawnDummy(getAnimatorParams.DummyParams p)
	{
		Quaternion angle = p.Angle;
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		Vector3 bodyPosition = base.transform.position;
		bool useBodyPosition = false;
		if (this.setup && this.setup.ai && this.setup.ai.pale && !p.IsDummyLoad && this.health.trapGo)
		{
			trapTrigger trigger = this.health.trapGo.GetComponentInChildren<trapTrigger>();
			if (trigger)
			{
				trigger.FixPalePosition(this.setup, false);
				bodyPosition += -0.15f * Vector3.up;
				useBodyPosition = true;
			}
		}
		GameObject dummy = UnityEngine.Object.Instantiate(this.dummyMutant, bodyPosition, base.transform.rotation) as GameObject;
		dummy.transform.rotation = angle;
		if (p.IsDummyLoad || (this.animator.GetBool("trapBool") && this.animator.GetInteger("trapTypeInt1") == 2))
		{
			dummy.transform.localEulerAngles = new Vector3(0f, dummy.transform.localEulerAngles.y, 0f);
		}
		dummy.transform.localScale = this.mutantBase.localScale;
		dummy.SendMessage("setCalledFromDeath", SendMessageOptions.DontRequireReceiver);
		SkinnedMeshRenderer[] sk = dummy.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer s in sk)
		{
			s.enabled = false;
		}
		Animator dummyAnim = dummy.GetComponent<Animator>();
		AnimatorStateInfo state = this.animator.GetCurrentAnimatorStateInfo(0);
		dummyAnim.CrossFade(state.nameHash, 0f, 0, state.normalizedTime);
		dummyAnim.CopyParamsFrom(this.animator);
		if (p.IsDummyLoad)
		{
			if (this.animator.GetInteger("trapTypeInt1") != 3)
			{
				dummyAnim.SetBoolReflected("trapBool", true);
				if (this.animator.GetInteger("trapTypeInt1") == 2)
				{
					dummyAnim.SetBoolReflected("enterTrapBool", true);
					dummyAnim.SetBoolReflected("deathBOOL", true);
				}
				else
				{
					dummyAnim.SetTriggerReflected("deathTrigger");
				}
			}
		}
		this.dummyHips.rotation = this.hips.rotation;
		this.dummyHips.position = bodyPosition;
		dummy.transform.position = bodyPosition;
		dummy.transform.localScale = this.mutantBase.localScale;
		if (this.setup.health.onFire)
		{
			dummy.SendMessage("startDummyFire", SendMessageOptions.DontRequireReceiver);
		}
		if (p.IsDummyLoad || this.animator.GetBool("trapBool"))
		{
			if (this.animator.GetInteger("trapTypeInt1") == 2 && this.health.trapParent)
			{
				dummy.transform.parent = this.health.trapParent.transform;
				dummy.SendMessageUpwards("setTrapDummy", dummy);
				CoopMutantDummy mutantDummySetup = dummy.GetComponentInChildren<CoopMutantDummy>();
				if (mutantDummySetup && this.health.trapGo)
				{
					this.health.trapGo.SendMessage("SetNooseRope", mutantDummySetup, SendMessageOptions.DontRequireReceiver);
				}
			}
			if ((p.IsDummyLoad || this.animator.GetInteger("trapTypeInt1") == 0) && this.health.trapGo)
			{
				this.health.trapGo.SendMessage("addTrappedMutant", new object[]
				{
					dummy,
					this.setup
				}, SendMessageOptions.DontRequireReceiver);
				dummy.SendMessage("setTrapGo", this.health.trapGo, SendMessageOptions.DontRequireReceiver);
			}
		}
		string TYPE = string.Empty;
		bool skinny = false;
		dummy.SendMessage("invokePickupSpawn", SendMessageOptions.DontRequireReceiver);
		if (this.setup.ai.femaleSkinny)
		{
			skinny = true;
			dummy.SendMessage(TYPE = "setFemaleSkinny", base.transform);
		}
		if (this.setup.ai.maleSkinny)
		{
			skinny = true;
			if (this.setup.ai.pale)
			{
				dummy.SendMessage(TYPE = "enableMaleSkinnyPaleProps");
				dummy.SendMessage(TYPE = "setMaleSkinnyPale", base.transform);
				if (this.setup.ai.skinned)
				{
					dummy.SendMessage(TYPE = "setSkinnedMutant", this.setup.ai.skinned);
				}
			}
			else
			{
				dummy.SendMessage(TYPE = "enableMaleSkinnyProps");
				dummy.SendMessage(TYPE = "setMaleSkinny", base.transform);
			}
		}
		if (this.setup.ai.pale && !this.setup.ai.maleSkinny)
		{
			dummy.SendMessage(TYPE = "enablePaleProps");
			dummy.SendMessage(TYPE = "setPaleMale");
			if (this.setup.ai.skinned)
			{
				dummy.SendMessage(TYPE = "setSkinnedMutant", this.setup.ai.skinned);
			}
		}
		if (this.setup.ai.leader && !this.setup.ai.maleSkinny && !this.setup.ai.pale && !this.setup.ai.female)
		{
			dummy.SendMessage(TYPE = "setRegularMale", this.setup.propManager ? this.setup.propManager.regularMaleDice : 0);
			dummy.SendMessage(TYPE = "enableLeaderProps", this.setup.propManager ? this.setup.propManager.LeaderDice : 0);
			if (this.setup.ai.painted)
			{
				dummy.SendMessage(TYPE = "setPaintedLeader");
			}
			dummy.SendMessage(TYPE = "setRegularMaleLeader");
		}
		if (this.setup.ai.fireman)
		{
			if (this.setup.ai.fireman_dynamite)
			{
				dummy.SendMessage(TYPE = "setDynamiteMan");
			}
			dummy.SendMessage(TYPE = "enableFiremanProps");
			dummy.SendMessage(TYPE = "setRegularMaleFireman");
		}
		if (this.setup.ai.female && !this.setup.ai.femaleSkinny)
		{
			if (this.setup.ai.painted)
			{
				dummy.SendMessage(TYPE = "setFemalePainted", this.setup.bodyVariation ? this.setup.bodyVariation.femaleDice : 0);
			}
			else
			{
				dummy.SendMessage(TYPE = "setFemaleRegular", this.setup.bodyVariation ? this.setup.bodyVariation.femaleDice : 0);
			}
			if (this.bodyVar && this.bodyVar.Hair && this.bodyVar.Hair.activeSelf)
			{
				dummy.SendMessage(TYPE = "setFemaleHair");
			}
			if (this.bodyVar && this.bodyVar.Clothing && this.bodyVar.Clothing.activeSelf)
			{
				dummy.SendMessage(TYPE = "setFemaleClothes");
			}
			if (this.bodyVar && this.bodyVar.FireDice == 2)
			{
				dummy.SendMessage(TYPE = "setFemaleFire");
			}
		}
		if (this.setup.ai.male && !this.setup.ai.maleSkinny && !this.setup.ai.pale && !this.setup.ai.fireman && !this.setup.ai.leader)
		{
			if (this.setup.ai.painted)
			{
				dummy.SendMessage(TYPE = "setPaintedMale", this.setup.propManager ? this.setup.propManager.regularMaleDice : 0);
			}
			else
			{
				dummy.SendMessage(TYPE = "setRegularMale", this.setup.propManager ? this.setup.propManager.regularMaleDice : 0);
			}
		}
		if (this.setup.health.alreadyBurnt)
		{
			dummy.SendMessage("setBurntSkin", SendMessageOptions.DontRequireReceiver);
		}
		dummy.SendMessage("setPoisoned", this.health.poisoned, SendMessageOptions.DontRequireReceiver);
		if (this.setup.arrowSticker)
		{
			arrowStickToTarget dummySticker = dummy.GetComponentInChildren<arrowStickToTarget>();
			foreach (KeyValuePair<Transform, int> attachStat in this.setup.arrowSticker.stuckArrows)
			{
				if (attachStat.Key)
				{
					dummySticker.applyStuckArrowToDummy(attachStat.Key, attachStat.Key.localPosition, attachStat.Key.localRotation, attachStat.Value);
				}
			}
		}
		this.setup.health.MySkin.GetPropertyBlock(this.bloodPropertyBlock);
		dummy.SendMessage("setSkinDamageProperty", this.bloodPropertyBlock, SendMessageOptions.DontRequireReceiver);
		dummy.SendMessage("setSkinMaterialProperties", this.health.MySkin.material, SendMessageOptions.DontRequireReceiver);
		if (BoltNetwork.isServer)
		{
			CoopMutantDummyToken token = new CoopMutantDummyToken();
			token.Skinny = skinny;
			token.HipPosition = bodyPosition;
			token.HipRotation = this.hips.rotation;
			token.OriginalMutant = base.GetComponent<BoltEntity>();
			token.Scale = this.mutantBase.localScale;
			token.skinDamage1 = this.bloodPropertyBlock.GetFloat("_Damage1");
			token.skinDamage2 = this.bloodPropertyBlock.GetFloat("_Damage2");
			token.skinDamage3 = this.bloodPropertyBlock.GetFloat("_Damage3");
			token.skinDamage4 = this.bloodPropertyBlock.GetFloat("_Damage4");
			if (p.IsDummyLoad)
			{
				foreach (SkinnedMeshRenderer s2 in sk)
				{
					s2.enabled = true;
				}
				CoopMutantMaterialSync CoopMaterial = dummy.GetComponent<CoopMutantMaterialSync>();
				CoopMaterial.ForceStart();
				token.MaterialIndex = CoopMaterial.GetMaterialIndex();
				CoopMutantPropSync CoopProps = dummy.GetComponent<CoopMutantPropSync>();
				token.Props = CoopProps.GetPropMask();
			}
			else
			{
				IMutantState s3 = base.GetComponent<BoltEntity>().GetState<IMutantState>();
				token.MaterialIndex = s3.MainMaterialIndex;
				token.Props = s3.prop_mask;
			}
			if (this.health.poisoned)
			{
				token.skinColor = new Color(0.670588255f, 0.796078444f, 0.5529412f, 1f);
			}
			else if (this.setup.ai.pale && !this.setup.ai.skinned)
			{
				token.skinColor = new Color(0.8039216f, 0.870588243f, 0.9137255f, 1f);
			}
			else
			{
				token.skinColor = new Color(0.698039234f, 0.698039234f, 0.698039234f, 1f);
			}
			BoltNetwork.Attach(dummy, token);
		}
		if (BoltNetwork.isServer)
		{
			IMutantFemaleDummyState female;
			if (dummy.GetComponent<BoltEntity>().TryFindState<IMutantFemaleDummyState>(out female))
			{
				female.Type = TYPE;
				female.CrossFadeHash = state.nameHash;
				female.CrossFadeTime = state.normalizedTime;
			}
			IMutantMaleDummyState male;
			if (dummy.GetComponent<BoltEntity>().TryFindState<IMutantMaleDummyState>(out male))
			{
				male.Type = TYPE;
				male.CrossFadeHash = state.nameHash;
				male.CrossFadeTime = state.normalizedTime;
			}
		}
		List<Quaternion> jointRotations = new List<Quaternion>();
		for (int i = 0; i < this.mrs.jointsToSync.Length; i++)
		{
			jointRotations.Add(this.mrs.jointsToSync[i].localRotation);
		}
		if (!p.IsDummyLoad && !this.animator.GetBool("trapBool"))
		{
			dummy.SendMessage("syncRagDollJoints", jointRotations, SendMessageOptions.DontRequireReceiver);
		}
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		dummy.transform.localScale = this.mutantBase.localScale;
		dummy.transform.position = ((!useBodyPosition) ? this.DeadPosition : bodyPosition);
		if (this.animator.GetBool("trapBool") && this.animator.GetInteger("trapTypeInt1") == 2)
		{
			this.setup.rootTr.parent = null;
		}
		foreach (SkinnedMeshRenderer s4 in sk)
		{
			s4.enabled = true;
		}
		if (PoolManager.Pools["enemies"].IsSpawned(base.transform))
		{
			PoolManager.Pools["enemies"].Despawn(base.transform);
		}
		else if (BoltNetwork.isServer)
		{
			dummy.GetComponent<BoltEntity>().DestroyDelayed(0.1f);
		}
		else
		{
			UnityEngine.Object.Destroy(base.transform.gameObject);
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator fixHipPosition(Transform d)
	{
		float t = 0f;
		while (t < 0.5f)
		{
			d.transform.position = base.transform.position;
			t += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	
	private CoopMutantSetup cms;

	
	private mutantRagdollSetup mrs;

	
	private Animator animator;

	
	private EnemyHealth health;

	
	private mutantScriptSetup setup;

	
	private setupBodyVariation bodyVar;

	
	public GameObject dummyMutant;

	
	public Transform hips;

	
	public Transform dummyHips;

	
	public Transform mutantBase;

	
	private Vector3 deadPosition = Vector3.zero;

	
	public bool useSpikes;

	
	private MaterialPropertyBlock bloodPropertyBlock;

	
	public class DummyParams
	{
		
		public Quaternion Angle;

		
		public bool DiedLayingDown;

		
		public bool IsDummyLoad;
	}
}
