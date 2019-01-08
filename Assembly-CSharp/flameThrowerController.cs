using System;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

public class flameThrowerController : MonoBehaviour
{
	private void OnEnable()
	{
		if (this.remote)
		{
			return;
		}
		this.isActive = true;
		LocalPlayer.Inventory.SpecialItems.SendMessage("disableBreakRoutine", SendMessageOptions.DontRequireReceiver);
		if (LocalPlayer.Inventory.CurrentView > PlayerInventory.PlayerViews.Loading)
		{
		}
	}

	private void OnDisable()
	{
		if (this.remote)
		{
			return;
		}
		LocalPlayer.Animator.SetBool("flameAttack", false);
		this.isActive = false;
	}

	private void Start()
	{
		if (this.remote)
		{
			this.animator = base.transform.root.GetComponentInChildren<Animator>();
		}
		else
		{
			this.animator = LocalPlayer.Animator;
		}
	}

	private void Update()
	{
		this.animState1 = this.animator.GetCurrentAnimatorStateInfo(1);
		this.animState3 = this.animator.GetCurrentAnimatorStateInfo(3);
		if (this.remote && this.ts.VREnabled)
		{
			float num = Vector3.Distance(this.flameFX.transform.position, this.lighterHeldGo.transform.position);
			this.VRLighterInRange = (num < this.RemoteLighterInRangeDist);
		}
		if (!this.remote)
		{
			if (LocalPlayer.Stats.hairSprayFuel.CurrentFuel < 0f)
			{
				LocalPlayer.Animator.SetFloatReflected("fuel", 0f);
			}
			else
			{
				LocalPlayer.Animator.SetFloatReflected("fuel", LocalPlayer.Stats.hairSprayFuel.CurrentFuel);
			}
		}
		if (!this.remote && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
		{
			if (TheForest.Utils.Input.GetButton("Fire1") && LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._hairsprayId))
			{
				LocalPlayer.Animator.SetBool("flameAttack", true);
			}
			else
			{
				LocalPlayer.Animator.SetBool("flameAttack", false);
			}
			Scene.HudGui.FuelReserve.fillAmount = LocalPlayer.Stats.hairSprayFuel.CurrentFuel / LocalPlayer.Stats.hairSprayFuel.MaxFuelCapacity;
			bool flag = LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World;
		}
		if (this.leftHandActive())
		{
			if (this.hairSprayInPosition() && !this.lighterInPosition())
			{
				if (!this.remote)
				{
					LocalPlayer.Stats.hairSprayFuel.CurrentFuel -= Time.deltaTime * 2f;
				}
				this.flameFX.SetActive(false);
				if (this.animator.GetFloat("fuel") > 0.1f)
				{
					this.sprayFX.SetActive(true);
				}
			}
			else if (this.lighterInPosition() && this.hairSprayInPosition())
			{
				if (!this.remote)
				{
					LocalPlayer.Stats.hairSprayFuel.CurrentFuel -= Time.deltaTime * 2f;
				}
				if (this.animator.GetFloat("fuel") > 0.1f)
				{
					this.flameFX.SetActive(true);
				}
				this.sprayFX.SetActive(false);
			}
			else
			{
				this.flameFX.SetActive(false);
				this.sprayFX.SetActive(false);
			}
			if (this.animator.GetFloat("fuel") <= 0.1f)
			{
				this.flameFX.SetActive(false);
				this.sprayFX.SetActive(false);
				return;
			}
		}
		else
		{
			if (this.hairSprayInPosition())
			{
				if (!this.remote)
				{
					LocalPlayer.Stats.hairSprayFuel.CurrentFuel -= Time.deltaTime * 2f;
				}
				if (this.animator.GetFloat("fuel") > 0.1f)
				{
					this.sprayFX.SetActive(true);
				}
				else
				{
					this.sprayFX.SetActive(false);
				}
			}
			else
			{
				this.sprayFX.SetActive(false);
			}
			this.flameFX.SetActive(false);
		}
	}

	private void checkStashLighter()
	{
		if (this.isActive)
		{
			return;
		}
		if (this.lighterWasEquipped)
		{
			this.lighterWasEquipped = false;
			this.checkForEquipped = false;
			return;
		}
		if (!LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._hairsprayId))
		{
			LocalPlayer.Inventory.SpecialItems.SendMessage("StashLighter2", SendMessageOptions.DontRequireReceiver);
			this.lighterWasEquipped = false;
			this.checkForEquipped = false;
		}
	}

	private bool lighterInPosition()
	{
		if ((!this.remote && ForestVR.Enabled) || (this.remote && this.ts.VREnabled))
		{
			return this.VRLighterInRange;
		}
		return this.animState3.shortNameHash == this.flameThrowerAttackHash;
	}

	private bool hairSprayInPosition()
	{
		if (!this.remote && ForestVR.Enabled)
		{
			return LocalPlayer.Animator.GetBool("flameAttack");
		}
		return this.animState1.shortNameHash == this.flameThrowerAttackHash;
	}

	private bool leftHandActive()
	{
		if (this.remote)
		{
			if (this.lighterHeldGo.activeInHierarchy)
			{
				return true;
			}
		}
		else if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.LeftHand, this._lighterId))
		{
			return true;
		}
		return false;
	}

	public targetStats ts;

	public bool remote;

	public bool VRLighterInRange;

	public GameObject flameFX;

	public GameObject sprayFX;

	public GameObject lighterHeldGo;

	private Animator animator;

	[ItemIdPicker]
	public int _lighterId;

	[ItemIdPicker]
	public int _hairsprayId;

	public bool lighterWasEquipped;

	private bool checkForEquipped;

	public bool isActive;

	public float RemoteLighterInRangeDist = 0.5f;

	private AnimatorStateInfo animState1;

	private AnimatorStateInfo animState3;

	private int flameThrowerAttackHash = Animator.StringToHash("flameThrowerAttack");

	private int flameAttackToIdleHash = Animator.StringToHash("attackToIdle");
}
