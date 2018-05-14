using System;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using TheForest.Items.Special;
using TheForest.Items.World.Interfaces;
using TheForest.Player;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace TheForest.Items.World
{
	
	public class BowController : MonoBehaviour, IBurnableItem
	{
		
		private void Start()
		{
			if (base.GetComponentInParent<PlayerInventory>())
			{
				this._nextReArm = float.MaxValue;
				this._ammoAnimated.SetActive(true);
				this._attackHash = Animator.StringToHash("attacking");
				this._animator = LocalPlayer.Animator;
				this._bowAnimator = base.GetComponent<Animator>();
				if (this._aimingReticle)
				{
					this._aimingReticle.enabled = false;
				}
				base.Invoke("UpdateArrowRenderer", 0.25f);
			}
		}

		
		private void OnEnable()
		{
			if (ForestVR.Enabled)
			{
				if (this._bowVr)
				{
					if (this._bowVr)
					{
						this._bowVr.SetActive(true);
					}
					this._bowVr.SendMessage("SpawnAndAttachObject", LocalPlayer.vrPlayerControl.RightHand);
				}
				if (this._bowAnimator)
				{
					this._bowAnimator.SetBool("VR", true);
				}
			}
			else if (this._bowVr)
			{
				this._bowVr.SetActive(false);
			}
			EventRegistry.Player.Subscribe(TfEvent.AddedItem, new EventRegistry.SubscriberCallback(this.OnItemAdded));
			if (this.CurrentArrowItemView.ActiveBonus == WeaponStatUpgrade.Types.BurningAmmo)
			{
				LighterControler.HasLightableItem = true;
			}
			this.SetActiveArrowBonus(this._activeAmmoBonus);
			this.UpdateArrowRenderer();
			LocalPlayer.ActiveBurnableItem = this;
		}

		
		private void OnDisable()
		{
			if (ForestVR.Enabled && this._bowVr)
			{
				ItemPackageSpawner component = this._bowVr.GetComponent<ItemPackageSpawner>();
				if (component)
				{
					component.removeAttachedObjects(LocalPlayer.vrPlayerControl.RightHand);
				}
			}
			EventRegistry.Player.Unsubscribe(TfEvent.AddedItem, new EventRegistry.SubscriberCallback(this.OnItemAdded));
			LighterControler.HasLightableItem = false;
			this._nextReArm = float.MaxValue;
			this._ammoAnimated.SetActive(true);
			if (this._animator)
			{
				this._animator.SetBoolReflected("drawBowBool", false);
				if (base.gameObject.activeInHierarchy)
				{
					this._bowAnimator.SetBoolReflected("drawBool", false);
				}
				this.ShutDown(false);
				this.ShutDownFire();
			}
			if (this._activeFireArrowGO)
			{
				UnityEngine.Object.Destroy(this._activeFireArrowGO);
				LocalPlayer.Inventory.IsWeaponBurning = false;
				LocalPlayer.ScriptSetup.targetInfo.arrowFire = false;
			}
			this._lightingArrow = false;
			if (Scene.HudGui)
			{
				Scene.HudGui.ToggleArrowBonusIcon.SetActive(false);
			}
			if (this.Equals(LocalPlayer.ActiveBurnableItem))
			{
				LocalPlayer.ActiveBurnableItem = null;
			}
		}

		
		private void Update()
		{
			if (this._player.CurrentView == PlayerInventory.PlayerViews.World)
			{
				if (ForestVR.Enabled)
				{
					if (this._longbowVr == null)
					{
						this._longbowVr = LocalPlayer.vrPlayerControl.VRCameraRig.GetComponentInChildren<Longbow>();
					}
					if (this._arrowHandVr == null)
					{
						this._arrowHandVr = LocalPlayer.vrPlayerControl.VRCameraRig.GetComponentInChildren<ArrowHand>();
					}
					if (this._bowAnimatorVr == null && this._longbowVr)
					{
						this._bowAnimatorVr = this._longbowVr.GetComponent<Animator>();
					}
					LocalPlayer.Inventory.CancelNextChargedAttack = false;
				}
				if (this._player.Owns(this._ammoItemId, false))
				{
					LocalPlayer.Animator.SetBool("noAmmo", false);
				}
				else
				{
					LocalPlayer.Animator.SetBool("noAmmo", true);
				}
				this._bowAnimator.SetFloat("bowSpeed", this._bowSpeed);
				if (LocalPlayer.AnimControl.currLayerState1.shortNameHash == this._releaseBowHash && !LocalPlayer.Animator.IsInTransition(1) && !ForestVR.Enabled)
				{
					this._ammoAnimated.transform.position = LocalPlayer.ScriptSetup.leftHandHeld.position;
				}
				if (!LocalPlayer.Create.Grabber.Target && LocalPlayer.MainCamTr.forward.y < -0.85f && !LocalPlayer.Animator.GetBool("drawBowBool"))
				{
					WeaponStatUpgrade.Types types = this.NextAvailableArrowBonus(this.BowItemView.ActiveBonus);
					if (types != this.BowItemView.ActiveBonus)
					{
						this._showRotateArrowType = true;
						if (!Scene.HudGui.ToggleArrowBonusIcon.activeSelf)
						{
							Scene.HudGui.ToggleArrowBonusIcon.SetActive(true);
						}
						if (TheForest.Utils.Input.GetButtonDown("Rotate"))
						{
							LocalPlayer.Sfx.PlayWhoosh();
							this.SetActiveBowBonus(types);
							Scene.HudGui.ToggleArrowBonusIcon.SetActive(false);
						}
					}
					else if (this._showRotateArrowType)
					{
						this._showRotateArrowType = false;
						Scene.HudGui.ToggleArrowBonusIcon.SetActive(false);
					}
				}
				else if (this._showRotateArrowType)
				{
					this._showRotateArrowType = false;
					Scene.HudGui.ToggleArrowBonusIcon.SetActive(false);
				}
				if (this.CurrentArrowItemView.ActiveBonus != this.BowItemView.ActiveBonus)
				{
					LocalPlayer.Inventory.SortInventoryViewsByBonus(this.CurrentArrowItemView, this.BowItemView.ActiveBonus, false);
					if (this.CurrentArrowItemView.ActiveBonus != this.BowItemView.ActiveBonus)
					{
						this.SetActiveBowBonus(this.CurrentArrowItemView.ActiveBonus);
					}
					this.UpdateArrowRenderer();
				}
				WeaponStatUpgrade.Types activeBonus = this.CurrentArrowItemView.ActiveBonus;
				bool canSetArrowOnFire = this.CanSetArrowOnFire;
				if (canSetArrowOnFire)
				{
					if (TheForest.Utils.Input.GetButtonAfterDelay("Lighter", 0.5f, false))
					{
						this.SetArrowOnFire();
					}
				}
				else if (activeBonus != WeaponStatUpgrade.Types.BurningAmmo && this._activeAmmoBonus != activeBonus)
				{
					this.SetActiveArrowBonus(activeBonus);
				}
				if (!this._lightingArrow)
				{
					AnimatorStateInfo currentAnimatorStateInfo = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1);
					if (TheForest.Utils.Input.GetButtonDown("Fire1") && !LocalPlayer.Animator.GetBool("ballHeld") && !ForestVR.Enabled)
					{
						LocalPlayer.Inventory.CancelNextChargedAttack = false;
						if (this._aimingReticle)
						{
							this._aimingReticle.enabled = true;
						}
						if ((currentAnimatorStateInfo.shortNameHash != this._lightBowHash || currentAnimatorStateInfo.normalizedTime >= 0.95f) && currentAnimatorStateInfo.shortNameHash != this._releaseBow0Hash && (currentAnimatorStateInfo.shortNameHash != this._releaseBowHash || currentAnimatorStateInfo.normalizedTime >= 0.5f))
						{
							this.ReArm();
						}
						this._animator.SetBoolReflected("drawBowBool", true);
						this._bowAnimator.SetBoolReflected("drawBool", true);
						this._bowAnimator.SetBoolReflected("bowFireBool", false);
						this._animator.SetBoolReflected("bowFireBool", false);
						this._animator.SetBoolReflected("lightWeaponBool", false);
						LocalPlayer.SpecialItems.SendMessage("cancelLightingFromBow");
						LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
						this._player.StashLeftHand();
						this._animator.SetBoolReflected("checkArms", false);
						this._animator.SetBoolReflected("onHand", false);
					}
					else if ((TheForest.Utils.Input.GetButtonDown("AltFire") || LocalPlayer.Animator.GetBool("ballHeld")) && !ForestVR.Enabled)
					{
						LocalPlayer.AnimControl.animEvents.enableSpine();
						this._player.CancelNextChargedAttack = true;
						this._animator.SetBool("drawBowBool", false);
						this._bowAnimator.SetBool("drawBool", false);
						this.ShutDown(false);
					}
					if (currentAnimatorStateInfo.shortNameHash == this._drawIdleHash && !LocalPlayer.Inventory.IsLeftHandEmpty())
					{
						LocalPlayer.SpecialItems.SendMessage("cancelLightingFromBow");
						LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
						this._player.StashLeftHand();
					}
					if (currentAnimatorStateInfo.shortNameHash == this._drawBowHash)
					{
						this._bowAnimator.Play(this._drawBowHash, 0, currentAnimatorStateInfo.normalizedTime);
					}
					if ((TheForest.Utils.Input.GetButtonUp("Fire1") || LocalPlayer.Animator.GetBool("ballHeld")) && !ForestVR.Enabled)
					{
						this._currentAmmo = this.CurrentArrowItemView;
						if (this._aimingReticle)
						{
							this._aimingReticle.enabled = false;
						}
						base.CancelInvoke();
						if (this._animator.GetCurrentAnimatorStateInfo(1).tagHash == this._attackHash && this._animator.GetBool("drawBowBool") && !LocalPlayer.Animator.GetBool("ballHeld") && !LocalPlayer.Inventory.blockRangedAttack && LocalPlayer.AnimControl.currLayerState0.shortNameHash != LocalPlayer.AnimControl.landHeavyHash)
						{
							this._animator.SetBoolReflected("bowFireBool", true);
							this._bowAnimator.SetBoolReflected("bowFireBool", true);
							this._animator.SetBoolReflected("drawBowBool", false);
							this._bowAnimator.SetBoolReflected("drawBool", false);
							LocalPlayer.TargetFunctions.sendPlayerAttacking();
							this.InitReArm();
						}
						else if (LocalPlayer.Animator.GetBool("ballHeld"))
						{
							LocalPlayer.AnimControl.animEvents.enableSpine();
							this._player.CancelNextChargedAttack = true;
							this._animator.SetBoolReflected("drawBowBool", false);
							this._bowAnimator.SetBoolReflected("drawBool", false);
							this.ShutDown(false);
						}
						else if ((LocalPlayer.AnimControl.currLayerState1.shortNameHash == this._releaseBow0Hash || LocalPlayer.AnimControl.currLayerState0.shortNameHash == LocalPlayer.AnimControl.landHeavyHash || LocalPlayer.AnimControl.currLayerState1.shortNameHash == this._releaseBowHash || LocalPlayer.AnimControl.currLayerState1.shortNameHash == this._drawBowHash || LocalPlayer.AnimControl.nextLayerState1.shortNameHash == this._drawBowHash) && LocalPlayer.AnimControl.nextLayerState1.shortNameHash != this._bowIdleHash)
						{
							if (LocalPlayer.Inventory.blockRangedAttack)
							{
								this.ShutDown(false);
							}
							else
							{
								this.ShutDown(true);
							}
						}
					}
					else if (this._nextReArm < Time.time)
					{
						this.ReArm();
					}
				}
				else if (!ForestVR.Enabled)
				{
					LocalPlayer.Inventory.CancelNextChargedAttack = true;
				}
			}
		}

		
		private void LateUpdate()
		{
			if (LocalPlayer.AnimControl.currLayerState1.shortNameHash == this._releaseBowHash && !LocalPlayer.Animator.IsInTransition(1) && !ForestVR.Enabled)
			{
				this._ammoAnimated.transform.position = LocalPlayer.ScriptSetup.leftHandHeld.position;
			}
			if (ForestVR.Enabled)
			{
				if (this._longbowVr)
				{
					base.transform.position = this._longbowVr.bowFollowTransform.transform.position;
					base.transform.rotation = this._longbowVr.bowFollowTransform.transform.rotation;
					this._bowAnimator.Play(this._bowPullVrHash, 0, this._bowAnimatorVr.GetCurrentAnimatorStateInfo(0).normalizedTime);
				}
				if (this._arrowHandVr)
				{
					if (this._arrowHandVr.currentArrow != null)
					{
						this._ammoAnimated.transform.position = this._arrowHandVr.currentArrow.GetComponent<Arrow>().arrowFollowTransform.position;
						this._ammoAnimated.transform.rotation = this._arrowHandVr.currentArrow.GetComponent<Arrow>().arrowFollowTransform.rotation;
						this._ammoAnimated.SetActive(true);
					}
					else
					{
						this._ammoAnimated.SetActive(false);
					}
				}
			}
		}

		
		private void OnDestroy()
		{
			if (this._ammoAnimated)
			{
				UnityEngine.Object.Destroy(this._ammoAnimated);
			}
		}

		
		private void OnItemAdded(object o)
		{
			this.OnItemAdded((int)o);
		}

		
		private void OnItemAdded(int itemId)
		{
			if (this._ammoItemId == itemId && this._player.AmountOf(this._ammoItemId, false) == 1)
			{
				this._player.ToggleAmmo(this._ammoItemId, true);
			}
		}

		
		private void ShutDownFire()
		{
			this.SetActiveArrowBonus((WeaponStatUpgrade.Types)(-1));
			LocalPlayer.Inventory.IsWeaponBurning = false;
			LocalPlayer.ScriptSetup.targetInfo.arrowFire = false;
		}

		
		private void ShutDown(bool rearm)
		{
			base.CancelInvoke();
			if (base.gameObject.activeInHierarchy)
			{
				this._animator.SetBool("drawBowBool", false);
				this._bowAnimator.SetBool("drawBool", false);
				this._animator.SetBool("bowFireBool", false);
				this._bowAnimator.SetBool("bowFireBool", false);
			}
			if (rearm)
			{
				this.InitReArm();
			}
		}

		
		private void InitReArm()
		{
			if (this._activeFireArrowGO)
			{
				this._activeFireArrowGO.transform.parent = null;
			}
			this._ammoAnimated.SetActive(false);
			this._nextReArm = Time.time + this._reArmDelay;
		}

		
		private void ReArm()
		{
			this._nextReArm = float.MaxValue;
			this.EnsureArrowIsInHierchy();
			this._ammoAnimated.SetActive(true);
		}

		
		private void EnsureArrowIsInHierchy()
		{
			if (!Reparent.Locked && this._ammoAnimationRenderer.transform.parent.parent != this._ammoHook && base.transform.root.CompareTag("Player"))
			{
				this._ammoAnimationRenderer.transform.parent.parent = this._ammoHook;
				this._ammoAnimationRenderer.transform.parent.localPosition = Vector3.zero;
				this._ammoAnimationRenderer.transform.parent.localRotation = Quaternion.identity;
			}
		}

		
		private void LightArrow()
		{
			GameStats.LitArrow.Invoke();
			this._lightingArrow = false;
			this.SetActiveArrowBonus(WeaponStatUpgrade.Types.BurningAmmo);
			this._activeFireArrowGO = UnityEngine.Object.Instantiate<MasterFireSpread>(this._fireArrowPrefab);
			this._activeFireArrowGO.enabled = false;
			this._activeFireArrowGO.transform.parent = this._ammoAnimated.transform;
			this._activeFireArrowGO.transform.position = this._ammoAnimationRenderer.transform.position;
			this._activeFireArrowGO.transform.rotation = this._ammoAnimationRenderer.transform.rotation;
			this._activeFireArrowGO.owner = LocalPlayer.Transform;
			WeaponBonus componentInChildren = this._activeFireArrowGO.GetComponentInChildren<WeaponBonus>();
			if (componentInChildren)
			{
				componentInChildren._owner = LocalPlayer.Transform;
				componentInChildren.enabled = true;
			}
			LocalPlayer.Inventory.IsWeaponBurning = true;
			LocalPlayer.ScriptSetup.targetInfo.arrowFire = true;
			LighterControler.HasLightableItem = (this.CurrentArrowItemView.ActiveBonus == WeaponStatUpgrade.Types.BurningAmmo);
		}

		
		private void SetActiveBowBonus(WeaponStatUpgrade.Types bonusType)
		{
			if (this.BowItemView.ActiveBonus == WeaponStatUpgrade.Types.BurningAmmo && this.CurrentArrowItemView.ActiveBonus == WeaponStatUpgrade.Types.BurningAmmo && bonusType != WeaponStatUpgrade.Types.BurningAmmo && this._activeFireArrowGO)
			{
				this.CurrentArrowItemView.ActiveBonus = (WeaponStatUpgrade.Types)(-1);
				UnityEngine.Object.Destroy(this._activeFireArrowGO.gameObject);
				this._activeFireArrowGO = null;
				LocalPlayer.Inventory.IsWeaponBurning = false;
				LocalPlayer.ScriptSetup.targetInfo.arrowFire = false;
			}
			this.BowItemView.ActiveBonus = bonusType;
		}

		
		private void SetActiveArrowBonus(WeaponStatUpgrade.Types bonusType)
		{
			if (this._activeAmmoBonus != bonusType)
			{
				this._activeAmmoBonus = bonusType;
				this.UpdateArrowRenderer();
			}
			if (this._activeFireArrowGO)
			{
				UnityEngine.Object.Destroy(this._activeFireArrowGO.gameObject);
				this._activeFireArrowGO = null;
				LocalPlayer.Inventory.IsWeaponBurning = false;
				LocalPlayer.ScriptSetup.targetInfo.arrowFire = true;
			}
		}

		
		private void UpdateArrowRenderer()
		{
			this.EnsureArrowIsInHierchy();
			if (this.CurrentArrowItemView.Properties.ActiveBonus == WeaponStatUpgrade.Types.BoneAmmo)
			{
				this._ammoAnimationRenderer.enabled = false;
				this._boneAmmoAnimationRenderer.enabled = true;
				this._modernAmmoAnimationRenderer.enabled = false;
			}
			else if (this.CurrentArrowItemView.Properties.ActiveBonus == WeaponStatUpgrade.Types.ModernAmmo)
			{
				this._ammoAnimationRenderer.enabled = false;
				this._boneAmmoAnimationRenderer.enabled = false;
				this._modernAmmoAnimationRenderer.enabled = true;
			}
			else
			{
				this._ammoAnimationRenderer.enabled = true;
				this._boneAmmoAnimationRenderer.enabled = false;
				this._modernAmmoAnimationRenderer.enabled = false;
				this._ammoAnimationRenderer.sharedMaterials = this.CurrentArrowItemView.GetComponent<Renderer>().sharedMaterials;
			}
		}

		
		private WeaponStatUpgrade.Types NextAvailableArrowBonus(WeaponStatUpgrade.Types current)
		{
			if (!LocalPlayer.Inventory.Owns(this._ammoItemId, true))
			{
				return (WeaponStatUpgrade.Types)(-1);
			}
			WeaponStatUpgrade.Types types;
			switch (current)
			{
			case WeaponStatUpgrade.Types.BoneAmmo:
				types = WeaponStatUpgrade.Types.ModernAmmo;
				break;
			default:
				if (current != WeaponStatUpgrade.Types.BurningAmmo)
				{
					if (current != WeaponStatUpgrade.Types.PoisonnedAmmo)
					{
						types = WeaponStatUpgrade.Types.BurningAmmo;
					}
					else
					{
						types = WeaponStatUpgrade.Types.BoneAmmo;
					}
				}
				else
				{
					types = WeaponStatUpgrade.Types.PoisonnedAmmo;
				}
				break;
			case WeaponStatUpgrade.Types.ModernAmmo:
				types = (WeaponStatUpgrade.Types)(-1);
				break;
			}
			if (LocalPlayer.Inventory.OwnsItemWithBonus(this._ammoItemId, types))
			{
				return types;
			}
			return this.NextAvailableArrowBonus(types);
		}

		
		public bool IsUnlit()
		{
			return this.CanSetArrowOnFire && this._ammoAnimated.activeInHierarchy && !(this._aimingReticle != null);
		}

		
		private void SetArrowOnFire()
		{
			this.ReArm();
			this._player.SpecialItems.SendMessage("LightHeldFire");
			base.CancelInvoke("LightArrow");
			base.Invoke("LightArrow", 2f);
			this._lightingArrow = true;
		}

		
		private void OnAmmoFired(GameObject Ammo)
		{
			GameStats.ArrowFired.Invoke();
			WeaponStatUpgrade.Types activeAmmoBonus = this._activeAmmoBonus;
			if (activeAmmoBonus != WeaponStatUpgrade.Types.BurningAmmo)
			{
				if (activeAmmoBonus == WeaponStatUpgrade.Types.PoisonnedAmmo)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this._poisonArrowPrefab);
					gameObject.transform.parent = Ammo.transform;
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localRotation = Quaternion.identity;
				}
			}
			else if (this._activeFireArrowGO)
			{
				this._activeFireArrowGO.transform.parent = Ammo.transform;
				if (!this._activeFireArrowGO.GetComponent<destroyAfter>())
				{
					this._activeFireArrowGO.gameObject.AddComponent<destroyAfter>().destroyTime = 15f;
				}
				this._activeFireArrowGO = null;
				LocalPlayer.Inventory.IsWeaponBurning = false;
				LocalPlayer.ScriptSetup.targetInfo.arrowFire = false;
			}
			this.SetActiveArrowBonus((WeaponStatUpgrade.Types)(-1));
			this._currentAmmo.ActiveBonus = (WeaponStatUpgrade.Types)(-1);
		}

		
		private void setupBowForVr()
		{
			Hand componentInParent = base.transform.GetComponentInParent<Hand>();
			this._bowVr.SendMessage("SpawnAndAttachObject", componentInParent);
		}

		
		
		public bool CanSetArrowOnFire
		{
			get
			{
				return !this._lightingArrow && this._activeAmmoBonus == (WeaponStatUpgrade.Types)(-1) && !LighterControler.IsBusy && !LocalPlayer.Animator.GetBool("drawBowBool") && this._player.Owns(this._ammoItemId, false) && this.CurrentArrowItemView.ActiveBonus == WeaponStatUpgrade.Types.BurningAmmo;
			}
		}

		
		
		private InventoryItemView BowItemView
		{
			get
			{
				return LocalPlayer.Inventory.InventoryItemViewsCache[this._bowItemId][0];
			}
		}

		
		
		private InventoryItemView CurrentArrowItemView
		{
			get
			{
				return LocalPlayer.Inventory.InventoryItemViewsCache[this._ammoItemId][Mathf.Max(LocalPlayer.Inventory.AmountOf(this._ammoItemId, false) - 1, 0)];
			}
		}

		
		
		private InventoryItemView PrevioustArrowItemView
		{
			get
			{
				return LocalPlayer.Inventory.InventoryItemViewsCache[this._ammoItemId][Mathf.Max(LocalPlayer.Inventory.AmountOf(this._ammoItemId, false) - 1, 0)];
			}
		}

		
		public PlayerInventory _player;

		
		[ItemIdPicker(Item.Types.RangedWeapon)]
		public int _bowItemId;

		
		[ItemIdPicker(Item.Types.Ammo)]
		public int _ammoItemId;

		
		public Transform _ammoHook;

		
		public GameObject _ammoAnimated;

		
		public Renderer _ammoAnimationRenderer;

		
		public Renderer _boneAmmoAnimationRenderer;

		
		public Renderer _modernAmmoAnimationRenderer;

		
		public float _reArmDelay = 0.5f;

		
		public float _bowSpeed;

		
		public MasterFireSpread _fireArrowPrefab;

		
		public GameObject _poisonArrowPrefab;

		
		public AimingReticle _aimingReticle;

		
		public GameObject _bowVr;

		
		public Longbow _longbowVr;

		
		public ArrowHand _arrowHandVr;

		
		private Animator _bowAnimatorVr;

		
		private InventoryItemView _currentAmmo;

		
		private bool _showRotateArrowType;

		
		private bool _lightingArrow;

		
		private int _attackHash;

		
		private int _drawIdleHash = Animator.StringToHash("drawBowIdle");

		
		private int _drawBowHash = Animator.StringToHash("drawBow");

		
		private int _releaseBow0Hash = Animator.StringToHash("releaseBow 0");

		
		private int _releaseBowHash = Animator.StringToHash("releaseBow");

		
		private int _bowIdleHash = Animator.StringToHash("bowIdle");

		
		private int _lightBowHash = Animator.StringToHash("lightBow");

		
		private int _bowPullVrHash = Animator.StringToHash("bowPull_VR");

		
		private Animator _animator;

		
		private Animator _bowAnimator;

		
		private float _nextReArm;

		
		public WeaponStatUpgrade.Types _activeAmmoBonus = (WeaponStatUpgrade.Types)(-1);

		
		public MasterFireSpread _activeFireArrowGO;
	}
}
