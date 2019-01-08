using System;
using System.Collections;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Items.World;
using TheForest.Utils;
using UnityEngine;

public class crossbowController : MonoBehaviour
{
	private void OnEnable()
	{
		this._boltAnimated.SetActive(this.weaponLoaded);
		if (this.crossbowAnimator == null)
		{
			this.crossbowAnimator = base.transform.GetComponent<Animator>();
		}
		this.crossbowFired = true;
		if (!this.NetPrefab)
		{
			LocalPlayer.Animator.SetBool("attack", false);
		}
		if (this.weaponLoaded)
		{
			this.crossbowAnimator.CrossFade(this.idleLoadedHash, 0f, 0, 0f);
		}
		else
		{
			this.crossbowAnimator.CrossFade(this.unloadedHash, 0f, 0, 0f);
		}
	}

	private void OnDisable()
	{
		this.crossbowFired = false;
		if (!this.NetPrefab)
		{
			LocalPlayer.Animator.SetBool("spearRaiseBool", false);
			LocalPlayer.Animator.SetBool("attack", false);
			LocalPlayer.Animator.SetBool("crossbowReload", false);
			LocalPlayer.Animator.SetBoolReflected("clampSpine", false);
		}
	}

	private void Update()
	{
		if (!this.NetPrefab)
		{
			if (LocalPlayer.CurrentView != PlayerInventory.PlayerViews.World)
			{
				return;
			}
			this.setupWeaponLoaded();
			this.setupClampedSpine();
			this.setupAnimatedBolt();
			this.syncIdleState();
		}
		if (!this.NetPrefab)
		{
			this.setupAttackInput();
		}
		if (this.NetPrefab)
		{
			this.setupNetSync();
		}
	}

	private void setupNetSync()
	{
		AnimatorStateInfo currentAnimatorStateInfo = this.netAnimator.GetCurrentAnimatorStateInfo(1);
		AnimatorStateInfo nextAnimatorStateInfo = this.netAnimator.GetNextAnimatorStateInfo(1);
		AnimatorStateInfo currentAnimatorStateInfo2 = this.crossbowAnimator.GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.shortNameHash == this.crossbowReloadHash)
		{
			if (currentAnimatorStateInfo.normalizedTime > 0.1f)
			{
				this._boltAnimated.SetActive(true);
			}
			else
			{
				this._boltAnimated.SetActive(false);
			}
			this.crossbowAnimator.SetBool("cancelReload", false);
			this.crossbowAnimator.SetBool("fire", false);
			this.crossbowAnimator.Play(this.reloadHash, 0, currentAnimatorStateInfo.normalizedTime);
			this.doBowSnap = false;
		}
		else if (currentAnimatorStateInfo.shortNameHash == this.crossbowIdleReloadHash || nextAnimatorStateInfo.shortNameHash == this.crossbowIdleReloadHash)
		{
			this.crossbowAnimator.CrossFade(this.idleLoadedHash, 0.1f, 0, 0f);
			this.crossbowAnimator.SetBool("cancelReload", false);
			this.crossbowAnimator.SetBool("fire", false);
			this._boltAnimated.SetActive(true);
			this.doBowSnap = false;
		}
		else if (currentAnimatorStateInfo.shortNameHash == this.crossbowFireHash || nextAnimatorStateInfo.shortNameHash == this.crossbowFireHash)
		{
			if (!this.doBowSnap)
			{
				FMODCommon.PlayOneshot("event:/combat/bow/bow_fire", base.transform);
				this.doBowSnap = true;
			}
			this.crossbowAnimator.SetBool("fire", true);
			this._boltAnimated.SetActive(false);
		}
		else if (currentAnimatorStateInfo.shortNameHash == this.crossbowIdleHash)
		{
			this.crossbowAnimator.SetBool("cancelReload", true);
			this.crossbowAnimator.SetBool("fire", false);
			this._boltAnimated.SetActive(false);
			this.doBowSnap = false;
		}
	}

	private void syncIdleState()
	{
		AnimatorStateInfo currentAnimatorStateInfo = this.crossbowAnimator.GetCurrentAnimatorStateInfo(0);
		if (LocalPlayer.AnimControl.currLayerState1.shortNameHash == this.crossbowIdleHash && currentAnimatorStateInfo.shortNameHash == this.idleLoadedHash && LocalPlayer.Inventory.Owns(this._ammoId, true))
		{
			this.weaponLoaded = true;
			this.crossbowFired = false;
			LocalPlayer.Animator.SetBool("crossbowReload", false);
			LocalPlayer.Animator.SetInteger("crossbowReloadState", 2);
			this._boltAnimated.SetActive(true);
		}
	}

	private void setupAttackInput()
	{
		if (TheForest.Utils.Input.GetButton("AltFire") || ForestVR.Enabled)
		{
			LocalPlayer.Animator.SetBool("spearRaiseBool", true);
			this.aiming = true;
			if (TheForest.Utils.Input.GetButtonDown("Fire1") && LocalPlayer.AnimControl.currLayerState1.shortNameHash == this.crossbowAimIdleHash && this.weaponLoaded)
			{
				base.Invoke("fireProjectile", 0.1f);
				base.Invoke("disableAnimatedBolt", 0.1f);
				LocalPlayer.Sfx.PlayBowSnap();
				LocalPlayer.Animator.SetBool("attack", true);
				this.crossbowAnimator.SetBool("fire", true);
				this.weaponLoaded = false;
				this.crossbowFired = true;
			}
		}
		else if (this.aiming)
		{
			LocalPlayer.Animator.SetBool("spearRaiseBool", false);
			this.aiming = false;
		}
	}

	private void setupAnimatedBolt()
	{
		if (LocalPlayer.AnimControl.currLayerState1.shortNameHash == this.crossbowReloadHash)
		{
			this.crossbowAnimator.Play(this.reloadHash, 0, LocalPlayer.AnimControl.currLayerState1.normalizedTime);
			if (LocalPlayer.AnimControl.currLayerState1.normalizedTime > 0.1f)
			{
				this._boltAnimated.SetActive(true);
			}
			else
			{
				this._boltAnimated.SetActive(false);
			}
			if (LocalPlayer.AnimControl.currLayerState1.normalizedTime > 0.7f)
			{
				LocalPlayer.Animator.SetInteger("crossbowReloadState", 2);
				this.weaponLoaded = true;
			}
		}
	}

	private void setupWeaponLoaded()
	{
		if (!this.weaponLoaded)
		{
			if (this.weaponLoaded)
			{
				LocalPlayer.Animator.SetInteger("crossbowReloadState", 2);
				this.crossbowFired = false;
			}
			if (!this.weaponLoaded)
			{
				if (LocalPlayer.Inventory.Owns(this._ammoId, true))
				{
					LocalPlayer.Animator.SetInteger("crossbowReloadState", 1);
				}
				else
				{
					this._boltAnimated.SetActive(false);
					LocalPlayer.Animator.SetInteger("crossbowReloadState", 0);
				}
			}
			LocalPlayer.Animator.SetBool("attack", false);
			this.crossbowAnimator.SetBool("fire", false);
			if (LocalPlayer.AnimControl.currLayerState1.shortNameHash == this.crossbowReloadHash)
			{
				this.crossbowFired = false;
				this.crossbowAnimator.SetBool("cancelReload", false);
			}
			else
			{
				this._boltAnimated.SetActive(false);
				this.crossbowAnimator.SetBool("cancelReload", true);
			}
		}
	}

	private void setupClampedSpine()
	{
		if (LocalPlayer.AnimControl.currLayerState1.shortNameHash == this.crossbowAimIdleHash || LocalPlayer.AnimControl.currLayerState1.shortNameHash == this.crossbowFireHash || LocalPlayer.AnimControl.nextLayerState1.shortNameHash == this.crossbowAimIdleHash)
		{
			LocalPlayer.Animator.SetBoolReflected("clampSpine", true);
		}
		else
		{
			LocalPlayer.Animator.SetBoolReflected("clampSpine", false);
		}
	}

	private void fireProjectile()
	{
		if (LocalPlayer.Inventory.RemoveItem(this._ammoId, 1, false, true))
		{
			Vector3 position = this._ammoSpawnPosGo.transform.position;
			Quaternion rotation = this._ammoSpawnPosGo.transform.rotation;
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this._boltProjectile, position, rotation);
			Rigidbody component = gameObject.GetComponent<Rigidbody>();
			if (BoltNetwork.isRunning)
			{
				BoltEntity component2 = gameObject.GetComponent<BoltEntity>();
				if (component2)
				{
					BoltNetwork.Attach(gameObject);
				}
			}
			PickUp componentInChildren = gameObject.GetComponentInChildren<PickUp>(true);
			if (componentInChildren)
			{
				SheenBillboard[] componentsInChildren = gameObject.GetComponentsInChildren<SheenBillboard>();
				foreach (SheenBillboard sheenBillboard in componentsInChildren)
				{
					sheenBillboard.gameObject.SetActive(false);
				}
				componentInChildren.gameObject.SetActive(false);
				if (base.gameObject.activeInHierarchy)
				{
					base.StartCoroutine(this.enablePickupTrigger(componentInChildren.gameObject));
				}
			}
			Vector3 up = gameObject.transform.up;
			component.AddForce(22000f * (0.016666f / Time.fixedDeltaTime) * up);
		}
	}

	private IEnumerator enablePickupTrigger(GameObject go)
	{
		yield return YieldPresets.WaitOneSecond;
		if (go)
		{
			go.SetActive(true);
		}
		yield break;
	}

	private void disableAnimatedBolt()
	{
		this._boltAnimated.SetActive(false);
	}

	public GameObject _boltAnimated;

	public GameObject _ammoSpawnPosGo;

	public GameObject _boltProjectile;

	public Animator netAnimator;

	[ItemIdPicker(Item.Types.RangedWeapon)]
	public int _crossbowId;

	[ItemIdPicker(Item.Types.Ammo)]
	public int _ammoId;

	public bool weaponLoaded;

	private Animator crossbowAnimator;

	public bool aiming;

	public bool crossbowFired;

	public bool NetPrefab;

	private bool doBowSnap;

	private int crossbowAimIdleHash = Animator.StringToHash("crossbowAimIdle");

	private int crossbowFireHash = Animator.StringToHash("crossbowFire");

	private int crossbowIdleHash = Animator.StringToHash("crossbowIdle");

	private int crossbowReloadHash = Animator.StringToHash("crossbowReload");

	private int crossbowIdleReloadHash = Animator.StringToHash("crossbowIdleReload");

	private int reloadHash = Animator.StringToHash("reload");

	private int idleLoadedHash = Animator.StringToHash("idleLoaded");

	private int unloadedHash = Animator.StringToHash("unloaded");
}
