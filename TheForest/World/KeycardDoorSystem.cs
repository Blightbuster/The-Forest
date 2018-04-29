using System;
using System.Collections;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.World
{
	
	public class KeycardDoorSystem : MonoBehaviour
	{
		
		private void Awake()
		{
			base.enabled = false;
			this.ToggleGoArray(this._scanAnimGos, false);
			this.ToggleGoArray(this._lockedGos, this._door._locked);
			this.ToggleGoArray(this._unlockedGos, !this._door._locked);
		}

		
		private void Update()
		{
			if (this._state == KeycardDoorSystem.State.Idle)
			{
				if (TheForest.Utils.Input.GetButtonDown("Take") && (this._door._state == AutomatedDoorSystem.State.Closed || this._door._state == AutomatedDoorSystem.State.Opened))
				{
					this._alpha = 0f;
					this._state = KeycardDoorSystem.State.Scanning;
					this.ToggleGoArray(this._scanAnimGos, true);
					this.ToggleGoArray(this._lockedGos, false);
					this.ToggleGoArray(this._unlockedGos, false);
					this.HideIcons();
					if (LocalPlayer.Inventory.Owns(this._itemId, true))
					{
						if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._itemId))
						{
							this._stashKeycard = false;
						}
						else
						{
							this._stashKeycard = true;
							LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.RightHand);
							LocalPlayer.Inventory.Equip(this._itemId, false);
						}
					}
				}
			}
			else if (this._state == KeycardDoorSystem.State.Scanning)
			{
				this._alpha += Time.deltaTime;
				if (this._alpha >= this._scanDuration)
				{
					this._state = KeycardDoorSystem.State.Idle;
					this.ToggleGoArray(this._scanAnimGos, false);
					this.EndScanCheck();
					if (Grabber.FocusedItemGO != base.gameObject)
					{
						base.enabled = false;
					}
					this.ToggleIcons(base.enabled);
				}
			}
		}

		
		private void GrabEnter()
		{
			this.ToggleIcons(true);
			base.enabled = true;
		}

		
		private void GrabExit()
		{
			base.enabled = (this._state == KeycardDoorSystem.State.Scanning);
			if (!base.enabled)
			{
				this.ToggleIcons(false);
			}
		}

		
		private void ToggleIcons(bool showPickup)
		{
			this._sheenIcon.SetActive(!showPickup);
			this._pickupIcon.SetActive(showPickup);
		}

		
		private void HideIcons()
		{
			this._sheenIcon.SetActive(false);
			this._pickupIcon.SetActive(false);
		}

		
		private void EndScanCheck()
		{
			if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._itemId))
			{
				LocalPlayer.Sfx.PlayWhoosh();
				this._door._locked = !this._door._locked;
				this._door.GetComponent<Collider>().enabled = false;
				this._door.GetComponent<Collider>().enabled = true;
				if (this._door._locked && this._door._state > AutomatedDoorSystem.State.Closing)
				{
					this._door._state = AutomatedDoorSystem.State.Closing;
					this._door.enabled = true;
				}
				if (this._stashKeycard)
				{
					base.StartCoroutine(this.StashKeycardRoutine());
				}
			}
			this.ToggleGoArray(this._lockedGos, this._door._locked);
			this.ToggleGoArray(this._unlockedGos, !this._door._locked);
		}

		
		private IEnumerator StashKeycardRoutine()
		{
			this._state = KeycardDoorSystem.State.Stashing;
			yield return YieldPresets.WaitPointFiveSeconds;
			if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._itemId))
			{
				LocalPlayer.Inventory.ItemAnimHash.ApplyAnimVars(LocalPlayer.Inventory.RightHand.ItemCache, false);
				yield return YieldPresets.WaitPointSevenSeconds;
				if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._itemId))
				{
					LocalPlayer.Inventory.StashEquipedWeapon(true);
					yield return YieldPresets.WaitOneSecond;
				}
			}
			this._state = KeycardDoorSystem.State.Idle;
			yield break;
		}

		
		private void ToggleGoArray(GameObject[] gos, bool active)
		{
			foreach (GameObject gameObject in gos)
			{
				if (gameObject)
				{
					gameObject.SetActive(active);
				}
			}
		}

		
		public AutomatedDoorSystem _door;

		
		[ItemIdPicker]
		public int _itemId;

		
		public GameObject[] _scanAnimGos;

		
		public GameObject[] _lockedGos;

		
		public GameObject[] _unlockedGos;

		
		public float _scanDuration = 3f;

		
		public GameObject _sheenIcon;

		
		public GameObject _pickupIcon;

		
		private float _alpha;

		
		private KeycardDoorSystem.State _state;

		
		private bool _stashKeycard;

		
		public enum State
		{
			
			Idle,
			
			Scanning,
			
			Stashing
		}
	}
}
