using System;
using System.Collections;
using Bolt;
using TheForest.Utils;
using UnityEngine;


public class netAnimatorSetup : EntityEventListener<IPlayerState>
{
	
	private void Start()
	{
		this.animator = base.transform.GetComponent<Animator>();
		this.ts = base.transform.parent.GetComponent<targetStats>();
		this.rootCollider = base.transform.parent.GetComponent<Collider>();
		this.aiInfo = base.transform.parent.GetComponent<playerAiInfo>();
		this.rootTr = base.transform.parent;
		if (BoltNetwork.isRunning)
		{
			base.Invoke("fixRootRotation", 1f);
		}
		this.vis = base.transform.parent.GetComponent<netPlayerVis>();
		if (this.heldBow)
		{
			this.bowAnimator = this.heldBow.GetComponent<Animator>();
		}
		if (this.heldSlingShot)
		{
			this.slingAnimator = this.heldSlingShot.GetComponentInChildren<Animator>();
		}
		this.animator.SetBool("net", true);
		this.animator.SetLayerWeight(0, 1f);
		this.animator.SetLayerWeight(1, 0f);
		this.animator.SetLayerWeight(2, 0f);
		this.animator.SetLayerWeight(3, 0f);
		this.animator.SetLayerWeight(4, 1f);
		if (BoltNetwork.isRunning)
		{
			base.StartCoroutine(this.fixNetSpawnPosition());
		}
	}

	
	public override void Attached()
	{
		this.startDisableCollisionTimer = Time.time + 3f;
		this.animator = base.transform.GetComponent<Animator>();
		if (!this.netHider)
		{
			this.netHider = base.transform.GetComponent<netHideDuringPlaneCrash>();
		}
		if (BoltNetwork.isRunning)
		{
			base.Invoke("fixRootRotation", 1f);
		}
		this.tempSphere.SetActive(false);
		Debug.Log("net animator attached!");
		base.StartCoroutine(this.fixNetSpawnPosition());
	}

	
	private void Update()
	{
		if (this.vis.localplayerDist < 45f)
		{
			if (!this.displacementGo.activeSelf)
			{
				this.displacementGo.SetActive(true);
			}
		}
		else if (this.displacementGo.activeSelf)
		{
			this.displacementGo.SetActive(false);
		}
		if (this.animator == null)
		{
			this.animator = base.transform.GetComponent<Animator>();
		}
		this.currState0 = this.animator.GetCurrentAnimatorStateInfo(0);
		this.nextState1 = this.animator.GetNextAnimatorStateInfo(1);
		this.currState1 = this.animator.GetCurrentAnimatorStateInfo(1);
		this.currState2 = this.animator.GetCurrentAnimatorStateInfo(2);
		this.nextState2 = this.animator.GetNextAnimatorStateInfo(2);
		if (this.animator.GetBool("zipLineAttach"))
		{
			if (this.currState0.shortNameHash != this.toZipHash && this.currState0.shortNameHash != this.zipIdleHash && !this.doZipSync)
			{
				this.animator.CrossFade("Base Layer.idleToZip", 0.1f, 0, 0.2f);
				this.doZipSync = true;
			}
		}
		else
		{
			this.doZipSync = false;
		}
		if (this.heldChainSaw.activeSelf && this.currState2.tagHash != this.axeCombo1Hash)
		{
			this.animator.SetBool("spineOverride", true);
		}
		else
		{
			this.animator.SetBool("spineOverride", false);
		}
		if (this.tempCube)
		{
			if (this.animator.GetBool("stickAttack"))
			{
				this.tempCube.SetActive(true);
			}
			else
			{
				this.tempCube.SetActive(false);
			}
		}
		if (this.currState0.tagHash == this.enterClimbHash || this.currState0.tagHash == this.climbIdleHash || this.currState0.tagHash == this.climbingHash)
		{
			this.ts.onRope = true;
		}
		else
		{
			this.ts.onRope = false;
		}
		if (this.currState2.shortNameHash != this.injuredLoopHash)
		{
			this.playerPos = this.rootTr.position;
		}
		else
		{
			this.rootTr.position = this.playerPos;
		}
		if (this.currState2.tagHash == this.getupHash || Time.time < this.startDisableCollisionTimer)
		{
			if (this.rootEnabled)
			{
				this.hitDetectGo.SetActive(false);
				this.rootCollider.enabled = false;
				this.rootEnabled = false;
			}
		}
		else if (!this.rootEnabled)
		{
			this.hitDetectGo.SetActive(true);
			this.rootCollider.enabled = true;
			this.rootEnabled = true;
		}
		if (this.currState2.shortNameHash == this.wakeOnPlaneHash)
		{
			if (!this.doPlaneStartSync)
			{
				this.delayPlaneStartTimer = Time.time + 1.25f;
				this.doPlaneStartSync = true;
			}
			if (Time.time < this.delayPlaneStartTimer)
			{
				this.animator.CrossFade(this.wakeOnPlaneHash, 0f, 2, 0f);
			}
		}
		if (this.currState0.shortNameHash == this.throwerIdleHash || this.currState0.shortNameHash == this.toThrowerIdleHash || this.currState0.shortNameHash == this.throwerReleaseHash)
		{
			if (!this.doThrowerSync)
			{
				this.activeThrowerGo = this.findClosestThrower();
			}
			if (this.activeThrowerGo)
			{
				if (!this.crt)
				{
					this.crt = this.activeThrowerGo.GetComponent<coopRockThrower>();
				}
				this.crt.leverRotateTr.localEulerAngles = new Vector3(0f, this.animator.GetFloat("normCamY") / 2f, 0f);
			}
			this.doThrowerSync = true;
		}
		else
		{
			this.doThrowerSync = false;
			this.activeThrowerGo = null;
			this.crt = null;
		}
		if (this.currState1.shortNameHash == this.heldGirlHash && this.currState2.shortNameHash != this.idleToGirlHash && this.nextState2.shortNameHash != this.idleToGirlHash)
		{
			if (!this.holdingMegan)
			{
				this.heldGirlGo = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("CutScene/girl_POSE_held"), base.transform.position, base.transform.rotation);
				this.heldGirlGo.transform.parent = this.chestJoint;
				this.heldGirlGo.transform.localPosition = new Vector3(-0.0716f, -3.493f, 0.25898f);
				this.heldGirlGo.transform.localEulerAngles = new Vector3(-4.1716f, -0.302f, -1.566f);
				this.holdingMegan = true;
			}
		}
		else if (this.heldGirlGo && this.holdingMegan)
		{
			UnityEngine.Object.Destroy(this.heldGirlGo);
			this.holdingMegan = false;
		}
		if (this.currState1.shortNameHash == this.eatMeatHash)
		{
			if (this.currState1.normalizedTime < 0.72f)
			{
				this.animatedMeatGo.SetActive(true);
				if (!this.meatAnimator)
				{
					this.meatAnimator = this.animatedMeatGo.GetComponent<Animator>();
				}
				if (!this.doMeatSync)
				{
					this.meatAnimator.CrossFade("Base Layer.eatMeatCycle", 0f, 0, this.currState1.normalizedTime);
					this.doMeatSync = true;
				}
			}
			else
			{
				this.animatedMeatGo.SetActive(false);
				this.doMeatSync = false;
			}
		}
		else if (this.animatedMeatGo.activeSelf)
		{
			this.animatedMeatGo.SetActive(false);
			this.doMeatSync = false;
		}
		this.currState3 = this.animator.GetCurrentAnimatorStateInfo(3);
		if (this.currState3.shortNameHash == this.birdOnHandHash)
		{
			if (!this.doBirdSync)
			{
				this.enableBirdOnHand(this.currState3.normalizedTime);
				this.doBirdSync = true;
			}
		}
		else if (this.doBirdSync)
		{
			this.disableBirdOnHand();
			this.doBirdSync = false;
		}
		if (this.heldBow == null)
		{
			return;
		}
		if (this.heldBow.activeSelf || this.heldRecurveBow.activeSelf)
		{
			if (this.heldBow.activeSelf)
			{
				this.bowAnimator = this.heldBow.GetComponent<Animator>();
				this.bowAnimator.SetFloat("bowSpeed", 1.2f);
			}
			else if (this.heldRecurveBow.activeSelf)
			{
				this.bowAnimator = this.heldRecurveBow.GetComponent<Animator>();
				this.bowAnimator.SetFloat("bowSpeed", 0.65f);
			}
			if (this.currState1.shortNameHash == this.lightBowHash || this.nextState1.shortNameHash == this.lightBowHash)
			{
				this.bowAnimator.SetBool("drawBool", false);
			}
			else if (this.currState1.shortNameHash != this.bowIdleHash && this.nextState1.shortNameHash != this.bowIdleHash && this.nextState1.shortNameHash != this.lightBowHash)
			{
				this.bowAnimator.SetBool("drawBool", true);
			}
			if (this.nextState1.shortNameHash == this.drawBowHash || this.nextState1.shortNameHash == this.drawBowIdleHash || this.currState1.shortNameHash == this.drawBowHash || this.currState1.shortNameHash == this.drawBowIdleHash)
			{
				if (this.currState1.shortNameHash != this.lightBowHash)
				{
					this.bowAnimator.SetBool("drawBool", true);
				}
			}
			else if (this.currState1.shortNameHash == this.drawBowHash)
			{
				this.bowAnimator.SetBool("drawBool", true);
			}
			else if (this.nextState1.shortNameHash == this.bowIdleHash || this.currState1.shortNameHash == this.bowIdleHash)
			{
				this.bowAnimator.SetBool("bowFireBool", false);
				this.bowAnimator.SetBool("drawBool", false);
				if (this.currState1.shortNameHash == this.bowIdleHash)
				{
					this.heldArrow.SetActive(true);
					this.heldRecurveArrow.SetActive(true);
				}
				this.arrowPosSync = false;
			}
			if ((this.nextState1.shortNameHash == this.releaseBow0Hash || this.nextState1.shortNameHash == this.releaseBowHash) && (this.currState1.shortNameHash == this.drawBowHash || this.currState1.shortNameHash == this.drawBowIdleHash))
			{
				this.bowAnimator.SetBool("bowFireBool", true);
				this.heldArrow.SetActive(false);
				this.heldRecurveArrow.SetActive(false);
			}
			else if (this.currState1.shortNameHash == this.releaseBowHash || this.currState1.shortNameHash == this.releaseBow0Hash)
			{
				if (this.currState1.shortNameHash == this.releaseBowHash && this.currState1.normalizedTime > 0.5f)
				{
					if (this.currState1.normalizedTime < 0.8f)
					{
						this.arrowPosSync = true;
						this.heldRecurveArrow.transform.position = this.leftHandWeaponTr.position;
						this.heldArrow.transform.position = this.leftHandWeaponTr.position;
					}
					else
					{
						this.arrowPosSync = false;
					}
					this.heldArrow.SetActive(true);
					this.heldRecurveArrow.SetActive(true);
					this.bowAnimator.SetBool("bowFireBool", false);
				}
				else
				{
					this.bowAnimator.SetBool("bowFireBool", true);
					this.heldArrow.SetActive(false);
					this.heldRecurveArrow.SetActive(false);
					this.arrowPosSync = false;
				}
			}
		}
		else
		{
			this.arrowPosSync = false;
		}
		if (this.heldSlingShot.activeSelf)
		{
			if (this.currState1.shortNameHash == this.slingShotIdleHash)
			{
				this.slingAnimator.SetBool("toAim", false);
				this.slingAnimator.SetBool("attack", false);
			}
			else if (this.currState1.shortNameHash == this.toSlingShotAimHash || this.nextState1.shortNameHash == this.toSlingShotAimHash)
			{
				if (this.currState1.shortNameHash == this.toSlingShotAimHash)
				{
					this.slingAnimator.Play(this.idleToAimHash, 0, this.currState1.normalizedTime);
				}
				this.slingAnimator.SetBool("toAim", true);
				this.slingAnimator.SetBool("attack", false);
			}
			else if (this.currState1.shortNameHash == this.slingShotAimIdleHash)
			{
				this.slingAnimator.SetBool("toAim", true);
				this.slingAnimator.SetBool("attack", false);
			}
			else if (this.currState1.shortNameHash == this.slingShotShootHash || this.nextState1.shortNameHash == this.slingShotShootHash)
			{
				if (this.currState1.shortNameHash == this.slingShotShootHash)
				{
					this.slingAnimator.Play(this.slingShotShootHash, 0, this.currState1.normalizedTime);
				}
				this.slingAnimator.SetBool("attack", true);
			}
			else
			{
				this.slingAnimator.SetBool("toAim", false);
				this.slingAnimator.SetBool("attack", false);
			}
		}
	}

	
	private GameObject findClosestThrower()
	{
		float num = float.PositiveInfinity;
		if (Scene.SceneTracker.builtRockThrowers.Count == 0)
		{
			return null;
		}
		GameObject result = null;
		for (int i = 0; i < Scene.SceneTracker.builtRockThrowers.Count; i++)
		{
			if (Scene.SceneTracker.builtRockThrowers[i])
			{
				float sqrMagnitude = (Scene.SceneTracker.builtRockThrowers[i].position - base.transform.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = Scene.SceneTracker.builtRockThrowers[i].gameObject;
				}
			}
		}
		return result;
	}

	
	private void LateUpdate()
	{
		this.currState0 = this.animator.GetCurrentAnimatorStateInfo(0);
		this.currState1 = this.animator.GetCurrentAnimatorStateInfo(1);
		if (this.arrowPosSync)
		{
			this.heldRecurveArrow.transform.position = this.leftHandWeaponTr.position;
			this.heldArrow.transform.position = this.leftHandWeaponTr.position;
		}
		float to = 0f;
		if (this.animator.GetFloat("overallSpeed") > 0.05f)
		{
			to = 10f;
		}
		float @float = this.animator.GetFloat("net_clampIdle");
		this.animator.SetFloat("net_clampIdle", Mathf.Lerp(@float, to, Time.deltaTime * 2f));
		if (this.currState0.tagHash == this.moveHash && this.animator.GetFloat("overallSpeed") > 0.05f && !this.animator.IsInTransition(1))
		{
			if (this.currState1.shortNameHash == this.stickIdleHash)
			{
				this.animator.Play(this.stickIdleHash, 1, this.currState0.normalizedTime);
			}
			else if (this.currState1.shortNameHash == this.bowIdleHash)
			{
				this.animator.Play(this.bowIdleHash, 1, this.currState0.normalizedTime);
			}
			else if (this.currState1.shortNameHash == this.carryLogHash)
			{
				this.animator.Play(this.carryLogHash, 1, this.currState0.normalizedTime);
			}
			else if (this.currState1.shortNameHash == this.flareGunIdleHash)
			{
				this.animator.Play(this.flareGunIdleHash, 1, this.currState0.normalizedTime);
			}
			else if (this.currState1.shortNameHash == this.flareHeldHash)
			{
				this.animator.Play(this.flareHeldHash, 1, this.currState0.normalizedTime);
			}
			else if (this.currState1.shortNameHash == this.flintlockIdlehash)
			{
				this.animator.Play(this.flintlockIdlehash, 1, this.currState0.normalizedTime);
			}
			else if (this.currState1.shortNameHash == this.mapHeldHash)
			{
				this.animator.Play(this.mapHeldHash, 1, this.currState0.normalizedTime);
			}
			else if (this.currState1.shortNameHash == this.molotovIdleHash)
			{
				this.animator.Play(this.molotovIdleHash, 1, this.currState0.normalizedTime);
			}
			else if (this.currState1.shortNameHash == this.rockIdleHash)
			{
				this.animator.Play(this.rockIdleHash, 1, this.currState0.normalizedTime);
			}
			else if (this.currState1.shortNameHash == this.shellIdleHash)
			{
				this.animator.Play(this.shellIdleHash, 1, this.currState0.normalizedTime);
			}
			else if (this.currState1.shortNameHash == this.spearIdleHash)
			{
				this.animator.Play(this.spearIdleHash, 1, this.currState0.normalizedTime);
			}
			else if (this.currState1.shortNameHash == this.tennisBallIdleHash)
			{
				this.animator.Play(this.tennisBallIdleHash, 1, this.currState0.normalizedTime);
			}
		}
		if (this.forceNetPosition)
		{
			base.transform.parent.position = new Vector3(1800f, 300f, 1800f);
		}
	}

	
	private void enableBirdOnHand(float t)
	{
		this.bird = (GameObject)UnityEngine.Object.Instantiate((GameObject)Resources.Load("CutScene/smallBird_ANIM_landOnFinger_prefab"), this.birdOnHandPos.position, this.birdOnHandPos.rotation);
		this.bird.transform.parent = this.birdOnHandPos;
		this.bird.transform.localPosition = Vector3.zero;
		this.bird.transform.localRotation = Quaternion.identity;
		Animator component = this.bird.GetComponent<Animator>();
		component.CrossFade("Base Layer.landOnFinger", 0f, 0, t);
	}

	
	private void disableBirdOnHand()
	{
		if (this.bird)
		{
			UnityEngine.Object.Destroy(this.bird);
		}
	}

	
	private void ChangeClip(string currClip, AnimationClip newClip)
	{
		Animator component = base.GetComponent<Animator>();
		AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController();
		animatorOverrideController.runtimeAnimatorController = component.runtimeAnimatorController;
		animatorOverrideController[currClip] = newClip;
		component.runtimeAnimatorController = animatorOverrideController;
	}

	
	private void fixRootRotation()
	{
		base.transform.parent.localEulerAngles = new Vector3(0f, base.transform.parent.localEulerAngles.y, 0f);
	}

	
	public void enableAttackParams()
	{
	}

	
	private void resetAttackParams()
	{
	}

	
	public void spawnNewGirlPickup(bool putDown)
	{
		CoopSyncGirlPickupToken coopSyncGirlPickupToken = new CoopSyncGirlPickupToken();
		coopSyncGirlPickupToken.putDown = putDown;
		coopSyncGirlPickupToken.pickup = false;
		coopSyncGirlPickupToken.playerTarget = base.transform.root.GetComponent<BoltEntity>();
		BoltNetwork.Instantiate(Resources.Load("CutScene/girl_Pickup") as GameObject, coopSyncGirlPickupToken, base.transform.position, base.transform.rotation);
	}

	
	private IEnumerator fixNetSpawnPosition()
	{
		float t = 0f;
		while (t < 2.5f)
		{
			this.forceNetPosition = true;
			t += Time.deltaTime;
			yield return null;
		}
		this.forceNetPosition = false;
		yield break;
	}

	
	private void OnDestroy()
	{
		if (this.holdingMegan)
		{
			UnityEngine.Object.Instantiate(Resources.Load("CutScene/girl_Pickup"), base.transform.position, base.transform.rotation);
		}
	}

	
	private netHideDuringPlaneCrash netHider;

	
	private playerAiInfo aiInfo;

	
	private targetStats ts;

	
	private netPlayerVis vis;

	
	private Transform rootTr;

	
	public GameObject displacementGo;

	
	public GameObject heldGirlGo;

	
	public CapsuleCollider enemyBlockerCollider;

	
	public GameObject animatedMeatGo;

	
	public GameObject heldBow;

	
	public GameObject heldRecurveBow;

	
	public GameObject heldArrow;

	
	public GameObject heldRecurveArrow;

	
	public Transform birdOnHandPos;

	
	public GameObject hitDetectGo;

	
	public Collider rootCollider;

	
	public Transform chestJoint;

	
	public Transform leftHandWeaponTr;

	
	public GameObject heldChainSaw;

	
	public GameObject heldSlingShot;

	
	public GameObject skinnedMeatGo;

	
	public GameObject skinDeerGo;

	
	public GameObject skinRabbitGo;

	
	public GameObject skinLizardGo;

	
	public GameObject axePlaneHeldGo;

	
	public GameObject tempCube;

	
	public GameObject tempSphere;

	
	private GameObject activeThrowerGo;

	
	private bool holdingMegan;

	
	private bool rootEnabled;

	
	private bool forceNetPosition;

	
	public int animalType;

	
	private Animator animator;

	
	private Animator meatAnimator;

	
	private Animator bowAnimator;

	
	private Animator slingAnimator;

	
	private GameObject bird;

	
	private coopRockThrower crt;

	
	private float lyr1;

	
	private float lyr2;

	
	private int eatMeatHash = Animator.StringToHash("eatMeat");

	
	private int birdOnHandHash = Animator.StringToHash("birdOnHand");

	
	private int drawBowHash = Animator.StringToHash("drawBow");

	
	private int drawBowIdleHash = Animator.StringToHash("drawBowIdle");

	
	private int releaseBow0Hash = Animator.StringToHash("releaseBow 0");

	
	private int releaseBowHash = Animator.StringToHash("releaseBow");

	
	private int bowIdleHash = Animator.StringToHash("bowIdle");

	
	private int lightBowHash = Animator.StringToHash("lightBow");

	
	private int getupHash = Animator.StringToHash("getup");

	
	private int skinAnimalHash = Animator.StringToHash("skinAnimal");

	
	private int wakeOnPlaneHash = Animator.StringToHash("clientWakOnPlane");

	
	private int deathHash = Animator.StringToHash("death");

	
	private int heldGirlHash = Animator.StringToHash("girlPickupIdle");

	
	private int idleToGirlHash = Animator.StringToHash("idleToGirlPickup");

	
	private int stickIdleHash = Animator.StringToHash("idleStick");

	
	private int injuredLoopHash = Animator.StringToHash("injuredLoop");

	
	private int enterClimbHash = Animator.StringToHash("enterClimb");

	
	private int climbIdleHash = Animator.StringToHash("climbIdle");

	
	private int climbingHash = Animator.StringToHash("climbing");

	
	private int axeCombo1Hash = Animator.StringToHash("axeCombo1");

	
	private int toZipHash = Animator.StringToHash("idleToZip");

	
	private int zipIdleHash = Animator.StringToHash("zipIdle");

	
	private int throwerIdleHash = Animator.StringToHash("throwerIdle");

	
	private int toThrowerIdleHash = Animator.StringToHash("toThrowerIdle");

	
	private int throwerReleaseHash = Animator.StringToHash("throwerRelease");

	
	private int moveHash = Animator.StringToHash("moving");

	
	private bool doMeatSync;

	
	private bool doBirdSync;

	
	private bool doDrawBowSync;

	
	private bool doReleaseBowSync;

	
	private bool doSkinAnimalSync;

	
	private bool doPlaneStartSync;

	
	private bool doZipSync;

	
	private bool arrowPosSync;

	
	private bool doThrowerSync;

	
	public AnimationClip crouchIdle;

	
	private AnimatorStateInfo currState0;

	
	private AnimatorStateInfo currState1;

	
	private AnimatorStateInfo currState2;

	
	private AnimatorStateInfo currState3;

	
	private AnimatorStateInfo nextState1;

	
	private AnimatorStateInfo nextState2;

	
	private float startDisableCollisionTimer;

	
	private float delayPlaneStartTimer;

	
	private Vector3 playerPos;

	
	private int carryLogHash = Animator.StringToHash("carryLog");

	
	private int flareGunIdleHash = Animator.StringToHash("flareGunIdle");

	
	private int flareHeldHash = Animator.StringToHash("flareIdle");

	
	private int flintlockIdlehash = Animator.StringToHash("flintlockIdle");

	
	private int mapHeldHash = Animator.StringToHash("holdMap");

	
	private int molotovIdleHash = Animator.StringToHash("molotovIdle");

	
	private int rockIdleHash = Animator.StringToHash("rockIdle");

	
	private int shellIdleHash = Animator.StringToHash("shellHeldIdle");

	
	private int spearIdleHash = Animator.StringToHash("spearIdle");

	
	private int tennisBallIdleHash = Animator.StringToHash("ballIdle");

	
	private int slingShotIdleHash = Animator.StringToHash("slingShotIdle");

	
	private int toSlingShotAimHash = Animator.StringToHash("toSlingShotAim");

	
	private int slingShotAimIdleHash = Animator.StringToHash("slingShotAimIdle");

	
	private int slingShotShootHash = Animator.StringToHash("slingShotShoot");

	
	private int idleToAimHash = Animator.StringToHash("idleToAim");
}
