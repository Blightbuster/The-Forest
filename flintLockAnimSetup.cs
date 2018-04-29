using System;
using ModAPI;
using TheForest.Items;
using TheForest.Utils;
using UltimateCheatmenu;
using UnityEngine;


public class flintLockAnimSetup : MonoBehaviour
{
	
	private void Start()
	{
		this.animator = base.transform.GetComponent<Animator>();
	}

	
	private void OnEnable()
	{
		if (!base.transform.GetComponentInParent<CoopPlayerVariations>())
		{
			this.animator = base.transform.GetComponent<Animator>();
			if (this.animator)
			{
				this.animator.enabled = false;
			}
			base.enabled = false;
		}
		this.leftHandFull = false;
		if (this.storeReloadDelay > 0.5f && Time.time < this.realTimeSinceLastReload + 5f)
		{
			base.Invoke("ForceReloadWeapon", 0.05f);
		}
		else if (LocalPlayer.Animator)
		{
			LocalPlayer.Animator.SetBool("forceReload", false);
		}
	}

	
	private void OnDisable()
	{
		if (LocalPlayer.Inventory)
		{
			this.storeReloadDelay = LocalPlayer.Inventory.CalculateRemainingReloadDelay();
			this.realTimeSinceLastReload = Time.time;
			LocalPlayer.Inventory.CancelReloadDelay();
		}
	}

	
	private void __Update__Original()
	{
		if (!this._net && LocalPlayer.GameObject)
		{
			if (LocalPlayer.Inventory.Owns(this._flintAmmoId, false))
			{
				LocalPlayer.Animator.SetBool("canReload", true);
			}
			else
			{
				LocalPlayer.Animator.SetBool("canReload", false);
			}
		}
		if (this._playerAnimator)
		{
			this.currState1 = this._playerAnimator.GetCurrentAnimatorStateInfo(1);
			this.nextState1 = this._playerAnimator.GetNextAnimatorStateInfo(1);
			this.currState2 = this._playerAnimator.GetCurrentAnimatorStateInfo(2);
			if (this.currState1.shortNameHash == this.playerReloadHash && !this._net)
			{
				this._playerAnimator.SetBool("forceReload", false);
			}
			if (this.currState1.tagHash == this.knockBackHash || this.currState2.shortNameHash == this.sittingHash)
			{
				this.animator.CrossFade(this.idleHash, 0f, 0, 0f);
			}
			if (this.nextState1.shortNameHash == this.playerShootHash || this.nextState1.shortNameHash == this.playerAimShootHash)
			{
				this.animator.SetBool("shoot", true);
			}
			else
			{
				this.animator.SetBool("shoot", false);
			}
			if (this.currState1.shortNameHash == this.playerReloadHash && !this.reloadSync)
			{
				this.animator.CrossFade(this.reloadHash, 0f, 0, this.currState1.normalizedTime);
				this.reloadSync = true;
				this.animator.SetBool("shoot", false);
			}
			else if (this.animator.GetCurrentAnimatorStateInfo(0).shortNameHash == this.shootHash)
			{
				this.reloadSync = false;
			}
			if (this._net && this.currState1.shortNameHash == this.playerShootHash)
			{
				this.animator.Play(this.shootHash, 0, this.currState1.normalizedTime);
			}
			else if (this._net && this.currState1.shortNameHash == this.playerReloadHash)
			{
				this.animator.Play(this.reloadHash, 0, this.currState1.normalizedTime);
			}
			if (this.nextState1.shortNameHash == this.playerIdleHash && !this._net)
			{
				LocalPlayer.Inventory.CancelReloadDelay();
			}
		}
	}

	
	private void ForceReloadWeapon()
	{
		this.reloadSync = false;
		if (LocalPlayer.Transform)
		{
			LocalPlayer.Animator.SetBool("forceReload", true);
			LocalPlayer.Inventory.ForceReloadDelay();
		}
	}

	
	private void Update()
	{
		try
		{
			if (UCheatmenu.FastFlint)
			{
				if (LocalPlayer.Inventory.AmountOf(this._flintAmmoId, true) > 0)
				{
					LocalPlayer.Animator.SetBool("canReload", false);
				}
				else
				{
					LocalPlayer.Animator.SetBool("canReload", false);
				}
				if (!LocalPlayer.Animator)
				{
					return;
				}
				this.currState1 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1);
				this.nextState1 = LocalPlayer.Animator.GetNextAnimatorStateInfo(1);
				this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
				if (this.currState1.tagHash == this.knockBackHash || this.currState2.shortNameHash == this.sittingHash)
				{
					this.animator.CrossFade(this.idleHash, 0f, 0, 0f);
				}
				if (this.nextState1.shortNameHash == this.playerShootHash)
				{
					this.animator.SetBool("shoot", true);
				}
				else
				{
					this.animator.SetBool("shoot", false);
				}
				if (this.currState1.shortNameHash == this.playerReloadHash && !this.reloadSync && !LocalPlayer.Inventory.IsLeftHandEmpty())
				{
					this.leftHandFull = true;
					LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.LeftHand);
					LocalPlayer.Inventory.StashLeftHand();
				}
				if (this.nextState1.shortNameHash != this.playerIdleHash)
				{
					return;
				}
				LocalPlayer.Inventory.CancelReloadDelay();
				if (!this.leftHandFull)
				{
					return;
				}
				LocalPlayer.Inventory.EquipPreviousUtility(false);
				this.leftHandFull = false;
				return;
			}
			else
			{
				this.__Update__Original();
			}
		}
		catch (Exception ex)
		{
			Log.Write("Exception thrown: " + ex.ToString(), "UltimateCheatmenu");
			this.__Update__Original();
		}
	}

	
	public Animator _playerAnimator;

	
	private Animator animator;

	
	private AnimatorStateInfo currState1;

	
	private AnimatorStateInfo nextState1;

	
	private AnimatorStateInfo currState2;

	
	public bool _net;

	
	private int playerReloadHash = Animator.StringToHash("reloadFlintLock");

	
	private int reloadHash = Animator.StringToHash("reload");

	
	private int shootHash = Animator.StringToHash("shoot");

	
	private int playerShootHash = Animator.StringToHash("shootFlintLock");

	
	private int playerAimShootHash = Animator.StringToHash("aimIdleShoot");

	
	private int playerIdleHash = Animator.StringToHash("flintlockIdle");

	
	private int knockBackHash = Animator.StringToHash("knockBack");

	
	private int idleHash = Animator.StringToHash("idle");

	
	private int sittingHash = Animator.StringToHash("seatedRockIdle");

	
	private bool reloadSync;

	
	private bool leftHandFull;

	
	private float storeReloadDelay;

	
	private float realTimeSinceLastReload;

	
	[ItemIdPicker]
	public int _flintAmmoId;
}
