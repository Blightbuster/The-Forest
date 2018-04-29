using System;
using Bolt;
using HutongGames.PlayMaker;
using Pathfinding;
using TheForest.Utils;
using UnityEngine;


public class creepyAnimatorControl : MonoBehaviour
{
	
	private void OnDeserialized()
	{
		this.doStart();
	}

	
	private void Awake()
	{
		this.setup = base.GetComponent<mutantScriptSetup>();
		this.events = base.GetComponent<creepyAnimEvents>();
		this.animator = base.GetComponent<Animator>();
		this.controller = base.transform.root.GetComponent<CharacterController>();
		this.thisTr = base.transform;
		this.rootTr = base.transform.root;
		this.ai = base.GetComponent<mutantAI>();
	}

	
	private void Start()
	{
		this.doStart();
	}

	
	private void OnEnable()
	{
		base.Invoke("initAnimator", 0.5f);
	}

	
	private void OnDisable()
	{
		base.CancelInvoke("initAnimator");
		this.initBool = false;
		this.worldUpdateCheck = false;
	}

	
	private void initAnimator()
	{
		this.initBool = true;
	}

	
	public void activateGirlMutant()
	{
		Scene.SceneTracker.visibleEnemies.Add(this.rootTr.gameObject);
		girlMutantAiManager component = base.transform.GetComponent<girlMutantAiManager>();
		if (component)
		{
			component.setInitialWeightParams();
			component.girlStartPos = base.transform.position;
			component.setupHealthParams();
		}
		GameObject gameObject = new GameObject("girlSpawnGo");
		gameObject.transform.position = base.transform.position;
		this.setup.spawnGo = gameObject;
		this.setup.homeGo = gameObject;
		this.setup.typeSetup.inCave = true;
		this.setup.pmCombat.SendEvent("begin");
		this.setup.ai.enabled = true;
		this.ai.girlFullyTransformed = true;
		this.ai.animSpeed = 1.2f;
		this.animator.speed = 1.2f;
		if (LocalPlayer.Stats)
		{
			LocalPlayer.Stats.IsFightingBoss = true;
		}
		if (BoltNetwork.isServer)
		{
			setupEndBoss setupEndBoss = setupEndBoss.Create(GlobalTargets.Everyone);
			setupEndBoss.bossActive = 1;
			setupEndBoss.Send();
		}
	}

	
	private void swapController()
	{
		if (this.runTimeController)
		{
			this.animator.runtimeAnimatorController = this.runTimeController;
		}
	}

	
	private void doStart()
	{
		this.rg = AstarPath.active.astarData.recastGraph;
		this.nvg = AstarPath.active.astarData.navmesh;
		this.setup = base.GetComponent<mutantScriptSetup>();
		this.animator = base.GetComponent<Animator>();
		this.controller = base.transform.root.GetComponent<CharacterController>();
		this.thisTr = base.transform;
		this.rootTr = base.transform.root;
		this.ai = base.GetComponent<mutantAI>();
		this.walkHash = Animator.StringToHash("walk");
		this.idleHash = Animator.StringToHash("idle");
		this.attackHash = Animator.StringToHash("attack");
		this.chargeHash = Animator.StringToHash("charge");
		if (!this.setup.ai.creepy_fat)
		{
			this.fsmMoving = this.setup.pmMotor.FsmVariables.GetFsmBool("movingBool");
		}
		this.fsmInCaveBool = this.setup.pmCombat.FsmVariables.GetFsmBool("inCaveBool");
		if (this.setup.pmCombat)
		{
			this.fsmAttackingStructure = this.setup.pmCombat.FsmVariables.GetFsmBool("attackStructure");
		}
		this.checkWallDelay = Time.time + 5f;
		this.layer = 26;
		this.layerMask = 1 << this.layer;
		if (this.setup.search.worldPositionTr)
		{
			this.setup.search.worldPositionTr.parent = null;
		}
		this.setup.pmCombat.FsmVariables.GetFsmInt("HashIdle").Value = Animator.StringToHash("idle");
		this.setup.pmCombat.FsmVariables.GetFsmInt("HashWalk").Value = Animator.StringToHash("walk");
		this.setup.pmCombat.FsmVariables.GetFsmInt("HashAttack").Value = Animator.StringToHash("attack");
		this.enableAnim = true;
	}

	
	private void Update()
	{
		if ((this.setup.vis && this.setup.vis.animEnabled) || BoltNetwork.isServer)
		{
			this.setup.search.worldPositionTr.position = this.rootTr.position;
		}
		if (this.ai.mainPlayerDist < 40f && !this.ai.creepy_baby && !this.fsmInCaveBool.Value && Time.time > this.checkWallDelay && !this.fsmAttackingStructure.Value)
		{
			this.attackStructureBetweenPlayer();
			this.checkWallDelay = Time.time + 10f;
		}
		if (!this.animator.enabled && this.ai.doMove)
		{
			this.controllerOff();
		}
	}

	
	private void OnAnimatorMove()
	{
		if (!this.animator.enabled)
		{
			return;
		}
		this.currLayerState0 = this.animator.GetCurrentAnimatorStateInfo(0);
		if (!this.ai.creepy_fat && !this.ai.creepy_boss)
		{
			this.currLayerState1 = this.animator.GetCurrentAnimatorStateInfo(1);
			this.nextLayerState1 = this.animator.GetNextAnimatorStateInfo(1);
		}
		this.nextLayerState0 = this.animator.GetNextAnimatorStateInfo(0);
		if (this.ai.creepy && !this.ai.creepy_boss)
		{
			if (this.currLayerState1.tagHash == this.idleHash && !this.animator.IsInTransition(1))
			{
				this.animator.SetLayerWeightReflected(1, 0f);
			}
			else if (this.nextLayerState1.tagHash == this.walkHash && this.currLayerState1.tagHash != this.walkHash)
			{
				float normalizedTime = this.animator.GetAnimatorTransitionInfo(1).normalizedTime;
				this.animator.SetLayerWeightReflected(1, normalizedTime);
			}
			else if (this.nextLayerState1.tagHash == this.idleHash)
			{
				float normalizedTime2 = this.animator.GetAnimatorTransitionInfo(1).normalizedTime;
				this.animator.SetLayerWeightReflected(1, 1f - normalizedTime2);
			}
			else if (this.currLayerState1.tagHash == this.walkHash && !this.animator.IsInTransition(1))
			{
				this.animator.SetLayerWeightReflected(1, 1f);
			}
		}
		else if (this.ai.creepy_male)
		{
			if ((this.currLayerState0.tagHash == this.idleHash || this.currLayerState0.tagHash == this.attackHash) && !this.animator.IsInTransition(0))
			{
				this.animator.SetLayerWeightReflected(1, 0f);
			}
			else if (this.nextLayerState0.tagHash == this.walkHash && this.animator.IsInTransition(0))
			{
				float normalizedTime3 = this.animator.GetAnimatorTransitionInfo(0).normalizedTime;
				this.animator.SetLayerWeightReflected(1, normalizedTime3);
			}
			else if ((this.nextLayerState0.tagHash == this.idleHash || this.nextLayerState0.tagHash == this.attackHash) && this.animator.IsInTransition(0))
			{
				float normalizedTime4 = this.animator.GetAnimatorTransitionInfo(0).normalizedTime;
				this.animator.SetLayerWeightReflected(1, 1f - normalizedTime4);
			}
			else if (this.currLayerState0.tagHash == this.walkHash && !this.animator.IsInTransition(0))
			{
				this.animator.SetLayerWeightReflected(1, 1f);
			}
			if (this.animator.GetBool("onFireBool"))
			{
				this.animator.SetFloatReflected("anger", 1f);
			}
			else
			{
				this.creepyAnger = 30f / this.setup.ai.playerDist - 1f;
				this.animator.SetFloatReflected("anger", this.creepyAnger);
			}
		}
		else if (this.ai.creepy_fat)
		{
			if (this.currLayerState0.tagHash == this.chargeHash && !this.setup.search.followingLeader)
			{
				this.events.weaponMainCollider.enabled = true;
			}
			else if (this.currLayerState0.tagHash != this.attackHash)
			{
				this.events.weaponMainCollider.enabled = false;
			}
		}
		if (this.enableAnim)
		{
			float @float = this.animator.GetFloat("Gravity");
			this.moveDir = this.animator.deltaPosition;
			this.moveDir.y = this.moveDir.y - this.gravity * Time.deltaTime * @float;
			if (this.controller.enabled)
			{
				this.controller.Move(this.moveDir);
			}
			if (this.setup.ai.creepy_fat && this.controller.enabled)
			{
				this.rootTr.rotation = this.animator.rootRotation;
			}
			else if (!this.ai.doMove && this.controller.enabled)
			{
				this.rootTr.rotation = this.animator.rootRotation;
			}
		}
		if (this.setup.waterDetect.underWater)
		{
			this.animator.speed = 0.6f;
		}
		else if (this.setup.health.poisoned)
		{
			if (this.ai.creepy)
			{
				this.animator.speed = this.ai.animSpeed / 1.25f;
			}
			else if (this.ai.creepy_male)
			{
				this.animator.speed = this.ai.animSpeed / 1.25f;
			}
			else
			{
				this.animator.speed = this.ai.animSpeed / 1.25f;
			}
		}
		else
		{
			this.animator.speed = this.ai.animSpeed;
		}
		if (this.initBool && !this.ai.creepy_boss)
		{
			if (this.fsmInCaveBool.Value)
			{
				this.controllerOn();
			}
			else if (this.animator && this.ai.mainPlayerDist > 180f && !this.fsmInCaveBool.Value)
			{
				this.controllerOff();
			}
			else if (this.animator && this.ai.mainPlayerDist < 180f)
			{
				this.controllerOn();
			}
			else
			{
				this.controllerOff();
			}
		}
	}

	
	private void controllerOn()
	{
		if (!this.controller.enabled)
		{
			this.controller.enabled = true;
		}
	}

	
	private void controllerOff()
	{
		if (this.controller.enabled)
		{
			this.controller.enabled = false;
		}
		Quaternion rotation = Quaternion.identity;
		Vector3 wantedDir = this.ai.wantedDir;
		if (wantedDir != Vector3.zero && wantedDir.sqrMagnitude > 0f)
		{
			rotation = Quaternion.LookRotation(wantedDir, Vector3.up);
		}
		this.setup.search.worldPositionTr.rotation = rotation;
		if (this.initBool && !this.fsmInCaveBool.Value)
		{
			if (Terrain.activeTerrain)
			{
				this.terrainPosY = Terrain.activeTerrain.SampleHeight(this.setup.search.worldPositionTr.position) + Terrain.activeTerrain.transform.position.y;
			}
			else
			{
				this.terrainPosY = this.setup.search.worldPositionTr.position.y;
			}
			Vector3 position = this.setup.search.worldPositionTr.position;
			position.y = this.terrainPosY;
			this.setup.search.worldPositionTr.position = position + wantedDir * this.offScreenSpeed * Time.deltaTime;
			if (!BoltNetwork.isRunning)
			{
				if (Time.time > this.offScreenUpdateTimer)
				{
					this.rootTr.position = this.setup.search.worldPositionTr.position;
					this.thisTr.rotation = this.setup.search.worldPositionTr.rotation;
					this.offScreenUpdateTimer = Time.time + 1f;
				}
			}
			else
			{
				this.rootTr.position = this.setup.search.worldPositionTr.position;
				this.thisTr.rotation = this.setup.search.worldPositionTr.rotation;
			}
		}
	}

	
	private void LateUpdate()
	{
		this.pos = this.rootTr.position;
		this.pos.y = this.pos.y + 5f;
		if (this.ai.creepy && this.animator.enabled && !this.ai.creepy_boss && Physics.Raycast(this.pos, Vector3.down, out this.hit, 20f, this.layerMask))
		{
			this.rootTr.rotation = Quaternion.Lerp(this.animator.rootRotation, Quaternion.LookRotation(Vector3.Cross(this.rootTr.right, this.hit.normal), this.hit.normal), Time.deltaTime * 10f);
		}
		if (this.ai.creepy_fat || this.ai.creepy_male || this.ai.creepy)
		{
			this.thisTr.localRotation = Quaternion.identity;
		}
	}

	
	private void resetCreepyMaleAnimator()
	{
		this.animator.SetBoolReflected("attackBool", false);
		this.animator.SetBoolReflected("attackCharge", false);
		this.animator.SetBoolReflected("treeAttack", false);
		this.animator.SetBoolReflected("turnBool", false);
		this.animator.SetBoolReflected("attackLeftBool", false);
		this.animator.SetBoolReflected("attackRightBool", false);
		this.animator.SetBoolReflected("hitBool", false);
	}

	
	private void resetCreepyFemaleAnimator()
	{
		this.animator.SetBoolReflected("attackBool", false);
		this.animator.SetBoolReflected("rearUpBool", false);
		this.animator.SetBoolReflected("turnBool", false);
		this.animator.SetBoolReflected("attackCombo", false);
	}

	
	private void resetCreepyFatAnimator()
	{
		this.animator.SetBoolReflected("attackBool", false);
		this.animator.SetBoolReflected("turnBool", false);
		this.animator.SetBoolReflected("charge", false);
		this.animator.SetBoolReflected("knockBackBool", false);
		this.animator.SetBoolReflected("dodgeBool", false);
	}

	
	private void attackStructureBetweenPlayer()
	{
		Vector3 direction = Vector3.zero;
		if (this.ai.allPlayers.Count > 0)
		{
			if (this.ai.allPlayers[0] != null)
			{
				direction = (this.ai.allPlayers[0].transform.position - this.controller.bounds.center).normalized;
			}
			RaycastHit raycastHit;
			if (Physics.Raycast(this.controller.bounds.center, direction, out raycastHit, 40f, this.checkWallMask))
			{
				GameObject gameObject = raycastHit.collider.gameObject;
				if (gameObject.CompareTag("structure") || gameObject.CompareTag("SLTier1") || gameObject.CompareTag("SLTier2") || gameObject.CompareTag("SLTier3"))
				{
					getStructureStrength component = gameObject.GetComponent<getStructureStrength>();
					if (component == null)
					{
						return;
					}
					Vector3 normalized = (this.thisTr.position - raycastHit.collider.bounds.center).normalized;
					Vector3 vector = raycastHit.collider.bounds.center + normalized * 7.5f;
					vector.y = Terrain.activeTerrain.SampleHeight(vector) + Terrain.activeTerrain.transform.position.y;
					GraphNode node = this.rg.GetNearest(vector).node;
					if (node.Walkable)
					{
						vector = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
						this.setup.search.updateCurrentWaypoint(vector);
						this.setup.pmCombat.FsmVariables.GetFsmGameObject("structureGo").Value = gameObject;
						this.setup.pmCombat.SendEvent("goToAttackStructure");
						return;
					}
				}
			}
			return;
		}
	}

	
	public Animator animator;

	
	private CharacterController controller;

	
	private Transform thisTr;

	
	private Transform rootTr;

	
	private mutantAI ai;

	
	private mutantScriptSetup setup;

	
	private creepyAnimEvents events;

	
	public RuntimeAnimatorController runTimeController;

	
	private PlayMakerFSM pmSearch;

	
	private AnimatorStateInfo currLayerState0;

	
	private AnimatorStateInfo currLayerState1;

	
	private AnimatorStateInfo nextLayerState0;

	
	private AnimatorStateInfo nextLayerState1;

	
	public float gravity;

	
	private Vector3 moveDir = Vector3.zero;

	
	private float currYPos;

	
	private float velY;

	
	private float accelY;

	
	private float creepyAnger;

	
	public float offScreenSpeed;

	
	private Vector3 pos;

	
	private RaycastHit hit;

	
	private int layer;

	
	private int layerMask;

	
	private bool enableAnim;

	
	private bool initBool;

	
	private float terrainPosY;

	
	private float checkWallDelay = 2f;

	
	private FsmBool fsmMoving;

	
	private FsmBool fsmInCaveBool;

	
	private FsmBool fsmAttackingStructure;

	
	public bool worldUpdateCheck;

	
	private Vector3 girlStartPos;

	
	private int walkHash;

	
	private int idleHash;

	
	private int attackHash;

	
	private int chargeHash;

	
	public LayerMask checkWallMask;

	
	private RecastGraph rg;

	
	private NavMeshGraph nvg;

	
	private float offScreenUpdateTimer;
}
