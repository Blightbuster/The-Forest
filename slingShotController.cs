using System;
using System.Collections;
using TheForest.Items;
using TheForest.Items.World;
using TheForest.Utils;
using UnityEngine;


public class slingShotController : MonoBehaviour
{
	
	private void Start()
	{
		this._slingAnimator = base.transform.GetComponent<Animator>();
		this._playerAnimator = LocalPlayer.Animator;
	}

	
	private void OnEnable()
	{
		if (!this._playerAnimator)
		{
			this._slingAnimator = base.transform.GetComponent<Animator>();
			this._playerAnimator = LocalPlayer.Animator;
		}
		this.resetAttack();
	}

	
	private void Update()
	{
		if ((TheForest.Utils.Input.GetButtonDown("Fire1") || TheForest.Utils.Input.GetButton("Fire1")) && !LocalPlayer.Animator.GetBool("ballHeld"))
		{
			this._playerAnimator.SetBool("aimSlingBool", true);
			this._slingAnimator.SetBool("toAim", true);
			LocalPlayer.Inventory.StashLeftHand();
		}
		if (TheForest.Utils.Input.GetButtonUp("Fire1"))
		{
			if (LocalPlayer.AnimControl.currLayerState1.shortNameHash == this.aimSlingIdleHash)
			{
				this._playerAnimator.SetBool("attack", true);
				this._slingAnimator.SetBool("attack", true);
				base.Invoke("resetAttack", 0.5f);
				base.Invoke("fireProjectile", 0.15f);
				LocalPlayer.Sfx.PlayBowSnap();
				LocalPlayer.TargetFunctions.sendPlayerAttacking();
			}
			else
			{
				this._playerAnimator.SetBool("attack", false);
				this._playerAnimator.SetBool("aimSlingBool", false);
				this._slingAnimator.SetBool("toAim", false);
			}
		}
		if (this._AmmoHeld.activeSelf != LocalPlayer.Inventory.Owns(this._ammoItemId, true))
		{
			this._AmmoHeld.SetActive(!this._AmmoHeld.activeSelf);
		}
	}

	
	private void fireProjectile()
	{
		if (LocalPlayer.Inventory.RemoveItem(this._ammoItemId, 1, false, true))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(this._Ammo, this._ammoSpawnPos.transform.position, this._ammoSpawnPos.transform.rotation) as GameObject;
			Rigidbody component = gameObject.GetComponent<Rigidbody>();
			rockSound component2 = gameObject.GetComponent<rockSound>();
			if (component2)
			{
				component2.slingShot = true;
			}
			if (BoltNetwork.isRunning)
			{
				BoltEntity component3 = gameObject.GetComponent<BoltEntity>();
				if (component3)
				{
					BoltNetwork.Attach(gameObject);
				}
			}
			PickUp componentInChildren = gameObject.GetComponentInChildren<PickUp>();
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
			component.AddForce(4000f * (0.016666f / Time.fixedDeltaTime) * this._ammoSpawnPos.transform.forward);
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

	
	private void resetAttack()
	{
		this._playerAnimator.SetBool("attack", false);
		this._playerAnimator.SetBool("aimSlingBool", false);
		this._slingAnimator.SetBool("attack", false);
		this._slingAnimator.SetBool("toAim", false);
	}

	
	private void OnDisable()
	{
		this.resetAttack();
	}

	
	public GameObject _AmmoHeld;

	
	public GameObject _Ammo;

	
	public GameObject _ammoSpawnPos;

	
	[ItemIdPicker(Item.Types.Ammo)]
	public int _ammoItemId;

	
	private Animator _playerAnimator;

	
	private Animator _slingAnimator;

	
	private int aimSlingIdleHash = Animator.StringToHash("slingShotAimIdle");
}
