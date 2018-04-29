using System;
using System.Collections;
using ModAPI;
using TheForest.Items.Inventory;
using TheForest.Items.Special;
using TheForest.Items.World.Interfaces;
using TheForest.Utils;
using UltimateCheatmenu;
using UnityEngine;

namespace TheForest.Items.World
{
	
	public class LightableItemController : MonoBehaviour, IBurnableItem
	{
		
		
		
		public bool isLighting { get; private set; }

		
		private void __OnEnable__Original()
		{
			if (!this.checkForEquipped && LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.LeftHand, this._lighterId))
			{
				this.checkForEquipped = true;
				this.lighterWasEquipped = true;
			}
			this.checkForEquipped = true;
			this.isActive = true;
			base.StartCoroutine("forceFireOff");
			LocalPlayer.Inventory.SpecialItems.SendMessage("disableBreakRoutine", SendMessageOptions.DontRequireReceiver);
			if (LocalPlayer.Inventory.CurrentView > PlayerInventory.PlayerViews.Loading)
			{
				LocalPlayer.Inventory.SpecialItems.SendMessage("equipLighterOnly", SendMessageOptions.DontRequireReceiver);
			}
			this.fire.SetActive(false);
			LighterControler.HasLightableItem = true;
			LocalPlayer.Inventory.UseAltWorldPrefab = true;
			LocalPlayer.ActiveBurnableItem = this;
		}

		
		private void __Update__Original()
		{
			if (!LighterControler.IsBusy)
			{
				if (TheForest.Utils.Input.GetButton("Lighter") && !this.isLighting)
				{
					Scene.HudGui.SetDelayedIconController(this);
				}
				else
				{
					Scene.HudGui.UnsetDelayedIconController(this);
				}
				if (TheForest.Utils.Input.GetButtonAfterDelay("Lighter", 0.5f, false) && !this.isLighting)
				{
					if (!LocalPlayer.Inventory.DefaultLight.IsReallyActive)
					{
						LocalPlayer.Inventory.LastLight = LocalPlayer.Inventory.DefaultLight;
						LocalPlayer.Inventory.TurnOnLastLight();
					}
					base.CancelInvoke("ResetIsLighting");
					base.Invoke("ResetIsLighting", 4f);
					this.isLighting = true;
					LocalPlayer.SpecialItems.SendMessage("LightHeldFire");
					Scene.HudGui.UnsetDelayedIconController(this);
				}
			}
			if (TheForest.Utils.Input.GetButtonUp("Lighter"))
			{
				TheForest.Utils.Input.ResetDelayedAction();
				Scene.HudGui.UnsetDelayedIconController(this);
			}
		}

		
		private void ResetIsLighting()
		{
			this.isLighting = false;
		}

		
		private void GotClean()
		{
			if (this.isLit)
			{
				FMODCommon.PlayOneshotNetworked("event:/player/actions/molotov_quench", base.transform, FMODCommon.NetworkRole.Any);
			}
			this.isLit = false;
			base.StartCoroutine("forceFireOff");
			LighterControler.HasLightableItem = true;
		}

		
		private void OnDisable()
		{
			base.Invoke("disableActive", 0.2f);
			base.Invoke("checkStashLighter", 0.55f);
			LocalPlayer.SpecialItems.SendMessage("stopLightHeldFire", SendMessageOptions.DontRequireReceiver);
			LighterControler.HasLightableItem = false;
			base.CancelInvoke("enableFire");
			this.isLit = false;
			this.fire.SetActive(false);
			LocalPlayer.Inventory.UseAltWorldPrefab = false;
			if (this.Equals(LocalPlayer.ActiveBurnableItem))
			{
				LocalPlayer.ActiveBurnableItem = null;
			}
		}

		
		private void OnDestroy()
		{
			if (LocalPlayer.Inventory)
			{
				LocalPlayer.Inventory.SpecialItems.SendMessage("disableBreakRoutine", SendMessageOptions.DontRequireReceiver);
			}
		}

		
		private void OnProjectileThrown(GameObject projectile)
		{
			if (this.fire.activeSelf)
			{
				FMOD_StudioEventEmitter[] components = this.fire.GetComponents<FMOD_StudioEventEmitter>();
				for (int i = 0; i < components.Length; i++)
				{
					if (components[i] != null && components[i].path == this.fmodEvent)
					{
						components[i].TransplantEventInstance(projectile.transform);
						break;
					}
				}
			}
		}

		
		private IEnumerator forceFireOff()
		{
			float timer = 0f;
			while (timer < 0.26f)
			{
				this.fire.SetActive(false);
				this.isLighting = false;
				LocalPlayer.Inventory.UseAltWorldPrefab = true;
				timer += Time.deltaTime;
				yield return null;
			}
			yield break;
		}

		
		private void enableFire()
		{
			this.isLighting = false;
			this.fire.SetActive(true);
			base.Invoke("disableBlock", 0.5f);
			LocalPlayer.Tuts.MolotovTutDone();
		}

		
		private void disableBlock()
		{
		}

		
		private void disableActive()
		{
			this.isActive = false;
		}

		
		private void setIsLit()
		{
			this.isLit = true;
			base.CancelInvoke("checkIfLighting");
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
			if (!LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._molotovId))
			{
				LocalPlayer.Inventory.SpecialItems.SendMessage("StashLighter2", SendMessageOptions.DontRequireReceiver);
				this.lighterWasEquipped = false;
				this.checkForEquipped = false;
			}
		}

		
		public bool IsUnlit()
		{
			return !this.isLighting && !this.isLit;
		}

		
		private void OnEnable()
		{
			try
			{
				this.__OnEnable__Original();
				if (!this.isLit && UCheatmenu.InstLighter)
				{
					base.CancelInvoke("ResetIsLighting");
					this.isLighting = true;
					LocalPlayer.SpecialItems.SendMessage("LightHeldFire");
				}
			}
			catch (Exception ex)
			{
				Log.Write("Exception thrown: " + ex.ToString(), "UltimateCheatmenu");
				this.__OnEnable__Original();
			}
		}

		
		private void Update()
		{
			try
			{
				if (!this.isLit && UCheatmenu.InstLighter)
				{
					base.CancelInvoke("ResetIsLighting");
					this.isLighting = true;
					LocalPlayer.SpecialItems.SendMessage("LightHeldFire");
				}
				if (!UCheatmenu.InstLighter)
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

		
		public GameObject fire;

		
		public bool isLit;

		
		[ItemIdPicker]
		public int _molotovId;

		
		[ItemIdPicker]
		public int _lighterId;

		
		public bool lighterWasEquipped;

		
		private bool checkForEquipped;

		
		public bool isActive;

		
		public string fmodEvent = "event:/combat/molotov_held";
	}
}
