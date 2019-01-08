using System;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

public class flareHeldAnimSetup : MonoBehaviour
{
	private void Start()
	{
		this._animator = base.transform.GetComponent<Animator>();
	}

	private void OnEnable()
	{
		if (!base.transform.GetComponentInParent<CoopPlayerVariations>())
		{
			this._animator = base.transform.GetComponent<Animator>();
			if (this._animator)
			{
				this._animator.enabled = false;
			}
			base.enabled = false;
		}
		if (ForestVR.Enabled)
		{
			return;
		}
		if (!this._net)
		{
			LocalPlayer.Inventory.UseAltWorldPrefab = true;
		}
		this._flareBody.sharedMaterial = this._unlitBodyMat;
		this._cap.SetActive(true);
		this._capHeld.SetActive(false);
		if (this._flareLight)
		{
			this._flareLight.SetActive(false);
		}
		this.blockAmmoSpawn = false;
		this.leftHandFull = false;
		this.flareIsLit = false;
		if (!this._net)
		{
		}
	}

	private void OnDisable()
	{
		if (!this._net)
		{
			LocalPlayer.Inventory.UseAltWorldPrefab = false;
			base.CancelInvoke("setFlareIsLit");
		}
		this._cap.SetActive(true);
		this._capHeld.SetActive(false);
		if (this._flareLight)
		{
			this._flareLight.SetActive(false);
		}
	}

	private void Update()
	{
		if (ForestVR.Enabled)
		{
			return;
		}
		if (this._playerAnimator)
		{
			AnimatorStateInfo currentAnimatorStateInfo = this._animator.GetCurrentAnimatorStateInfo(0);
			this.currState1 = this._playerAnimator.GetCurrentAnimatorStateInfo(1);
			this.nextState1 = this._playerAnimator.GetNextAnimatorStateInfo(1);
			this.currState2 = this._playerAnimator.GetCurrentAnimatorStateInfo(2);
			if (!this._net && !this.flareIsLit && TheForest.Utils.Input.GetButtonDown("AltFire") && this.currState1.shortNameHash == this.flareIdleHash)
			{
				if (!LocalPlayer.Inventory.IsSlotEmpty(Item.EquipmentSlot.LeftHand))
				{
					LocalPlayer.Inventory.StashLeftHand();
					LocalPlayer.Animator.CrossFade(this.idleBaseHash, 0f, 3, 0f);
				}
				this._playerAnimator.SetBool("strikeFlare", true);
				this.flareIsLit = true;
				base.CancelInvoke("setFlareIsLit");
				base.Invoke("resetLightWeapon", 1f);
				base.Invoke("setFlareIsLit", 1f);
			}
			if (this.currState1.tagHash == this.knockBackHash || this.currState2.shortNameHash == this.sittingHash)
			{
				this._animator.CrossFade(this.idleHash, 0f, 0, 0f);
			}
			if (this.currState1.shortNameHash == this.playerReloadHash)
			{
				this._animator.Play(this.reloadHash, 0, this.currState1.normalizedTime);
				if (!this._net)
				{
					this._playerAnimator.SetBool("cancelCheckArms", true);
					this._playerAnimator.SetBool("checkArms", false);
				}
			}
			else if (!this._net)
			{
				this._playerAnimator.SetBool("cancelCheckArms", false);
			}
			if (this.currState1.shortNameHash == this.flareToIdleHash)
			{
				this._animator.CrossFade(this.idleHash, 0f, 0, 0f);
				if (this._cap.activeSelf)
				{
					this._cap.SetActive(false);
				}
				if (this._capHeld.activeSelf)
				{
					this._capHeld.SetActive(false);
				}
				if (this._flareLight && this._flareLight.activeSelf)
				{
					this._flareLight.SetActive(false);
				}
			}
			else if (this.currState1.shortNameHash == this.idleToFlareHash)
			{
				this._animator.CrossFade(this.idleHash, 0f, 0, 0f);
				if (!this._cap.activeSelf)
				{
					this._cap.SetActive(true);
				}
				if (this._capHeld.activeSelf)
				{
					this._capHeld.SetActive(false);
				}
				if (this._flareLight && this._flareLight.activeSelf)
				{
					this._flareLight.SetActive(false);
				}
			}
			else if (currentAnimatorStateInfo.shortNameHash == this.litIdleHash)
			{
				if (this._cap.activeSelf)
				{
					this._cap.SetActive(false);
				}
				if (this._capHeld.activeSelf)
				{
					this._capHeld.SetActive(false);
				}
				if (this._flareLight && !this._flareLight.activeSelf)
				{
					this._flareLight.SetActive(true);
				}
			}
			else if (currentAnimatorStateInfo.shortNameHash == this.reloadHash)
			{
				if (currentAnimatorStateInfo.normalizedTime > 0.88f)
				{
					this._cap.SetActive(false);
					this._capHeld.SetActive(false);
				}
				else if (currentAnimatorStateInfo.normalizedTime > 0.63f)
				{
					if (this._flareLight)
					{
						this._flareLight.SetActive(true);
					}
					this._flareBody.sharedMaterial = this._litBodyMat;
				}
				else if (currentAnimatorStateInfo.normalizedTime > 0.25f)
				{
					if (this._flareLight)
					{
						this._flareLight.SetActive(false);
					}
					this._cap.SetActive(false);
					this._capHeld.SetActive(true);
				}
				else if (currentAnimatorStateInfo.normalizedTime < 0.25f)
				{
					if (this._flareLight)
					{
						this._flareLight.SetActive(false);
					}
					this._cap.SetActive(true);
					this._capHeld.SetActive(false);
					this._flareBody.sharedMaterial = this._unlitBodyMat;
				}
			}
		}
	}

	private void resetLightWeapon()
	{
		this._playerAnimator.SetBool("strikeFlare", false);
	}

	private void setFlareIsLit()
	{
		LocalPlayer.Inventory.UseAltWorldPrefab = false;
	}

	public Animator _playerAnimator;

	public GameObject _cap;

	public GameObject _capHeld;

	public GameObject _flareLight;

	public MeshRenderer _flareBody;

	public Material _unlitBodyMat;

	public Material _litBodyMat;

	public bool _net;

	public bool flareIsLit;

	private MeshRenderer mr;

	private Animator _animator;

	private AnimatorStateInfo currState1;

	private AnimatorStateInfo nextState1;

	private AnimatorStateInfo currState2;

	private int playerReloadHash = Animator.StringToHash("flareLight");

	private int reloadHash = Animator.StringToHash("strikeFlare");

	private int shootHash = Animator.StringToHash("shoot");

	private int playerShootHash = Animator.StringToHash("shootFlareGun");

	private int playerAimShootHash = Animator.StringToHash("flareGunAimShoot");

	private int playerIdleHash = Animator.StringToHash("flareGunIdle");

	private int knockBackHash = Animator.StringToHash("knockBack");

	private int idleHash = Animator.StringToHash("unlitIdle");

	private int litIdleHash = Animator.StringToHash("litIdle");

	private int sittingHash = Animator.StringToHash("seatedRockIdle");

	private int flareToIdleHash = Animator.StringToHash("idleToFlare 0");

	private int idleToFlareHash = Animator.StringToHash("idleToFlare");

	private int flareIdleHash = Animator.StringToHash("flareIdle");

	private int idleBaseHash = Animator.StringToHash("idleBase");

	private bool reloadSync;

	private bool leftHandFull;

	private bool blockAmmoSpawn;
}
