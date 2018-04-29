using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using PathologicalGames;
using TheForest.Buildings.World;
using TheForest.Commons.Delegates;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;


[DoNotSerializePublic]
public class trapTrigger : EntityBehaviour
{
	
	
	
	public GameObject TrappedGo
	{
		get
		{
			return this.trappedGo;
		}
		set
		{
			this.trappedGo = value;
			this.loaded = true;
		}
	}

	
	public override void Attached()
	{
		if (this.entity.isOwner)
		{
			if (this.entity.StateIs<ITrapLargeState>())
			{
				this.entity.GetState<ITrapLargeState>().Sprung = this.sprung;
			}
			if (this.entity.StateIs<ITrapRabbitState>())
			{
				this.entity.GetState<ITrapRabbitState>().Sprung = this.sprung;
			}
		}
		else
		{
			if (this.entity.StateIs<ITrapLargeState>())
			{
				ITrapLargeState state = this.entity.GetState<ITrapLargeState>();
				this.SprungTag = state.Sprung;
				if (state.Sprung)
				{
					this.OnSprungMP(true);
				}
				state.AddCallback("Sprung", new PropertyCallbackSimple(this.OnSprungMP));
				if (this.largeNoose)
				{
					state.AddCallback("CanCutDown", new PropertyCallbackSimple(this.OnCanCutDownChanged));
					state.AddCallback("CanReset", new PropertyCallbackSimple(this.OnCanResetChanged));
				}
			}
			if (this.entity.StateIs<ITrapRabbitState>())
			{
				ITrapRabbitState state2 = this.entity.GetState<ITrapRabbitState>();
				this.SprungTag = state2.Sprung;
				if (state2.Sprung)
				{
					this.OnSprungMP(true);
				}
				state2.AddCallback("Sprung", new PropertyCallbackSimple(this.OnSprungMP));
			}
		}
	}

	
	private void OnCanCutDownChanged()
	{
		ITrapLargeState state = this.entity.GetState<ITrapLargeState>();
		if (state.CanCutDown)
		{
			this.cutTrigger.SetActive(true);
		}
		else
		{
			this.cutTrigger.SetActive(false);
		}
	}

	
	private void OnCanResetChanged()
	{
		ITrapLargeState state = this.entity.GetState<ITrapLargeState>();
		if (state.CanReset)
		{
			this.resetTrigger.SetActive(true);
		}
		else
		{
			this.resetTrigger.SetActive(false);
		}
	}

	
	private void Update()
	{
		if (this.entity.IsAttached() && this.entity.isOwner && this.largeNoose)
		{
			ITrapLargeState state = this.entity.GetState<ITrapLargeState>();
			state.CanCutDown = this.cutTrigger.activeInHierarchy;
			state.CanReset = this.resetTrigger.activeInHierarchy;
		}
	}

	
	public void SaveTrappedMutants()
	{
		if (this.largeDeadfall || this.largeSwingingRock || this.largeSpike)
		{
			return;
		}
		if (this.trappedMutants.Count > 0)
		{
			this.TrappedMutantNames = new string[this.trappedMutants.Count];
			this.TrappedMutantPositions = new float[this.trappedMutants.Count];
			for (int i = 0; i < this.trappedMutants.Count; i++)
			{
				this.TrappedMutantNames[i] = this.JoinNameAndMask(this.trappedMutants[i].transform.root.name, this.trappedMutantMasks[i]);
				this.TrappedMutantPositions[i] = ((!this.largeSpike) ? ((!this.largeDeadfall) ? 0f : this.trappedMutants[i].transform.root.position.y) : this.hit.RelativeToSpikes(this.trappedMutants[i].transform.root.position));
			}
		}
		else if (this.dummyGo != null)
		{
			if (this.largeNoose)
			{
				this.TrappedMutantNames = new string[1];
				this.TrappedMutantNames[0] = this.JoinNameAndMask(this.dummyGo.transform.root.name, 0);
			}
		}
		else
		{
			this.TrappedMutantNames = null;
			this.TrappedMutantPositions = null;
		}
	}

	
	public void LoadTrappedMutants(Vector3? tempNooseRef)
	{
		if (this.largeDeadfall || this.largeSwingingRock)
		{
			return;
		}
		if (this.TrappedMutantNames != null && this.TrappedMutantNames.Length > 0)
		{
			this.trappedMutants.Clear();
			this.trappedMutantMasks.Clear();
			this.dummyGo = null;
			if (this.largeNoose && this.TrappedMutantNames.Length == 1 && this.TrappedMutantPositions != null && this.TrappedMutantPositions.Length == 1)
			{
				string nameFromSave = this.GetNameFromSave(this.TrappedMutantNames[0]);
				int maskFromSave = this.GetMaskFromSave(this.TrappedMutantNames[0]);
				if (this.CanSpawnEnemy(nameFromSave))
				{
					GameObject gameObject = this.SpawnTrappedEnemy(nameFromSave, tempNooseRef, maskFromSave);
				}
			}
			else if (this.largeSpike && this.TrappedMutantPositions != null && this.TrappedMutantPositions.Length > 0 && this.TrappedMutantPositions.Length == this.TrappedMutantNames.Length)
			{
				for (int i = 0; i < this.TrappedMutantNames.Length; i++)
				{
					string nameFromSave2 = this.GetNameFromSave(this.TrappedMutantNames[i]);
					int maskFromSave2 = this.GetMaskFromSave(this.TrappedMutantNames[i]);
					if (this.CanSpawnEnemy(nameFromSave2))
					{
						GameObject gameObject2 = this.SpawnTrappedEnemy(nameFromSave2, new Vector3?(this.hit.RelativeFromSpikes(this.TrappedMutantPositions[i])), maskFromSave2);
					}
				}
			}
		}
	}

	
	private bool CanSpawnEnemy(string name)
	{
		return PoolManager.Pools["enemies"].prefabs.ContainsKey(this.Prefabname(name));
	}

	
	private GameObject SpawnTrappedEnemy(string name, Vector3? _position, int propertiesMask)
	{
		Vector3 pos = base.transform.position;
		if (_position != null)
		{
			pos = _position.Value;
		}
		Quaternion rot = Quaternion.identity;
		if (this.largeNoose)
		{
			pos = this.noosePivot.transform.position + -0.2f * this.noosePivot.transform.right + -0f * this.noosePivot.transform.forward;
			pos.y = base.transform.root.position.y + -0.15f;
			rot = Quaternion.LookRotation(-base.transform.root.right);
		}
		string key = this.Prefabname(name);
		Transform transform = PoolManager.Pools["enemies"].prefabs[key];
		GameObject gameObject;
		if (transform.GetComponent<getAnimatorParams>())
		{
			Transform transform2 = PoolManager.Pools["enemies"].Spawn(transform, pos, rot);
			if (this.largeSpike)
			{
				transform2.gameObject.SendMessageUpwards("setTrapLookat", base.transform.root.gameObject, SendMessageOptions.DontRequireReceiver);
			}
			if (this.largeNoose)
			{
				transform2.SendMessage("setInstantSpawn", true);
			}
			if (this.largeSpike)
			{
				Vector3 forward = base.transform.root.position - base.transform.position;
				transform2.transform.rotation = Quaternion.LookRotation(forward);
			}
			else if (this.largeDeadfall)
			{
				transform2.transform.rotation = Quaternion.LookRotation(base.transform.root.right);
			}
			transform2.gameObject.SendMessageUpwards("setCurrentTrap", base.gameObject, SendMessageOptions.DontRequireReceiver);
			mutantScriptSetup componentInChildren = transform2.gameObject.GetComponentInChildren<mutantScriptSetup>();
			getAnimatorParams component = transform2.gameObject.GetComponent<getAnimatorParams>();
			mutantVis componentInChildren2 = transform2.gameObject.GetComponentInChildren<mutantVis>();
			if (componentInChildren2)
			{
				componentInChildren2.disableVis = true;
			}
			mutantAI component2 = transform2.GetComponent<mutantAI>();
			if (component2)
			{
				this.isNotCreepy = (!component2.creepy && !component2.creepy_male && !component2.creepy_baby && !component2.creepy_fat);
			}
			else
			{
				mutantAI_net component3 = transform2.GetComponent<mutantAI_net>();
				this.isNotCreepy = (!component3.creepy && !component3.creepy_male && !component3.creepy_baby && !component3.creepy_fat);
			}
			int num = (!this.largeSpike) ? ((!this.largeDeadfall) ? ((!this.largeNoose) ? ((!this.largeSwingingRock) ? 0 : 3) : 2) : 1) : 0;
			transform2.gameObject.SendMessageUpwards("DieTrap", num, SendMessageOptions.DontRequireReceiver);
			if (this.largeSpike)
			{
				Vector3 vector = this.hit.PutOnSpikes(transform2.gameObject);
				transform2.gameObject.SendMessageUpwards("setPositionAtSpikes", vector, SendMessageOptions.DontRequireReceiver);
				component.DeadPosition = transform2.transform.position;
			}
			if (component)
			{
				this.ApplyEnemyMask(componentInChildren, propertiesMask);
				component.SpawnDummy(base.gameObject, transform2.transform.rotation, true, false);
			}
			gameObject = this.trappedMutants[0];
			if (this.largeDeadfall)
			{
				Animator component4 = gameObject.GetComponent<Animator>();
				component4.SetIntegerReflected("trapTypeInt1", 1);
				component4.SetBoolReflected("trapBool", true);
				component4.SetBoolReflected("dropFromTrap", true);
				component4.SetBoolReflected("deathfinalBOOL", true);
			}
			if (this.largeNoose)
			{
				Animator component5 = gameObject.GetComponent<Animator>();
				component5.SetIntegerReflected("direction", 0);
				component5.SetBoolReflected("enterTrapBool", true);
				component5.SetIntegerReflected("trapTypeInt1", 2);
				component5.SetBoolReflected("trapBool", true);
				gameObject.SendMessageUpwards("setTrapLookat", base.transform.root.gameObject, SendMessageOptions.DontRequireReceiver);
				gameObject.SendMessageUpwards("setCurrentTrap", base.gameObject, SendMessageOptions.DontRequireReceiver);
				gameObject.SendMessageUpwards("setFootPivot", this.nooseFootPivot, SendMessageOptions.DontRequireReceiver);
				mutantHitReceiver component6 = gameObject.transform.GetComponent<mutantHitReceiver>();
				if (component6)
				{
					component6.inNooseTrap = true;
				}
				mutantScriptSetup componentInChildren3 = gameObject.transform.root.GetComponentInChildren<mutantScriptSetup>();
				if (componentInChildren3)
				{
					EnemyHealth componentInChildren4 = gameObject.transform.root.GetComponentInChildren<EnemyHealth>();
					if (!componentInChildren3.ai.creepy && !componentInChildren3.ai.creepy_male && !componentInChildren3.ai.creepy_fat && !componentInChildren3.ai.creepy_baby)
					{
						componentInChildren4.gameObject.SendMessageUpwards("setInNooseTrap", this.noosePivot);
					}
					componentInChildren4.gameObject.SendMessageUpwards("setCurrentTrap", base.gameObject);
				}
				gameObject.gameObject.SendMessageUpwards("DieTrap", 2, SendMessageOptions.DontRequireReceiver);
				component5.SetBoolReflected("dropFromTrap", false);
				component5.SetBoolReflected("deathfinalBOOL", true);
				base.StartCoroutine(this.setDummyAndNooseTrap());
			}
		}
		else
		{
			gameObject = null;
		}
		return gameObject;
	}

	
	private IEnumerator setDummyAndNooseTrap()
	{
		yield return new WaitForSeconds(1f);
		if (this.largeNoose)
		{
			Vector3 pos = this.noosePivot.transform.position + -0.2f * this.noosePivot.transform.right + -0f * this.noosePivot.transform.forward;
			pos.y = base.transform.root.position.y + -0.15f;
			this.trappedMutants[0].transform.position = pos;
		}
		Animator dummyAnim = this.trappedMutants[0].GetComponent<Animator>();
		dummyAnim.enabled = true;
		dummyAnim.SetBoolReflected("deathfinalBOOL", false);
		if (this.largeNoose)
		{
			dummyAnim.enabled = true;
			dummyAnim.SetBoolReflected("dropFromTrap", false);
			dummyAnim.SetBoolReflected("enterTrapBool", false);
			dummyAnim.CrossFade(this.nooseTrapDeathHash, 0f, 0, 0f);
		}
		yield return new WaitForSeconds(1f);
		dummyAnim.SetIntegerReflected("direction", 0);
		if (!this.largeNoose)
		{
			dummyAnim.SetBoolReflected("enterTrapBool", true);
		}
		dummyAnim.SetIntegerReflected("trapTypeInt1", 2);
		dummyAnim.SetBoolReflected("trapBool", true);
		dummyAnim.SetBoolReflected("dropFromTrap", false);
		dummyAnim.SetBoolReflected("deathfinalBOOL", true);
		this.dummyGo = this.trappedMutants[0];
		this.mutantSetup = this.trappedMutants[0].GetComponentInChildren<mutantScriptSetup>();
		this.nooseRope1.SetActive(false);
		this.nooseRope2.SetActive(true);
		if (this.isNotCreepy)
		{
			CoopMutantDummy ragDollDummy = this.trappedMutants[0].GetComponent<CoopMutantDummy>();
			if (ragDollDummy)
			{
				this.nooseParent.transform.parent = ragDollDummy.ragDollJoints[2];
				this.nooseParent.transform.localPosition = new Vector3(-0.134f, 0f, 0.056f);
				this.nooseParent.transform.localEulerAngles = new Vector3(0f, -90f, 90f);
				yield return new WaitForSeconds(3f);
			}
		}
		else
		{
			this.nooseParent.transform.localPosition = new Vector3(0f, -0.834f, 0f);
			this.nooseParent.transform.localEulerAngles = new Vector3(-90f, 90f, 0f);
			if (this.MpHostCheck)
			{
				base.Invoke("enableTrapReset", 3f);
			}
		}
		this.trappedMutants[0].transform.parent = this.noosePivot.transform;
		yield return new WaitForSeconds(0.25f);
		dummyAnim.enabled = true;
		if (this.largeNoose)
		{
			this.SetColliders(this.trappedMutants[0], false);
		}
		yield break;
	}

	
	private void SetColliders(GameObject mutant, bool state)
	{
		foreach (CapsuleCollider capsuleCollider in mutant.GetComponentsInChildren<CapsuleCollider>())
		{
			capsuleCollider.isTrigger = state;
		}
	}

	
	private string Prefabname(string name)
	{
		string text = name;
		int num = name.IndexOf("(");
		if (num > 0)
		{
			text = text.Substring(0, num);
		}
		int num2 = name.IndexOf("_Dummy");
		if (num2 > 0)
		{
			text = text.Substring(0, num2);
		}
		return text;
	}

	
	private void UnregisterRefreshTrapped()
	{
		if (this.WsToken != -1)
		{
			WorkScheduler.Unregister(new WsTask(this.RefreshTrapped), this.WsToken);
			this.WsToken = -1;
		}
	}

	
	private void RegisterRefreshTrapped(bool force)
	{
		base.StartCoroutine(this.RegisterWhenready(force));
	}

	
	private IEnumerator RegisterWhenready(bool force)
	{
		while (!Scene.FinishGameLoad)
		{
			yield return null;
		}
		if (this.WsToken == -1)
		{
			this.WsToken = WorkScheduler.Register(new WsTask(this.RefreshTrapped), base.transform.position, force);
		}
		yield break;
	}

	
	private void RefreshTrapped()
	{
		if (this.rabbitTrap || this.fishTrap)
		{
			if (BoltNetwork.isRunning && BoltNetwork.isClient && this.fishTrapStart && this.fishTrap && this.IsLocalPlayerInRange(30f))
			{
				this.fishTrapStart = false;
				this.fishTrapForce = true;
				this.RequesTrappedFishMP();
				return;
			}
			this.OptimizeForTrappedAnimals();
		}
		else if (this.largeDeadfall || !this.largeSwingingRock)
		{
		}
	}

	
	private void OptimizeForTrappedAnimals()
	{
		if (this.sprung && (this.rabbitTrap || this.fishTrap) && this.TrappedName != null && this.TrappedName != string.Empty)
		{
			if (this.trappedGo != null)
			{
				if (this.IsPlayerInRange())
				{
					if (!this.trappedGo.activeSelf)
					{
						string name = this.trappedGo.transform.root.name;
						PoolManager.Pools["creatures"].Despawn(this.trappedGo.transform.root);
						this.trappedGo = this.SpawnTrappedAnimal(this.TrappedName);
					}
				}
				else if (PoolManager.Pools["creatures"].IsSpawned(this.trappedGo.transform.root))
				{
					PoolManager.Pools["creatures"].Despawn(this.trappedGo.transform.root);
					this.trappedGo = null;
				}
				else
				{
					UnityEngine.Object.Destroy((!this.fishTrap) ? this.trappedGo.transform.root.gameObject : this.trappedGo);
					this.trappedGo = null;
				}
			}
			else if (this.IsPlayerInRange())
			{
				this.trappedGo = this.SpawnTrappedAnimal(this.TrappedName);
			}
		}
	}

	
	private void OptimizeForTrappedEnemies()
	{
		if (this.sprung && (this.largeSpike || this.largeNoose) && this.TrappedMutantNames != null && this.TrappedMutantNames.Length > 0)
		{
			if (this.trappedMutants.Count == 0)
			{
				if (this.IsPlayerInRange())
				{
					if (this.largeNoose)
					{
						Vector3 position = this.nooseFootPivot.transform.position;
						position.y = base.transform.position.y;
						this.LoadTrappedMutants(new Vector3?(position));
					}
					else
					{
						this.LoadTrappedMutants(null);
					}
				}
			}
			else if (!this.IsPlayerInRange())
			{
				for (int i = 0; i < this.trappedMutants.Count; i++)
				{
					if (PoolManager.Pools["enemies"].IsSpawned(this.trappedMutants[i].transform.root))
					{
						dummyAnimatorControl componentInChildren = this.trappedMutants[i].transform.root.GetComponentInChildren<dummyAnimatorControl>();
						if (componentInChildren)
						{
							componentInChildren.trapGo = null;
						}
						EnemyHealth componentInChildren2 = this.trappedMutants[i].transform.root.GetComponentInChildren<EnemyHealth>();
						if (componentInChildren2)
						{
							componentInChildren2.trapGo = null;
						}
						this.UpdateSkinnedMeshes(this.trappedMutants[i].transform.root.gameObject, false);
						PoolManager.Pools["enemies"].Despawn(this.trappedMutants[i].transform.root);
					}
					else
					{
						dummyAnimatorControl componentInChildren3 = this.trappedMutants[i].transform.root.GetComponentInChildren<dummyAnimatorControl>();
						if (componentInChildren3)
						{
							componentInChildren3.trapGo = null;
						}
						EnemyHealth componentInChildren4 = this.trappedMutants[i].transform.root.GetComponentInChildren<EnemyHealth>();
						if (componentInChildren4)
						{
							componentInChildren4.trapGo = null;
						}
						UnityEngine.Object.Destroy(this.trappedMutants[i].transform.root.gameObject);
					}
				}
				this.trappedMutants.Clear();
				this.trappedMutantMasks.Clear();
			}
		}
	}

	
	private void LoadTrapped(Vector3? tempNooseRef)
	{
		if (!this.loaded)
		{
			this.loaded = true;
			base.StartCoroutine(this.LoadWhenready(tempNooseRef));
		}
	}

	
	private IEnumerator LoadWhenready(Vector3? tempNooseRef)
	{
		if (this.TrappedName != null && this.TrappedName != string.Empty)
		{
			while (!PoolManager.Pools.ContainsKey("creatures"))
			{
				yield return null;
			}
			if (this.IsPlayerInRange())
			{
				GameObject newAnimal = this.SpawnTrappedAnimal(this.TrappedName);
			}
		}
		else
		{
			while (!PoolManager.Pools.ContainsKey("enemies"))
			{
				yield return null;
			}
			if (this.IsPlayerInRange())
			{
				this.LoadTrappedMutants(tempNooseRef);
			}
		}
		yield break;
	}

	
	public void SaveTrapped(string name)
	{
		this.TrappedName = name;
	}

	
	private void ResetTrapped()
	{
		this.TrappedName = string.Empty;
	}

	
	private bool CanSpawnCreature(string name)
	{
		return PoolManager.Pools["creatures"].prefabs.ContainsKey(this.Prefabname(name));
	}

	
	private GameObject SpawnTrappedAnimal(string name)
	{
		if (!this.CanSpawnCreature(this.Prefabname(name)))
		{
			this.trappedGo = null;
			this.ResetTrapped();
			return null;
		}
		Transform transform = PoolManager.Pools["creatures"].Spawn(this.Prefabname(name), base.transform.position, Quaternion.identity);
		if (transform)
		{
			animalHealth componentInChildren = transform.GetComponentInChildren<animalHealth>();
			if (componentInChildren != null)
			{
				componentInChildren.Trap = base.gameObject;
			}
			transform.SendMessage("startUpdateSpawn");
			if (BoltNetwork.isServer && transform.gameObject.GetComponent<CoopAnimalServer>())
			{
				AnimalSpawnController.AttachAnimalToNetwork(null, transform.gameObject);
			}
			transform.gameObject.SendMessageUpwards("setTrapped", base.gameObject, SendMessageOptions.DontRequireReceiver);
			this.trappedGo = transform.gameObject;
			this.SaveTrapped(name);
			return transform.gameObject;
		}
		return null;
	}

	
	public override void Detached()
	{
		Scene.HudGui.AddIcon.SetActive(false);
	}

	
	private void OnSprungMP()
	{
		this.OnSprungMP(false);
	}

	
	private void OnSprungMP(bool byPassAnim)
	{
		if (this.entity.StateIs<ITrapLargeState>() && this.entity.GetState<ITrapLargeState>().Sprung)
		{
			this.TriggerLargeTrap(byPassAnim);
		}
		if (this.entity.StateIs<ITrapRabbitState>() && this.entity.GetState<ITrapRabbitState>().Sprung)
		{
			this.TriggerRabbitTrap(byPassAnim);
		}
	}

	
	private void Start()
	{
		if (this.SprungTag)
		{
			this.sprung = true;
		}
		if (this.hitbox)
		{
			this.hit = this.hitbox.GetComponent<trapHit>();
		}
		this.CheckAnimReference();
		if (this.anim.GetComponent<Animation>() && !this.sprung)
		{
			this.anim.GetComponent<Animation>().wrapMode = WrapMode.ClampForever;
			this.anim.GetComponent<Animation>().Stop();
		}
		if (this.largeDeadfall)
		{
			this.anim.GetComponent<Animation>()["trapFall"].speed = 1.5f;
		}
		if (this.largeSpike)
		{
			this.anim.GetComponent<Animation>()["trapSpring"].speed = 4.8f;
			if (!this.sprung)
			{
				this.anim.GetComponent<Animation>()["trapSet"].speed = -1f;
				this.anim.GetComponent<Animation>().Play("trapSet");
			}
		}
		if (this.largeNoose && !this.sprung)
		{
			base.Invoke("disableAnimator", 0.2f);
			this.triggerCollider.enabled = false;
			base.Invoke("enableTrigger", 1f);
		}
		if (this.sprung)
		{
			this.OnDeserialized();
		}
		else if (this.trappedMutants.Count > 0)
		{
			this.releaseAllMutants();
		}
		if (this.hitbox && !this.sprung)
		{
			this.hitbox.SetActive(false);
		}
	}

	
	private void Awake()
	{
		if (!this.largeDeadfall && !this.largeSwingingRock)
		{
			this.RegisterRefreshTrapped(false);
		}
	}

	
	private void OnDisable()
	{
		if (!this.largeDeadfall && !this.largeSwingingRock)
		{
			this.UnregisterRefreshTrapped();
		}
	}

	
	private void OnEnable()
	{
		this.triggerCollider = base.transform.GetComponent<Collider>();
		if (this.triggerCollider)
		{
			if (this.rabbitTrap || this.fishTrap)
			{
				this.triggerCollider.enabled = false;
				base.Invoke("enableTrigger", 4f);
			}
			if (this.largeSwingingRock)
			{
				this.triggerCollider.enabled = false;
				base.Invoke("enableTrigger", 2.5f);
			}
			if (this.largeSpike || this.largeDeadfall)
			{
				this.triggerCollider.enabled = false;
				base.Invoke("enableTrigger", 1f);
			}
		}
		if (!this.largeDeadfall && !this.largeSwingingRock)
		{
			this.RegisterRefreshTrapped(false);
		}
	}

	
	private void CheckAnimReference()
	{
		if (!this.anim)
		{
			this.anim = base.transform.parent.gameObject;
		}
		if (!this.animator)
		{
			if (this.anim)
			{
				this.animator = this.anim.GetComponent<Animator>();
			}
			else
			{
				this.animator = base.transform.parent.GetComponentInChildren<Animator>();
			}
		}
	}

	
	private void disableAnimator()
	{
		this.CheckAnimReference();
		this.animator.enabled = false;
	}

	
	private void enableTrigger()
	{
		this.triggerCollider.enabled = true;
	}

	
	public void AnimateTrapMP()
	{
		if (this.hitbox)
		{
			this.hitbox.SetActive(true);
		}
		if (this.largeDeadfall)
		{
			this.CheckAnimReference();
			this.anim.GetComponent<Animation>().Play("trapFall");
			base.Invoke("enableTrapReset", 3f);
		}
		if (this.largeSpike)
		{
			this.CheckAnimReference();
			this.anim.GetComponent<Animation>().Play("trapSpring");
			base.Invoke("enableTrapReset", 3f);
		}
		if (this.largeNoose)
		{
		}
		if (this.largeSwingingRock)
		{
			base.Invoke("enableTrapReset", 3f);
		}
	}

	
	private void TriggerRabbitTrap(bool byPassAnim = false)
	{
		this.CheckAnimReference();
		if (this.anim != null)
		{
			Animation component = this.anim.GetComponent<Animation>();
			if (component != null)
			{
				if (byPassAnim)
				{
					component["trapFall"].normalizedTime = 1f;
				}
				else
				{
					component.Play("trapFall");
				}
			}
		}
		if (!byPassAnim)
		{
			this.PlayTriggerSFX();
		}
		base.Invoke("enableTrapReset", 2f);
		this.SprungTag = true;
	}

	
	
	private bool MpClientCheck
	{
		get
		{
			return !this.local && BoltNetwork.isClient;
		}
	}

	
	
	private bool MpHostCheck
	{
		get
		{
			return !this.local || BoltNetwork.isServer;
		}
	}

	
	
	private bool CheckIsMp
	{
		get
		{
			return !this.local || BoltNetwork.isRunning;
		}
	}

	
	public void TriggerLargeTrap(bool byPassAnim)
	{
		if (byPassAnim)
		{
			this.SprungTrap();
			if (!this.largeNoose)
			{
				base.Invoke("enableTrapReset", 2f);
			}
		}
		else
		{
			this.TriggerLargeTrap(null);
		}
	}

	
	public void TriggerLargeTrap(Collider other)
	{
		if (this.MpClientCheck && other == null && this.largeNoose)
		{
			this.switchNooseRope();
			base.Invoke("EnableCutTrigger", 1.5f);
			this.animator.enabled = true;
			this.animator.SetIntegerReflected("direction", 0);
			this.animator.SetBoolReflected("trapSpringBool", true);
		}
		else if (this.MpHostCheck && this.largeNoose)
		{
			this.SprungTrap();
		}
		if (this.sprung)
		{
			return;
		}
		this.CheckAnimReference();
		bool flag = !BoltNetwork.isRunning || this.MpHostCheck;
		if (this.hitbox && !this.hitbox.activeSelf)
		{
			this.hitbox.SetActive(true);
		}
		if (this.largeSwingingRock)
		{
			this.cutRope.SetActive(false);
			this.swingingRock.SendMessage("enableSwingingRock", false);
			base.Invoke("enableTrapReset", 3f);
		}
		if (this.largeDeadfall)
		{
			this.anim.GetComponent<Animation>().Play("trapFall");
			this.spikeTrapBlockerGo.SetActive(true);
			base.Invoke("enableTrapReset", 3f);
		}
		if (this.largeSpike)
		{
			this.anim.GetComponent<Animation>().Play("trapSpring");
			this.spikeTrapBlockerGo.SetActive(true);
			if (flag && other)
			{
				other.gameObject.SendMessageUpwards("enableController", SendMessageOptions.DontRequireReceiver);
				if (other.gameObject.CompareTag("enemyCollide"))
				{
					this.mutantSetup = other.transform.root.GetComponentInChildren<mutantScriptSetup>();
					if (this.mutantSetup && !this.mutantSetup.ai.creepy && !this.mutantSetup.ai.creepy_male && !this.mutantSetup.ai.creepy_fat && !this.mutantSetup.ai.creepy_baby)
					{
						other.gameObject.SendMessageUpwards("setCurrentTrap", base.gameObject, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
			base.Invoke("enableTrapReset", 3f);
		}
		if (this.largeNoose)
		{
			if (flag && other)
			{
				mutantHitReceiver component = other.transform.GetComponent<mutantHitReceiver>();
				if (other.gameObject.CompareTag("enemyCollide"))
				{
					if (component)
					{
						component.inNooseTrap = true;
						component.DisableWeaponHits(2f);
					}
					this.mutantSetup = other.transform.root.GetComponentInChildren<mutantScriptSetup>();
				}
				this.trappedMutants.Clear();
				this.trappedMutantMasks.Clear();
				GameObject gameObject = other.transform.root.gameObject;
				this.addTrappedMutant(gameObject);
				mutantScriptSetup componentInChildren = other.transform.root.GetComponentInChildren<mutantScriptSetup>();
				if (componentInChildren && componentInChildren.ai && componentInChildren.ai.pale)
				{
					this.FixPalePosition(componentInChildren, true);
				}
				if (base.transform.InverseTransformPoint(other.transform.position).x > 0f)
				{
					this.animator.SetIntegerReflected("direction", 0);
				}
				else
				{
					this.animator.SetIntegerReflected("direction", 1);
				}
				other.gameObject.SendMessageUpwards("setFootPivot", this.nooseFootPivot, SendMessageOptions.DontRequireReceiver);
				this.animator.enabled = true;
				this.animator.SetBoolReflected("trapSpringBool", true);
				if (this.mutantSetup)
				{
					if (!this.mutantSetup.ai.creepy && !this.mutantSetup.ai.creepy_male && !this.mutantSetup.ai.creepy_fat && !this.mutantSetup.ai.creepy_baby)
					{
						other.gameObject.SendMessageUpwards("setInNooseTrap", this.noosePivot);
					}
					other.gameObject.SendMessageUpwards("setCurrentTrap", base.gameObject);
				}
			}
			if (other)
			{
				other.gameObject.SendMessageUpwards("DieTrap", 2, SendMessageOptions.DontRequireReceiver);
			}
			this.switchNooseRope();
			base.Invoke("EnableCutTrigger", 1.5f);
			if (this.entity.IsOwner() && this.largeNoose)
			{
				this.entity.GetState<ITrapLargeState>().CanCutDown = true;
				this.entity.GetState<ITrapLargeState>().CanReset = false;
			}
			if (other && (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("PlayerNet")))
			{
				base.Invoke("enableTrapReset", 2f);
			}
		}
		if (this.hitbox)
		{
			base.Invoke("disableHitbox", 1.5f);
		}
		base.transform.GetComponent<Collider>().enabled = false;
		this.SprungTag = true;
		this.PlayTriggerSFX();
	}

	
	public void FixPalePosition(mutantScriptSetup enemySetup, bool state = true)
	{
		if (this.largeNoose)
		{
			enemySetup.controller.height = ((!state) ? 4.5f : 3f);
		}
	}

	
	
	public GameObject TheCage
	{
		get
		{
			BuildingHealth componentInParent = base.transform.GetComponentInParent<BuildingHealth>();
			if (componentInParent != null)
			{
				return componentInParent.gameObject;
			}
			return null;
		}
	}

	
	
	
	public bool SprungTag
	{
		get
		{
			PrefabIdentifier componentInParent = base.transform.GetComponentInParent<PrefabIdentifier>();
			return componentInParent.gameObject.CompareTag("trapSprung");
		}
		set
		{
			PrefabIdentifier componentInParent = base.transform.GetComponentInParent<PrefabIdentifier>();
			if (value)
			{
				this.sprung = true;
				componentInParent.gameObject.tag = "trapSprung";
			}
			else
			{
				componentInParent.gameObject.tag = "Untagged";
			}
			if (BoltNetwork.isRunning && this.entity.isAttached && this.entity.isOwner)
			{
				if (this.entity.StateIs<ITrapLargeState>())
				{
					this.entity.GetState<ITrapLargeState>().Sprung = this.sprung;
				}
				if (this.entity.StateIs<ITrapRabbitState>())
				{
					this.entity.GetState<ITrapRabbitState>().Sprung = this.sprung;
				}
			}
		}
	}

	
	private void OnTriggerEnter(Collider other)
	{
		bool flag = this.rabbitTrap || this.fishTrap;
		if ((other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("enemyCollide")) && !flag && !this.sprung)
		{
			if (other.gameObject.CompareTag("enemyCollide"))
			{
				mutantHitReceiver component = other.transform.GetComponent<mutantHitReceiver>();
				netId component2 = other.transform.GetComponent<netId>();
				explodeDummy component3 = other.transform.GetComponent<explodeDummy>();
				if (component && component.inNooseTrap)
				{
					return;
				}
				if (component2 || component3)
				{
					return;
				}
				Animator componentInChildren = other.transform.root.GetComponentInChildren<Animator>();
				if (componentInChildren)
				{
					if (!componentInChildren.enabled)
					{
						return;
					}
					if (componentInChildren.GetBool("deathfinalBOOL"))
					{
						return;
					}
					if (componentInChildren.GetBool("sleepBOOL"))
					{
						return;
					}
				}
			}
			if (this.MpClientCheck)
			{
				if (other.gameObject.CompareTag("Player"))
				{
					TriggerLargeTrap triggerLargeTrap = global::TriggerLargeTrap.Create(GlobalTargets.OnlyServer);
					triggerLargeTrap.Player = LocalPlayer.Entity;
					triggerLargeTrap.Trap = this.entity;
					triggerLargeTrap.Send();
					this.TriggerLargeTrap(null);
				}
			}
			else
			{
				if (this.MpHostCheck && this.entity && this.entity.isAttached && this.entity.StateIs<ITrapLargeState>())
				{
					this.entity.GetState<ITrapLargeState>().Sprung = true;
				}
				this.TriggerLargeTrap(other);
			}
		}
		if (this.fishTrap && !this.sprung && other.gameObject.CompareTag("Fish") && this.CheckIsMp)
		{
			string name = other.transform.name;
			Fish componentInChildren2 = other.transform.root.GetComponentInChildren<Fish>();
			string spawnner = (!componentInChildren2) ? string.Empty : componentInChildren2.controlScript.gameObject.transform.name;
			Vector3 position = (!componentInChildren2) ? default(Vector3) : componentInChildren2.controlScript.gameObject.transform.root.position;
			if (this.MpClientCheck || BoltNetwork.isClient)
			{
				this.SendTrappedFish(name, spawnner, position);
				return;
			}
			if (this.MpHostCheck)
			{
				this.SendTrappedFishAllClients(name, spawnner, position);
			}
		}
		if (this.MpClientCheck)
		{
			return;
		}
		if ((this.rabbitTrap && other.gameObject.CompareTag("animalCollide")) || (this.fishTrap && other.gameObject.CompareTag("Fish")))
		{
			bool flag2 = false;
			if (this.largeDeadfall)
			{
				animalType component4 = other.GetComponent<animalType>();
				if (component4 && component4.deer)
				{
					if (this.hitbox)
					{
						this.hitbox.SetActive(true);
					}
					this.CheckAnimReference();
					this.anim.GetComponent<Animation>().Play("trapFall");
					flag2 = true;
					base.Invoke("enableTrapReset", 3f);
				}
			}
			if (flag && !this.SprungTag)
			{
				string name2 = (!this.fishTrap) ? other.transform.root.gameObject.name : other.transform.gameObject.name;
				Transform transform = (!this.fishTrap) ? other.transform.root : other.transform;
				if (PoolManager.Pools["creatures"].IsSpawned(transform))
				{
					PoolManager.Pools["creatures"].Despawn(transform);
				}
				else
				{
					UnityEngine.Object.Destroy(transform.gameObject);
				}
				GameObject gameObject = this.SpawnTrappedAnimal(name2);
				if (this.fishTrap)
				{
					EventRegistry.Achievements.Publish(TfEvent.Achievements.FishTrapped, null);
				}
				this.TriggerRabbitTrap(false);
				if (this.MpHostCheck && this.entity && this.entity.isAttached && this.entity.StateIs<ITrapRabbitState>())
				{
					this.entity.GetState<ITrapRabbitState>().Sprung = true;
				}
			}
			if (flag2)
			{
				this.PlayTriggerSFX();
			}
		}
	}

	
	private void PlayTriggerSFX()
	{
		if (this.sfxPosition == null)
		{
			this.sfxPosition = base.transform;
		}
		FMODCommon.PlayOneshot(this.triggerSFX, this.sfxPosition);
	}

	
	private void OnDeserialized()
	{
		if (base.GetComponent<EmptyObjectIdentifier>())
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (this.SprungTag)
		{
			this.sprung = true;
			this.enableTrapReset();
			this.CheckAnimReference();
			Vector3 zero = Vector3.zero;
			if (this.largeNoose)
			{
				this.nooseFootPivot.transform.position.y = base.transform.position.y;
			}
			this.SprungTrap();
			if (!this.largeNoose && !this.swingingRock && !this.largeSpike && !this.largeDeadfall)
			{
				this.LoadTrapped(null);
			}
		}
		else if (this.resetTrigger && !BoltNetwork.isClient)
		{
			this.resetTrigger.SetActive(false);
		}
	}

	
	public void SendTrappedFishToClient(BoltConnection target)
	{
		if (this.sprung && this.TrappedName != null && this.TrappedName != string.Empty && this.trappedGo != null)
		{
			TriggerFishTrap triggerFishTrap = TriggerFishTrap.Create(target);
			triggerFishTrap.Trap = base.transform.root.GetComponentInChildren<BoltEntity>();
			triggerFishTrap.Fish = this.TrappedName;
			triggerFishTrap.Spawnner = string.Empty;
			triggerFishTrap.Position = base.transform.root.position;
			triggerFishTrap.Send();
		}
	}

	
	public void SendTrappedFishAllClients(string fish, string spawnner, Vector3 position)
	{
		TriggerFishTrap triggerFishTrap = TriggerFishTrap.Create(GlobalTargets.AllClients);
		triggerFishTrap.Trap = base.transform.root.GetComponentInChildren<BoltEntity>();
		triggerFishTrap.Fish = fish;
		triggerFishTrap.Spawnner = spawnner;
		triggerFishTrap.Position = position;
		triggerFishTrap.Send();
	}

	
	public void SendTrappedFish(string fish, string spawnner, Vector3 position)
	{
		TriggerFishTrap triggerFishTrap = TriggerFishTrap.Create(GlobalTargets.OnlyServer);
		triggerFishTrap.Trap = base.transform.root.GetComponentInChildren<BoltEntity>();
		triggerFishTrap.Fish = fish;
		triggerFishTrap.Spawnner = spawnner;
		triggerFishTrap.Position = position;
		triggerFishTrap.Send();
	}

	
	public void RequesTrappedFishMP()
	{
		TriggerFishTrap triggerFishTrap = TriggerFishTrap.Create(GlobalTargets.OnlyServer);
		triggerFishTrap.Trap = base.transform.root.GetComponentInChildren<BoltEntity>();
		triggerFishTrap.Fish = "request";
		triggerFishTrap.Spawnner = string.Empty;
		triggerFishTrap.Position = default(Vector3);
		triggerFishTrap.Send();
	}

	
	public IEnumerator SetTrapFish(string fish, string spawnner, Vector3 position)
	{
		if (fish == string.Empty)
		{
			this.fishTrapForce = false;
			if (this.trappedGo)
			{
				this.trappedGo.transform.parent = null;
				if (PoolManager.Pools["creatures"].IsSpawned(this.trappedGo.transform))
				{
					PoolManager.Pools["creatures"].Despawn(this.trappedGo.transform);
				}
				else
				{
					UnityEngine.Object.Destroy(this.trappedGo);
				}
				this.trappedGo = null;
				this.ResetTrapped();
			}
		}
		else if (!this.sprung || this.fishTrapForce)
		{
			if (this.MpHostCheck)
			{
				this.TriggerRabbitTrap(false);
				if (this.entity && this.entity.isAttached && this.entity.StateIs<ITrapRabbitState>())
				{
					this.entity.GetState<ITrapRabbitState>().Sprung = true;
				}
			}
			else if (this.MpClientCheck && !this.sprung)
			{
				this.OnSprungMP();
			}
			if (this.fishTrapForce)
			{
				while (!this.CanSpawnCreature(this.Prefabname(fish)) && this.fishTrapForce)
				{
					yield return null;
				}
			}
			this.fishTrapForce = false;
			Transform fishTransform = this.FindFishNearTrap(base.transform.root.position);
			if (fishTransform)
			{
				if (PoolManager.Pools["creatures"].IsSpawned(fishTransform))
				{
					PoolManager.Pools["creatures"].Despawn(fishTransform);
				}
				else
				{
					UnityEngine.Object.Destroy(fishTransform.gameObject);
				}
			}
			GameObject newAnimal = this.SpawnTrappedAnimal(fish);
		}
		yield return null;
		yield break;
	}

	
	private Transform FindFishNearTrap(Vector3 center)
	{
		float num = 10f;
		Collider[] array = Physics.OverlapSphere(center, num);
		float num2 = 3f * num;
		Fish fish = null;
		for (int i = 0; i < array.Length; i++)
		{
			Fish component = array[i].transform.root.GetComponent<Fish>();
			if (component)
			{
				float num3 = Vector3.Distance(component.transform.position, base.transform.root.position);
				if (num3 < num2)
				{
					num2 = num3;
					fish = component;
				}
			}
		}
		if (fish != null)
		{
			return fish.transform;
		}
		return null;
	}

	
	public string GetFish()
	{
		return string.Empty;
	}

	
	public string GetSpawnner()
	{
		return string.Empty;
	}

	
	public Vector3 GetSpawnnerPosition()
	{
		return Vector3.one;
	}

	
	private void SendNooseRopeAllClients()
	{
		if (this.trappedMutants.Count > 0)
		{
			SetTrappedEnemy setTrappedEnemy = SetTrappedEnemy.Create(GlobalTargets.AllClients);
			setTrappedEnemy.Enemy = this.trappedMutants[0].GetComponentInChildren<BoltEntity>();
			setTrappedEnemy.Trap = base.transform.root.GetComponentInChildren<BoltEntity>();
			setTrappedEnemy.Send();
		}
	}

	
	public void SendNooseRopeMP(BoltConnection target)
	{
		SetTrappedEnemy setTrappedEnemy = SetTrappedEnemy.Create(target);
		setTrappedEnemy.Enemy = this.trappedMutants[0].GetComponentInChildren<BoltEntity>();
		setTrappedEnemy.Trap = base.transform.root.GetComponentInChildren<BoltEntity>();
		setTrappedEnemy.Send();
	}

	
	public void RequestNooseRopeMP()
	{
		SetTrappedEnemy setTrappedEnemy = SetTrappedEnemy.Create(GlobalTargets.OnlyServer);
		setTrappedEnemy.Trap = base.transform.root.GetComponentInChildren<BoltEntity>();
		setTrappedEnemy.Send();
	}

	
	public void SetNooseRope(mutantScriptSetup setup)
	{
		this.mutantSetup = setup;
		this.switchNooseRope();
	}

	
	public void SetNooseRope(CoopMutantDummy setup)
	{
		this.nooseParent.transform.parent = setup.ragDollJoints[2];
		this.nooseParent.transform.localPosition = new Vector3(-0.134f, 0f, 0.056f);
		this.nooseParent.transform.localEulerAngles = new Vector3(0f, -90f, 90f);
	}

	
	private void switchNooseRope()
	{
		this.nooseRope1.SetActive(false);
		this.nooseRope2.SetActive(true);
		if (this.mutantSetup)
		{
			if (!this.mutantSetup.ai.creepy && !this.mutantSetup.ai.creepy_male && !this.mutantSetup.ai.creepy_baby && !this.mutantSetup.ai.creepy_fat)
			{
				this.nooseParent.transform.parent = this.mutantSetup.leftFoot;
				this.nooseParent.transform.localPosition = new Vector3(-0.134f, 0f, 0.056f);
				this.nooseParent.transform.localEulerAngles = new Vector3(0f, -90f, 90f);
				if (this.MpHostCheck && BoltNetwork.isRunning)
				{
					this.SendNooseRopeAllClients();
				}
			}
			else
			{
				this.nooseParent.transform.localPosition = new Vector3(0f, -0.834f, 0f);
				this.nooseParent.transform.localEulerAngles = new Vector3(-90f, 90f, 0f);
				if (this.MpHostCheck)
				{
					base.Invoke("enableTrapReset", 3f);
				}
			}
		}
		else
		{
			this.nooseParent.transform.localPosition = new Vector3(0f, -0.834f, 0f);
			this.nooseParent.transform.localEulerAngles = new Vector3(-90f, 90f, 0f);
			if (this.MpHostCheck)
			{
				base.Invoke("enableTrapReset", 3f);
			}
			if (this.MpClientCheck)
			{
				this.RequestNooseRopeMP();
			}
		}
	}

	
	private void disableHitbox()
	{
		if (this.hit)
		{
			this.hit.disable = true;
		}
	}

	
	public void releaseNooseTrap()
	{
		foreach (GameObject gameObject in this.trappedMutants)
		{
			if (gameObject)
			{
				this.UpdateSkinnedMeshes(gameObject, false);
			}
		}
		this.trappedMutants.Clear();
		this.trappedMutantMasks.Clear();
		if (this.cutRope)
		{
			this.cutRope.SetActive(false);
		}
		if (this.nooseRope2)
		{
			this.nooseRope2.SetActive(false);
		}
		if (this.nooseParent)
		{
			this.nooseParent.transform.parent = base.transform;
		}
		if (this.mutantSetup)
		{
			this.mutantSetup.pmEncounter.SendEvent("toRelease");
		}
		if (this.dummyGo)
		{
			this.dummyGo.SendMessage("setTrapFall", SendMessageOptions.DontRequireReceiver);
		}
		base.Invoke("enableTrapReset", 3f);
		this.dummyGo = null;
		this.SaveTrappedMutants();
	}

	
	public void releaseNooseTrapMP()
	{
		CutTriggerActivated cutTriggerActivated = CutTriggerActivated.Create(GlobalTargets.OnlyServer);
		cutTriggerActivated.Trap = this.entity;
		cutTriggerActivated.Send();
	}

	
	public void enableTrapReset()
	{
		if (this.resetTrigger)
		{
			this.resetTrigger.SetActive(true);
			if (this.entity.IsOwner() && this.largeNoose)
			{
				this.entity.GetState<ITrapLargeState>().CanCutDown = false;
				this.entity.GetState<ITrapLargeState>().CanReset = true;
			}
		}
		if (this.largeSwingingRock && this.navBlockerGo)
		{
			this.navBlockerGo.SetActive(true);
		}
	}

	
	public void removeMutant(GameObject go)
	{
		this.UpdateSkinnedMeshes(go, false);
		if (this.largeNoose)
		{
			this.trappedMutants.Clear();
			this.trappedMutantMasks.Clear();
			this.dummyGo = null;
			this.TrappedMutantNames = null;
			if (this.cutRope)
			{
				this.cutRope.SetActive(false);
			}
			if (this.nooseRope2)
			{
				this.nooseRope2.SetActive(false);
			}
			if (this.nooseParent)
			{
				this.nooseParent.transform.parent = base.transform;
			}
		}
		else if (this.trappedMutants.Contains(go))
		{
			int index = this.trappedMutants.IndexOf(go);
			this.trappedMutants.Remove(go);
			this.trappedMutantMasks.RemoveAt(index);
			this.SaveTrappedMutants();
		}
	}

	
	public void releaseTrapped()
	{
		if (this.trappedGo)
		{
			if (this.rabbitTrap || this.fishTrap)
			{
				this.releaseAnimal();
			}
			else if (this.largeNoose)
			{
				this.releaseNooseTrap();
			}
		}
		if (this.trappedMutants.Count > 0)
		{
			this.releaseAllMutants();
		}
	}

	
	public void releaseAnimal()
	{
		if (this.trappedGo)
		{
			this.ResetTrapped();
			this.trappedGo.SendMessageUpwards("releaseFromTrap", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public void setAnimalAsDead()
	{
		this.ResetTrapped();
		this.trappedGo = null;
	}

	
	public void removeFishFromTrap()
	{
		this.setAnimalAsDead();
		if (BoltNetwork.isClient)
		{
			this.SendTrappedFish(string.Empty, string.Empty, Vector3.zero);
		}
		else if (this.MpHostCheck)
		{
			this.SendTrappedFishAllClients(string.Empty, string.Empty, Vector3.zero);
		}
	}

	
	public void addTrappedMutant(object[] data)
	{
		if (this.largeDeadfall || this.largeSwingingRock)
		{
			return;
		}
		if (data.Length != 2)
		{
			Debug.LogError("Error at addTrappedMutant");
			return;
		}
		GameObject gameObject = (GameObject)data[0];
		mutantScriptSetup mutantScriptSetup = (mutantScriptSetup)data[1];
		if (!gameObject.GetComponent<getAnimatorParams>())
		{
			CoopMutantDummy component = gameObject.GetComponent<CoopMutantDummy>();
			if (!component || component.Creepy)
			{
				return;
			}
			if (this.trappedMutants.Contains(mutantScriptSetup.gameObject))
			{
				int num = this.trappedMutants.IndexOf(mutantScriptSetup.gameObject);
				this.trappedMutants.Remove(mutantScriptSetup.gameObject);
				if (num >= 0 && num < this.trappedMutantMasks.Count)
				{
					this.trappedMutants.RemoveAt(num);
				}
			}
			else if (this.trappedMutants.Contains(mutantScriptSetup.transform.parent.gameObject))
			{
				int num2 = this.trappedMutants.IndexOf(mutantScriptSetup.transform.parent.gameObject);
				this.trappedMutants.Remove(mutantScriptSetup.transform.parent.gameObject);
				if (num2 >= 0 && num2 < this.trappedMutantMasks.Count)
				{
					this.trappedMutants.RemoveAt(num2);
				}
			}
		}
		this.trappedMutants.Add(gameObject);
		this.trappedMutantMasks.Add(this.GetEnemyMask(mutantScriptSetup));
		this.UpdateSkinnedMeshes(gameObject, true);
		this.SaveTrappedMutants();
	}

	
	public void addTrappedMutant(GameObject go)
	{
		if (this.largeDeadfall || this.largeSwingingRock || this.largeNoose || this.largeSpike)
		{
			return;
		}
		this.addTrappedMutant(new object[]
		{
			go,
			go.GetComponentInChildren<mutantScriptSetup>()
		});
	}

	
	public void UpdateSkinnedMeshes(GameObject go, bool state)
	{
		SkinnedMeshRenderer[] componentsInChildren = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			skinnedMeshRenderer.updateWhenOffscreen = state;
		}
	}

	
	private string JoinNameAndMask(string name, int mask)
	{
		return this.Prefabname(name) + ";" + mask.ToString();
	}

	
	private string GetNameFromSave(string save)
	{
		string[] array = save.Split(new char[]
		{
			';'
		});
		return array[0];
	}

	
	private int GetMaskFromSave(string save)
	{
		string[] array = save.Split(new char[]
		{
			';'
		});
		int result = 0;
		if (array.Length > 1)
		{
			int.TryParse(array[1], out result);
		}
		return result;
	}

	
	private bool[] IntToMask(int value, bool[] mask, int index)
	{
		value = Mathf.Clamp(value, 0, 7);
		mask[index] = (value % 2 != 0);
		mask[index + 1] = ((value >= 2 && value < 4) || value >= 6);
		mask[index + 2] = (value >= 4);
		return mask;
	}

	
	private int MaskToInt(bool[] mask, int index)
	{
		int num = 0;
		num += ((!mask[index]) ? 0 : 1);
		num += ((!mask[index + 1]) ? 0 : 2);
		return num + ((!mask[index + 2]) ? 0 : 4);
	}

	
	public int GetEnemyMask(mutantScriptSetup setup)
	{
		bool[] array = new bool[30];
		int num = 0;
		int num2 = 0;
		if (setup && setup.ai)
		{
			array[num++] = setup.ai.maleSkinny;
			array[num++] = setup.ai.femaleSkinny;
			array[num++] = setup.ai.male;
			array[num++] = setup.ai.female;
			array[num++] = setup.ai.fireman;
			array[num++] = setup.ai.fireman_dynamite;
			array[num++] = setup.ai.pale;
			array[num++] = setup.ai.painted;
			array[num++] = setup.ai.skinned;
			array[num++] = (setup.bodyVariation && setup.bodyVariation.Hair && setup.bodyVariation.Hair.activeSelf);
			array[num++] = (setup.bodyVariation && setup.bodyVariation.Clothing && setup.bodyVariation.Clothing.activeSelf);
			array[num++] = (setup.health && setup.health.alreadyBurnt);
			array[num++] = (setup.health && setup.health.poisoned);
			array = this.IntToMask((!setup.propManager) ? 0 : setup.propManager.regularMaleDice, array, num);
			num += 3;
			array = this.IntToMask((!setup.propManager) ? 0 : setup.propManager.LeaderDice, array, num);
			num += 3;
			array = this.IntToMask((!setup.bodyVariation) ? 0 : setup.bodyVariation.femaleDice, array, num);
			num += 3;
			array = this.IntToMask((!setup.bodyVariation) ? 0 : setup.bodyVariation.FireDice, array, num);
			num += 3;
			for (int i = 0; i < num; i++)
			{
				if (array[i])
				{
					num2 |= 1 << i;
				}
			}
		}
		return num2;
	}

	
	public void ApplyEnemyMask(mutantScriptSetup setup, int properties)
	{
		if (properties == 0)
		{
			return;
		}
		if (setup && setup.ai)
		{
			bool[] array = new bool[30];
			for (int i = 0; i < array.Length; i++)
			{
				int num = properties & 1 << i;
				array[i] = (num > 0);
			}
			int num2 = 0;
			setup.ai.maleSkinny = array[num2++];
			setup.ai.femaleSkinny = array[num2++];
			setup.ai.male = array[num2++];
			setup.ai.female = array[num2++];
			setup.ai.fireman = array[num2++];
			setup.ai.fireman_dynamite = array[num2++];
			setup.ai.pale = array[num2++];
			setup.ai.painted = array[num2++];
			setup.ai.skinned = array[num2++];
			if (array[num2++] && setup.bodyVariation && setup.bodyVariation.Hair)
			{
				setup.bodyVariation.Hair.SetActive(true);
			}
			if (array[num2++] && setup.bodyVariation && setup.bodyVariation.Clothing)
			{
				setup.bodyVariation.Clothing.SetActive(true);
			}
			setup.health.alreadyBurnt = array[num2++];
			setup.health.poisoned = array[num2++];
			if (setup.propManager)
			{
				setup.propManager.regularMaleDice = this.MaskToInt(array, num2);
			}
			num2 += 3;
			if (setup.propManager)
			{
				setup.propManager.LeaderDice = this.MaskToInt(array, num2);
			}
			num2 += 3;
			if (setup.bodyVariation)
			{
				setup.bodyVariation.femaleDice = this.MaskToInt(array, num2);
			}
			num2 += 3;
			if (setup.bodyVariation)
			{
				setup.bodyVariation.FireDice = this.MaskToInt(array, num2);
			}
			num2 += 3;
		}
		else
		{
			Debug.LogError("ApplyEnemyMask: No setup or setp.ai");
		}
	}

	
	private void releaseAllMutants()
	{
		if (this.largeSpike)
		{
			foreach (GameObject gameObject in this.trappedMutants)
			{
				if (gameObject)
				{
					this.UpdateSkinnedMeshes(gameObject, false);
					gameObject.SendMessage("releaseFromSpikeTrap", SendMessageOptions.DontRequireReceiver);
				}
			}
			this.trappedMutants.Clear();
			this.trappedMutantMasks.Clear();
		}
		else if (this.largeNoose)
		{
			this.releaseNooseTrap();
		}
		this.SaveTrappedMutants();
	}

	
	private void OnDestroy()
	{
		if (this.largeNoose || this.largeSpike)
		{
			this.releaseAllMutants();
		}
		if (this.nooseParent && !this.nooseParent.transform.parent)
		{
			UnityEngine.Object.Destroy(this.nooseParent);
		}
		if (!this.sprung)
		{
			this.releaseAllMutants();
		}
		if (this.rabbitTrap || this.fishTrap)
		{
			this.releaseAnimal();
		}
	}

	
	private void animToEndState(string animState)
	{
		if (this.anim != null)
		{
			Animation component = this.anim.GetComponent<Animation>();
			if (component != null)
			{
				component[animState].normalizedTime = 1f;
				component.Play(animState);
			}
		}
	}

	
	private bool IsPlayerInRange()
	{
		if (this.rabbitTrap || this.fishTrap)
		{
			return this.IsPlayerInRange(20f, 5f);
		}
		return this.largeNoose || this.IsPlayerInRange(50f, 5f);
	}

	
	private bool IsPlayerInRange(float range, float threshold)
	{
		if (this.spawnZone)
		{
			if (Scene.SceneTracker.GetClosestPlayerDistanceFromPos(base.transform.position) > range + threshold)
			{
				this.spawnZone = false;
			}
		}
		else if (!this.spawnZone && Scene.SceneTracker.GetClosestPlayerDistanceFromPos(base.transform.position) < range)
		{
			this.spawnZone = true;
		}
		return this.spawnZone;
	}

	
	private bool IsLocalPlayerInRange(float range)
	{
		return Vector3.Distance(base.transform.root.position, LocalPlayer.Transform.position) < range;
	}

	
	private void SprungTrap()
	{
		if (this.hitbox && !this.hitbox.activeSelf)
		{
			this.hitbox.SetActive(true);
		}
		if (this.rabbitTrap)
		{
			this.animToEndState("trapFall");
		}
		else if (this.largeSwingingRock)
		{
			this.cutRope.SetActive(false);
			this.swingingRock.SendMessage("enableSwingingRock", true);
		}
		else if (this.largeDeadfall)
		{
			this.animToEndState("trapFall");
			this.spikeTrapBlockerGo.SetActive(true);
		}
		else if (this.largeSpike)
		{
			this.animToEndState("trapSpring");
			this.spikeTrapBlockerGo.SetActive(true);
		}
		else if (this.largeNoose)
		{
			this.animator.enabled = true;
			this.animator.SetBoolReflected("trapSpringBool", true);
			this.animator.Play("sprungLoop");
			this.switchNooseRope();
			base.Invoke("EnableCutTrigger", 1.5f);
			if (this.MpClientCheck)
			{
				this.animator.SetIntegerReflected("direction", 0);
			}
			if (this.entity.IsOwner())
			{
				this.entity.GetState<ITrapLargeState>().CanCutDown = true;
				this.entity.GetState<ITrapLargeState>().CanReset = false;
			}
		}
	}

	
	private void EnableCutTrigger()
	{
		this.cutTrigger.SetActive(true);
	}

	
	public void CopyTrapStatusTo(trapTrigger otherTrap)
	{
		if (this.SprungTag && this != null && otherTrap != null)
		{
			otherTrap.SprungTag = true;
			if (!this.largeNoose)
			{
				if (this.trappedMutants.Count > 0)
				{
					otherTrap.trappedMutants.Clear();
					otherTrap.trappedMutantMasks.Clear();
					if (this.largeNoose && this.trappedMutants.Count == 1)
					{
						otherTrap.dummyGo = this.dummyGo;
						otherTrap.mutantSetup = this.mutantSetup;
					}
					foreach (GameObject go in this.trappedMutants)
					{
						otherTrap.addTrappedMutant(go);
					}
				}
				if (this.TrappedGo)
				{
					otherTrap.TrappedGo = this.TrappedGo;
				}
				if (this.TrappedName != null && this.TrappedName != string.Empty)
				{
					otherTrap.SaveTrapped(this.TrappedName);
				}
				otherTrap.fishTrapStart = this.fishTrapStart;
				otherTrap.fishTrapForce = false;
			}
		}
	}

	
	public GameObject anim;

	
	public GameObject hitbox;

	
	public GameObject noosePivot;

	
	public GameObject nooseParent;

	
	public GameObject nooseRope1;

	
	public GameObject nooseRope2;

	
	public GameObject nooseFootPivot;

	
	public GameObject cutRope;

	
	public GameObject cutTrigger;

	
	public GameObject resetTrigger;

	
	public GameObject dummyGo;

	
	public GameObject spikeTrapBlockerGo;

	
	public GameObject swingingRock;

	
	public GameObject navBlockerGo;

	
	private Animator animator;

	
	public mutantScriptSetup mutantSetup;

	
	private Collider triggerCollider;

	
	public List<GameObject> trappedMutants = new List<GameObject>();

	
	public List<int> trappedMutantMasks = new List<int>();

	
	private int nooseTrapDeathHash = Animator.StringToHash("nooseTrapDeath");

	
	private int runToTrapNooseHash = Animator.StringToHash("runToTrapNoose");

	
	private GameObject trappedGo;

	
	public bool loaded;

	
	public bool local;

	
	public bool fishTrapStart = true;

	
	public bool fishTrapForce;

	
	public bool largeSpike;

	
	public bool largeDeadfall;

	
	public bool largeNoose;

	
	public bool largeSwingingRock;

	
	public bool rabbitTrap;

	
	public bool fishTrap;

	
	[Header("FMOD")]
	public Transform sfxPosition;

	
	public string triggerSFX;

	
	private trapHit hit;

	
	public bool sprung;

	
	private int WsToken = -1;

	
	[SerializeThis]
	private string TrappedName;

	
	[SerializeThis]
	public string[] TrappedMutantNames;

	
	[SerializeThis]
	public float[] TrappedMutantPositions;

	
	private bool isNotCreepy = true;

	
	private bool spawnZone;
}
