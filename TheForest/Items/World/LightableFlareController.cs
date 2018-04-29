using System;
using System.Collections;
using TheForest.Items.Special;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.World
{
	
	public class LightableFlareController : MonoBehaviour
	{
		
		
		
		public bool isLighting { get; private set; }

		
		private void OnEnable()
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
			this.fire.SetActive(false);
			LighterControler.HasLightableItem = true;
			LocalPlayer.Inventory.UseAltWorldPrefab = true;
		}

		
		private void Update()
		{
			if (!this.isLighting && TheForest.Utils.Input.GetButtonAfterDelay("Lighter", 0.5f, false) && !this.isLighting)
			{
				base.CancelInvoke("ResetIsLighting");
				base.Invoke("ResetIsLighting", 1f);
				this.isLighting = true;
				this.lightFlare();
			}
			if (TheForest.Utils.Input.GetButtonUp("Lighter"))
			{
				TheForest.Utils.Input.ResetDelayedAction();
			}
		}

		
		private void ResetIsLighting()
		{
			this.isLighting = false;
			LocalPlayer.Animator.SetBool("lightWeaponBool", false);
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

		
		private void lightFlare()
		{
			LocalPlayer.Animator.SetBool("lightWeaponBool", true);
		}

		
		private void OnDisable()
		{
			base.Invoke("disableActive", 0.2f);
			LighterControler.HasLightableItem = false;
			base.CancelInvoke("enableFire");
			this.isLit = false;
			this.fire.SetActive(false);
			LocalPlayer.Inventory.UseAltWorldPrefab = false;
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
