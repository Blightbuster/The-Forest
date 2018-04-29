using System;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;


public class flareGunAnimSetup : MonoBehaviour
{
	
	private void Start()
	{
		this._animator = base.transform.GetComponent<Animator>();
	}

	
	private void OnEnable()
	{
		if (!base.transform.root.GetComponent<CoopPlayerVariations>())
		{
			this._animator = base.transform.GetComponent<Animator>();
			if (this._animator)
			{
				this._animator.enabled = false;
			}
			base.enabled = false;
		}
		this._ammoEmpty.SetActive(false);
		this._ammoFull.SetActive(false);
		this.blockAmmoSpawn = false;
		this.leftHandFull = false;
		if (this.storeReloadDelay > 0.5f && Time.time < this.realTimeSinceLastReload + 5f)
		{
			base.Invoke("ForceReloadWeapon", 0.05f);
		}
		else if (!this._net)
		{
			this._playerAnimator.SetBool("forceReload", false);
		}
	}

	
	private void OnDisable()
	{
		if (!this._net)
		{
			this.storeReloadDelay = LocalPlayer.Inventory.CalculateRemainingReloadDelay();
			this.realTimeSinceLastReload = Time.time;
			LocalPlayer.Inventory.CancelReloadDelay();
		}
		this._ammoEmpty.SetActive(false);
		this._ammoFull.SetActive(false);
		this.blockAmmoSpawn = false;
		this.noiseCoolDown = false;
	}

	
	private void Update()
	{
		if (!this._net)
		{
			if (LocalPlayer.Inventory.Owns(this._ammoId, false))
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
			AnimatorStateInfo currentAnimatorStateInfo = this._animator.GetCurrentAnimatorStateInfo(0);
			this.currState1 = this._playerAnimator.GetCurrentAnimatorStateInfo(1);
			this.nextState1 = this._playerAnimator.GetNextAnimatorStateInfo(1);
			this.currState2 = this._playerAnimator.GetCurrentAnimatorStateInfo(2);
			if (this.currState1.shortNameHash == this.playerReloadHash && !this._net)
			{
				this._playerAnimator.SetBool("forceReload", false);
			}
			if (this.currState1.tagHash == this.knockBackHash || this.currState2.shortNameHash == this.sittingHash)
			{
				this._animator.CrossFade(this.idleHash, 0f, 0, 0f);
			}
			if (this.nextState1.shortNameHash == this.playerShootHash || this.nextState1.shortNameHash == this.playerAimShootHash)
			{
				if (!this._net)
				{
					this.doWeaponNoise();
				}
				this._animator.SetBool("shoot", true);
			}
			else
			{
				this._animator.SetBool("shoot", false);
			}
			if (this.currState1.shortNameHash == this.playerReloadHash && !this.reloadSync)
			{
				this._animator.CrossFade(this.reloadHash, 0f, 0, this.currState1.normalizedTime);
				this.reloadSync = true;
				this._animator.SetBool("shoot", false);
			}
			else if (currentAnimatorStateInfo.shortNameHash == this.shootHash)
			{
				this.reloadSync = false;
			}
			if (this._net && this.currState1.shortNameHash == this.playerShootHash)
			{
				this._animator.Play(this.shootHash, 0, this.currState1.normalizedTime);
			}
			else if (this._net && this.currState1.shortNameHash == this.playerReloadHash)
			{
				this._animator.Play(this.reloadHash, 0, this.currState1.normalizedTime);
			}
			if (this.nextState1.shortNameHash == this.playerIdleHash && !this._net)
			{
				LocalPlayer.Inventory.CancelReloadDelay();
			}
			if (currentAnimatorStateInfo.shortNameHash == this.reloadHash)
			{
				if (currentAnimatorStateInfo.normalizedTime < 0.1f)
				{
					this._ammoEmpty.SetActive(false);
					this._ammoFull.SetActive(false);
				}
				else if (currentAnimatorStateInfo.normalizedTime < 0.306f)
				{
					this._ammoEmpty.SetActive(true);
					this._ammoFull.SetActive(true);
				}
				else if (currentAnimatorStateInfo.normalizedTime < 0.73f)
				{
					this._ammoEmpty.SetActive(false);
					this._ammoFull.SetActive(true);
				}
				else
				{
					this._ammoEmpty.SetActive(false);
					this._ammoFull.SetActive(false);
				}
			}
			else if (currentAnimatorStateInfo.shortNameHash == this.idleHash)
			{
				if (this._ammoEmpty.activeSelf)
				{
					this._ammoEmpty.SetActive(false);
				}
				if (this._ammoFull.activeSelf)
				{
					this._ammoFull.SetActive(false);
				}
			}
		}
	}

	
	private void ForceReloadWeapon()
	{
		this.reloadSync = false;
		if (!this._net)
		{
			LocalPlayer.Animator.SetBool("forceReload", true);
			LocalPlayer.Inventory.ForceReloadDelay();
		}
	}

	
	private void doWeaponNoise()
	{
		if (!this.noiseCoolDown)
		{
			LocalPlayer.ScriptSetup.pmNoise.SendEvent("toWeaponNoise");
			this.noiseCoolDown = true;
			base.Invoke("resetNoiseCoolDown", 1f);
		}
	}

	
	private void resetNoiseCoolDown()
	{
		this.noiseCoolDown = false;
	}

	
	private void spawnFlareGunAmmo()
	{
		if (base.enabled && base.gameObject.activeSelf && !this.blockAmmoSpawn)
		{
			this.blockAmmoSpawn = true;
			GameObject gameObject = UnityEngine.Object.Instantiate(this._ammoSpawn, this._ammoEmpty.transform.position, this._ammoEmpty.transform.rotation) as GameObject;
			gameObject.GetComponent<Rigidbody>().AddForce(this._ammoEmpty.transform.forward * -1f * (0.016666f / Time.fixedDeltaTime), ForceMode.VelocityChange);
			gameObject.GetComponent<Rigidbody>().AddTorque(this._ammoEmpty.transform.right * -3f, ForceMode.VelocityChange);
			this._ammoEmpty.SetActive(false);
			base.Invoke("resetBlockAmmoSpawn", 0.25f);
		}
	}

	
	private void resetBlockAmmoSpawn()
	{
		this.blockAmmoSpawn = false;
	}

	
	public Animator _playerAnimator;

	
	public GameObject _ammoEmpty;

	
	public GameObject _ammoFull;

	
	public GameObject _ammoSpawn;

	
	public bool _net;

	
	[ItemIdPicker]
	public int _ammoId;

	
	private Animator _animator;

	
	private AnimatorStateInfo currState1;

	
	private AnimatorStateInfo nextState1;

	
	private AnimatorStateInfo currState2;

	
	private int playerReloadHash = Animator.StringToHash("flaregunReload");

	
	private int reloadHash = Animator.StringToHash("reload");

	
	private int shootHash = Animator.StringToHash("shoot");

	
	private int playerShootHash = Animator.StringToHash("shootFlareGun");

	
	private int playerAimShootHash = Animator.StringToHash("flareGunAimShoot");

	
	private int playerIdleHash = Animator.StringToHash("flareGunIdle");

	
	private int knockBackHash = Animator.StringToHash("knockBack");

	
	private int idleHash = Animator.StringToHash("idle");

	
	private int sittingHash = Animator.StringToHash("seatedRockIdle");

	
	private bool reloadSync;

	
	private bool leftHandFull;

	
	private bool blockAmmoSpawn;

	
	private bool noiseCoolDown;

	
	private float storeReloadDelay;

	
	private float realTimeSinceLastReload;
}
