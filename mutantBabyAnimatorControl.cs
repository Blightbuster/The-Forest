using System;
using System.Collections;
using HutongGames.PlayMaker;
using UnityEngine;


public class mutantBabyAnimatorControl : MonoBehaviour
{
	
	private void OnDeserialized()
	{
		this.doStart();
	}

	
	private void Start()
	{
		this.doStart();
	}

	
	private void doStart()
	{
		this.rb = base.transform.parent.GetComponent<Rigidbody>();
		this.setup = base.GetComponent<mutantScriptSetup>();
		this.waterDetect = base.transform.GetComponentInChildren<mutantWaterDetect>();
		this.animator = base.GetComponent<Animator>();
		this.controller = base.transform.parent.GetComponent<SphereCollider>();
		this.thisTr = base.transform;
		this.rootTr = base.transform.parent;
		this.ai = base.GetComponent<mutantAI>();
		this.fsmInCaveBool = this.setup.pmCombat.FsmVariables.GetFsmBool("inCaveBool");
		this.layer = 26;
		this.layerMask = 1 << this.layer;
		this.setup.pmCombat.FsmVariables.GetFsmInt("HashIdle").Value = Animator.StringToHash("idle");
		this.setup.pmCombat.FsmVariables.GetFsmInt("HashWalk").Value = Animator.StringToHash("walk");
		this.setup.pmCombat.FsmVariables.GetFsmInt("HashAttack").Value = Animator.StringToHash("attack");
		base.Invoke("callChangeIdle", (float)UnityEngine.Random.Range(0, 2));
	}

	
	private void OnSpawned()
	{
		base.transform.localEulerAngles = Vector3.zero;
	}

	
	private void OnEnable()
	{
		base.Invoke("initAnimator", 0.5f);
	}

	
	private void OnDisable()
	{
		base.StopAllCoroutines();
		base.CancelInvoke("callChangeIdle");
		base.CancelInvoke("initAnimator");
		this.initBool = false;
	}

	
	private void initAnimator()
	{
		this.initBool = true;
	}

	
	private void enableBabyBurning()
	{
	}

	
	private void disableBabyBurning()
	{
	}

	
	private void Update()
	{
		if (this.waterDetect.inWater && Time.time < this.inWaterTimer)
		{
			if (this.waterDetect.currentWaterCollider)
			{
				float num = this.waterDetect.currentWaterCollider.bounds.center.y + this.waterDetect.currentWaterCollider.bounds.extents.y;
				if (this.thisTr.position.y - num < -2f && Time.time > this.inWaterTimer - 1f)
				{
					this.setup.health.Hit(100);
				}
			}
		}
		else
		{
			this.inWaterTimer = Time.time + 5f;
		}
		if (!this.animator.enabled && this.ai.doMove)
		{
			this.controller.enabled = false;
			Quaternion rotation = Quaternion.identity;
			this.rb.isKinematic = true;
			Vector3 wantedDir = this.ai.wantedDir;
			if (wantedDir != Vector3.zero && wantedDir.sqrMagnitude > 0f)
			{
				rotation = Quaternion.LookRotation(wantedDir, Vector3.up);
			}
			this.thisTr.rotation = rotation;
			if (this.initBool)
			{
				if (!this.fsmInCaveBool.Value)
				{
					this.terrainPosY = Terrain.activeTerrain.SampleHeight(this.rootTr.position) + Terrain.activeTerrain.transform.position.y;
				}
				else
				{
					this.terrainPosY = this.rootTr.position.y;
				}
				Vector3 position = this.rootTr.position;
				position.y = this.terrainPosY;
				this.rb.isKinematic = true;
				this.rootTr.position = position + wantedDir * this.offScreenSpeed * Time.deltaTime;
			}
		}
	}

	
	private void callChangeIdle()
	{
		base.StartCoroutine("changeIdleValue");
	}

	
	private IEnumerator changeIdleValue()
	{
		if (!this.animator.enabled)
		{
			yield break;
		}
		float val = 0f;
		float initVal = this.animator.GetFloat("idleFloat1");
		float side = initVal;
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime;
			if (side > 0.5f)
			{
				val = Mathf.SmoothStep(initVal, 0f, t);
			}
			else
			{
				val = Mathf.SmoothStep(initVal, 1f, t);
			}
			if (this.animator.enabled)
			{
				this.animator.SetFloatReflected("idleFloat1", val);
			}
			yield return null;
		}
		base.Invoke("callChangeIdle", UnityEngine.Random.Range(7f, 15f));
		yield break;
	}

	
	private void OnAnimatorMove()
	{
		if (this.animator && this.animator.enabled && !this.worldUpdateCheck)
		{
			AnimatorStateInfo currentAnimatorStateInfo = this.animator.GetCurrentAnimatorStateInfo(0);
			if (currentAnimatorStateInfo.shortNameHash == this.idleToBurnHash || currentAnimatorStateInfo.shortNameHash == this.burnCycleHash || currentAnimatorStateInfo.shortNameHash == this.burnToDeathHash)
			{
				this.enableBabyBurning();
			}
			else
			{
				this.disableBabyBurning();
			}
			this.controller.enabled = true;
			float @float = this.animator.GetFloat("Gravity");
			this.moveDir = this.animator.deltaPosition;
			this.gravForce = new Vector3(0f, -@float * 15f * this.rb.mass, 0f);
			this.rb.isKinematic = false;
			this.rb.MovePosition(this.rootTr.position + this.moveDir);
			if (this.gravForce.y < 0f)
			{
				this.rb.AddForce(this.gravForce, ForceMode.Force);
			}
			Vector3 velocity = this.rb.velocity;
			if (velocity.y > 20f)
			{
				velocity.y = 20f;
			}
			velocity.x = 0f;
			velocity.z = 0f;
			this.rb.velocity = velocity;
		}
	}

	
	private void LateUpdate()
	{
		this.pos = this.rootTr.position;
		this.pos.y = this.pos.y + 3f;
		Vector3 velocity = this.rb.velocity;
		if (velocity.y > 15f)
		{
			velocity.y = 15f;
		}
		velocity.x = 0f;
		velocity.z = 0f;
		this.rb.velocity = velocity;
		if (this.ai.creepy)
		{
			if ((this.thisTr.localEulerAngles.y < 350f && this.thisTr.localEulerAngles.y > 180f) || this.thisTr.localEulerAngles.y < -10f)
			{
				this.thisTr.localEulerAngles = new Vector3(this.thisTr.localEulerAngles.x, 0f, this.thisTr.localEulerAngles.z);
			}
			if (Physics.Raycast(this.pos, Vector3.down, out this.hit, 20f, this.layerMask))
			{
				this.thisTr.rotation = Quaternion.Lerp(this.animator.rootRotation, Quaternion.LookRotation(Vector3.Cross(this.thisTr.right, this.hit.normal), this.hit.normal), Time.deltaTime * 10f);
			}
		}
	}

	
	public bool worldUpdateCheck;

	
	private mutantWaterDetect waterDetect;

	
	private Rigidbody rb;

	
	private Animator animator;

	
	private SphereCollider controller;

	
	private Transform thisTr;

	
	private Transform rootTr;

	
	private mutantAI ai;

	
	private mutantScriptSetup setup;

	
	private PlayMakerFSM pmSearch;

	
	public float gravity;

	
	private Vector3 moveDir = Vector3.zero;

	
	private float currYPos;

	
	private float velY;

	
	private float accelY;

	
	private float creepyAnger;

	
	public float offScreenSpeed;

	
	public float moveSpeed;

	
	private Vector3 pos;

	
	private RaycastHit hit;

	
	private int layer;

	
	private int layerMask;

	
	private bool initBool;

	
	private float terrainPosY;

	
	private FsmBool fsmInCaveBool;

	
	private int idleToBurnHash = Animator.StringToHash("idleToBurn");

	
	private int burnCycleHash = Animator.StringToHash("burnCycle");

	
	private int burnToDeathHash = Animator.StringToHash("burnToDeath");

	
	private float inWaterTimer;

	
	private Vector3 gravForce;
}
