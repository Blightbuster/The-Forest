using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using TheForest.Utils;
using UnityEngine;


public class mutantScriptSetup : MonoBehaviour
{
	
	private void OnDeserialized()
	{
		this.doAwake();
	}

	
	private void Awake()
	{
		this.doAwake();
	}

	
	private void doAwake()
	{
		if (this.disableAiForDebug)
		{
			this.disableForDebug();
		}
		this.allFSM = base.gameObject.GetComponents<PlayMakerFSM>();
		foreach (PlayMakerFSM playMakerFSM in this.allFSM)
		{
			if (playMakerFSM.FsmName == "action_combatFSM")
			{
				this.pmCombat = playMakerFSM;
				this.actionFSM.Add(playMakerFSM);
			}
			if (playMakerFSM.FsmName == "global_motorFSM")
			{
				this.pmMotor = playMakerFSM;
			}
			if (playMakerFSM.FsmName == "action_sleepingFSM")
			{
				this.pmSleep = playMakerFSM;
				this.actionFSM.Add(playMakerFSM);
			}
			if (playMakerFSM.FsmName == "global_brainFSM")
			{
				this.pmBrain = playMakerFSM;
			}
			if (playMakerFSM.FsmName == "action_encounterFSM")
			{
				this.pmEncounter = playMakerFSM;
				this.actionFSM.Add(playMakerFSM);
			}
		}
		this.thisGo = base.gameObject;
		this.rootTr = base.transform.root.transform;
		this.rotateTr = base.transform;
		this.waterDetect = base.transform.root.GetComponentInChildren<mutantWaterDetect>();
		this.familyFunctions = base.transform.root.GetComponent<mutantFamilyFunctions>();
		this.animator = base.GetComponent<Animator>();
		this.ai = base.GetComponent<mutantAI>();
		this.aiManager = base.GetComponent<mutantAiManager>();
		this.typeSetup = base.transform.root.GetComponent<mutantTypeSetup>();
		this.health = base.GetComponent<EnemyHealth>();
		this.dayCycle = base.GetComponentInChildren<mutantDayCycle>();
		this.enemyEvents = base.transform.GetComponent<enemyAnimEvents>();
		this.controller = base.transform.root.GetComponent<CharacterController>();
		this.hashs = base.transform.GetComponent<mutantMaleHashId>();
		this.propManager = base.transform.GetComponent<mutantPropManager>();
		this.bodyVariation = base.transform.GetComponentInChildren<setupBodyVariation>();
		this.collisionDetect = base.transform.GetComponentInChildren<mutantCollisionDetect>();
		this.hitReceiver = base.transform.GetComponentInChildren<mutantHitReceiver>();
		this.animControl = base.transform.GetComponentInChildren<mutantAnimatorControl>();
		this.hitReactions = base.transform.root.GetComponent<mutantHitReactions>();
		this.targetFunctions = base.transform.GetComponent<mutantTargetFunctions>();
		this.followerFunctions = base.transform.root.GetComponent<mutantFollowerFunctions>();
		this.pmCombatScript = base.transform.GetComponent<pmCombatReplace>();
		this.pmSearchScript = base.transform.GetComponent<pmSearchReplace>();
		this.arrowSticker = base.transform.GetComponentInChildren<arrowStickToTarget>();
		this.vis = base.transform.GetComponentInChildren<mutantVis>();
		base.Invoke("initVis", 1f);
		if (!this.disableAiForDebug)
		{
			this.sceneInfo = Scene.SceneTracker;
		}
		this.search = base.GetComponent<mutantSearchFunctions>();
		this.worldSearch = base.transform.GetComponent<mutantWorldSearchFunctions>();
		this.mutantStats = base.transform.root.GetComponent<targetStats>();
		Transform[] componentsInChildren = base.transform.root.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == "char_Hips")
			{
				this.hipsJoint = transform;
			}
			if (transform.name == "char_Hips1")
			{
				this.hipsJoint = transform;
			}
			if (transform.name == "char_Head")
			{
				this.headJoint = transform.gameObject;
			}
			if (transform.name == "char_LeftHandWeapon")
			{
				this.charLeftWeaponGo = transform.gameObject;
			}
			if (transform.name == "currentWaypoint")
			{
				this.currentWaypoint = transform.gameObject;
			}
			if (transform.name == "lastPlayerSighting")
			{
				this.lastSighting = transform.gameObject;
			}
			if (transform.name == "tempLookat")
			{
				this.lookatTr = transform;
			}
			if (transform.name == "char_club_mesh1")
			{
				this.weapon = transform.gameObject;
			}
			if (transform.name == "FireStick")
			{
				this.fireWeapon = transform.gameObject;
			}
			if (transform.name == "char_RightHand")
			{
				this.clawsWeapon = transform.gameObject;
			}
			if (transform.name == "weaponLeftGO")
			{
				this.leftWeapon = transform.gameObject;
			}
			if (transform.name == "weaponLeftGO1")
			{
				this.leftWeapon1 = transform.gameObject;
			}
			if (transform.name == "weaponRightGO")
			{
				this.rightWeapon = transform.gameObject;
			}
			if (transform.name == "mainHitTrigger")
			{
				this.mainWeapon = transform.gameObject;
			}
			if (transform.name == "fireBomb")
			{
				this.fireBombGo = transform.gameObject;
			}
			if (transform.name == "dragPointGo")
			{
				this.familyFunctions.dragPoint = transform.gameObject;
			}
			if (transform.name == "bodyCollision")
			{
				if (this.pmCombat)
				{
					this.pmCombat.FsmVariables.GetFsmGameObject("bodyCollisionGo").Value = transform.gameObject;
				}
				this.bodyCollisionCollider = transform.GetComponent<CapsuleCollider>();
			}
			if (transform.name == "char_LeftFoot")
			{
				this.leftFoot = transform;
			}
			if (transform.name == "char_RightFoot")
			{
				this.rightFoot = transform;
			}
			if (transform.name == "headCollision")
			{
				this.headColliderGo = transform.gameObject;
			}
		}
	}

	
	private void Start()
	{
		this.hashName = Animator.StringToHash(base.transform.root.gameObject.name);
		if (this.pmSleep)
		{
			this.pmSleep.FsmVariables.GetFsmGameObject("leftHandGo").Value = this.charLeftWeaponGo;
		}
		if (this.pmEncounter)
		{
			this.pmEncounter.FsmVariables.GetFsmGameObject("leftHandGo").Value = this.charLeftWeaponGo;
		}
		if (this.pmCombat)
		{
			this.pmCombat.FsmVariables.GetFsmGameObject("wayPointGO").Value = this.currentWaypoint;
		}
		if (this.pmCombat)
		{
			this.pmCombat.FsmVariables.GetFsmInt("HashName").Value = this.hashName;
		}
		if (this.pmCombat)
		{
			this.pmCombat.FsmVariables.GetFsmGameObject("playerFsmGO").Value = LocalPlayer.PlayerBase;
			this.fsmTarget = this.pmCombat.FsmVariables.GetFsmGameObject("target");
			this.pmCombat.FsmVariables.GetFsmGameObject("playerGO").Value = LocalPlayer.GameObject;
		}
	}

	
	private void initVis()
	{
		if (this.vis)
		{
			this.vis.enabled = true;
		}
	}

	
	public void setupPlayer()
	{
		if (this.pmCombat)
		{
			this.pmCombat.FsmVariables.GetFsmGameObject("playerFsmGO").Value = LocalPlayer.PlayerBase;
			this.fsmTarget = this.pmCombat.FsmVariables.GetFsmGameObject("target");
			this.pmCombat.FsmVariables.GetFsmGameObject("playerGO").Value = LocalPlayer.GameObject;
		}
	}

	
	public IEnumerator disableNonActiveFSM(string name)
	{
		if (!this.disableAiForDebug)
		{
			foreach (PlayMakerFSM playMakerFSM in this.actionFSM)
			{
				if (playMakerFSM.FsmName != name)
				{
					playMakerFSM.SendEvent("toDisableFSM");
					playMakerFSM.enabled = false;
				}
			}
			if (name != "action_searchFSM")
			{
				this.pmSearchScript.toDisableSearchEvent();
			}
			yield return null;
		}
		yield break;
	}

	
	public IEnumerator disableAllFSM()
	{
		foreach (PlayMakerFSM pm in this.allFSM)
		{
			pm.SendEvent("toDisableFSM");
			yield return YieldPresets.WaitForFixedUpdate;
			pm.enabled = false;
		}
		yield return null;
		yield break;
	}

	
	public void disableForDebug()
	{
		PlayMakerFSM[] components = base.GetComponents<PlayMakerFSM>();
		foreach (PlayMakerFSM playMakerFSM in components)
		{
			playMakerFSM.enabled = false;
			Debug.Log(playMakerFSM.FsmName + " was disabled");
		}
		PlayMakerArrayListProxy[] components2 = base.GetComponents<PlayMakerArrayListProxy>();
		foreach (PlayMakerArrayListProxy playMakerArrayListProxy in components2)
		{
			playMakerArrayListProxy.enabled = false;
		}
		base.GetComponent<mutantAI>().enabled = false;
		mutantAnimatorControl component = base.GetComponent<mutantAnimatorControl>();
		if (component)
		{
			component.enabled = false;
		}
		creepyAnimatorControl component2 = base.GetComponent<creepyAnimatorControl>();
		if (component2)
		{
			component2.enabled = false;
		}
		mutantBabyAnimatorControl component3 = base.GetComponent<mutantBabyAnimatorControl>();
		if (component3)
		{
			component3.enabled = false;
		}
	}

	
	public mutantWaterDetect waterDetect;

	
	public mutantFamilyFunctions familyFunctions;

	
	public Animator animator;

	
	public mutantAI ai;

	
	public mutantAiManager aiManager;

	
	public sceneTracker sceneInfo;

	
	public mutantTypeSetup typeSetup;

	
	public mutantSearchFunctions search;

	
	public mutantWorldSearchFunctions worldSearch;

	
	public targetStats mutantStats;

	
	public EnemyHealth health;

	
	public mutantDayCycle dayCycle;

	
	public enemyAnimEvents enemyEvents;

	
	public CharacterController controller;

	
	public mutantMaleHashId hashs;

	
	public mutantPropManager propManager;

	
	public setupBodyVariation bodyVariation;

	
	public mutantCollisionDetect collisionDetect;

	
	public mutantHitReceiver hitReceiver;

	
	public mutantAnimatorControl animControl;

	
	public mutantHitReactions hitReactions;

	
	public mutantTargetFunctions targetFunctions;

	
	public mutantFollowerFunctions followerFunctions;

	
	public arrowStickToTarget arrowSticker;

	
	public mutantVis vis;

	
	public pmCombatReplace pmCombatScript;

	
	public pmSearchReplace pmSearchScript;

	
	private PlayMakerFSM[] allFSM;

	
	public PlayMakerFSM pmCombat;

	
	public PlayMakerFSM pmMotor;

	
	public PlayMakerFSM pmSleep;

	
	public PlayMakerFSM pmBrain;

	
	public PlayMakerFSM pmEncounter;

	
	public List<PlayMakerFSM> actionFSM = new List<PlayMakerFSM>();

	
	public GameObject fireBombGo;

	
	public GameObject thrownFireBombGo;

	
	public GameObject dynamiteBeltGo;

	
	public GameObject soundGo;

	
	public GameObject thisGo;

	
	public Transform rootTr;

	
	public Transform rotateTr;

	
	public Transform hipsJoint;

	
	public GameObject headJoint;

	
	public Transform lookatTr;

	
	public GameObject currentWaypoint;

	
	public GameObject lastSighting;

	
	public GameObject weapon;

	
	public GameObject fireWeapon;

	
	public GameObject clawsWeapon;

	
	public GameObject leftWeapon;

	
	public GameObject leftWeapon1;

	
	public GameObject rightWeapon;

	
	public GameObject mainWeapon;

	
	public GameObject charLeftWeaponGo;

	
	public GameObject clubPickup;

	
	public GameObject stickPickup;

	
	public GameObject stickPickupMP;

	
	public GameObject spawnGo;

	
	public GameObject homeGo;

	
	public GameObject feederGo;

	
	public GameObject bodyCollider;

	
	public GameObject planeCrashGo;

	
	public CapsuleCollider bodyCollisionCollider;

	
	public GameObject headColliderGo;

	
	private HashSet<Type> allowedTypes;

	
	public Transform leftFoot;

	
	public Transform rightFoot;

	
	public Transform rightHand;

	
	public GameObject[] feedingProps;

	
	public GameObject heldMeat;

	
	public GameObject spawnedMeat;

	
	public int hashName;

	
	public FsmGameObject fsmTarget;

	
	public bool disableAiForDebug;
}
