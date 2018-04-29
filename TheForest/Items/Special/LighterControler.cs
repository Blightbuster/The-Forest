using System;
using System.Collections;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.Special
{
	
	public class LighterControler : SpecialItemControlerBase
	{
		
		private void Awake()
		{
			LocalPlayer.Inventory.DefaultLight = this;
			LocalPlayer.Inventory.LastLight = this;
			LighterControler.IsBusy = false;
		}

		
		protected override void Update()
		{
			if (LocalPlayer.Inventory.enabled && this.CurrentViewTest() && !LocalPlayer.Inventory.QuickSelectGamepadSwitch)
			{
				if (!LighterControler.HasLightableItem || !this.IsActive)
				{
					if (TheForest.Utils.Input.GetButtonDown(this._buttonCached))
					{
						if (!this.IsActive)
						{
							this.OnActivating();
						}
						else
						{
							this.OnDeactivating();
						}
					}
				}
				else
				{
					if (TheForest.Utils.Input.GetButtonDown(this._buttonCached))
					{
						this._buttonPressStart = Time.realtimeSinceStartup;
					}
					if (TheForest.Utils.Input.GetButtonUp(this._buttonCached) && Time.realtimeSinceStartup - this._buttonPressStart < 0.2f)
					{
						this.OnDeactivating();
					}
				}
			}
			if (this._checkRemoval)
			{
				this._checkRemoval = false;
				if (!LocalPlayer.Inventory.Owns(this._itemId, true))
				{
					base.enabled = false;
				}
			}
		}

		
		public override bool ToggleSpecial(bool enable)
		{
			base.CancelInvoke();
			if (enable)
			{
				if (LocalPlayer.Inventory.LastLight != this)
				{
					LocalPlayer.Inventory.StashLeftHand();
					LocalPlayer.Inventory.LastLight = this;
				}
				this._breakRoutine = false;
				this.TurnLighterOn();
			}
			else
			{
				LocalPlayer.Animator.SetBoolReflected("lighterIgnite", false);
			}
			return true;
		}

		
		private void TurnLighterOn()
		{
			this._sparks = 0;
			base.InvokeRepeating("SparkLighter", 0.5f, 0.5f);
			this._lighterFlame.SetActive(false);
			LocalPlayer.Tuts.HideLighter();
		}

		
		public void equipLighterOnly()
		{
			base.StartCoroutine(this.equipLighterRoutine());
		}

		
		public void LightTheFire()
		{
			base.StartCoroutine(this.LightingFireRoutine());
		}

		
		public void LightHeldFire()
		{
			this._lightingHeldFireRoutine = base.StartCoroutine(this.LightingHeldFireRoutine());
		}

		
		public void CancelLightHeldFire()
		{
			if (this._lightingHeldFireRoutine != null)
			{
				LocalPlayer.Animator.SetBoolReflected("lightWeaponBool", false);
				LocalPlayer.Animator.SetBoolReflected("leftLightWeaponBool", false);
				LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
				base.StopCoroutine(this._lightingHeldFireRoutine);
				this._lightingHeldFireRoutine = null;
				LighterControler.IsBusy = false;
			}
		}

		
		private IEnumerator LightingFireRoutine()
		{
			if (!LighterControler.IsBusy)
			{
				bool wasActiveBeforeLightingFire = this.IsReallyActive;
				LighterControler.IsBusy = true;
				if (!wasActiveBeforeLightingFire && LocalPlayer.Inventory.Equip(this._itemId, false))
				{
					yield return YieldPresets.WaitPointSevenSeconds;
					yield return null;
				}
				if (this.IsReallyActive)
				{
					LocalPlayer.AnimControl.disableBirdOnHand();
					LocalPlayer.Animator.SetBoolReflected("leanForward", true);
					LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
					yield return new WaitForSeconds(2.8f);
					LocalPlayer.Animator.SetBoolReflected("leanForward", false);
					yield return YieldPresets.WaitOneSecond;
					LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
					if (!Clock.Dark && !wasActiveBeforeLightingFire && this.IsReallyActive)
					{
						LighterControler.IsBusy = false;
						this.StashLighter();
					}
				}
				LighterControler.IsBusy = false;
			}
			yield break;
		}

		
		private IEnumerator LightingHeldFireRoutine()
		{
			if (!LighterControler.IsBusy)
			{
				bool wasActiveBeforeLightingFire = this.IsReallyActive;
				LighterControler.IsBusy = true;
				if (!wasActiveBeforeLightingFire && LocalPlayer.Inventory.Equip(this._itemId, false))
				{
					yield return YieldPresets.WaitPointSevenSeconds;
					yield return null;
				}
				bool timeout = false;
				float timer = 0f;
				while (!timeout)
				{
					LocalPlayer.Animator.SetBool("leftLightWeaponBool", true);
					if (LocalPlayer.Animator.GetCurrentAnimatorStateInfo(3).shortNameHash == this._lightWeaponHash)
					{
						timeout = true;
					}
					timer += Time.deltaTime;
					if (timer > 4f)
					{
						this.CancelLightHeldFire();
						yield break;
					}
					yield return null;
				}
				if (this.IsReallyActive)
				{
					if (!this._breakRoutine && LocalPlayer.Inventory.RightHand != null)
					{
						LocalPlayer.Inventory.RightHand._held.SendMessage("setIsLit", SendMessageOptions.DontRequireReceiver);
					}
					LocalPlayer.Animator.SetBoolReflected("lightWeaponBool", true);
					LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
					yield return new WaitForSeconds(2.35f);
					LocalPlayer.Animator.SetBoolReflected("lightWeaponBool", false);
					LocalPlayer.Animator.SetBool("leftLightWeaponBool", false);
					if (!this._breakRoutine && !LocalPlayer.AnimControl.onRope && LocalPlayer.Inventory.RightHand && LocalPlayer.Inventory.RightHand._held.activeSelf)
					{
						LocalPlayer.Inventory.RightHand._held.SendMessage("enableFire", SendMessageOptions.DontRequireReceiver);
						LocalPlayer.Inventory.UseAltWorldPrefab = false;
					}
					yield return YieldPresets.WaitOneSecond;
					LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
					if (!Clock.Dark && !wasActiveBeforeLightingFire && this.IsReallyActive)
					{
						LighterControler.IsBusy = false;
						this.StashLighter();
					}
				}
				LighterControler.IsBusy = false;
				this._lightingHeldFireRoutine = null;
			}
			yield break;
		}

		
		private void SparkLighter()
		{
			if (!this._lighterFlame.activeSelf && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
			{
				LocalPlayer.Sfx.PlayLighterSound();
				LocalPlayer.Animator.SetBoolReflected("lighterIgnite", true);
				if (UnityEngine.Random.Range(0, 2) == 0 || ++this._sparks == this._maxSparksBeforeLight)
				{
					this._sparks = 0;
					base.Invoke("TurnLighterOff", (float)UnityEngine.Random.Range(10, 35));
					LocalPlayer.Animator.SetBoolReflected("lighterIgnite", false);
					this._lighterFlame.SetActive(true);
				}
			}
		}

		
		private void TurnLighterOff()
		{
			this._lighterFlame.SetActive(false);
		}

		
		public void StashLighter()
		{
			base.StartCoroutine(this.StashLighterRoutine());
		}

		
		public void StashLighter2()
		{
			if (LighterControler.IsBusy)
			{
				return;
			}
			base.StartCoroutine(this.StashLighterRoutine());
		}

		
		private IEnumerator StashLighterRoutine()
		{
			if (!LighterControler.IsBusy && !LocalPlayer.Inventory.IsSlotLocked(Item.EquipmentSlot.LeftHand) && !LocalPlayer.Animator.GetBool("leanForward"))
			{
				LighterControler.IsBusy = true;
				LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
				base.CancelInvoke();
				if (this.IsReallyActive)
				{
					LocalPlayer.Sfx.PlayWhoosh();
				}
				LocalPlayer.Animator.SetBoolReflected("lighterIgnite", false);
				LocalPlayer.Animator.SetBoolReflected("lighterHeld", false);
				this.TurnLighterOff();
				yield return YieldPresets.WaitPointFiveSeconds;
				LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
				if (this.IsReallyActive)
				{
					LocalPlayer.Inventory.SkipNextAddItemWoosh = true;
					LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.LeftHand, false, true, false);
				}
				LighterControler.IsBusy = false;
			}
			yield break;
		}

		
		private IEnumerator equipLighterRoutine()
		{
			if (!LighterControler.IsBusy)
			{
				bool wasActiveBeforeLightingFire = this.IsReallyActive;
				LighterControler.IsBusy = true;
				if (!wasActiveBeforeLightingFire && LocalPlayer.Inventory.Equip(this._itemId, false))
				{
					yield return YieldPresets.WaitPointSevenSeconds;
					yield return null;
				}
				bool timeout = false;
				float timer = 0f;
				while (!timeout)
				{
					if (LocalPlayer.Animator.GetCurrentAnimatorStateInfo(3).IsName("lighterIdle"))
					{
						timeout = true;
					}
					timer += Time.deltaTime;
					if (timer > 4f)
					{
						timeout = true;
					}
					yield return null;
				}
			}
			LighterControler.IsBusy = false;
			yield break;
		}

		
		protected override void OnActivating()
		{
			if (!LighterControler.IsBusy && LocalPlayer.Inventory.LastLight == this && !LocalPlayer.Animator.GetBool("drawBowBool") && !LocalPlayer.Inventory.IsSlotLocked(Item.EquipmentSlot.LeftHand))
			{
				LocalPlayer.Inventory.TurnOnLastLight();
			}
		}

		
		protected override void OnDeactivating()
		{
			if (this.IsReallyActive)
			{
				this.StashLighter();
			}
		}

		
		
		protected override bool IsActive
		{
			get
			{
				return LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.LeftHand, LocalPlayer.Inventory.LastLight._itemId);
			}
		}

		
		
		public bool IsReallyActive
		{
			get
			{
				return this.IsActive && LocalPlayer.Inventory.LastLight == this;
			}
		}

		
		private void disableBreakRoutine()
		{
			this._breakRoutine = false;
		}

		
		private void cancelLightingFromBow()
		{
			LighterControler.IsBusy = false;
			base.StopCoroutine("LightingHeldFireRoutine");
			LocalPlayer.Animator.SetBoolReflected("lightWeaponBool", false);
			LocalPlayer.Animator.SetBoolReflected("leftLightWeaponBool", false);
			base.StartCoroutine(this.StashLighterRoutine());
		}

		
		private void stopLightHeldFire()
		{
			this._breakRoutine = true;
			base.StopCoroutine("LightingHeldFireRoutine");
			base.StopCoroutine("LightingHeldFireRoutine");
			LocalPlayer.Animator.SetBoolReflected("lightWeaponBool", false);
			LocalPlayer.Animator.SetBoolReflected("leftLightWeaponBool", false);
		}

		
		public GameObject _lighterFlame;

		
		public int _maxSparksBeforeLight = 10;

		
		private float _buttonPressStart;

		
		private int _sparks;

		
		public bool _breakRoutine;

		
		private int _lightWeaponHash = Animator.StringToHash("lightWeapon");

		
		public static bool HasLightableItem;

		
		public static bool IsBusy;

		
		private Coroutine _lightingHeldFireRoutine;
	}
}
