﻿using System;
using System.Collections;
using FMOD.Studio;
using PathologicalGames;
using TheForest.Tools;
using TheForest.Utils;
using TheForest.Utils.Settings;
using UnityEngine;

public class animalHealth : MonoBehaviour
{
	public bool Trapped
	{
		get
		{
			return this.trapped;
		}
		set
		{
			this.trapped = value;
		}
	}

	public GameObject Trap
	{
		get
		{
			return this.trap;
		}
		set
		{
			this.trap = value;
			if (this.trap != null)
			{
				this.Trapped = true;
			}
		}
	}

	private void Awake()
	{
		this.ai = base.GetComponent<animalAI>();
		this.spawnFunctions = base.GetComponent<animalSpawnFunctions>();
		this.scene = Scene.SceneTracker;
		this.animator = base.GetComponentInChildren<Animator>();
		this.bloodPropertyBlock = new MaterialPropertyBlock();
		this.startHealth = this.Health;
	}

	private void OnEnable()
	{
		base.StartCoroutine(this.delayedAnimalVisRoutine());
		if (this.Fire)
		{
			this.Fire.SetActive(false);
		}
		this.Burning = false;
	}

	private void Start()
	{
		PlayMakerFSM[] components = base.gameObject.GetComponents<PlayMakerFSM>();
		foreach (PlayMakerFSM playMakerFSM in components)
		{
			if (playMakerFSM.FsmName == "aiBaseFSM")
			{
				this.pmBase = playMakerFSM;
			}
		}
		if (this.Trapped && this.Trap != null)
		{
			this.setTrapped(this.Trap);
		}
		this.ResetSkinDamage();
		this.coop = base.GetComponent<CoopAnimal>();
	}

	private void Update()
	{
		this.UpdateOnFireEvent();
	}

	private void OnDisabled()
	{
		base.CancelInvoke("HitPoison");
		base.CancelInvoke("disablePoison");
		base.CancelInvoke("HitFire");
		base.StopAllCoroutines();
		if (this.spawnFunctions.deer)
		{
			this.resetAnimalFleeDistance();
		}
	}

	public void Poison()
	{
		base.CancelInvoke("HitPoison");
		base.CancelInvoke("disablePoison");
		if (this.HitWithoutSound(2) == animalHealth.HitResult.Alive)
		{
			base.InvokeRepeating("HitPoison", 0.5f, UnityEngine.Random.Range(5f, 6f));
			base.Invoke("disablePoison", UnityEngine.Random.Range(40f, 60f));
			this.PlayEvent(this.HitEvent);
		}
	}

	private void disablePoison()
	{
		base.CancelInvoke("HitPoison");
	}

	private void HitPoison()
	{
		int min = 2;
		int max = 4;
		int damage = UnityEngine.Random.Range(min, max);
		this.Hit(damage);
	}

	public void FireDamage()
	{
		if (this.Fire)
		{
			this.Fire.SetActive(true);
		}
		this.Hit(10);
		base.SendMessageUpwards("getAttackDirection", 3);
		base.InvokeRepeating("HitFire", 1f, 1f);
	}

	private void PlayEvent(string path)
	{
		FMODCommon.PlayOneshotNetworked(path, base.transform, FMODCommon.NetworkRole.Server);
	}

	public void Burn()
	{
		if (this.Fire)
		{
			this.Fire.SetActive(true);
		}
		animalHealth.HitResult hitResult = this.HitWithoutSound(3);
		if (this.spawnFunctions.deer)
		{
			this.StartOnFireEvent();
			this.PlayEvent(this.DieEvent);
		}
		if (hitResult == animalHealth.HitResult.Alive)
		{
		}
		base.InvokeRepeating("HitFire", 3f, 3f);
		base.Invoke("cancelFire", 15f);
	}

	private void TrapDamage()
	{
		this.DieTrap();
	}

	private void HitFire()
	{
		this.Health -= 5;
		this.HitWithoutSound(1);
		this.Burning = true;
	}

	private void cancelFire()
	{
		if (this.Fire)
		{
			this.Fire.SetActive(false);
		}
		base.CancelInvoke("HitFire");
		this.StopOnFireEvent();
		this.Burning = false;
	}

	private void HitReal(int damage)
	{
		this.Hit(damage);
	}

	private animalHealth.HitResult HitWithoutSound(int damage)
	{
		animalHealth.HitResult result;
		try
		{
			this.hitSoundEnabled = false;
			result = this.Hit(damage);
		}
		finally
		{
			this.hitSoundEnabled = true;
		}
		return result;
	}

	public void ApplyAnimalSkinDamage(int direction)
	{
		if (!this.mySkin)
		{
			return;
		}
		this.mySkin.GetPropertyBlock(this.bloodPropertyBlock);
		switch (direction)
		{
		case 0:
		{
			float @float = this.bloodPropertyBlock.GetFloat("_Damage1");
			float num = Mathf.Clamp01(@float + 0.5f);
			this.bloodPropertyBlock.SetFloat("_Damage1", num);
			if (this.coop && this.coop.entity.IsAttached())
			{
				this.coop.state.skinDamage1 = num;
			}
			break;
		}
		case 1:
		{
			float @float = this.bloodPropertyBlock.GetFloat("_Damage2");
			float num = Mathf.Clamp01(@float + 0.5f);
			this.bloodPropertyBlock.SetFloat("_Damage2", num);
			if (this.coop && this.coop.entity.IsAttached())
			{
				this.coop.state.skinDamage2 = num;
			}
			break;
		}
		case 2:
		{
			float @float = this.bloodPropertyBlock.GetFloat("_Damage3");
			float num = Mathf.Clamp01(@float + 0.5f);
			this.bloodPropertyBlock.SetFloat("_Damage3", num);
			if (this.coop && this.coop.entity.IsAttached())
			{
				this.coop.state.skinDamage3 = num;
			}
			break;
		}
		case 3:
		{
			float @float = this.bloodPropertyBlock.GetFloat("_Damage4");
			float num = Mathf.Clamp01(@float + 0.5f);
			this.bloodPropertyBlock.SetFloat("_Damage4", num);
			if (this.coop && this.coop.entity.IsAttached())
			{
				this.coop.state.skinDamage4 = num;
			}
			break;
		}
		}
		this.mySkin.SetPropertyBlock(this.bloodPropertyBlock);
	}

	private void ResetSkinDamage()
	{
		if (!this.mySkin)
		{
			return;
		}
		this.mySkin.GetPropertyBlock(this.bloodPropertyBlock);
		this.bloodPropertyBlock.SetFloat("_Damage1", 0f);
		this.bloodPropertyBlock.SetFloat("_Damage2", 0f);
		this.bloodPropertyBlock.SetFloat("_Damage3", 0f);
		this.bloodPropertyBlock.SetFloat("_Damage4", 0f);
		this.mySkin.SetPropertyBlock(this.bloodPropertyBlock);
	}

	public animalHealth.HitResult Hit(int damage)
	{
		if (this.spawnFunctions.tortoise && this.animator.GetBool("inShell") && !this.Burning)
		{
			return animalHealth.HitResult.Alive;
		}
		if (Time.time <= this.hitCoolDown)
		{
			return animalHealth.HitResult.Alive;
		}
		this.hitCoolDown = Time.time + 0.3f;
		if (this.spawnFunctions.deer)
		{
			base.StopCoroutine("setAnimalFleeDistance");
			base.StartCoroutine("setAnimalFleeDistance");
		}
		this.Health -= damage;
		this.Blood();
		this.pmBase.SendEvent("gotHit");
		if (this.Health < 1)
		{
			this.Die();
			return animalHealth.HitResult.Dead;
		}
		if (this.hitSoundEnabled)
		{
			this.PlayEvent(this.HitEvent);
		}
		return animalHealth.HitResult.Alive;
	}

	private void Blood()
	{
		if (this.BloodSplat1)
		{
			if (!this.BloodSplat1.activeSelf)
			{
				this.BloodSplat1.SetActive(true);
			}
			else if (!this.BloodSplat2.activeSelf)
			{
				this.BloodSplat2.SetActive(true);
			}
			else if (!this.BloodSplat3.activeSelf)
			{
				this.BloodSplat3.SetActive(true);
			}
		}
	}

	private void StopBlood()
	{
	}

	private void getCurrentHealth()
	{
		this.pmBase.FsmVariables.GetFsmInt("statHealth").Value = this.Health;
	}

	private void Die()
	{
		AnimalDespawner component = base.GetComponent<AnimalDespawner>();
		if (component && component.SpawnedFromZone)
		{
			component.SpawnedFromZone.MaxAnimalsTotal--;
			component.SpawnedFromZone.AddSpawnBackTimes.Enqueue(Time.time + component.SpawnedFromZone.DelayAfterKillTime * GameSettings.Animals.RespawnDelayRatio);
		}
		if (this.spawnFunctions.lizard)
		{
			EventRegistry.Animal.Publish(TfEvent.KilledLizard, base.gameObject);
			this.scene.maxLizardAmount--;
		}
		if (this.spawnFunctions.rabbit)
		{
			EventRegistry.Animal.Publish(TfEvent.KilledRabbit, base.gameObject);
			this.scene.maxRabbitAmount -= 2;
		}
		if (this.spawnFunctions.turtle)
		{
			EventRegistry.Animal.Publish(TfEvent.KilledTurtle, base.gameObject);
			this.scene.maxTurtleAmount--;
			if (this.spawnFunctions.controller)
			{
				this.spawnFunctions.controller.addToSpawnDelay(120f);
			}
		}
		if (this.spawnFunctions.tortoise)
		{
			EventRegistry.Animal.Publish(TfEvent.KilledTurtle, base.gameObject);
			this.scene.maxTortoiseAmount--;
		}
		if (this.spawnFunctions.raccoon)
		{
			EventRegistry.Animal.Publish(TfEvent.KilledRaccoon, base.gameObject);
			this.scene.maxRaccoonAmount--;
		}
		if (this.spawnFunctions.deer)
		{
			EventRegistry.Animal.Publish(TfEvent.KilledDeer, base.gameObject);
			this.scene.maxDeerAmount--;
		}
		base.CancelInvoke("HitPoison");
		base.CancelInvoke("disablePoison");
		base.CancelInvoke("HitFire");
		if (this.Trapped && this.Trap)
		{
			this.Trap.SendMessageUpwards("setAnimalAsDead", SendMessageOptions.DontRequireReceiver);
		}
		this.Trap = null;
		this.Trapped = false;
		if (!this.spawnFunctions.deer && !this.Fire)
		{
			this.PlayEvent(this.DieEvent);
		}
		this.ai.goRagdoll();
		this.resetHealth();
	}

	private void resetHealth()
	{
		this.Health = this.startHealth;
		this.ResetSkinDamage();
	}

	private void resetDeathBlock()
	{
	}

	private void DieTrap()
	{
		this.Hit(100);
		base.Invoke("resetHealth", 1f);
		this.Trapped = false;
		this.Trap = null;
	}

	private void scaredTimeout()
	{
		this.pmBase.FsmVariables.GetFsmBool("scaredBool").Value = false;
	}

	private void setTrapped(GameObject go)
	{
		this.Trapped = true;
		this.Trap = go;
		Rigidbody component = base.gameObject.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = true;
		}
		this.ai.stopMovement();
		if (this.pmBase == null)
		{
			PlayMakerFSM[] components = base.gameObject.GetComponents<PlayMakerFSM>();
			foreach (PlayMakerFSM playMakerFSM in components)
			{
				if (playMakerFSM.FsmName == "aiBaseFSM")
				{
					this.pmBase = playMakerFSM;
				}
			}
		}
		this.pmBase.SendEvent("toTrapped");
		Vector3 position = go.transform.position;
		position.y = base.transform.position.y;
		base.transform.position = position;
		if (this.spawnFunctions != null && this.spawnFunctions.lizard && this.ai != null && this.ai.animatorTr != null)
		{
			this.ai.animatorTr.rotation = go.transform.rotation;
		}
	}

	public void releaseFromTrap()
	{
		this.Trapped = false;
		this.Trap = null;
		Rigidbody component = base.gameObject.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = false;
		}
		this.ai.startMovement();
		this.pmBase.SendEvent("toTrapReset");
	}

	public void Explosion()
	{
		if (this.explodedGo)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.explodedGo, base.transform.position, base.transform.rotation);
			if (PoolManager.Pools["creatures"].IsSpawned(base.transform))
			{
				this.spawnFunctions.despawn();
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else
		{
			this.ai.goRagdoll();
		}
	}

	private void UpdateOnFireEvent()
	{
		if (this.onFireEventInstance != null)
		{
			UnityUtil.ERRCHECK(this.onFireEventInstance.set3DAttributes(base.transform.to3DAttributes()));
		}
	}

	private void StartOnFireEvent()
	{
		if (SteamDSConfig.isDedicatedServer)
		{
			return;
		}
		if (!FMOD_StudioSystem.ForceFmodOff && FMOD_StudioSystem.instance && this.onFireEventInstance == null && this.onFireEvent != null && this.onFireEvent.Length > 0)
		{
			this.onFireEventInstance = FMOD_StudioSystem.instance.GetEvent(this.onFireEvent);
			if (this.onFireEventInstance != null)
			{
				UnityUtil.ERRCHECK(this.onFireEventInstance.start());
			}
		}
	}

	private void StopOnFireEvent()
	{
		if (this.onFireEventInstance != null)
		{
			UnityUtil.ERRCHECK(this.onFireEventInstance.stop(STOP_MODE.ALLOWFADEOUT));
			UnityUtil.ERRCHECK(this.onFireEventInstance.release());
			this.onFireEventInstance = null;
		}
	}

	public static int GetAnimalHitDirection(float targetAngle)
	{
		if (Mathf.Abs(targetAngle) < 45f)
		{
			return 0;
		}
		if (Mathf.Abs(targetAngle) > 135f)
		{
			return 3;
		}
		if (targetAngle < 0f)
		{
			return 1;
		}
		return 2;
	}

	private IEnumerator setAnimalFleeDistance()
	{
		this.ai.playMaker.FsmVariables.GetFsmFloat("runAwayDist").Value = 50f;
		yield return YieldPresets.WaitThirteenSeconds;
		yield return YieldPresets.WaitTwentySeconds;
		this.ai.playMaker.FsmVariables.GetFsmFloat("runAwayDist").Value = 22f;
		yield break;
	}

	private void resetAnimalFleeDistance()
	{
		base.StopCoroutine("setAnimalFleeDistance");
		this.ai.playMaker.FsmVariables.GetFsmFloat("runAwayDist").Value = 22f;
	}

	private IEnumerator delayedAnimalVisRoutine()
	{
		float t = 0f;
		while (t < 0.5f)
		{
			t += Time.deltaTime;
			if (this.mySkin)
			{
				this.mySkin.enabled = false;
			}
			yield return null;
		}
		if (this.mySkin)
		{
			this.mySkin.enabled = true;
		}
		yield break;
	}

	private PlayMakerFSM pmBase;

	public GameObject ClubTrigger;

	private animalAI ai;

	private animalSpawnFunctions spawnFunctions;

	private sceneTracker scene;

	private Animator animator;

	private CoopAnimal coop;

	public int Health;

	public GameObject Fire;

	private float distance;

	public string HitEvent;

	public string DieEvent;

	public string onFireEvent;

	public GameObject RagDoll;

	public GameObject RagDollFire;

	public GameObject RagDollTrap;

	private int RandomSplurt;

	private bool Burning;

	private GameObject AiControl;

	public GameObject MyBody;

	public GameObject explodedGo;

	public GameObject BloodSplat1;

	public GameObject BloodSplat2;

	public GameObject BloodSplat3;

	public GameObject BloodSplat4;

	public GameObject BloodSplat5;

	public Material Blood1;

	public Material Blood2;

	public Material Blood3;

	public Material Blood4;

	public Renderer mySkin;

	private MaterialPropertyBlock bloodPropertyBlock;

	private bool trapped;

	private GameObject trap;

	private int startHealth;

	private EventInstance onFireEventInstance;

	private bool hitSoundEnabled = true;

	private float hitCoolDown;

	public enum HitResult
	{
		Alive,
		Dead
	}
}
