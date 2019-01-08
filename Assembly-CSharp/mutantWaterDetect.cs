using System;
using Bolt;
using HutongGames.PlayMaker;
using UnityEngine;

public class mutantWaterDetect : MonoBehaviour
{
	private void Start()
	{
		if (!this.netPrefab)
		{
			this.setup = base.transform.root.GetComponentInChildren<mutantScriptSetup>();
			this.animControl = base.transform.root.GetComponentInChildren<creepyAnimatorControl>();
			this.animator = this.setup.animator;
			this.events = this.setup.enemyEvents;
			if (this.setup.pmCombat)
			{
				this.fsmOnStructureBool = this.setup.pmCombat.FsmVariables.GetFsmBool("onStructureBool");
			}
			if (this.setup.pmCombat)
			{
				this.fsmExitWaterBool = this.setup.pmCombat.FsmVariables.GetFsmBool("exitWaterBool");
			}
		}
		this.rootTr = base.transform.root;
		this.thisCollider = base.transform.GetComponent<Collider>();
	}

	private void OnEnable()
	{
		this.valideColliderDelay = Time.time + 2f;
		this.splashCoolDown = false;
		this.drowned = false;
		this.inWater = false;
		this.inWaterTimer = Time.time + 5f;
	}

	private void OnDisable()
	{
		this.drowned = false;
		this.splashCoolDown = false;
	}

	private void Update()
	{
		if (Time.time > this.valideColliderDelay)
		{
			this.ValidateWaterCollider();
			this.valideColliderDelay = Time.time + 2f;
		}
		if (this.inWater)
		{
			if (this.currentWaterCollider)
			{
				float num = this.currentWaterCollider.bounds.center.y + this.currentWaterCollider.bounds.extents.y;
				if (!this.netPrefab && num - this.setup.rootTr.position.y > 0.9f && !this.setup.search.fsmInCave.Value)
				{
					if (!this.fsmExitWaterBool.Value)
					{
						this.setup.pmCombat.SendEvent("goToExitWater");
					}
					this.fsmExitWaterBool.Value = true;
				}
				this.waterHeight = num - this.rootTr.position.y;
				if (this.waterHeight > 4f)
				{
					this.underWater = true;
					if (Time.time > this.inWaterTimer - 1f && !this.drowned)
					{
						if (BoltNetwork.isClient)
						{
							PlayerHitEnemy playerHitEnemy = PlayerHitEnemy.Create(GlobalTargets.OnlyServer);
							playerHitEnemy.Target = base.GetComponent<BoltEntity>();
							playerHitEnemy.Hit = 1000;
							playerHitEnemy.Send();
						}
						else
						{
							this.setup.health.HitReal(1000);
						}
						this.drowned = true;
					}
				}
				else
				{
					this.underWater = false;
					this.inWaterTimer = Time.time + 5f;
				}
			}
			else
			{
				this.underWater = false;
			}
		}
		else
		{
			this.inWaterTimer = Time.time + 5f;
			this.underWater = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Water"))
		{
			if (!this.creepy)
			{
				this.inWater = true;
				this.currentWaterCollider = other;
			}
			if (this.creepy)
			{
				this.currentWaterCollider = other;
				this.inWater = true;
			}
			if (!this.creepy && !this.netPrefab)
			{
				this.waterCheck = true;
				this.currentWaterCollider = other;
				if (this.animator.GetCurrentAnimatorStateInfo(0).tagHash == this.fallHash && !this.splashCoolDown)
				{
					this.events.doWaterSplash(other);
					this.splashCoolDown = true;
					base.Invoke("resetSplashCoolDown", 1f);
				}
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Water") && this.currentWaterCollider == other)
		{
			this.currentWaterCollider = null;
			this.waterCheck = false;
			this.inWater = false;
		}
	}

	private void resetSplashCoolDown()
	{
		this.splashCoolDown = false;
	}

	private void ValidateWaterCollider()
	{
		if (this.currentWaterCollider && !this.currentWaterCollider.bounds.Intersects(this.thisCollider.bounds))
		{
			this.inWater = false;
			this.waterCheck = false;
			this.currentWaterCollider = null;
		}
	}

	public bool creepy;

	private Collider thisCollider;

	private Animator animator;

	private enemyAnimEvents events;

	private mutantScriptSetup setup;

	private creepyAnimatorControl animControl;

	private Transform rootTr;

	private int fallHash = Animator.StringToHash("jumpFall");

	private bool splashCoolDown;

	private bool waterCheck;

	public bool inWater;

	public bool underWater;

	public Collider currentWaterCollider;

	public float inWaterTimer;

	public float waterHeight;

	private float eventInterval;

	private float eventMaxTime;

	private float valideColliderDelay;

	private bool doWaterEvent;

	public bool drowned;

	public bool netPrefab;

	private FsmBool fsmOnStructureBool;

	private FsmBool fsmExitWaterBool;
}
