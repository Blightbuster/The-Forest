using System;
using System.Collections;
using HutongGames.PlayMaker;
using Pathfinding;
using UnityEngine;


public class mutantCollisionDetect : MonoBehaviour
{
	
	private void Awake()
	{
		this.setup = base.transform.root.GetComponentInChildren<mutantScriptSetup>();
		this.animator = base.transform.root.GetComponentInChildren<Animator>();
		this.rootTr = this.animator.transform;
		this.hitCollider = base.GetComponent<Collider>();
	}

	
	private void Start()
	{
		this.fsmDeathBool = this.setup.pmCombat.FsmVariables.GetFsmBool("deathBool");
		this.fsmEnemyCloseBool = this.setup.pmCombat.FsmVariables.GetFsmBool("enemyCloseBOOL");
		this.fsmFearOverrideBool = this.setup.pmBrain.FsmVariables.GetFsmBool("fearOverrideBool");
		this.fsmRunToAttackBool = this.setup.pmCombat.FsmVariables.GetFsmBool("runToAttack");
		this.fsmOnWallBool = this.setup.pmCombat.FsmVariables.GetFsmBool("toClimbWall");
		this.layer = 26;
		this.layerMask = 1 << this.layer;
		this.origColliderPos = base.transform.localPosition;
		base.Invoke("enableCollisions", 2f);
		this.hitCollider.enabled = true;
	}

	
	private void enableCollisions()
	{
		this.doCollision = true;
	}

	
	private void OnEnable()
	{
		if (this.animator && this.animator.enabled)
		{
			this.animator.SetBoolReflected("goLeftBOOL", false);
			this.animator.SetBoolReflected("goRightBOOL", false);
		}
		base.StartCoroutine("pollCollider");
		base.StartCoroutine("terrainHeightCheck");
		base.Invoke("enableCollisions", 2f);
	}

	
	private void OnDisable()
	{
		if (this.animator && this.animator.enabled)
		{
			this.animator.SetBoolReflected("goLeftBOOL", false);
			this.animator.SetBoolReflected("goRightBOOL", false);
		}
		base.StopCoroutine("resetJumpCollision");
		base.StopCoroutine("pollCollider");
		base.StopCoroutine("terrainHeightCheck");
		this.doCollision = false;
	}

	
	private void Update()
	{
		if ((this.inEffigy || this.inPlayerFire) && (!this.currentTrigger || !this.currentTrigger.activeSelf))
		{
			this.forceExit();
		}
	}

	
	private IEnumerator terrainHeightCheck()
	{
		for (;;)
		{
			if (this.animator.enabled && this.setup.ai.mainPlayerDist < 100f && !this.setup.hitReactions.onStructure)
			{
				this.currentPos = this.rootTr.position;
				this.nextPos = this.rayCastPos.position;
				this.pos = this.rayCastPos.position;
				if (Physics.Raycast(this.pos, Vector3.down, out this.hit, 100f, this.layerMask))
				{
					if (this.hit.collider.CompareTag("TerrainMain"))
					{
						this.node = this.setup.ai.rg.GetNearest(this.hit.point).node;
						if (this.node.Walkable)
						{
							this.nextPos = this.hit.point;
							this.terrainDist = this.currentPos.y - this.nextPos.y;
							if (this.terrainDist > 7.5f)
							{
								this.animator.SetBoolReflected("jumpGapBool", true);
								yield return YieldPresets.WaitOneSecond;
								this.animator.SetBoolReflected("jumpGapBool", false);
							}
						}
					}
				}
				else
				{
					this.terrainDist = 0f;
					this.animator.SetBoolReflected("jumpGapBool", false);
				}
			}
			yield return YieldPresets.WaitPointThreeSeconds;
		}
		yield break;
	}

	
	private IEnumerator pollCollider()
	{
		for (;;)
		{
			if (this.animator.enabled && this.setup.ai.mainPlayerDist < 120f)
			{
				this.hitCollider.enabled = true;
				yield return YieldPresets.WaitPointTwoSeconds;
				this.hitCollider.enabled = false;
				yield return YieldPresets.WaitForFixedUpdate;
				yield return YieldPresets.WaitForFixedUpdate;
			}
			else
			{
				this.hitCollider.enabled = false;
				yield return YieldPresets.WaitForFixedUpdate;
				yield return YieldPresets.WaitForFixedUpdate;
			}
		}
		yield break;
	}

	
	private void forceExit()
	{
		this.setup.pmCombat.FsmVariables.GetFsmBool("fearBOOL").Value = false;
		this.setup.pmBrain.SendEvent("toActivateFSM");
		base.Invoke("resetFlee", 15f);
		this.inEffigy = false;
		this.inPlayerFire = false;
	}

	
	private void doCoolDown()
	{
		if (!this.lighterCooldown)
		{
			this.lighterCooldown = true;
			base.Invoke("resetCoolDown", 20f);
		}
	}

	
	private void resetCoolDown()
	{
		this.lighterCooldown = false;
		Debug.Log("COOLDOWN OFF.");
	}

	
	private void sidestepReset()
	{
		this.animator.SetBoolReflected("goLeftBOOL", false);
		this.animator.SetBoolReflected("goRightBOOL", false);
	}

	
	private void jumpCoolDown()
	{
		this.jumpBlock = false;
		this.animator.SetBoolReflected("jumpBOOL", false);
		this.animator.SetBoolReflected("attackJumpBOOL", false);
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (!this.doCollision)
		{
			return;
		}
		if (!this.setup)
		{
			return;
		}
		if (other.gameObject.CompareTag("effigy") && !this.setup.ai.fireman && !this.animator.GetBool("trapBool") && !this.animator.GetBool("deathBOOL") && !this.fsmDeathBool.Value && !this.setup.health.onFire)
		{
			if ((this.fsmFearOverrideBool.Value && !this.setup.pmCombatScript.toRepelArtifact) || this.setup.hitReactions.onStructure)
			{
				this.forceExit();
			}
			else
			{
				if (!this.inEffigy)
				{
					this.currentTrigger = other.gameObject;
				}
				this.setup.pmBrain.SendEvent("toSetFearful");
				this.setup.aiManager.flee = true;
				this.setup.pmBrain.FsmVariables.GetFsmGameObject("fearTargetGo").Value = other.gameObject;
				this.inEffigy = true;
			}
		}
		if (other.gameObject.CompareTag("explodeReact") && this.animator.GetInteger("hurtLevelInt") < 3 && !this.animator.GetBool("trapBool") && !this.animator.GetBool("deathBOOL") && !this.fsmDeathBool.Value && !this.setup.health.onFire && Vector3.Distance(other.transform.position, base.transform.position) > 15f)
		{
			this.currentTrigger = other.gameObject;
			this.setup.pmBrain.SendEvent("toSetRunAway");
			this.setup.pmBrain.FsmVariables.GetFsmGameObject("fearTargetGo").Value = other.gameObject;
		}
		bool flag = false;
		if (other.gameObject.CompareTag("UnderfootWood") && other.gameObject.GetComponent<jumpable>())
		{
			flag = true;
		}
		if (other.gameObject.CompareTag("DeadTree") || flag || other.gameObject.CompareTag("jumpObject") || other.gameObject.CompareTag("Target") || other.gameObject.CompareTag("Multisled") || other.gameObject.CompareTag("SLTier1") || other.gameObject.CompareTag("SLTier2") || (other.gameObject.CompareTag("SLTier3") && !this.fsmOnWallBool.Value && !this.setup.hitReactions.onStructure))
		{
			if (other.gameObject.CompareTag("SLTier1") || other.gameObject.CompareTag("SLTier2") || (other.gameObject.CompareTag("SLTier3") && this.setup.pmCombatScript.runToAttack))
			{
				Bounds bounds = other.bounds;
				float num = bounds.center.y + bounds.extents.y;
				if (num < this.rootTr.position.y + 4f && num > this.rootTr.position.y)
				{
					Vector3 position = other.bounds.center;
					if (this.setup.ai.allPlayers.Count > 0 && this.setup.ai.allPlayers[0])
					{
						position = this.setup.ai.allPlayers[0].transform.position;
					}
					this.localTarget = this.rootTr.InverseTransformPoint(position);
					this.targetAngle = Mathf.Atan2(this.localTarget.x, this.localTarget.z) * 57.29578f;
					if (this.targetAngle > -45f && this.targetAngle < 45f && !this.jumpBlock && this.animator.GetFloat("Speed") > 0.2f)
					{
						this.animator.SetBoolReflected("attackJumpBOOL", true);
						this.animator.SetBoolReflected("jumpBOOL", true);
						this.jumpBlock = true;
						this.currentJumpCollider = other;
						base.Invoke("jumpCoolDown", 0.37f);
					}
				}
			}
			else if (other.GetType() == typeof(MeshCollider))
			{
				this.animator.SetIntegerReflected("randInt2", UnityEngine.Random.Range(0, 2));
				this.animator.SetBoolReflected("jumpBOOL", true);
				this.jumpBlock = true;
				this.currentJumpCollider = other;
				base.Invoke("jumpCoolDown", 0.37f);
			}
			else
			{
				this.localTarget = this.rootTr.InverseTransformPoint(other.transform.position);
				this.targetAngle = Mathf.Atan2(this.localTarget.x, this.localTarget.z) * 57.29578f;
				if (this.targetAngle > -45f && this.targetAngle < 45f && !this.jumpBlock)
				{
					this.animator.SetIntegerReflected("randInt2", UnityEngine.Random.Range(0, 2));
					this.animator.SetBoolReflected("jumpBOOL", true);
					this.jumpBlock = true;
					this.currentJumpCollider = other;
					base.Invoke("jumpCoolDown", 0.37f);
				}
			}
		}
		if (other.gameObject.CompareTag("Tree"))
		{
			this.targetDist = Vector3.Distance(this.rootTr.position, other.transform.position);
			if (this.targetDist < 5f)
			{
				this.localTarget = this.rootTr.InverseTransformPoint(other.transform.position);
				this.targetAngle = Mathf.Atan2(this.localTarget.x, this.localTarget.z) * 57.29578f;
				if (this.targetAngle < 0f && this.targetAngle > -70f)
				{
					this.animator.SetBoolReflected("goRightBOOL", true);
					base.Invoke("sidestepReset", 0.6f);
				}
				if (this.targetAngle > 0f && this.targetAngle < 70f)
				{
					this.animator.SetBoolReflected("goLeftBOOL", true);
					base.Invoke("sidestepReset", 0.6f);
				}
			}
		}
		if (other.gameObject.CompareTag("enemy"))
		{
			this.fsmEnemyCloseBool.Value = true;
		}
	}

	
	private IEnumerator resetJumpCollision(Collider other)
	{
		yield return YieldPresets.WaitPointOneSeconds;
		if (other)
		{
			Physics.IgnoreCollision(this.setup.rootTr.GetComponent<Collider>(), other, false);
		}
		yield break;
	}

	
	private void resetTriggerCoolDown()
	{
		this.triggerCoolDown = false;
	}

	
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("effigy") || other.gameObject.CompareTag("playerFire"))
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("fearBOOL").Value = false;
			if (!this.animator.GetBool("trapBool") && !this.animator.GetBool("deathBOOL") && !this.fsmDeathBool.Value && !this.setup.health.onFire)
			{
				this.setup.pmBrain.SendEvent("toActivateFSM");
			}
			this.inEffigy = false;
			this.inPlayerFire = false;
			base.Invoke("resetFlee", 15f);
		}
		if (other.gameObject.CompareTag("Obstruction") || other.gameObject.CompareTag("DeadTree") || other.gameObject.CompareTag("Tree") || other.gameObject.CompareTag("jumpObject") || other.gameObject.CompareTag("Multisled") || other.gameObject.CompareTag("Target"))
		{
			this.animator.SetBoolReflected("jumpBOOL", false);
			this.animator.SetBoolReflected("goLeftBOOL", false);
			this.animator.SetBoolReflected("goRightBOOL", false);
		}
		if (other.gameObject.CompareTag("Obstruction"))
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("closeRockLeftBOOL").Value = false;
		}
		if (other.gameObject.CompareTag("Obstruction"))
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("closeRockRightBOOL").Value = false;
		}
		if (other.gameObject.CompareTag("enemy"))
		{
			this.fsmEnemyCloseBool.Value = false;
		}
	}

	
	private void resetFlee()
	{
		this.setup.aiManager.flee = false;
	}

	
	private IEnumerator returnObjectAngle(GameObject go)
	{
		Vector3 localTarget = this.rootTr.InverseTransformPoint(go.transform.position);
		float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * 57.29578f;
		this.setup.pmBrain.FsmVariables.GetFsmFloat("objectAngle").Value = targetAngle;
		yield return null;
		yield break;
	}

	
	private Transform rootTr;

	
	public Transform rayCastPos;

	
	private mutantScriptSetup setup;

	
	private Animator animator;

	
	private Collider hitCollider;

	
	public float terrainDist;

	
	private FsmBool fsmEnemyCloseBool;

	
	public FsmBool fsmFearOverrideBool;

	
	private FsmBool fsmDeathBool;

	
	private FsmBool fsmRunToAttackBool;

	
	private FsmBool fsmOnWallBool;

	
	public GameObject currentTrigger;

	
	public Collider currentJumpCollider;

	
	private Vector3 localTarget;

	
	public float targetAngle;

	
	private float targetDist;

	
	private bool lighterCooldown;

	
	private bool detected;

	
	public bool inEffigy;

	
	private bool inPlayerFire;

	
	public bool jumpBlock;

	
	private bool triggerCoolDown;

	
	private Vector3 currentPos;

	
	private Vector3 nextPos;

	
	private Vector3 pos;

	
	private Vector3 origColliderPos;

	
	private RaycastHit hit;

	
	private int layer;

	
	private int layerMask;

	
	public bool doCollision;

	
	private GraphNode node;
}
